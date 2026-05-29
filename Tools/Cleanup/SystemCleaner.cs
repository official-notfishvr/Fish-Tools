using Fish.Console;
using FishConsole = Fish.Console.FishConsole;

namespace FishTools.App;

internal sealed class SystemCleaner : ITool
{
    public string Id => "system-cleaner";
    public string Name => "System Cleaner";
    public string Category => ToolCategories.Cleanup;
    public string Description => "Clear temp files and browser caches with saved settings.";

    public Task RunAsync(ToolContext context)
    {
        while (true)
        {
            ConsoleUi.ResetScreen(Name);
            var options = context.Settings.WindowsCleaner;

            ConsoleUi.Info("Targets are conservative by default and saved between runs.");
            ConsoleUi.Section("Current Settings");
            ConsoleUi.Info($"User temp: {OnOff(options.CleanUserTemp)}");
            ConsoleUi.Info($"Windows temp: {OnOff(options.CleanWindowsTemp)}");
            ConsoleUi.Info($"Browser caches: {OnOff(options.CleanBrowserCaches)}");
            ConsoleUi.Info($"Prefetch: {OnOff(options.CleanPrefetch)}");
            ConsoleUi.Info($"SoftwareDistribution\\Download: {OnOff(options.CleanSoftwareDistributionDownload)}");

            var selected = ConsoleUi.ShowMenu("Choose an action", ["Run cleanup", "Edit settings", "Back"]);
            if (selected == 2)
            {
                return Task.CompletedTask;
            }

            if (selected == 1)
            {
                EditSettings(context.Settings.WindowsCleaner);
                context.Settings.Save();
                continue;
            }

            RunCleanup(context);
            ConsoleUi.Pause();
        }
    }

    private static void EditSettings(WindowsCleanerOptions options)
    {
        while (true)
        {
            ConsoleUi.ResetScreen("System Cleaner Settings");
            var selected = ConsoleUi.ShowMenu(
                "Toggle a setting",
                [
                    $"User temp ({OnOff(options.CleanUserTemp)})",
                    $"Windows temp ({OnOff(options.CleanWindowsTemp)})",
                    $"Browser caches ({OnOff(options.CleanBrowserCaches)})",
                    $"Prefetch ({OnOff(options.CleanPrefetch)})",
                    $"SoftwareDistribution\\\\Download ({OnOff(options.CleanSoftwareDistributionDownload)})",
                    "Back",
                ]
            );

            switch (selected)
            {
                case 0:
                    options.CleanUserTemp = !options.CleanUserTemp;
                    break;
                case 1:
                    options.CleanWindowsTemp = !options.CleanWindowsTemp;
                    break;
                case 2:
                    options.CleanBrowserCaches = !options.CleanBrowserCaches;
                    break;
                case 3:
                    options.CleanPrefetch = !options.CleanPrefetch;
                    break;
                case 4:
                    options.CleanSoftwareDistributionDownload = !options.CleanSoftwareDistributionDownload;
                    break;
                default:
                    return;
            }
        }
    }

    private static void RunCleanup(ToolContext context)
    {
        ConsoleUi.ResetScreen("System Cleaner");
        if (!ConsoleUi.Confirm("Proceed with cleanup using the current settings?"))
        {
            return;
        }

        ConsoleUi.ResetScreen("System Cleaner");
        var targets = BuildTargets(context.Settings.WindowsCleaner);
        long reclaimedBytes = 0;
        var deletedFiles = 0;
        var deletedDirectories = 0;
        var failures = new List<string>();

        foreach (var target in targets)
        {
            foreach (var path in target.Paths.Where(Directory.Exists))
            {
                reclaimedBytes += Helpers.GetDirectorySize(path);
                DeleteContents(path, ref deletedFiles, ref deletedDirectories, failures);
            }
        }

        ConsoleUi.Success($"Deleted files: {deletedFiles}");
        ConsoleUi.Success($"Deleted directories: {deletedDirectories}");
        ConsoleUi.Info($"Approximate space reclaimed: {Helpers.FormatBytes(reclaimedBytes)}");

        if (failures.Count > 0)
        {
            ConsoleUi.Warning($"Some items could not be removed: {failures.Count}");
            foreach (var failure in failures.Take(8))
            {
                ConsoleUi.Warning($"- {failure}");
            }
        }
    }

    private static List<(string Name, IEnumerable<string> Paths)> BuildTargets(WindowsCleanerOptions options)
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var targets = new List<(string Name, IEnumerable<string> Paths)>();

        if (options.CleanUserTemp)
        {
            targets.Add(("User Temp", [Path.GetTempPath()]));
        }

        if (options.CleanWindowsTemp)
        {
            targets.Add(("Windows Temp", [Path.Combine(windows, "Temp")]));
        }

        if (options.CleanBrowserCaches)
        {
            targets.Add(
                (
                    "Browser Caches",
                    [
                        Path.Combine(localAppData, "Google", "Chrome", "User Data", "Default", "Cache"),
                        Path.Combine(localAppData, "Google", "Chrome", "User Data", "Default", "Code Cache"),
                        Path.Combine(localAppData, "Microsoft", "Edge", "User Data", "Default", "Cache"),
                        Path.Combine(localAppData, "Microsoft", "Edge", "User Data", "Default", "Code Cache"),
                    ]
                )
            );
        }

        if (options.CleanPrefetch)
        {
            targets.Add(("Prefetch", [Path.Combine(windows, "Prefetch")]));
        }

        if (options.CleanSoftwareDistributionDownload)
        {
            targets.Add(("SoftwareDistribution", [Path.Combine(windows, "SoftwareDistribution", "Download")]));
        }

        return targets;
    }

    private static void DeleteContents(string root, ref int deletedFiles, ref int deletedDirectories, List<string> failures)
    {
        try
        {
            foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                    deletedFiles++;
                }
                catch (Exception ex)
                {
                    failures.Add($"{file} ({ex.Message})");
                }
            }

            foreach (var directory in Directory.EnumerateDirectories(root, "*", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    Directory.Delete(directory, true);
                    deletedDirectories++;
                }
                catch (Exception ex)
                {
                    failures.Add($"{directory} ({ex.Message})");
                }
            }
        }
        catch (Exception ex)
        {
            failures.Add($"{root} ({ex.Message})");
        }
    }

    private static string OnOff(bool enabled) => enabled ? "ON" : "OFF";
}
