namespace FishTools.App;

internal sealed class FolderMerger : ITool
{
    public string Id => "folder-merger";
    public string Name => "Folder Merger";
    public string Category => ToolCategories.Operations;
    public string Description => "Merge top-level folders from a source directory into a destination if names match.";

    public Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        var destinationRoot = ConsoleUi.ExistingDirectoryPrompt("Destination root");
        var sourceRoot = ConsoleUi.ExistingDirectoryPrompt("Source root");
        var previewOnly = ConsoleUi.Confirm("Preview only?");
        ConsoleUi.ResetScreen(Name);

        var destinationFolders = Directory.EnumerateDirectories(destinationRoot, "*", SearchOption.TopDirectoryOnly).ToDictionary(path => Path.GetFileName(path), path => path, StringComparer.OrdinalIgnoreCase);
        var sourceFolders = Directory.EnumerateDirectories(sourceRoot, "*", SearchOption.TopDirectoryOnly).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();

        var matches = sourceFolders.Where(path => destinationFolders.ContainsKey(Path.GetFileName(path))).ToArray();

        if (matches.Length == 0)
        {
            ConsoleUi.Warning("No same-named top-level folders found.");
            ConsoleUi.Pause();
            return Task.CompletedTask;
        }

        ConsoleUi.Info($"Matching folders: {matches.Length}");
        foreach (var match in matches.Take(20))
        {
            var name = Path.GetFileName(match);
            ConsoleUi.Info($"{name}: {match} -> {destinationFolders[name]}");
        }

        if (previewOnly)
        {
            ConsoleUi.Pause();
            return Task.CompletedTask;
        }

        if (!ConsoleUi.Confirm("Move matching source folders into the destination root?"))
        {
            return Task.CompletedTask;
        }

        var moved = 0;
        foreach (var source in matches)
        {
            var name = Path.GetFileName(source);
            var destination = Helpers.EnsureUniquePath(Path.Combine(destinationRoot, name));
            try
            {
                Directory.Move(source, destination);
                moved++;
            }
            catch (Exception ex)
            {
                ConsoleUi.Warning($"{source}: {ex.Message}");
            }
        }

        ConsoleUi.Success($"Moved {moved} folders.");
        ConsoleUi.Pause();
        return Task.CompletedTask;
    }
}
