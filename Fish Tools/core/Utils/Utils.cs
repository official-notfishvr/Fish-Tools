/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.FileManagementTools;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Fish_Tools.core.Utils
{
    internal class Utils
    {
        public static void ExecuteCommand(string command, Logger logger)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/C " + command,
                        WindowStyle = WindowsCleaner._showOperationWindows ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden,
                        UseShellExecute = true,
                        Verb = "runas"
                    }
                };
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                logger.Error($"Command '{command}' failed. Error: {ex.Message}");
            }
        }
        public static async Task RunCommand(string arguments, string workingDirectory, Logger logger)
        {
            await Task.Run(() =>
            {
                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = arguments,
                            WorkingDirectory = workingDirectory,
                            WindowStyle = WindowsCleaner._showOperationWindows ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden,
                            UseShellExecute = true,
                            Verb = "runas"
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    logger.Error($"Command '{arguments}' failed. Error: {ex.Message}");
                }
            });
        }
        public static void RunCommand(string command)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                try
                {
                    process.Start();
                    process.StandardInput.WriteLine(command);
                    process.StandardInput.Close();
                    process.WaitForExit();
                }
                catch (Exception)
                {
                }
            }
        }
        public static async Task KillProcessAndDeleteDirectories(string processName, string[] directories, Logger logger)
        {
            await Task.Run(() =>
            {
                ExecuteCommand($"/C TASKKILL /F /IM {processName}", logger);
                foreach (var directory in directories)
                {
                    if (Directory.Exists(directory))
                    {
                        try
                        {
                            Directory.Delete(directory, true);
                            logger.Success($"Deleted {directory}");
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Failed to delete {directory}. Error: {ex.Message}");
                        }
                    }
                }
            });
        }
        public static void Wait(int miliseconds) { Task.Run(async () => await Task.Delay(miliseconds)).Wait(); }
        public static void NewThread(Action action) { Task.Run(() => action.Invoke()); }
        public class Constants
        {
            //modifiers
            public const int NOMOD = 0x0000;
            public const int ALT = 0x0001;
            public const int CTRL = 0x0002;
            public const int SHIFT = 0x0004;
            public const int WIN = 0x0008;

            //windows message id for hotkey
            public const int WM_HOTKEY_MSG_ID = 0x0312;
        }
        public class KeyHandler
        {
            [DllImport("user32.dll")]
            private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

            [DllImport("user32.dll")]
            private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

            private int modifier;
            private int key;
            private IntPtr hWnd;
            private int id;

            public KeyHandler(int modifier, Keys key, Form form)
            {
                this.modifier = modifier;
                this.key = (int)key;
                this.hWnd = form.Handle;
                id = this.GetHashCode();
            }

            public override int GetHashCode()
            {
                return modifier ^ key ^ hWnd.ToInt32();
            }

            public bool Register()
            {
                return RegisterHotKey(hWnd, id, modifier, key);
            }

            public bool Unregiser()
            {
                return UnregisterHotKey(hWnd, id);
            }
        }
        public class Config
        {
            public class DiscordConfig
            {
                public string Username { get; set; }
                public string Token { get; set; }
            }

            public class WindowsCleanerConfig
            {
                public bool CleanChromeCache { get; set; } = true;
                public bool CleanEdgeCache { get; set; } = true;
                public bool CleanTemporaryFiles { get; set; } = true;
                public bool CleanDeliveryOptimizationFiles { get; set; } = true;
                public bool CleanWindowsOld { get; set; } = true;
                public bool CleanErrorReports { get; set; } = true;
                public bool CleanEventViewerLogs { get; set; } = true;
                public bool CleanWindowsUpdateLogs { get; set; } = true;
                public bool CleanWindowsInstallerCache { get; set; } = true;
                public bool CleanMSOCache { get; set; } = true;
                public bool CleanWindowsDefenderLogs { get; set; } = true;
                public bool EmptyRecycleBin { get; set; } = true;
                public bool CleanTemporaryInternetFiles { get; set; } = true;
                public bool CleanStartupFiles { get; set; } = true;
            }

            public class ConfigData
            {
                public DiscordConfig Discord { get; set; } = new DiscordConfig();
                public WindowsCleanerConfig WindowsCleaner { get; set; } = new WindowsCleanerConfig();
            }

            private static ConfigData _configData;
            private static readonly Dictionary<string, string> _settingMappings = new Dictionary<string, string>
            {
                {"Clean Chrome Cache", "CleanChromeCache"},
                {"Clean Edge Cache", "CleanEdgeCache"},
                {"Clean Temporary Files", "CleanTemporaryFiles"},
                {"Clean Delivery Optimization Files", "CleanDeliveryOptimizationFiles"},
                {"Clean Windows Old", "CleanWindowsOld"},
                {"Clean Error Reports", "CleanErrorReports"},
                {"Clean Event Viewer Logs", "CleanEventViewerLogs"},
                {"Clean Windows Update Logs", "CleanWindowsUpdateLogs"},
                {"Clean Windows Installer Cache", "CleanWindowsInstallerCache"},
                {"Clean MSO Cache", "CleanMSOCache"},
                {"Clean Windows Defender Logs", "CleanWindowsDefenderLogs"},
                {"Empty Recycle Bin", "EmptyRecycleBin"},
                {"Clean Temporary Internet Files", "CleanTemporaryInternetFiles"},
                {"Clean Startup Files", "CleanStartupFiles"}
            };

            public static void SaveConfig(ConfigData configData)
            {
                _configData = configData;
                string json = JsonConvert.SerializeObject(configData, Formatting.Indented);
                string dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                Directory.CreateDirectory(dataDirectory);  
                string configPath = Path.Combine(dataDirectory, "config.json");
                File.WriteAllText(configPath, json);
            }

            public static void LoadConfig()
            {
                string dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                string configPath = Path.Combine(dataDirectory, "config.json");
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    _configData = JsonConvert.DeserializeObject<ConfigData>(json);
                }
                else
                {
                    _configData = new ConfigData(); 
                }
            }

            public static ConfigData GetConfigData()
            {
                return _configData;
            }

            public static void UpdateCleanerSetting(string settingName, bool value)
            {
                if (_settingMappings.TryGetValue(settingName, out string internalName))
                {
                    var settings = GetConfigData().WindowsCleaner;
                    var property = typeof(WindowsCleanerConfig).GetProperty(internalName);
                    if (property != null)
                    {
                        property.SetValue(settings, value);
                    }
                }
            }

            public static bool IsValidSetting(string settingName)
            {
                return _settingMappings.ContainsKey(settingName);
            }

            public static void DisplaySettings()
            {
                var settings = GetConfigData().WindowsCleaner;
                foreach (var mapping in _settingMappings)
                {
                    var property = typeof(WindowsCleanerConfig).GetProperty(mapping.Value);
                    if (property != null)
                    {
                        bool value = (bool)property.GetValue(settings);
                        Console.WriteLine($"{mapping.Key}: {value}");
                    }
                }
            }
        }
    }
}
