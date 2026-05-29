namespace FishTools.App;

internal sealed class ComboManager : ITool
{
    public string Id => "combo-manager";
    public string Name => "Combo List Manager";
    public string Category => ToolCategories.DataText;
    public string Description => "Process and format credential combo lists (email:password) for use in checkers.";

    public Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        var path = ConsoleUi.ExistingFilePrompt("Combo List File");
        var localCombos = File.ReadAllLines(path).ToList();

        while (true)
        {
            ConsoleUi.ResetScreen(Name);
            ConsoleUi.Info($"Loaded: {localCombos.Count} lines from {Path.GetFileName(path)}");
            var res = ConsoleUi.ShowMenu("Combo Operations", ["Trim Spaces", "Extract Credentials Only (before space)", "Add ':' after .com/TLDs", "Deduplicate", "Alpha Sort", "Preview Samples", "Export To New File", "Back"]);

            switch (res)
            {
                case 0:
                    localCombos = localCombos.Select(l => l.Trim()).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
                    ConsoleUi.Success("Trimmed.");
                    break;
                case 1:
                    localCombos = localCombos.Select(l => l.Split(' ')[0]).ToList();
                    ConsoleUi.Success("Extracted prefix.");
                    break;
                case 2:
                    localCombos = localCombos.Select(l => l.Replace(".com", ".com:")).ToList();
                    ConsoleUi.Success("Inserted delimiter.");
                    break;
                case 3:
                    localCombos = localCombos.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    ConsoleUi.Success("Distinct only.");
                    break;
                case 4:
                    localCombos = localCombos.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
                    ConsoleUi.Success("Sorted.");
                    break;
                case 5:
                    ConsoleUi.ResetScreen("Combo Samples");
                    foreach (var s in localCombos.Take(25))
                        ConsoleUi.Info(s);
                    ConsoleUi.Pause();
                    break;
                case 6:
                    var outPath = Helpers.EnsureUniquePath(Path.Combine(Path.GetDirectoryName(path)!, "edited_" + Path.GetFileName(path)));
                    File.WriteAllLines(outPath, localCombos);
                    ConsoleUi.Success($"Saved to {outPath}");
                    ConsoleUi.Pause();
                    break;
                default:
                    return Task.CompletedTask;
            }
            if (res < 5)
                ConsoleUi.Pause();
        }
    }
}
