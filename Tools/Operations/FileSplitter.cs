namespace FishTools.App;

internal sealed class FileSplitter : ITool
{
    public string Id => "file-splitter";
    public string Name => "File Splitter & Merger";
    public string Category => ToolCategories.Operations;
    public string Description => "Split a large file into smaller parts or merge multiple part files.";

    public Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        var choice = ConsoleUi.ShowMenu("Choose an action", ["Split file", "Merge parts", "Back"]);
        switch (choice)
        {
            case 0:
                SplitFile();
                break;
            case 1:
                MergeFiles();
                break;
        }

        return Task.CompletedTask;
    }

    private static void SplitFile()
    {
        ConsoleUi.ResetScreen("Split File");
        var filePath = ConsoleUi.ExistingFilePrompt("File");
        var partSizeMb = ConsoleUi.PromptInt("Part size in MB", 25, 1, 4096);
        var partSizeBytes = (long)partSizeMb * 1024 * 1024;
        var outputDirectory = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(filePath)!, $"{Path.GetFileName(filePath)}.parts")).FullName;
        ConsoleUi.ResetScreen("Split File");

        using var input = File.OpenRead(filePath);
        var buffer = new byte[81920];
        var index = 1;
        while (input.Position < input.Length)
        {
            var partPath = Path.Combine(outputDirectory, $"{Path.GetFileName(filePath)}.part{index:D4}");
            using var output = File.Create(partPath);
            long written = 0;
            while (written < partSizeBytes && input.Position < input.Length)
            {
                var remainingForPart = (int)Math.Min(buffer.Length, partSizeBytes - written);
                var read = input.Read(buffer, 0, remainingForPart);
                if (read == 0)
                {
                    break;
                }

                output.Write(buffer, 0, read);
                written += read;
            }

            index++;
        }

        ConsoleUi.Success($"Created {index - 1} part files in {outputDirectory}");
        ConsoleUi.Pause();
    }

    private static void MergeFiles()
    {
        ConsoleUi.ResetScreen("Merge Parts");
        var directory = ConsoleUi.ExistingDirectoryPrompt("Parts directory");
        var parts = Directory.EnumerateFiles(directory, "*.part*", SearchOption.TopDirectoryOnly).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        ConsoleUi.ResetScreen("Merge Parts");

        if (parts.Length == 0)
        {
            ConsoleUi.Warning("No part files were found.");
            ConsoleUi.Pause();
            return;
        }

        var defaultName = Path.GetFileNameWithoutExtension(parts[0]);
        var outputPath = Helpers.EnsureUniquePath(Path.Combine(directory, $"{defaultName}.merged"));

        using var output = File.Create(outputPath);
        foreach (var part in parts)
        {
            using var input = File.OpenRead(part);
            input.CopyTo(output);
        }

        ConsoleUi.Success($"Merged {parts.Length} parts into {outputPath}");
        ConsoleUi.Pause();
    }
}
