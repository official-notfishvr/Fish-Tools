using Fish_Tools.core.Utils;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.IO;
using Console = System.Console;
using static SteamKit2.GC.Underlords.Internal.CMsgClientToGCGetFriendCodesResponse;
using System.Net.NetworkInformation;

namespace Fish_Tools.core.Misc_Tools
{
    internal class Spoofer : ITool
    {
        public string Name => "Spoofer";
        public string Category => "Security & Privacy Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Network and hardware ID spoofing tools";

        private static Random rnd = new Random();

        public void Main(Logger logger)
        {
            Console.Clear();
            logger.PrintArt();
            SpooferMain(logger);
        }

        public static void SpooferMain(Logger logger)
        {
            logger.WriteBarrierLine("1", "Network");
            logger.WriteBarrierLine("2", "MAC");
            logger.WriteBarrierLine("0", "Back");
            Console.Write("-> ");
            ConsoleKey choice = Console.ReadKey().Key;
            switch (choice)
            {
                case ConsoleKey.D1:
                    Console.WriteLine();
                    logger.WriteBarrierLine("1", "Fix Network");
                    logger.WriteBarrierLine("2", "Flush DNS");
                    logger.WriteBarrierLine("0", "Back");
                    Console.Write("-> ");
                    ConsoleKey Networkchoice = Console.ReadKey().Key;
                    switch (Networkchoice)
                    {
                        case ConsoleKey.D1:
                            var NetworkAdapters = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}");
                            foreach (string adapter in NetworkAdapters.GetSubKeyNames())
                            {
                                if (adapter != "Properties")
                                {
                                    var NetworkAdapter = Registry.LocalMachine.OpenSubKey($"SYSTEM\\CurrentControlSet\\Control\\Class\\{{4d36e972-e325-11ce-bfc1-08002be10318}}\\{adapter}", true);
                                    NetworkAdapter.GetValue("NetworkAddress");
                                    string adapterId = NetworkAdapter.GetValue("NetCfgInstanceId").ToString();

                                    string interfaceName = "Ethernet";
                                    foreach (NetworkInterface i in NetworkInterface.GetAllNetworkInterfaces()) { if (i.Id == adapterId) { interfaceName = i.Name; break; } }
                                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("netsh", $"interface set interface \"{interfaceName}\" {"enable"}");
                                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                                    p.StartInfo = psi;
                                    p.Start();
                                    p.WaitForExit();
                                }
                            }
                            break;
                        case ConsoleKey.D2:
                            RunCMD("ipconfig /release");
                            RunCMD("ipconfig /flushdns");
                            RunCMD("ipconfig /renew");
                            RunCMD("ipconfig /flushdns");
                            RunCMD("ping localhost -n 3 >nul");
                            break;
                        case ConsoleKey.D0:
                            return;
                    }
                    break;
                case ConsoleKey.D2:
                    Console.WriteLine();
                    logger.WriteBarrierLine("1", "List HWIDs");
                    logger.WriteBarrierLine("2", "Spoof HWIDs");
                    logger.WriteBarrierLine("3", "Clean Traces");
                    logger.WriteBarrierLine("0", "Back");
                    Console.Write("-> ");
                    ConsoleKey MACchoice = Console.ReadKey().Key;
                    switch (MACchoice)
                    {
                        case ConsoleKey.D1:
                            ListHWIDs(logger);
                            break;
                        case ConsoleKey.D2:
                            SpoofHWIDs(logger);
                            break;
                        case ConsoleKey.D3:
                            CleanTraces(logger);
                            break;
                        case ConsoleKey.D0:
                            return;
                    }
                    break;
                case ConsoleKey.D0:
                    return;
            }
        }
        private static void ListHWIDs(Logger logger)
        {
            try
            {
                Console.WriteLine();
                logger.Debug("Scanning Local System.. Please Wait!");
                var hwidKeys = new Dictionary<string, string>
                {
                    { "HWProfileGuid", @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\IDConfigDB\Hardware Profiles\0001" },
                    { "MachineGuid", @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography" },
                    { "MachineId", @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\SQMClient" },
                    { "ProductID", @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion" },
                    { "SusClientId", @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate" },
                    { "ComputerHardwareId", @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SystemInformation" }
                };

                StringBuilder hwidList = new StringBuilder();

                foreach (var key in hwidKeys)
                {
                    string value = Registry.GetValue(key.Value, key.Key, null)?.ToString().Replace("{", "").Replace("}", "");
                    if (value != null)
                    {
                        hwidList.AppendLine($"HWID Found -> {value}");
                    }
                }

                var hardwareIds = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SystemInformation", "ComputerHardwareIds", new string[0]) as string[];
                if (hardwareIds != null)
                {
                    foreach (var hardwareId in hardwareIds)
                    {
                        hwidList.AppendLine($"HWID Found -> {hardwareId.Trim().Replace("{", "").Replace("}", "")}");
                    }
                }

                logger.Success("All HWIDs that have been Found have been Listed!");
                Console.WriteLine(hwidList.ToString());
                Console.ReadKey();
            }
            catch
            {
                logger.Error("Listing HWIDs Failed.. Please Try Again and or Run as Admin!");
                Console.ReadKey();
            }
        }
        private static void SpoofHWIDs(Logger logger)
        {
            try
            {
                Console.WriteLine();
                logger.Debug("Scanning Local System.. Please Wait!");
                var hwidKeys = new Dictionary<string, string>
                {
                    { "HwProfileGuid", @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\IDConfigDB\Hardware Profiles\0001" },
                    { "MachineGuid", @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography" },
                    { "MachineId", @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\SQMClient" },
                    { "ProductID", @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion" },
                    { "SusClientId", @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate" },
                    { "ComputerHardwareId", @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SystemInformation" }
                };

                StringBuilder spoofedList = new StringBuilder();

                foreach (var key in hwidKeys)
                {
                    string newGuid = GenerateRandomGuid();
                    Registry.SetValue(key.Value, key.Key, newGuid);
                    spoofedList.AppendLine($"New HWID -> {newGuid} - Spoofed!");
                }

                string[] hardwareIds = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SystemInformation", "ComputerHardwareIds", new string[0]) as string[];
                if (hardwareIds != null)
                {
                    for (int i = 0; i < hardwareIds.Length; i++)
                    {
                        hardwareIds[i] = GenerateRandomGuid();
                    }
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SystemInformation", "ComputerHardwareIds", hardwareIds, RegistryValueKind.MultiString);
                }
                Console.WriteLine(spoofedList.ToString());
                logger.Success("All HWIDs that have been Found have been Spoofed!");
                Console.ReadKey();
            }
            catch
            {
                logger.Error("Spoof HWIDs Failed.. Please Try Again and or Run as Admin!");
                Console.ReadKey();
            }
        }
        private static void CleanTraces(Logger logger) 
        {
            try
            {
                Console.WriteLine();
                logger.Debug("Scanning Local System.. Please Wait!");
                // Blizzard/Battle.net
                string[] directoriesToDelete = new string[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Blizzard Entertainment"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Battle.net"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Battle.net"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Blizzard Entertainment"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\GPUCache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\GPUCache\\data_0.dcache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\GPUCache\\data_1.dcache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\GPUCache\\data_2.dcache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\GPUCache\\data_3.dcache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\GPUCache\\f_000001.dcache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\GPUCache\\index.dcache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\Cache\\index"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\GPUCache\\data_0"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\GPUCache\\data_1"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\GPUCache\\data_2"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\GPUCache\\data_3"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\GPUCache\\f_000001"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\GPUCache\\index"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\Cache\\index.dcache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\Cache\\data_0.dcache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\Cache\\data_1.dcache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\Cache\\data_2.dcache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\Cache\\data_3.dcache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\Cache\\data_0"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\Cache\\data_1"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\Cache\\data_2"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\BrowserCache\\Cache\\data_3"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\Cache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\Logs"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\WidevineCdm"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net\\CachedData"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Blizzard Entertainment"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Roaming\\Battle.net"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Battle.net"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Blizzard Entertainment")
                };
                foreach (var dir in directoriesToDelete)
                {
                    try
                    {
                        if (Directory.Exists(dir))
                        {
                            Directory.Delete(dir, true);
                            logger.Warn($"Found & Deleted -> {dir}");
                        }
                    }
                    catch { }
                }
                string tempFilename = Path.ChangeExtension(Path.GetTempFileName(), ".bat");
                using (StreamWriter writer = new StreamWriter(tempFilename))
                {
                    // GTAV/FiveM
                    writer.WriteLine(@"echo off");
                    writer.WriteLine("cls");
                    writer.WriteLine("taskkill /f /im Steam.exe /t");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"set hostspath=%windir%\System32\drivers\etc\hosts");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSLicensing\HardwareID / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSLicensing\Store / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_CURRENT_USER\Software\WinRAR\ArcHistory / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\bam\State\UserSettings\S - 1 - 5 - 21 - 1282084573 - 1681065996 - 3115981261 - 1001 / va / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETEH KEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FeatureUsage\ShowJumpView / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETEH KEY_CURRENT_USER\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_CURRENT_USER\Software\WinRAR\ArcHistory / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FeatureUsage\AppSwitched / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_CLASSES_ROOT\Local Settings\Software\Microsoft\Windows\Shell\MuiCache / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FeatureUsage\ShowJumpView / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\bam\State\UserSettings\S - 1 - 5 - 21 - 332004695 - 2829936588 - 140372829 - 1002 / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_CLASSES_ROOT\Local Settings\Software\Microsoft\Windows\Shell\MuiCache / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_CURRENT_USER\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_CURRENT_USER\Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Compatibility Assistant\Store / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FeatureUsage\AppSwitched / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\bam\State\UserSettings\S - 1 - 5 - 21 - 1282084573 - 1681065996 - 3115981261 - 1001 / f");
                    writer.WriteLine("cls");
                    // (EAC) EasyAntiCheat
                    writer.WriteLine(@"REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\EasyAntiCheat / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\EasyAntiCheat_EOS / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\EasyAntiCheat / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\EasyAntiCheat_EOS / f");
                    writer.WriteLine("cls");
                    writer.WriteLine(@"REG DELETE HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\EasyAntiCheat\Security / f");
                    writer.WriteLine("cls");
                    // Fortnite
                    writer.WriteLine("taskkill /f /im epicgameslauncher.exe");
                    writer.WriteLine("taskkill /f /im EpicWebHelper.exe");
                    writer.WriteLine("taskkill /f /im FortniteClient-Win64-Shipping_EAC.exe");
                    writer.WriteLine("taskkill /f /im FortniteClient-Win64-Shipping_BE.exe");
                    writer.WriteLine("taskkill /f /im FortniteLauncher.exe");
                    writer.WriteLine("taskkill /f /im FortniteClient-Win64-Shipping.exe");
                    writer.WriteLine("taskkill /f /im EpicGamesLauncher.exe");
                    writer.WriteLine("taskkill /f /im EasyAntiCheat.exe");
                    writer.WriteLine("taskkill /f /im BEService.exe");
                    writer.WriteLine("taskkill /f /im BEServices.exe");
                    writer.WriteLine("taskkill /f /im BattleEye.exe");
                    writer.WriteLine("taskkill /f /im x64dbg.exe");
                    writer.WriteLine("taskkill /f /im x32dbg.exe");
                    writer.WriteLine("reg delete \"HKEY_LOCAL_MACHINE\\Software\\Epic Games\" /f");
                    writer.WriteLine("reg delete \"HKEY_CURRENT_USER\\Software\\Epic Games\" /f");
                    writer.WriteLine("reg delete \"HKEY_LOCAL_MACHINE\\Software\\Epic Games\" /f");
                    writer.WriteLine("reg delete \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\com.epicgames.launcher\" /f");
                    writer.WriteLine("reg delete \"HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\EpicGames\" /f");
                    writer.WriteLine("reg delete \"HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Epic Games\" /f");
                    writer.WriteLine("reg delete \"HKEY_CLASSES_ROOT\\com.epicgames.launcher\" /f");
                    writer.WriteLine("reg delete \"HKEY_LOCAL_MACHINE\\Software\\Epic Games\" /f");
                    writer.WriteLine("reg delete \"HKEY_CURRENT_USER\\Software\\Classes\\com.epicgames.launcher\" /f");
                    writer.WriteLine("reg delete \"HKEY_CURRENT_USER\\Software\\Epic Games\\Unreal Engine\\Hardware Survey\" /f");
                    writer.WriteLine("reg delete \"HKEY_CURRENT_USER\\Software\\Epic Games\\Unreal Engine\\Identifiers\" /f");
                    writer.WriteLine("reg delete \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\com.epicgames.launcher\" /f");
                    writer.WriteLine("reg delete \"HKEY_LOCAL_MACHINE\\SOFTWARE\\EpicGames\" /f");
                    writer.WriteLine("reg delete \"HKEY_CURRENT_USER\\SOFTWARE\\EpicGames\" /f");
                    writer.WriteLine("reg delete \"HKEY_USERS\\" + WindowsIdentity.GetCurrent().User.Value + "\\Software\\Epic Games\" /f");
                }
                Process process = Process.Start(tempFilename);
                process.WaitForExit();
                File.Delete(tempFilename);
                logger.Success("All Traces Have Been Deleted If Any Were Found!");
                Console.ReadKey();
            }
            catch
            {
                logger.Error("Clean Traces Failed.. Please Try Again and or Run as Admin!");
                Console.ReadKey();
            }
        }
        private static void RunCMD(string Code)
        {
            Process process = Process.Start(new ProcessStartInfo("cmd.exe", "/c " + Code) { CreateNoWindow = true, UseShellExecute = false });
            process.WaitForExit();
            process.Close();
        }
        private static string GenerateRandomGuid()
        {
            return $"{{{RandomString(8)}-{RandomString(4)}-{RandomString(4)}-{RandomString(4)}-{RandomString(12)}}}";
        }
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Range(0, length).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
        }
    }
}
