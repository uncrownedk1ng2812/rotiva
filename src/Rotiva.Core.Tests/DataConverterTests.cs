using System.Text;
using System.Text.Json.Nodes;
using Rotiva.Conversion;

namespace Rotiva.Core.Tests;

[Collection("Language")]
public sealed class DataConverterTests : IDisposable
{
    readonly string _dir = Directory.CreateTempSubdirectory("rotiva-test-").FullName;

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    string Convert(string name, string content, string extension)
    {
        string source = Path.Combine(_dir, name);
        File.WriteAllText(source, content, new UTF8Encoding(false));
        string output = Path.Combine(_dir, "out" + extension);
        DataConverter.Convert(source, extension, output);
        return File.ReadAllText(output);
    }

    [Fact]
    public void YamlBecomesTypedJson()
    {
        var json = JsonNode.Parse(Convert("a.yaml", "name: Rotiva\ncount: 3\nratio: 1.5\non: true\nnothing: ~\nquoted: \"42\"\nzip: 007\nlist: [a, 2]\n", ".json"))!;

        Assert.Equal("Rotiva", json["name"]!.GetValue<string>());
        Assert.Equal(3, json["count"]!.GetValue<int>());
        Assert.Equal(1.5, json["ratio"]!.GetValue<double>());
        Assert.True(json["on"]!.GetValue<bool>());
        Assert.Null(json["nothing"]);
        Assert.Equal("42", json["quoted"]!.GetValue<string>());
        Assert.Equal("007", json["zip"]!.GetValue<string>());
        Assert.Equal(2, json["list"]![1]!.GetValue<int>());
    }

    [Fact]
    public void JsonToYamlAndBackKeepsTypes()
    {
        const string original = """{"s":"true","n":"123","b":false,"x":1.25,"e":"","nil":null,"nested":{"a":[1,"two"]},"u":"héllo: world"}""";

        string yaml = Convert("a.json", original, ".yaml");
        var back = JsonNode.Parse(Convert("b.yaml", yaml, ".json"))!;

        Assert.DoesNotContain("...", yaml);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(original), back), yaml);
    }

    [Fact]
    public void JsonArrayBecomesCsvWithEveryColumn()
    {
        string csv = Convert("a.json", """[{"a":1,"b":"x,y"},{"a":2,"c":{"d":true}}]""", ".csv");

        Assert.Equal("a,b,c\r\n1,\"x,y\",\r\n2,,\"{\"\"d\"\":true}\"\r\n", csv);
    }

    [Fact]
    public void ScalarListBecomesASingleColumn()
    {
        Assert.Equal("value\r\n1\r\ntwo\r\n", Convert("a.json", """[1,"two"]""", ".csv"));
    }

    [Fact]
    public void SemicolonCsvBecomesJsonAndKeepsLeadingZeros()
    {
        var json = JsonNode.Parse(Convert("a.csv", "name;zip;qty;;name\n\"Bu, Ani\";01234;5;x;dup\n\nB;;7\n", ".json"))!.AsArray();

        Assert.Equal(2, json.Count);
        Assert.Equal("Bu, Ani", json[0]!["name"]!.GetValue<string>());
        Assert.Equal("01234", json[0]!["zip"]!.GetValue<string>());
        Assert.Equal(5, json[0]!["qty"]!.GetValue<int>());
        Assert.Equal("x", json[0]!["column 4"]!.GetValue<string>());
        Assert.Equal("dup", json[0]!["name (2)"]!.GetValue<string>());
        Assert.Null(json[1]!["zip"]);
    }

    [Theory]
    [InlineData("bad.json", "{ nope", ".yaml")]
    [InlineData("bad.yaml", "a: [1", ".json")]
    public void InvalidInputFailsWithAClearMessage(string name, string content, string extension)
    {
        Assert.Throws<ConversionException>(() => Convert(name, content, extension));
    }

    [Fact]
    public void YamlAliasBombIsRejected()
    {
        var yaml = new StringBuilder("a0: &a0 [x,x,x,x,x,x,x,x,x,x]\n");
        for (int i = 1; i < 12; i++)
            yaml.Append($"a{i}: &a{i} [{string.Join(',', Enumerable.Repeat($"*a{i - 1}", 10))}]\n");

        Assert.Throws<ConversionException>(() => Convert("bomb.yaml", yaml.ToString(), ".json"));
    }
}
