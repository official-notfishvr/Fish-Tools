namespace FishTools.App;

internal sealed class BulkRenamer : ITool
{
    public string Id => "bulk-renamer";
    public string Name => "Bulk Renamer";
    public string Category => ToolCategories.Operations;
    public string Description => "Rename multiple files with find/replace, prefixes, and suffixes.";

    public Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        var root = ConsoleUi.ExistingDirectoryPrompt("Directory");
        var recursive = ConsoleUi.Confirm("Include subdirectories?");
        var find = ConsoleUi.Prompt("Find text", string.Empty);
        var replace = ConsoleUi.Prompt("Replace with", string.Empty);
        var prefix = ConsoleUi.Prompt("Prefix to add", string.Empty);
        var suffix = ConsoleUi.Prompt("Suffix to add", string.Empty);
        var lowercaseExtension = ConsoleUi.Confirm("Force file extensions to lowercase?");
        var previewOnly = ConsoleUi.Confirm("Preview only?");
        ConsoleUi.ResetScreen(Name);

        var files = Helpers.SafeEnumerateFiles(root, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).OrderBy(path => path, Helpers.PathComparer).ToArray();

        if (files.Length == 0)
        {
            ConsoleUi.Warning("No files found.");
            ConsoleUi.Pause();
            return Task.CompletedTask;
        }

        var plans = new List<(string Source, string Destination)>();
        foreach (var file in files)
        {
            var directory = Path.GetDirectoryName(file)!;
            var stem = Path.GetFileNameWithoutExtension(file);
            var extension = Path.GetExtension(file);

            if (!string.IsNullOrEmpty(find))
            {
                stem = stem.Replace(find, replace, StringComparison.OrdinalIgnoreCase);
            }

            stem = prefix + stem + suffix;
            extension = lowercaseExtension ? extension.ToLowerInvariant() : extension;

            var destination = Path.Combine(directory, stem + extension);
            if (!Helpers.PathComparer.Equals(file, destination))
            {
                plans.Add((file, destination));
            }
        }

        if (plans.Count == 0)
        {
            ConsoleUi.Warning("No file names would change with the selected options.");
            ConsoleUi.Pause();
            return Task.CompletedTask;
        }

        ConsoleUi.Info($"Planned renames: {plans.Count}");
        foreach (var plan in plans.Take(25))
        {
            ConsoleUi.Info($"{Path.GetFileName(plan.Source)} -> {Path.GetFileName(plan.Destination)}");
        }

        if (previewOnly)
        {
            ConsoleUi.Pause();
            return Task.CompletedTask;
        }

        if (!ConsoleUi.Confirm("Apply these renames?"))
        {
            return Task.CompletedTask;
        }

        var staged = new List<(string TempPath, string FinalPath)>();
        foreach (var plan in plans)
        {
            var tempPath = Helpers.EnsureUniquePath(Path.Combine(Path.GetDirectoryName(plan.Source)!, $".rename_{Guid.NewGuid():N}{Path.GetExtension(plan.Source)}"));
            File.Move(plan.Source, tempPath);
            staged.Add((tempPath, Helpers.EnsureUniquePath(plan.Destination)));
        }

        var renamed = 0;
        foreach (var item in staged)
        {
            File.Move(item.TempPath, item.FinalPath);
            renamed++;
        }

        ConsoleUi.Success($"Renamed {renamed} files.");
        ConsoleUi.Pause();
        return Task.CompletedTask;
    }
}
