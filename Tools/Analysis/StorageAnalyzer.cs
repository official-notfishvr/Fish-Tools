using System.Text;

namespace FishTools.App;

internal sealed class StorageAnalyzer : ITool
{
    public string Id => "storage-analyzer";
    public string Name => "Storage Analyzer";
    public string Category => ToolCategories.Analysis;
    public string Description => "Generate a detailed report with file counts, extensions, and largest files.";

    public Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        var root = ConsoleUi.ExistingDirectoryPrompt("Directory");
        var recursive = ConsoleUi.Confirm("Include subdirectories?");
        ConsoleUi.ResetScreen(Name);

        var files = Helpers
            .SafeEnumerateFiles(root, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .Select(path =>
            {
                try
                {
                    return new FileInfo(path);
                }
                catch
                {
                    return null;
                }
            })
            .Where(info => info is not null)
            .Cast<FileInfo>()
            .ToArray();

        if (files.Length == 0)
        {
            ConsoleUi.Warning("No files found.");
            ConsoleUi.Pause();
            return Task.CompletedTask;
        }

        var totalBytes = files.Sum(file => file.Length);
        var byExtension = files.GroupBy(file => string.IsNullOrEmpty(file.Extension) ? "[no extension]" : file.Extension.ToLowerInvariant()).OrderByDescending(group => group.Count()).ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase).ToArray();
        var largest = files.OrderByDescending(file => file.Length).Take(20).ToArray();

        ConsoleUi.ResetScreen(Name);
        ConsoleUi.Success($"Files found: {files.Length}");
        ConsoleUi.Info($"Total directory size: {Helpers.FormatBytes(totalBytes)}");
        ConsoleUi.Info($"Distinct extensions: {byExtension.Length}");
        Fish.Console.FishConsole.WriteLine();

        ConsoleUi.Section("Top Extensions");
        foreach (var group in byExtension.Take(12))
        {
            var bytes = group.Sum(file => file.Length);
            ConsoleUi.Info($"{group.Key, -16} {group.Count(), 6} files  {Helpers.FormatBytes(bytes), 10}");
        }

        Fish.Console.FishConsole.WriteLine();
        ConsoleUi.Section("Largest Files (Top 20)");
        foreach (var file in largest)
        {
            ConsoleUi.Info($"{Helpers.FormatBytes(file.Length), 10}  {Path.GetRelativePath(root, file.FullName)}");
        }

        if (ConsoleUi.Confirm("Save detailed report to Results?"))
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Root: {root}");
            builder.AppendLine($"Recursive: {recursive}");
            builder.AppendLine($"Files: {files.Length}");
            builder.AppendLine($"Total size: {Helpers.FormatBytes(totalBytes)}");
            builder.AppendLine();
            builder.AppendLine("Extensions Summary:");
            foreach (var group in byExtension)
            {
                builder.AppendLine($"{group.Key}: {group.Count()} files, {Helpers.FormatBytes(group.Sum(file => file.Length))}");
            }

            builder.AppendLine();
            builder.AppendLine("Largest files list:");
            foreach (var file in largest)
            {
                builder.AppendLine($"{Helpers.FormatBytes(file.Length)} {file.FullName}");
            }

            var path = context.CreateReportPath("storage_analyzer_report");
            File.WriteAllText(path, builder.ToString());
            ConsoleUi.Success($"Report saved to {path}");
        }

        ConsoleUi.Pause();
        return Task.CompletedTask;
    }
}
