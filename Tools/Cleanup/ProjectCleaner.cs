namespace FishTools.App;

internal sealed class ProjectCleaner : ITool
{
    public string Id => "project-cleaner";
    public string Name => "Project Artifact Cleanup";
    public string Category => ToolCategories.Cleanup;
    public string Description => "Delete common build and cache folders (bin, obj, node_modules) from a directory tree.";

    public Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        var root = ConsoleUi.ExistingDirectoryPrompt("Root directory");
        var patternsRaw = ConsoleUi.Prompt("Folder names to delete (comma separated)", ".vs,bin,obj,node_modules,dist,build,packages");
        var patterns = patternsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var previewOnly = ConsoleUi.Confirm("Preview only?");
        ConsoleUi.ResetScreen(Name);

        var matches = Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories).Where(directory => patterns.Contains(Path.GetFileName(directory), StringComparer.OrdinalIgnoreCase)).OrderBy(directory => directory).ToArray();

        if (matches.Length == 0)
        {
            ConsoleUi.Warning("No matching folders found.");
            ConsoleUi.Pause();
            return Task.CompletedTask;
        }

        long bytes = matches.Sum(Helpers.GetDirectorySize);
        ConsoleUi.Info($"Matched folders: {matches.Length}");
        ConsoleUi.Info($"Approximate space: {Helpers.FormatBytes(bytes)}");
        foreach (var match in matches.Take(20))
        {
            ConsoleUi.Info($"- {match}");
        }

        if (previewOnly)
        {
            ConsoleUi.Pause();
            return Task.CompletedTask;
        }

        if (!ConsoleUi.Confirm("Delete these folders?"))
        {
            return Task.CompletedTask;
        }

        var deleted = 0;
        foreach (var match in matches)
        {
            try
            {
                Directory.Delete(match, true);
                deleted++;
            }
            catch (Exception ex)
            {
                ConsoleUi.Warning($"{match}: {ex.Message}");
            }
        }

        ConsoleUi.Success($"Deleted {deleted} folders.");
        ConsoleUi.Pause();
        return Task.CompletedTask;
    }
}
