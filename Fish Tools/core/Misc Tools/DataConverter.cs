using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml;
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.MiscTools
{
    internal class DataConverter : ITool
    {
        public string Name => "Data Converter";
        public string Category => "Misc Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Convert between different data formats (JSON, XML, CSV, Base64, etc.)";

        public void Main(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();
            
            Logger.Info("Data Converter - Multi-format Data Conversion Tool");
            Logger.Info("Convert between various data formats and encodings.");
            
            while (true)
            {
                Console.WriteLine();
                Logger.Info("Conversion Options:");
                Logger.WriteBarrierLine("1", "JSON to XML");
                Logger.WriteBarrierLine("2", "XML to JSON");
                Logger.WriteBarrierLine("3", "Text to Base64");
                Logger.WriteBarrierLine("4", "Base64 to Text");
                Logger.WriteBarrierLine("5", "Text to Hex");
                Logger.WriteBarrierLine("6", "Hex to Text");
                Logger.WriteBarrierLine("7", "CSV to JSON");
                Logger.WriteBarrierLine("8", "URL Encode/Decode");
                Logger.WriteBarrierLine("0", "Back to Menu");
                
                Console.Write("Select option: ");
                var input = Console.ReadKey();
                Console.WriteLine();
                
                switch (input.Key)
                {
                    case ConsoleKey.D1:
                        JsonToXml(Logger);
                        break;
                    case ConsoleKey.D2:
                        XmlToJson(Logger);
                        break;
                    case ConsoleKey.D3:
                        TextToBase64(Logger);
                        break;
                    case ConsoleKey.D4:
                        Base64ToText(Logger);
                        break;
                    case ConsoleKey.D5:
                        TextToHex(Logger);
                        break;
                    case ConsoleKey.D6:
                        HexToText(Logger);
                        break;
                    case ConsoleKey.D7:
                        CsvToJson(Logger);
                        break;
                    case ConsoleKey.D8:
                        UrlEncodeDecode(Logger);
                        break;
                    case ConsoleKey.D0:
                        return;
                    default:
                        Logger.Error("Invalid option selected.");
                        break;
                }
            }
        }

        private void JsonToXml(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== JSON to XML Conversion ===");
            
            Console.Write("Enter JSON text (or press Enter to load from file): ");
            string jsonInput = Console.ReadLine();
            
            string json;
            if (string.IsNullOrWhiteSpace(jsonInput))
            {
                Console.Write("Enter JSON file path: ");
                string filePath = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    Logger.Error("Invalid file path.");
                    return;
                }
                
                json = File.ReadAllText(filePath);
            }
            else
            {
                json = jsonInput;
            }
            
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var xmlDoc = new XmlDocument();
                    var root = xmlDoc.CreateElement("root");
                    xmlDoc.AppendChild(root);
                    
                    ConvertJsonToXml(doc.RootElement, root, xmlDoc);
                    
                    string xml = xmlDoc.OuterXml;
                    
                    Console.WriteLine();
                    Logger.Success("Conversion completed!");
                    Console.WriteLine();
                    Logger.Write("XML Output:");
                    Console.WriteLine();
                    Logger.Write(xml);
                    Console.WriteLine();
                    
                    Console.WriteLine();
                    Logger.Info("Save to file? (y/n): ");
                    var saveChoice = Console.ReadKey();
                    Console.WriteLine();
                    
                    if (saveChoice.Key == ConsoleKey.Y)
                    {
                        Console.Write("Enter output file path: ");
                        string outputPath = Console.ReadLine();
                        
                        if (!string.IsNullOrWhiteSpace(outputPath))
                        {
                            File.WriteAllText(outputPath, xml);
                            Logger.Success($"XML saved to: {outputPath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Conversion failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void XmlToJson(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== XML to JSON Conversion ===");
            
            Console.Write("Enter XML text (or press Enter to load from file): ");
            string xmlInput = Console.ReadLine();
            
            string xml;
            if (string.IsNullOrWhiteSpace(xmlInput))
            {
                Console.Write("Enter XML file path: ");
                string filePath = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    Logger.Error("Invalid file path.");
                    return;
                }
                
                xml = File.ReadAllText(filePath);
            }
            else
            {
                xml = xmlInput;
            }
            
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);
                
                var json = ConvertXmlToJson(xmlDoc.DocumentElement);
                string jsonString = JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true });
                
                Console.WriteLine();
                Logger.Success("Conversion completed!");
                Console.WriteLine();
                Logger.Write("JSON Output:");
                Console.WriteLine();
                Logger.Write(jsonString);
                Console.WriteLine();
                
                Console.WriteLine();
                Logger.Info("Save to file? (y/n): ");
                var saveChoice = Console.ReadKey();
                Console.WriteLine();
                
                if (saveChoice.Key == ConsoleKey.Y)
                {
                    Console.Write("Enter output file path: ");
                    string outputPath = Console.ReadLine();
                    
                    if (!string.IsNullOrWhiteSpace(outputPath))
                    {
                        File.WriteAllText(outputPath, jsonString);
                        Logger.Success($"JSON saved to: {outputPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Conversion failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void TextToBase64(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== Text to Base64 Conversion ===");
            
            Console.Write("Enter text to convert: ");
            string text = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(text))
            {
                Logger.Error("No text provided.");
                return;
            }
            
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                string base64 = Convert.ToBase64String(bytes);
                
                Console.WriteLine();
                Logger.Success("Conversion completed!");
                Console.WriteLine();
                Logger.Write("Base64 Output:");
                Console.WriteLine();
                Logger.Write(base64);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Logger.Error($"Conversion failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void Base64ToText(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== Base64 to Text Conversion ===");
            
            Console.Write("Enter Base64 string: ");
            string base64 = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(base64))
            {
                Logger.Error("No Base64 string provided.");
                return;
            }
            
            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                string text = Encoding.UTF8.GetString(bytes);
                
                Console.WriteLine();
                Logger.Success("Conversion completed!");
                Console.WriteLine();
                Logger.Write("Text Output:");
                Console.WriteLine();
                Logger.Write(text);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Logger.Error($"Conversion failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void TextToHex(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== Text to Hex Conversion ===");
            
            Console.Write("Enter text to convert: ");
            string text = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(text))
            {
                Logger.Error("No text provided.");
                return;
            }
            
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                string hex = BitConverter.ToString(bytes).Replace("-", "");
                
                Console.WriteLine();
                Logger.Success("Conversion completed!");
                Console.WriteLine();
                Logger.Write("Hex Output:");
                Console.WriteLine();
                Logger.Write(hex);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Logger.Error($"Conversion failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void HexToText(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== Hex to Text Conversion ===");
            
            Console.Write("Enter hex string: ");
            string hex = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(hex))
            {
                Logger.Error("No hex string provided.");
                return;
            }
            
            try
            {
                hex = hex.Replace(" ", "").Replace("-", "");
                
                byte[] bytes = new byte[hex.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                
                string text = Encoding.UTF8.GetString(bytes);
                
                Console.WriteLine();
                Logger.Success("Conversion completed!");
                Console.WriteLine();
                Logger.Write("Text Output:");
                Console.WriteLine();
                Logger.Write(text);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Logger.Error($"Conversion failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void CsvToJson(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== CSV to JSON Conversion ===");
            
            Console.Write("Enter CSV file path: ");
            string filePath = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                Logger.Error("Invalid file path.");
                return;
            }
            
            try
            {
                var lines = File.ReadAllLines(filePath);
                if (lines.Length == 0)
                {
                    Logger.Error("CSV file is empty.");
                    return;
                }
                
                var headers = lines[0].Split(',');
                var jsonArray = new List<Dictionary<string, string>>();
                
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');
                    var row = new Dictionary<string, string>();
                    
                    for (int j = 0; j < headers.Length && j < values.Length; j++)
                    {
                        row[headers[j].Trim()] = values[j].Trim();
                    }
                    
                    jsonArray.Add(row);
                }
                
                string json = JsonSerializer.Serialize(jsonArray, new JsonSerializerOptions { WriteIndented = true });
                
                Console.WriteLine();
                Logger.Success("Conversion completed!");
                Console.WriteLine();
                Logger.Write("JSON Output:");
                Console.WriteLine();
                Logger.Write(json);
                Console.WriteLine();
                
                Console.WriteLine();
                Logger.Info("Save to file? (y/n): ");
                var saveChoice = Console.ReadKey();
                Console.WriteLine();
                
                if (saveChoice.Key == ConsoleKey.Y)
                {
                    Console.Write("Enter output file path: ");
                    string outputPath = Console.ReadLine();
                    
                    if (!string.IsNullOrWhiteSpace(outputPath))
                    {
                        File.WriteAllText(outputPath, json);
                        Logger.Success($"JSON saved to: {outputPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Conversion failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void UrlEncodeDecode(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== URL Encode/Decode ===");
            
            Logger.Info("Options:");
            Logger.WriteBarrierLine("1", "URL Encode");
            Logger.WriteBarrierLine("2", "URL Decode");
            
            Console.Write("Select option: ");
            var choice = Console.ReadKey();
            Console.WriteLine();
            
            Console.Write("Enter text: ");
            string text = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(text))
            {
                Logger.Error("No text provided.");
                return;
            }
            
            try
            {
                string result;
                
                switch (choice.Key)
                {
                    case ConsoleKey.D1:
                        result = Uri.EscapeDataString(text);
                        Logger.Write("URL Encoded:");
                        break;
                    case ConsoleKey.D2:
                        result = Uri.UnescapeDataString(text);
                        Logger.Write("URL Decoded:");
                        break;
                    default:
                        Logger.Error("Invalid option.");
                        return;
                }
                
                Console.WriteLine();
                Logger.Success("Conversion completed!");
                Console.WriteLine();
                Logger.Write(result);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Logger.Error($"Conversion failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void ConvertJsonToXml(JsonElement element, XmlElement parent, XmlDocument doc)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        var child = doc.CreateElement(property.Name);
                        parent.AppendChild(child);
                        ConvertJsonToXml(property.Value, child, doc);
                    }
                    break;
                    
                case JsonValueKind.Array:
                    for (int i = 0; i < element.GetArrayLength(); i++)
                    {
                        var child = doc.CreateElement("item");
                        parent.AppendChild(child);
                        ConvertJsonToXml(element[i], child, doc);
                    }
                    break;
                    
                case JsonValueKind.String:
                    parent.InnerText = element.GetString();
                    break;
                    
                case JsonValueKind.Number:
                    parent.InnerText = element.GetRawText();
                    break;
                    
                case JsonValueKind.True:
                    parent.InnerText = "true";
                    break;
                    
                case JsonValueKind.False:
                    parent.InnerText = "false";
                    break;
                    
                case JsonValueKind.Null:
                    parent.InnerText = "";
                    break;
            }
        }

        private object ConvertXmlToJson(XmlElement element)
        {
            if (element.HasChildNodes)
            {
                var children = element.ChildNodes;
                
                if (children.Count > 1)
                {
                    var firstChildName = children[0].Name;
                    bool isArray = true;
                    
                    for (int i = 1; i < children.Count; i++)
                    {
                        if (children[i].Name != firstChildName)
                        {
                            isArray = false;
                            break;
                        }
                    }
                    
                    if (isArray)
                    {
                        var array = new List<object>();
                        foreach (XmlNode child in children)
                        {
                            if (child is XmlElement childElement)
                            {
                                array.Add(ConvertXmlToJson(childElement));
                            }
                        }
                        return array;
                    }
                }
                
                var obj = new Dictionary<string, object>();
                foreach (XmlNode child in children)
                {
                    if (child is XmlElement childElement)
                    {
                        obj[childElement.Name] = ConvertXmlToJson(childElement);
                    }
                }
                return obj;
            }
            else
            {
                return element.InnerText;
            }
        }
    }
} 