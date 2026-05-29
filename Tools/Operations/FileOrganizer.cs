namespace FishTools.App;

internal sealed class FileOrganizer : ITool
{
    public string Id => "file-organizer";
    public string Name => "File Organizer";
    public string Category => ToolCategories.Operations;
    public string Description => "Organize a folder by file type, date, or size brackets.";

    public Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        var mode = ConsoleUi.ShowMenu("Choose an organization mode", ["By file type", "By modified month", "By size", "Back"]);
        if (mode == 3)
        {
            return Task.CompletedTask;
        }

        var directory = ConsoleUi.ExistingDirectoryPrompt("Directory to organize");
        var previewOnly = ConsoleUi.Confirm("Preview only without moving files?");
        var files = Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly).ToArray();
        ConsoleUi.ResetScreen(Name);

        if (files.Length == 0)
        {
            ConsoleUi.Warning("No top-level files found.");
            ConsoleUi.Pause();
            return Task.CompletedTask;
        }

        var moved = 0;
        foreach (var file in files)
        {
            var destinationFolder = mode switch
            {
                0 => Path.Combine(directory, FileTypeBucket(Path.GetExtension(file))),
                1 => Path.Combine(directory, File.GetLastWriteTime(file).ToString("yyyy-MM")),
                2 => Path.Combine(directory, SizeBucket(new FileInfo(file).Length)),
                _ => directory,
            };

            var destinationPath = Helpers.EnsureUniquePath(Path.Combine(destinationFolder, Path.GetFileName(file)));
            ConsoleUi.Info($"{Path.GetFileName(file)} -> {destinationFolder}");

            if (previewOnly)
            {
                continue;
            }

            Directory.CreateDirectory(destinationFolder);
            File.Move(file, destinationPath);
            moved++;
        }

        ConsoleUi.Success(previewOnly ? "Preview completed." : $"Moved {moved} files.");
        ConsoleUi.Pause();
        return Task.CompletedTask;
    }

    private static string FileTypeBucket(string extension)
    {
        extension = extension.ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => "Images",
            ".mp4" or ".mov" or ".avi" or ".mkv" or ".webm" => "Videos",
            ".mp3" or ".wav" or ".flac" or ".aac" => "Audio",
            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "Archives",
            ".pdf" or ".doc" or ".docx" or ".xls" or ".xlsx" or ".ppt" or ".pptx" or ".txt" => "Documents",
            ".cs" or ".js" or ".ts" or ".json" or ".xml" or ".html" or ".css" or ".py" => "Code",
            _ => string.IsNullOrEmpty(extension) ? "No Extension" : extension.TrimStart('.').ToUpperInvariant(),
        };
    }

    private static string SizeBucket(long bytes)
    {
        if (bytes < 1 * 1024 * 1024)
            return "Small";
        if (bytes < 100 * 1024 * 1024)
            return "Medium";
        return "Large";
    }
}
