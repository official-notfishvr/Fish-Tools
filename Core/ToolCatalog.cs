namespace FishTools.App;

internal static class ToolCatalog
{
    public static ITool[] Create() =>
        [
            new SystemCleaner(),
            new ProjectCleaner(),
            new FileOrganizer(),
            new SequentialRenamer(),
            new BulkRenamer(),
            new AttributeSwitcher(),
            new FolderMerger(),
            new FileSplitter(),
            new DuplicateFinder(),
            new FolderComparator(),
            new ResourceExtractor(),
            new StorageAnalyzer(),
            new PasswordGenerator(),
            new StrengthAnalyzer(),
            new AesEncryptor(),
            new SystemSpoofer(),
            new SystemMonitor(),
            new SpeedTester(),
            new PortScanner(),
            new FormatConverter(),
            new EmailExtractor(),
            new ListEditor(),
            new ComboManager(),
            new AutoClicker(),
            new DiscordManager(),
            new AccountChecker(),
            new Il2CppCleanerTool(),
        ];
}
