using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using FishConsole = Fish.Console.FishConsole;

namespace FishTools.App;

internal sealed class FormatConverter : ITool
{
    public string Id => "format-converter";
    public string Name => "Data Format Converter";
    public string Category => ToolCategories.DataText;
    public string Description => "Transform data between various formats: JSON, XML, CSV, Hex, Base64, and URI.";

    public Task RunAsync(ToolContext context)
    {
        while (true)
        {
            ConsoleUi.ResetScreen(Name);
            var choice = ConsoleUi.ShowMenu("Choose a conversion", ["JSON to XML", "XML to JSON", "String to Base64", "Base64 to String", "String to Hex", "Hex to String", "CSV to JSON List", "URI Percent Transform", "Back"]);
            if (choice == 8)
                return Task.CompletedTask;

            try
            {
                var input = ReadInput(
                    choice switch
                    {
                        0 => "JSON Content or File",
                        1 => "XML Content or File",
                        6 => "CSV Content or File",
                        _ => "Plain Text Input",
                    }
                );

                var output = choice switch
                {
                    0 => JsonToXml(input),
                    1 => XmlToJson(input),
                    2 => Convert.ToBase64String(Encoding.UTF8.GetBytes(input)),
                    3 => Encoding.UTF8.GetString(Convert.FromBase64String(input)),
                    4 => Convert.ToHexString(Encoding.UTF8.GetBytes(input)),
                    5 => Encoding.UTF8.GetString(Convert.FromHexString(input)),
                    6 => CsvToJson(input),
                    7 => UrlTransform(input),
                    _ => string.Empty,
                };

                ConsoleUi.ResetScreen(Name);
                ConsoleUi.Success("Conversion Result:");
                FishConsole.WriteLine();
                FishConsole.WriteLine(output);

                if (ConsoleUi.Confirm("Export this result to a new file?"))
                {
                    var path = ConsoleUi.PromptRequired("Target file path");
                    File.WriteAllText(Helpers.NormalizePath(path), output);
                    ConsoleUi.Success("File saved.");
                }
            }
            catch (Exception ex)
            {
                ConsoleUi.Error(ex.Message);
            }

            ConsoleUi.Pause();
        }
    }

    private static string ReadInput(string prompt)
    {
        var val = ConsoleUi.PromptRequired(prompt);
        var norm = Helpers.NormalizePath(val);
        return File.Exists(norm) ? File.ReadAllText(norm) : val;
    }

    private static string UrlTransform(string i) => ConsoleUi.Confirm("Encode string instead of decode?") ? Uri.EscapeDataString(i) : WebUtility.UrlDecode(i);

    private static string JsonToXml(string j)
    {
        var node = JsonNode.Parse(j) ?? throw new Exception("Failed to parse JSON.");
        var doc = new XmlDocument();
        var root = doc.CreateElement("root");
        doc.AppendChild(root);
        WriteJsonNode(doc, root, node);
        return doc.OuterXml;
    }

    private static string XmlToJson(string x)
    {
        var doc = new XmlDocument();
        doc.LoadXml(x);
        return ReadXmlNode(doc.DocumentElement!).ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    private static string CsvToJson(string c)
    {
        var lines = c.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
            throw new Exception("Malformed CSV input data.");
        var headers = Helpers.ParseCsvLine(lines[0]);
        var rows = new JsonArray();
        foreach (var line in lines.Skip(1))
        {
            var vals = Helpers.ParseCsvLine(line);
            var obj = new JsonObject();
            for (int i = 0; i < headers.Length; i++)
                obj[headers[i]] = i < vals.Length ? vals[i] : "";
            rows.Add(obj);
        }
        return rows.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    private static void WriteJsonNode(XmlDocument doc, XmlElement el, JsonNode node)
    {
        if (node is JsonObject obj)
        {
            foreach (var p in obj)
            {
                el.AppendChild(doc.CreateElement(p.Key)).Pipe(ch => WriteJsonNode(doc, (XmlElement)ch, p.Value!));
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                el.AppendChild(doc.CreateElement("item")).Pipe(ch => WriteJsonNode(doc, (XmlElement)ch, item!));
            }
        }
        else
        {
            el.InnerText = node.ToJsonString().Trim('"');
        }
    }

    private static JsonNode ReadXmlNode(XmlNode node)
    {
        if (node.ChildNodes.Count == 1 && node.FirstChild is XmlText)
            return JsonValue.Create(node.InnerText)!;
        var groups = node.ChildNodes.OfType<XmlNode>().Where(c => c.NodeType == XmlNodeType.Element).GroupBy(c => c.Name);
        var obj = new JsonObject();
        foreach (var g in groups)
        {
            if (g.Count() == 1)
                obj[g.Key] = ReadXmlNode(g.First());
            else
                obj[g.Key] = new JsonArray(g.Select(ReadXmlNode).ToArray());
        }
        return obj;
    }
}

internal static class Extensions
{
    public static void Pipe<T>(this T obj, Action<T> action) => action(obj);
}
