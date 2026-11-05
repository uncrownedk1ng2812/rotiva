using System.Text.RegularExpressions;

namespace Rotiva.Updates;

public enum NoteBlockKind { Heading, Bullet, Paragraph }

/// <summary>Satu blok catatan rilis: judul, butir daftar, atau paragraf (masih berisi Markdown inline).</summary>
public sealed record NoteBlock(NoteBlockKind Kind, string Text);

[Flags]
public enum NoteStyle { None = 0, Bold = 1, Italic = 2, Code = 4, Link = 8 }

/// <summary>Potongan teks dengan gayanya; <see cref="Url"/> hanya untuk tautan.</summary>
public sealed record NoteSpan(string Text, NoteStyle Style, string? Url = null);

/// <summary>
/// Markdown kecil untuk catatan rilis di jendela update: cukup judul, daftar, paragraf, tebal,
/// miring, kode, dan tautan (format yang dipakai di rilis Rotiva). Tabel dan gambar dilewati.
/// </summary>
public static partial class ReleaseNotes
{
    // Bagian yang hanya relevan di halaman GitHub, bukan di aplikasi yang sudah terpasang.
    static readonly string[] SkippedSections = ["download", "install", "checksum", "unduh", "instal"];

    /// <summary>Paragraf pembuka + semua bagian selain Downloads/Install.</summary>
    public static string WhatsNew(string markdown)
    {
        var kept = new List<string>();
        bool skipping = false;
        foreach (var line in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            if (Heading().Match(line) is { Success: true } heading)
            {
                string title = heading.Groups[2].Value.ToLowerInvariant();
                skipping = SkippedSections.Any(title.Contains);
                // Judul "What's new" tidak perlu: jendela update sudah punya label itu sendiri.
                if (title.TrimEnd(':') is "what's new" or "what’s new" or "yang baru")
                    continue;
            }
            if (!skipping)
                kept.Add(line);
        }
        return string.Join('\n', kept).Trim();
    }

    public static List<NoteBlock> Blocks(string markdown)
    {
        var blocks = new List<NoteBlock>();
        var paragraph = new List<string>();

        void Flush()
        {
            if (paragraph.Count > 0)
                blocks.Add(new NoteBlock(NoteBlockKind.Paragraph, string.Join(' ', paragraph)));
            paragraph.Clear();
        }

        foreach (var raw in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('|') || line.StartsWith("![") || line is "---" or "***")
            {
                Flush();
                continue;
            }
            if (Heading().Match(line) is { Success: true } heading)
            {
                Flush();
                blocks.Add(new NoteBlock(NoteBlockKind.Heading, heading.Groups[2].Value.Trim()));
            }
            else if (Bullet().Match(line) is { Success: true } bullet)
            {
                Flush();
                blocks.Add(new NoteBlock(NoteBlockKind.Bullet, bullet.Groups[1].Value.Trim()));
            }
            else if (paragraph.Count == 0 && blocks.Count > 0 && blocks[^1].Kind == NoteBlockKind.Bullet &&
                     raw.StartsWith("  "))
            {
                // Lanjutan butir daftar di baris berikutnya (diindentasi).
                blocks[^1] = blocks[^1] with { Text = blocks[^1].Text + " " + line };
            }
            else
            {
                paragraph.Add(line);
            }
        }
        Flush();
        return blocks;
    }

    /// <summary>Pecah Markdown inline (<c>**tebal**</c>, <c>*miring*</c>, <c>`kode`</c>, <c>[teks](url)</c>).</summary>
    public static List<NoteSpan> Spans(string text)
    {
        var spans = new List<NoteSpan>();
        int at = 0;
        foreach (Match m in Inline().Matches(text))
        {
            if (m.Index > at)
                spans.Add(new NoteSpan(text[at..m.Index], NoteStyle.None));
            if (m.Groups["bold"].Success)
            {
                // Tebal boleh berisi miring/kode/tautan: gabungkan gayanya.
                foreach (var inner in Spans(m.Groups["bold"].Value))
                    spans.Add(inner with { Style = inner.Style | NoteStyle.Bold });
            }
            else if (m.Groups["code"].Success)
                spans.Add(new NoteSpan(m.Groups["code"].Value, NoteStyle.Code));
            else if (m.Groups["link"].Success)
            {
                string url = m.Groups["url"].Value;
                bool web = url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
                spans.Add(new NoteSpan(m.Groups["link"].Value, web ? NoteStyle.Link : NoteStyle.None, web ? url : null));
            }
            else
            {
                foreach (var inner in Spans(m.Groups["italic"].Value))
                    spans.Add(inner with { Style = inner.Style | NoteStyle.Italic });
            }
            at = m.Index + m.Length;
        }
        if (at < text.Length)
            spans.Add(new NoteSpan(text[at..], NoteStyle.None));
        return spans;
    }

    [GeneratedRegex(@"^\s*(#{1,6})\s+(.+?)\s*#*\s*$")]
    private static partial Regex Heading();

    [GeneratedRegex(@"^(?:[-*+]|\d+\.)\s+(.*)$")]
    private static partial Regex Bullet();

    [GeneratedRegex(@"\*\*(?<bold>.+?)\*\*|`(?<code>[^`]+)`|\[(?<link>[^\]]+)\]\((?<url>[^)\s]+)\)|(?<![\w*])[*_](?<italic>[^*_]+?)[*_](?![\w*])")]
    private static partial Regex Inline();
}
