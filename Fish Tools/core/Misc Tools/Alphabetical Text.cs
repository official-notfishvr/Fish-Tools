/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
namespace Fish_Tools.core.MiscTools
{
    // not done
    internal class Alphabetical_Text
    {
        public static void Main()
        {
            string filePath = @"C:\Users\notfishvr\Downloads\test.txt";

            try
            {
                var lines = File.ReadAllLines(filePath);
                var sortedLines = lines.OrderBy(line => line).ToArray();

                File.WriteAllLines(filePath, sortedLines);

                Console.WriteLine("The file has been sorted alphabetically.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.ReadKey();
            }
        }
        public static void Main2()
        {
            string filePath = @"C:\Users\notfishvr\Downloads\test.txt";

            try
            {
                string content = File.ReadAllText(filePath);
                var lines = content.Split('\n')
                                   .Select(line => line.Trim())
                                   .Where(line => !string.IsNullOrWhiteSpace(line))
                                   .ToList();

                var entries = new Dictionary<string, string>();

                foreach (var line in lines)
                {
                    var parts = line.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();
                        if (!entries.ContainsKey(key))
                        {
                            entries[key] = value;
                        }
                    }
                }

                var sortedEntries = entries
                    .OrderBy(entry => entry.Key)
                    .Select(entry => $"{entry.Key}: {entry.Value}")
                    .ToArray();


                File.WriteAllLines(filePath, sortedEntries);

                Console.WriteLine("The file has been sorted alphabetically and formatted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
