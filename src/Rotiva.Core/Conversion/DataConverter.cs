using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Rotiva.Localization;
using Rotiva.Models;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Rotiva.Conversion;

/// <summary>
/// Konversi JSON, YAML dan CSV tanpa engine luar. Semua format dibaca ke <see cref="JsonNode"/>
/// lalu ditulis ke format tujuan, jadi tiap pasangan format lewat jalur yang sama.
/// </summary>
internal static partial class DataConverter
{
    // Alias YAML bisa mereferensikan dirinya sendiri atau berlipat ganda ("billion laughs").
    const int MaxDepth = 256;
    const int MaxNodes = 2_000_000;

    static readonly string[] YamlWords =
        ["null", "~", "true", "false", "yes", "no", "on", "off", "y", "n"];

    public static void Convert(string source, string extension, string output)
    {
        string sourceExtension = FormatCatalog.Normalize(Path.GetExtension(source));
        JsonNode? data = sourceExtension switch
        {
            ".json" => ReadJson(source),
            ".yaml" => ReadYaml(source),
            _ => ReadCsv(source),
        };

        switch (extension)
        {
            case ".json":
                File.WriteAllText(output, ToJson(data) + Environment.NewLine, new UTF8Encoding(false));
                break;
            case ".yaml":
                File.WriteAllText(output, ToYaml(data), new UTF8Encoding(false));
                break;
            case ".csv":
                // BOM supaya Excel membaca huruf non-ASCII dengan benar.
                File.WriteAllText(output, ToCsv(data), new UTF8Encoding(true));
                break;
            default:
                throw new ConversionException(L.F("The {0} format is not supported yet.", extension));
        }
    }

    static JsonNode? ReadJson(string path)
    {
        try
        {
            return JsonNode.Parse(File.ReadAllText(path), documentOptions: new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip,
            });
        }
        catch (JsonException)
        {
            throw new ConversionException(L.F("This file isn't valid {0}.", "JSON"));
        }
    }

    static JsonNode? ReadYaml(string path)
    {
        var stream = new YamlStream();
        try
        {
            using var reader = new StreamReader(path);
            stream.Load(reader);
        }
        catch (YamlException)
        {
            throw new ConversionException(L.F("This file isn't valid {0}.", "YAML"));
        }
        if (stream.Documents.Count == 0)
            throw new ConversionException(L.F("This file isn't valid {0}.", "YAML"));

        int nodes = 0;
        return FromYaml(stream.Documents[0].RootNode, 0, ref nodes);
    }

    static JsonNode? FromYaml(YamlNode node, int depth, ref int nodes)
    {
        if (depth > MaxDepth || ++nodes > MaxNodes)
            throw new ConversionException(L.T("This file is too deeply nested or too large to convert."));

        switch (node)
        {
            case YamlMappingNode mapping:
                var result = new JsonObject();
                foreach (var (key, value) in mapping.Children)
                    result[key is YamlScalarNode { Value: { } name } ? name : key.ToString()] = FromYaml(value, depth + 1, ref nodes);
                return result;
            case YamlSequenceNode sequence:
                var array = new JsonArray();
                foreach (var item in sequence.Children)
                    array.Add(FromYaml(item, depth + 1, ref nodes));
                return array;
            case YamlScalarNode { Style: ScalarStyle.Plain, Value: var text }:
                return PlainScalar(text ?? "");
            default:
                return JsonValue.Create(((YamlScalarNode)node).Value ?? "");
        }
    }

    static JsonNode? PlainScalar(string text)
    {
        if (text is "" or "~" || text.Equals("null", StringComparison.OrdinalIgnoreCase))
            return null;
        if (text.Equals("true", StringComparison.OrdinalIgnoreCase))
            return JsonValue.Create(true);
        if (text.Equals("false", StringComparison.OrdinalIgnoreCase))
            return JsonValue.Create(false);
        return JsonNumber().IsMatch(text) ? JsonNode.Parse(text) : JsonValue.Create(text);
    }

    static string ToJson(JsonNode? data) =>
        data?.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        }) ?? "null";

    static string ToYaml(JsonNode? data)
    {
        var writer = new StringWriter { NewLine = "\n" };
        new YamlStream(new YamlDocument(ToYamlNode(data))).Save(writer, assignAnchors: false);
        // Save selalu menutup dokumen dengan "...", yang tidak perlu untuk satu dokumen.
        return Regex.Replace(writer.ToString(), @"\.\.\.\s*$", "");
    }

    static YamlNode ToYamlNode(JsonNode? node)
    {
        switch (node)
        {
            case null:
                return new YamlScalarNode("null") { Style = ScalarStyle.Plain };
            case JsonObject map:
                var mapping = new YamlMappingNode();
                foreach (var (key, value) in map)
                    mapping.Add(StringNode(key), ToYamlNode(value));
                return mapping;
            case JsonArray array:
                var sequence = new YamlSequenceNode();
                foreach (var item in array)
                    sequence.Add(ToYamlNode(item));
                return sequence;
            default:
                return node.GetValueKind() == JsonValueKind.String
                    ? StringNode(node.GetValue<string>())
                    : new YamlScalarNode(node.ToJsonString()) { Style = ScalarStyle.Plain };
        }
    }

    // String yang kalau ditulis polos terbaca sebagai angka, boolean atau null harus dikutip.
    static YamlScalarNode StringNode(string text)
    {
        bool ambiguous = text.Length == 0 || YamlWords.Contains(text, StringComparer.OrdinalIgnoreCase)
            || double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out _)
            || text.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
        return ambiguous ? new YamlScalarNode(text) { Style = ScalarStyle.DoubleQuoted } : new YamlScalarNode(text);
    }

    static string ToCsv(JsonNode? data)
    {
        // Objek tunggal = satu baris; daftar nilai biasa = satu kolom "value".
        var rows = (data as JsonArray ?? [data?.DeepClone()]).Select(row => row as JsonObject ?? new JsonObject
        {
            ["value"] = row?.DeepClone(),
        }).ToList();
        var columns = rows.SelectMany(row => row.Select(cell => cell.Key)).Distinct().ToList();

        var csv = new StringBuilder();
        csv.AppendJoin(',', columns.Select(CsvField)).Append("\r\n");
        foreach (var row in rows)
            csv.AppendJoin(',', columns.Select(column => CsvField(CellText(row[column])))).Append("\r\n");
        return csv.ToString();
    }

    // Isi bertingkat tidak punya bentuk kolom; disimpan sebagai teks JSON di satu sel.
    static string CellText(JsonNode? cell) => cell switch
    {
        null => "",
        JsonValue when cell.GetValueKind() == JsonValueKind.String => cell.GetValue<string>(),
        _ => cell.ToJsonString(new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }),
    };

    static string CsvField(string text) =>
        text.AsSpan().IndexOfAny(",\"\r\n") >= 0 ? $"\"{text.Replace("\"", "\"\"")}\"" : text;

    static JsonArray ReadCsv(string path)
    {
        var (separator, utf8) = Converter.SniffCsv(path);
        var records = ParseCsv(File.ReadAllText(path, utf8 ? Encoding.UTF8 : Encoding.Latin1), separator);
        var result = new JsonArray();
        if (records.Count == 0)
            return result;

        var header = UniqueNames(records[0]);
        foreach (var record in records.Skip(1))
        {
            var row = new JsonObject();
            for (int i = 0; i < record.Count; i++)
                row[i < header.Count ? header[i] : $"column {i + 1}"] = CsvCell(record[i]);
            result.Add(row);
        }
        return result;
    }

    // Kolom kosong atau bernama sama tidak bisa jadi kunci JSON apa adanya.
    static List<string> UniqueNames(List<string> header)
    {
        var used = new HashSet<string>();
        var names = new List<string>();
        for (int i = 0; i < header.Count; i++)
        {
            string name = header[i].Trim().Length > 0 ? header[i].Trim() : $"column {i + 1}";
            string candidate = name;
            for (int n = 2; !used.Add(candidate); n++)
                candidate = $"{name} ({n})";
            names.Add(candidate);
        }
        return names;
    }

    // Hanya angka yang bentuknya persis JSON: "007" dan kode pos tetap teks supaya nol di depannya tidak hilang.
    static JsonNode? CsvCell(string text) =>
        text.Length == 0 ? null : JsonNumber().IsMatch(text) ? JsonNode.Parse(text) : JsonValue.Create(text);

    /// <summary>Isi CSV (RFC 4180) menjadi baris dan kolom; baris kosong dilewati.</summary>
    static List<List<string>> ParseCsv(string text, char separator)
    {
        var records = new List<List<string>>();
        var record = new List<string>();
        var field = new StringBuilder();
        bool quoted = false;

        void EndRecord()
        {
            record.Add(field.ToString());
            field.Clear();
            if (record.Count > 1 || record[0].Length > 0)
                records.Add(record);
            record = [];
        }

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (quoted)
            {
                if (c != '"')
                    field.Append(c);
                else if (i + 1 < text.Length && text[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else
                    quoted = false;
            }
            else if (c == '"' && field.Length == 0)
                quoted = true;
            else if (c == separator)
            {
                record.Add(field.ToString());
                field.Clear();
            }
            else if (c == '\n')
                EndRecord();
            else if (c != '\r')
                field.Append(c);
        }
        if (field.Length > 0 || record.Count > 0)
            EndRecord();
        return records;
    }

    [GeneratedRegex(@"^-?(0|[1-9]\d*)(\.\d+)?([eE][-+]?\d+)?$")]
    private static partial Regex JsonNumber();
}
