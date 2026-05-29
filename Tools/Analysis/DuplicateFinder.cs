using System.Text;
using FishConsole = Fish.Console.FishConsole;

namespace FishTools.App;

internal sealed class DuplicateFinder : ITool
{
    public string Id => "duplicate-finder";
    public string Name => "Duplicate Finder";
    public string Category => ToolCategories.Analysis;
    public string Description => "Search for duplicate files by comparing size and SHA-256 hashes.";

    public async Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        var directory = ConsoleUi.ExistingDirectoryPrompt("Directory to scan");
        var recursive = ConsoleUi.Confirm("Include subdirectories?");

        ConsoleUi.ResetScreen(Name);
        ConsoleUi.Info("Scanning files...");

        var files = Helpers.SafeEnumerateFiles(directory, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
        var candidateGroups = files.Select(path => new FileInfo(path)).GroupBy(info => info.Length).Where(group => group.Count() > 1).ToList();

        var duplicates = new List<List<string>>();
        var groupCounter = 0;

        foreach (var group in candidateGroups)
        {
            groupCounter++;
            ConsoleUi.Info($"Hashing size group {groupCounter}/{candidateGroups.Count} ({Helpers.FormatBytes(group.Key)})");

            var hashMap = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (var file in group)
            {
                try
                {
                    var hash = await Helpers.ComputeSha256Async(file.FullName);
                    if (!hashMap.TryGetValue(hash, out var entries))
                    {
                        entries = [];
                        hashMap[hash] = entries;
                    }

                    entries.Add(file.FullName);
                }
                catch { }
            }

            duplicates.AddRange(hashMap.Values.Where(items => items.Count > 1));
        }

        ConsoleUi.ResetScreen(Name);
        if (duplicates.Count == 0)
        {
            ConsoleUi.Success("No duplicates found.");
            ConsoleUi.Pause();
            return;
        }

        var totalDuplicateFiles = duplicates.Sum(group => group.Count - 1);
        var wastedBytes = duplicates.Sum(group => new FileInfo(group[0]).Length * (group.Count - 1));

        ConsoleUi.Success($"Duplicate groups: {duplicates.Count}");
        ConsoleUi.Info($"Redundant files: {totalDuplicateFiles}");
        ConsoleUi.Info($"Potential reclaimable space: {Helpers.FormatBytes(wastedBytes)}");
        FishConsole.WriteLine();

        foreach (var group in duplicates.Take(8))
        {
            ConsoleUi.Info($"Group ({Helpers.FormatBytes(new FileInfo(group[0]).Length)}):");
            foreach (var file in group)
            {
                ConsoleUi.Info($"- {file}");
            }

            FishConsole.WriteLine();
        }

        var action = ConsoleUi.ShowMenu("Choose an action", ["Save report only", "Move duplicates aside", "Delete duplicates", "Back"]);
        switch (action)
        {
            case 0:
                SaveDuplicateReport(context, duplicates);
                break;
            case 1:
                MoveDuplicates(context, duplicates);
                break;
            case 2:
                DeleteDuplicates(duplicates);
                break;
        }

        ConsoleUi.Pause();
    }

    private static void SaveDuplicateReport(ToolContext context, IEnumerable<List<string>> duplicates)
    {
        var builder = new StringBuilder();
        var index = 1;
        foreach (var group in duplicates)
        {
            builder.AppendLine($"Group {index++}");
            foreach (var file in group)
            {
                builder.AppendLine(file);
            }

            builder.AppendLine();
        }

        var reportPath = context.CreateReportPath("duplicate_report");
        File.WriteAllText(reportPath, builder.ToString());
        ConsoleUi.Success($"Report saved to {reportPath}");
    }

    private static void MoveDuplicates(ToolContext context, IEnumerable<List<string>> duplicates)
    {
        var stagingRoot = Directory.CreateDirectory(context.CreateReportPath("duplicates", string.Empty)).FullName;
        var moved = 0;

        foreach (var group in duplicates)
        {
            foreach (var file in group.Skip(1))
            {
                try
                {
                    var destination = Helpers.EnsureUniquePath(Path.Combine(stagingRoot, Path.GetFileName(file)));
                    File.Move(file, destination);
                    moved++;
                }
                catch (Exception ex)
                {
                    ConsoleUi.Warning($"{file}: {ex.Message}");
                }
            }
        }

        ConsoleUi.Success($"Moved {moved} duplicate files into {stagingRoot}");
    }

    private static void DeleteDuplicates(IEnumerable<List<string>> duplicates)
    {
        if (!ConsoleUi.Confirm("Delete all duplicates while keeping the first copy in each group?"))
        {
            return;
        }

        var deleted = 0;
        foreach (var group in duplicates)
        {
            foreach (var file in group.Skip(1))
            {
                try
                {
                    File.Delete(file);
                    deleted++;
                }
                catch (Exception ex)
                {
                    ConsoleUi.Warning($"{file}: {ex.Message}");
                }
            }
        }

        ConsoleUi.Success($"Deleted {deleted} duplicate files.");
    }
}
