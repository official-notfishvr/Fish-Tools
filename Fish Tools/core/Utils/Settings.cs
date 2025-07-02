/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Fish_Tools.core.Utils
{
    internal class Settings
    {
        public class Config
        {
            public static ConfigData Data { get; private set; }
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
