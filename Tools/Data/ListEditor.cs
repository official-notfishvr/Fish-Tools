using System.Text.RegularExpressions;

namespace FishTools.App;

internal sealed class ListEditor : ITool
{
    public string Id => "list-editor";
    public string Name => "Text List Toolkit";
    public string Category => ToolCategories.DataText;
    public string Description => "Clean, filter, dedupe, and sort arbitrary line-based text files.";

    public Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        var inputPath = ConsoleUi.ExistingFilePrompt("Input text file");
        var workingLines = File.ReadAllLines(inputPath).ToList();

        while (true)
        {
            ConsoleUi.ResetScreen(Name);
            ConsoleUi.Info($"Working file: {Path.GetFileName(inputPath)}");
            ConsoleUi.Info($"Current lines in memory: {workingLines.Count}");

            var choice = ConsoleUi.ShowMenu("Choose an action", ["Trim whitespace & remove blank lines", "Remove duplicate lines", "Sort list (A-Z)", "Sort list (Z-A)", "Keep lines containing specific text", "Keep lines matching regex", "Preview first 50 lines", "Save as new file", "Back"]);

            switch (choice)
            {
                case 0:
                    workingLines = workingLines.Select(l => l.Trim()).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
                    break;
                case 1:
                    workingLines = workingLines.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case 2:
                    workingLines = workingLines.OrderBy(l => l, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case 3:
                    workingLines = workingLines.OrderByDescending(l => l, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case 4:
                {
                    var needle = ConsoleUi.PromptRequired("Filter text");
                    workingLines = workingLines.Where(l => l.Contains(needle, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                }
                case 5:
                {
                    var pattern = ConsoleUi.PromptRequired("Regex pattern");
                    var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                    workingLines = workingLines.Where(l => regex.IsMatch(l)).ToList();
                    break;
                }
                case 6:
                    ConsoleUi.ResetScreen("Preview");
                    foreach (var line in workingLines.Take(50))
                        ConsoleUi.Info(line);
                    ConsoleUi.Pause();
                    break;
                case 7:
                {
                    var outputPath = Helpers.EnsureUniquePath(Path.Combine(Path.GetDirectoryName(inputPath)!, $"processed_{Path.GetFileName(inputPath)}"));
                    File.WriteAllLines(outputPath, workingLines);
                    ConsoleUi.Success($"Saved result to {outputPath}");
                    ConsoleUi.Pause();
                    break;
                }
                default:
                    return Task.CompletedTask;
            }
        }
    }
}
