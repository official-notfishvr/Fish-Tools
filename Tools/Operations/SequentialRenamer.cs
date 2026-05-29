namespace FishTools.App;

internal sealed class SequentialRenamer : ITool
{
    public string Id => "sequential-renamer";
    public string Name => "Sequential Renamer";
    public string Category => ToolCategories.Operations;
    public string Description => "Rename files to sequential numbers (1, 2, 3...) globally or per folder.";

    public Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        var mode = ConsoleUi.ShowMenu("Choose a mode", ["Global sequence", "Per-folder sequence", "Back"]);
        if (mode == 2)
        {
            return Task.CompletedTask;
        }

        var root = ConsoleUi.ExistingDirectoryPrompt("Root directory");
        var recursive = ConsoleUi.Confirm("Include subdirectories?");
        ConsoleUi.ResetScreen(Name);

        if (mode == 0)
        {
            RenameGlobal(root, recursive);
        }
        else
        {
            RenamePerFolder(root, recursive);
        }

        ConsoleUi.Pause();
        return Task.CompletedTask;
    }

    private static void RenameGlobal(string root, bool recursive)
    {
        var files = Helpers.SafeEnumerateFiles(root, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).OrderBy(path => path, Helpers.PathComparer).ToArray();

        RenameInBatches(files, (_, index) => $"{index + 1}{Path.GetExtension(files[index])}");
    }

    private static void RenamePerFolder(string root, bool recursive)
    {
        var directories = new[] { root }.Concat(recursive ? Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories) : []);

        foreach (var directory in directories)
        {
            var files = Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly).OrderBy(path => path, Helpers.PathComparer).ToArray();

            RenameInBatches(files, (_, index) => $"{index + 1}{Path.GetExtension(files[index])}");
        }
    }

    private static void RenameInBatches(IReadOnlyList<string> files, Func<string, int, string> targetNameFactory)
    {
        if (files.Count == 0)
        {
            ConsoleUi.Warning("No files found.");
            return;
        }

        var staged = new List<(string TempPath, string FinalPath)>();
        foreach (var (file, index) in files.Select((file, index) => (file, index)))
        {
            var directory = Path.GetDirectoryName(file)!;
            var tempPath = Helpers.EnsureUniquePath(Path.Combine(directory, $".ftmp_{Guid.NewGuid():N}{Path.GetExtension(file)}"));
            File.Move(file, tempPath);
            staged.Add((tempPath, Path.Combine(directory, targetNameFactory(file, index))));
        }

        var renamed = 0;
        foreach (var item in staged)
        {
            var finalPath = Helpers.EnsureUniquePath(item.FinalPath);
            File.Move(item.TempPath, finalPath);
            renamed++;
        }

        ConsoleUi.Success($"Renamed {renamed} files.");
    }
}
