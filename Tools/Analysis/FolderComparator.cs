using System.Text;

namespace FishTools.App;

internal sealed class FolderComparator : ITool
{
    public string Id => "folder-comparator";
    public string Name => "Folder Comparator";
    public string Category => ToolCategories.Analysis;
    public string Description => "Compare two directories by relative structure and SHA-256 file hashes.";

    public async Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        var left = ConsoleUi.ExistingDirectoryPrompt("First folder");
        var right = ConsoleUi.ExistingDirectoryPrompt("Second folder");
        var recursive = ConsoleUi.Confirm("Include subdirectories?");
        ConsoleUi.ResetScreen(Name);

        var leftFiles = Helpers.SafeEnumerateFiles(left, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToDictionary(path => Path.GetRelativePath(left, path), path => path, Helpers.PathComparer);
        var rightFiles = Helpers.SafeEnumerateFiles(right, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToDictionary(path => Path.GetRelativePath(right, path), path => path, Helpers.PathComparer);

        var builder = new StringBuilder();
        var onlyLeft = 0;
        var onlyRight = 0;
        var changed = 0;

        foreach (var relativePath in leftFiles.Keys.Union(rightFiles.Keys, Helpers.PathComparer).OrderBy(path => path, Helpers.PathComparer))
        {
            var hasLeft = leftFiles.TryGetValue(relativePath, out var leftPath);
            var hasRight = rightFiles.TryGetValue(relativePath, out var rightPath);

            if (!hasLeft)
            {
                onlyRight++;
                builder.AppendLine($"ONLY RIGHT: {relativePath}");
                continue;
            }

            if (!hasRight)
            {
                onlyLeft++;
                builder.AppendLine($"ONLY LEFT: {relativePath}");
                continue;
            }

            var leftHash = await Helpers.ComputeSha256Async(leftPath!);
            var rightHash = await Helpers.ComputeSha256Async(rightPath!);
            if (!string.Equals(leftHash, rightHash, StringComparison.Ordinal))
            {
                changed++;
                builder.AppendLine($"DIFFERENT: {relativePath}");
            }
        }

        ConsoleUi.Success($"Only in first folder: {onlyLeft}");
        ConsoleUi.Success($"Only in second folder: {onlyRight}");
        ConsoleUi.Success($"Different content: {changed}");

        var reportPath = context.CreateReportPath("hash_compare_report");
        File.WriteAllText(reportPath, builder.ToString());
        ConsoleUi.Info($"Report saved to {reportPath}");
        ConsoleUi.Pause();
    }
}
