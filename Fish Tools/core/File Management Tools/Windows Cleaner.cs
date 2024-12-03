/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.Utils;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static Fish_Tools.core.Utils.Settings;

namespace Fish_Tools.core.FileManagementTools
{
    internal class WindowsCleaner
    {
        private static readonly string _systemDrive = Path.GetPathRoot(Environment.SystemDirectory);
        private static readonly string _userName = Environment.UserName;
        private static readonly string _windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        private static readonly string _localAppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static readonly string _programDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        public static bool _showOperationWindows = false;

        private static readonly string[] _chromeCacheDirectories = {
            Path.Combine(_localAppDataDirectory, "Google", "Chrome", "User Data", "Default", "Cache"),
            Path.Combine(_localAppDataDirectory, "Google", "Chrome", "User Data", "Default", "Media Cache"),
            Path.Combine(_localAppDataDirectory, "Google", "Chrome", "User Data", "Default", "GPUCache"),
            Path.Combine(_localAppDataDirectory, "Google", "Chrome", "User Data", "Default", "Storage", "ext"),
            Path.Combine(_localAppDataDirectory, "Google", "Chrome", "User Data", "Default", "Service Worker"),
            Path.Combine(_localAppDataDirectory, "Google", "Chrome", "User Data", "ShaderCache")
        };

        private static readonly string[] _edgeCacheDirectories = {
            Path.Combine(_localAppDataDirectory, "Microsoft", "Edge", "User Data", "Default", "Cache"),
            Path.Combine(_localAppDataDirectory, "Microsoft", "Edge", "User Data", "Default", "Media Cache"),
            Path.Combine(_localAppDataDirectory, "Microsoft", "Edge", "User Data", "Default", "GPUCache"),
            Path.Combine(_localAppDataDirectory, "Microsoft", "Edge", "User Data", "Default", "Storage", "ext"),
            Path.Combine(_localAppDataDirectory, "Microsoft", "Edge", "User Data", "Default", "Service Worker"),
            Path.Combine(_localAppDataDirectory, "Microsoft", "Edge", "User Data", "ShaderCache"),
            Path.Combine(_localAppDataDirectory, "Microsoft", "Edge SxS", "User Data", "Default", "Cache"),
            Path.Combine(_localAppDataDirectory, "Microsoft", "Edge SxS", "User Data", "Default", "Media Cache"),
            Path.Combine(_localAppDataDirectory, "Microsoft", "Edge SxS", "User Data", "Default", "GPUCache"),
            Path.Combine(_localAppDataDirectory, "Microsoft", "Edge SxS", "User Data", "Default", "Storage", "ext"),
            Path.Combine(_localAppDataDirectory, "Microsoft", "Edge SxS", "User Data", "Default", "Service Worker"),
            Path.Combine(_localAppDataDirectory, "Microsoft", "Edge SxS", "User Data", "ShaderCache")
        };

        public static void WindowsCleanerMain(Logger logger)
        {
            Config.LoadConfig();
            Console.Clear();
            Console.Title = "Fish Tools";
            logger.PrintArt();
            logger.WriteBarrierLine("1", "Clean");
            logger.WriteBarrierLine("2", "Edit Settings");
            logger.WriteBarrierLine("0", "Back");
            Console.Write("-> ");
            var choice = Console.ReadKey().Key;
            switch (choice)
            {
                case ConsoleKey.D1:
                    Clean(logger);
                    break;
                case ConsoleKey.D2:
                    EditSettings(logger);
                    break;
                case ConsoleKey.D0:
                    Console.Clear();
                    Program.MainMenu();
                    break;
            }
        }
        public static async Task Clean(Logger logger)
        {
            Console.Clear();
            Console.Title = "Fish Tools";
            logger.PrintArt();
            logger.Success("Are you sure? Y/N");
            Console.Write("-> ");
            string response = Console.ReadLine()?.ToLower();
            if (response == "y" || response == "yes")
            {
                var config = Config.GetConfigData().WindowsCleaner;
                var tasks = new List<Task>();

                if (config.CleanChromeCache) tasks.Add(ChromeCacheRemoval(logger));
                if (config.CleanEdgeCache) tasks.Add(MicrosoftEdgeCacheRemoval(logger));
                if (config.CleanTemporaryFiles)
                {
                    tasks.Add(Task.Run(() => TemporaryFilesRemoval(logger)));
                    tasks.Add(Task.Run(() => UnnecessaryFilesRemoval(logger)));
                }
                if (config.CleanDeliveryOptimizationFiles) tasks.Add(DeliveryOptimizationFilesRemoval(logger));
                if (config.CleanWindowsOld) tasks.Add(WindowsOldDirectoryRemoval(logger));
                if (config.CleanErrorReports) tasks.Add(WindowsErrorReportsRemoval(logger));
                if (config.CleanEventViewerLogs) tasks.Add(EventViewerLogsRemoval(logger));
                if (config.CleanWindowsUpdateLogs) tasks.Add(WindowsUpdateLogsRemoval(logger));
                if (config.CleanWindowsInstallerCache) tasks.Add(WindowsInstallerCacheRemoval(logger));
                if (config.CleanMSOCache) tasks.Add(MicrosoftOfficeCacheRemoval(logger));
                if (config.CleanWindowsDefenderLogs) tasks.Add(WindowsDefenderLogFilesRemoval(logger));
                if (config.EmptyRecycleBin) tasks.Add(EmptyRecycleBin(logger));
                if (config.CleanTemporaryInternetFiles) tasks.Add(TemporaryInternetFilesRemoval(logger));
                if (config.CleanStartupFiles) tasks.Add(TemporarySetupFilesRemoval(logger));

                await Task.WhenAll(tasks);
            }
            else
            {
                Console.Clear();
                WindowsCleanerMain(logger);
            }
        }
        public static void EditSettings(Logger logger)
        {
            Console.Clear();
            logger.PrintArt();
            DisplaySettings(logger);
            logger.Info("Enter the setting you want to update (e.g., CleanChromeCache): ");
            string settingName = Console.ReadLine();

            if (Config.IsValidSetting(settingName))
            {
                logger.Debug("Enter the new value (true/false): ");
                if (bool.TryParse(Console.ReadLine(), out bool newValue))
                {
                    Config.UpdateCleanerSetting(settingName, newValue);
                    Config.SaveConfig(Config.GetConfigData());
                    logger.Success("Configuration updated and saved.");
                }
                else
                {
                    logger.Error("Invalid value entered. Please enter true or false.");
                }
            }
            else
            {
                logger.Error("Invalid setting name.");
            }
            Console.ReadKey();
        }
        private static void DisplaySettings(Logger logger)
        {
            var config = Config.GetConfigData().WindowsCleaner;
            logger.Warn("Current Configuration:");
            logger.Warn($"Clean Chrome Cache: {config.CleanChromeCache}");
            logger.Warn($"Clean Edge Cache: {config.CleanEdgeCache}");
            logger.Warn($"Clean Temporary Files: {config.CleanTemporaryFiles}");
            logger.Warn($"Clean Delivery Optimization Files: {config.CleanDeliveryOptimizationFiles}");
            logger.Warn($"Clean Windows Old: {config.CleanWindowsOld}");
            logger.Warn($"Clean Error Reports: {config.CleanErrorReports}");
            logger.Warn($"Clean Event Viewer Logs: {config.CleanEventViewerLogs}");
            logger.Warn($"Clean Windows Update Logs: {config.CleanWindowsUpdateLogs}");
            logger.Warn($"Clean Windows Installer Cache: {config.CleanWindowsInstallerCache}");
            logger.Warn($"Clean MSO Cache: {config.CleanMSOCache}");
            logger.Warn($"Clean Windows Defender Logs: {config.CleanWindowsDefenderLogs}");
            logger.Warn($"Empty Recycle Bin: {config.EmptyRecycleBin}");
            logger.Warn($"Clean Temporary Internet Files: {config.CleanTemporaryInternetFiles}");
            logger.Warn($"Clean Startup Files: {config.CleanStartupFiles}");
        }
        public static void UnnecessaryFilesRemoval(Logger logger)
        {
            string[] unnecessaryFolders = {
                "%temp%",
                "%systemroot%\\Prefetch",
                "%systemroot%\\SoftwareDistribution",
                "%userprofile%\\AppData\\Local\\Temp",
                "%systemroot%\\Temp",
                "%localappdata%\\Microsoft\\Windows\\INetCache",
                "%systemdrive%\\Windows.old",
                "%systemroot%\\Logs\\CBS",
                "%systemdrive%\\ProgramData\\Microsoft\\Windows\\WER\\ReportQueue",
                "%systemdrive%\\ProgramData\\Microsoft\\Windows\\WER\\Temp",
                "%systemdrive%\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\StartUp",
                "%systemdrive%\\$Recycle.bin",
                "%systemdrive%\\System Volume Information",
                "%userprofile%\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup",
                "%systemdrive%\\ProgramData\\Microsoft\\Windows\\AppRepository",
                "%systemdrive%\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\StartUp",
                "%systemdrive%\\ProgramData\\Microsoft\\Windows\\Windows Error Reporting",
                "%systemdrive%\\ProgramData\\Microsoft\\Windows\\DeviceMetadataStore",
                "%systemdrive%\\ProgramData\\Microsoft\\Windows\\Registration",
                "%programfiles%\\Common Files\\Adobe\\OOBE",
                "%programfiles%\\Common Files\\Adobe\\ARM",
                "%programfiles%\\Common Files\\Apple\\Apple Application Support",
                "%programfiles%\\Common Files\\Apple\\Apple Application Support\\appstore",
                "%programfiles%\\Common Files\\Apple\\Apple Application Support\\iTunes",
                "%programfiles%\\Common Files\\Apple\\Apple Application Support\\Safari",
                "%programfiles%\\Common Files\\Microsoft Shared",
                "%programfiles%\\Common Files\\Windows Live"
            };

            foreach (string folder in unnecessaryFolders)
            {
                try
                {
                    string expandedFolder = Environment.ExpandEnvironmentVariables(folder);
                    if (Directory.Exists(expandedFolder))
                    {
                        Directory.Delete(expandedFolder, true);
                        logger.Success($"Successfully deleted folder: {expandedFolder}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to delete folder: {folder} - {ex.Message}");
                }
            }
        }
        private static async Task ChromeCacheRemoval(Logger logger)
        {
            foreach (var dir in _chromeCacheDirectories)
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                    logger.Success($"Chrome cache cleared: {dir}");
                }
            }
        }
        private static async Task MicrosoftEdgeCacheRemoval(Logger logger)
        {
            foreach (var dir in _edgeCacheDirectories)
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                    logger.Success($"Edge cache cleared: {dir}");
                }
            }
        }
        private static async Task TemporaryFilesRemoval(Logger logger)
        {
            var tempPath = Path.GetTempPath();
            var tempDirectory = new DirectoryInfo(tempPath);
            foreach (FileInfo file in tempDirectory.GetFiles())
            {
                try
                {
                    file.Delete();
                    logger.Success($"Deleted temporary file: {file.FullName}");
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to delete temporary file: {file.FullName} - {ex.Message}");
                }
            }
            foreach (DirectoryInfo dir in tempDirectory.GetDirectories())
            {
                try
                {
                    dir.Delete(true);
                    logger.Success($"Deleted temporary directory: {dir.FullName}");
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to delete temporary directory: {dir.FullName} - {ex.Message}");
                }
            }
        }
        private static async Task DeliveryOptimizationFilesRemoval(Logger logger)
        {
            string path = Path.Combine(_systemDrive, "Windows", "SoftwareDistribution", "DeliveryOptimization");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                logger.Success("Delivery Optimization files cleared.");
            }
        }
        private static async Task WindowsOldDirectoryRemoval(Logger logger)
        {
            string path = Path.Combine(_systemDrive, "Windows.old");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                logger.Success("Windows.old directory cleared.");
            }
        }
        private static async Task WindowsErrorReportsRemoval(Logger logger)
        {
            string path = Path.Combine(_windowsDirectory, "System32", "Wer", "ReportQueue");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                logger.Success("Windows Error Reports cleared.");
            }
        }
        private static async Task EventViewerLogsRemoval(Logger logger)
        {
            string path = Path.Combine(_systemDrive, "Windows", "System32", "winevt", "Logs");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                logger.Success("Event Viewer Logs cleared.");
            }
        }
        private static async Task WindowsUpdateLogsRemoval(Logger logger)
        {
            string path = Path.Combine(_systemDrive, "Windows", "Logs", "WindowsUpdate");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                logger.Success("Windows Update Logs cleared.");
            }
        }
        private static async Task WindowsInstallerCacheRemoval(Logger logger)
        {
            string path = Path.Combine(_systemDrive, "Windows", "Installer");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                logger.Success("Windows Installer Cache cleared.");
            }
        }
        private static async Task MicrosoftOfficeCacheRemoval(Logger logger)
        {
            string path = Path.Combine(_programDataDirectory, "Microsoft", "Office", "MSO1033");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                logger.Success("MSO Cache cleared.");
            }
        }
        private static async Task WindowsDefenderLogFilesRemoval(Logger logger)
        {
            string path = Path.Combine(_programDataDirectory, "Microsoft", "Windows Defender", "Scans", "History");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                logger.Success("Windows Defender Log Files cleared.");
            }
        }
        private static async Task EmptyRecycleBin(Logger logger)
        {
            string recycleBinPath = Path.Combine(_systemDrive, "$Recycle.Bin");
            if (Directory.Exists(recycleBinPath))
            {
                Directory.Delete(recycleBinPath, true);
                logger.Success("Recycle Bin emptied.");
            }
        }
        private static async Task TemporaryInternetFilesRemoval(Logger logger)
        {
            string path = Path.Combine(_localAppDataDirectory, "Microsoft", "Windows", "INetCache");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                logger.Success("Temporary Internet Files cleared.");
            }
        }
        private static async Task TemporarySetupFilesRemoval(Logger logger)
        {
            string path = Path.Combine(_systemDrive, "ProgramData", "Microsoft", "Windows", "Start Menu", "Programs", "StartUp");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                logger.Success("Temporary Setup Files cleared.");
            }
        }
        private static async Task RemoveOneDrive(Logger logger)
        {
            string OneDriveCLSID = "CLSID\\{018D5C66-4533-4307-9B53-224DE2ED1FE6}";
            int AttributesValue = BitConverter.ToInt32(BitConverter.GetBytes(2962227469U), 0);

            try
            {
                var oneDriveProcess = Process.GetProcessesByName("OneDrive").FirstOrDefault();
                oneDriveProcess?.Kill();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to kill OneDrive process" + ex);
            }

            string oneDriveSetupPath = Environment.Is64BitOperatingSystem
                ? Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\OneDriveSetup.exe"
                : Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\OneDriveSetup.exe";

            try
            {
                Process.Start(oneDriveSetupPath, "/uninstall");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to start {oneDriveSetupPath} with /uninstall argument" + ex);
            }

            var oneDriveDirectories = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OneDrive"),
                Path.Combine(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)), "OneDriveTemp"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\OneDrive"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Microsoft OneDrive")
            };

            foreach (var directory in oneDriveDirectories)
            {
                try
                {
                    if (Directory.Exists(directory))
                    {
                        Directory.Delete(directory, true);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to delete directory: {directory}" + ex);
                }
            }

            try
            {
                using (var baseRegistryKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64))
                using (var clsidKey = baseRegistryKey.CreateSubKey(OneDriveCLSID, true))
                {
                    clsidKey?.SetValue("System.IsPinnedToNameSpaceTree", 0, RegistryValueKind.DWord);

                    using (var shellFolderKey = clsidKey?.OpenSubKey("\\ShellFolder", true))
                    {
                        shellFolderKey?.SetValue("Attributes", AttributesValue, RegistryValueKind.DWord);
                    }
                }

                using (var currentUserRunKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    currentUserRunKey?.DeleteValue("OneDriveSetup", false);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to modify registry for OneDrive removal" + ex);
            }

            try
            {
                Utils.Utils.RunCommand("SCHTASKS /Delete /TN \"OneDrive Standalone Update Task\" /F");
                Utils.Utils.RunCommand("SCHTASKS /Delete /TN \"OneDrive Standalone Update Task v2\" /F");
            }
            catch (Exception ex)
            {
                logger.Error("Failed to delete OneDrive scheduled tasks" + ex);
            }
        }
    }
}