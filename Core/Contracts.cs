using System.Text.Json;

namespace FishTools.App;

internal interface ITool
{
    string Id { get; }
    string Name { get; }
    string Category { get; }
    string Description { get; }
    Task RunAsync(ToolContext context);
}

internal static class ToolCategories
{
    public const string Cleanup = "Cleanup & Maintenance";
    public const string Operations = "File Operations";
    public const string Analysis = "File Analysis";
    public const string Security = "Security & Privacy";
    public const string SystemHardware = "System & Network";
    public const string DataText = "Data & Text";
    public const string Automation = "Automation";
    public const string SocialWeb = "Social & Web";

    private static readonly IReadOnlyDictionary<string, int> OrderMap = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        [Cleanup] = 0,
        [Operations] = 1,
        [Analysis] = 2,
        [Security] = 3,
        [SystemHardware] = 4,
        [DataText] = 5,
        [Automation] = 6,
        [SocialWeb] = 7,
    };

    public static int GetOrder(string category) => OrderMap.TryGetValue(category, out var order) ? order : int.MaxValue;
}

internal sealed class AppPaths
{
    public AppPaths(string rootDirectory)
    {
        RootDirectory = rootDirectory;
        DataDirectory = Path.Combine(rootDirectory, "Data");
        ResultsDirectory = Path.Combine(rootDirectory, "Results");
        SettingsFilePath = Path.Combine(DataDirectory, "settings.json");
    }

    public string RootDirectory { get; }
    public string DataDirectory { get; }
    public string ResultsDirectory { get; }
    public string SettingsFilePath { get; }

    public void EnsureCreated()
    {
        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(ResultsDirectory);
    }
}

internal sealed class WindowsCleanerOptions
{
    public bool CleanUserTemp { get; set; } = true;
    public bool CleanWindowsTemp { get; set; } = true;
    public bool CleanBrowserCaches { get; set; } = true;
    public bool CleanPrefetch { get; set; }
    public bool CleanSoftwareDistributionDownload { get; set; }
}

internal sealed class AppSettings
{
    public HashSet<string> DisabledTools { get; set; } = [];
    public WindowsCleanerOptions WindowsCleaner { get; set; } = new();
}

internal sealed class AppSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _settingsPath;

    public AppSettingsStore(string settingsPath, AppSettings settings)
    {
        _settingsPath = settingsPath;
        Settings = settings;
    }

    public AppSettings Settings { get; }
    public WindowsCleanerOptions WindowsCleaner => Settings.WindowsCleaner;

    public static AppSettingsStore Load(string settingsPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);

        if (!File.Exists(settingsPath))
        {
            var store = new AppSettingsStore(settingsPath, new AppSettings());
            store.Save();
            return store;
        }

        try
        {
            var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(settingsPath), JsonOptions) ?? new AppSettings();
            settings.DisabledTools ??= [];
            settings.WindowsCleaner ??= new WindowsCleanerOptions();
            return new AppSettingsStore(settingsPath, settings);
        }
        catch
        {
            return new AppSettingsStore(settingsPath, new AppSettings());
        }
    }

    public bool IsEnabled(ITool tool) => !Settings.DisabledTools.Contains(tool.Id);

    public void SetEnabled(ITool tool, bool enabled)
    {
        if (enabled)
        {
            Settings.DisabledTools.Remove(tool.Id);
        }
        else
        {
            Settings.DisabledTools.Add(tool.Id);
        }

        Save();
    }

    public void EnableAll(IEnumerable<ITool> tools)
    {
        foreach (var tool in tools)
        {
            Settings.DisabledTools.Remove(tool.Id);
        }

        Save();
    }

    public void DisableAll(IEnumerable<ITool> tools)
    {
        foreach (var tool in tools)
        {
            Settings.DisabledTools.Add(tool.Id);
        }

        Save();
    }

    public void Save()
    {
        File.WriteAllText(_settingsPath, JsonSerializer.Serialize(Settings, JsonOptions));
    }
}

internal sealed class ToolContext
{
    public ToolContext(AppPaths paths, AppSettingsStore settings)
    {
        Paths = paths;
        Settings = settings;
    }

    public AppPaths Paths { get; }
    public AppSettingsStore Settings { get; }

    public string CreateReportPath(string prefix, string extension = ".txt")
    {
        var safePrefix = Helpers.SanitizeFileName(prefix);
        return Path.Combine(Paths.ResultsDirectory, $"{safePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}");
    }
}
