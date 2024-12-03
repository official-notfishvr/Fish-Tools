/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.Utils;
using Fish_Tools.core.FileManagementTools;
using Fish_Tools.core.BypassTools;
using Fish_Tools.core.DiscordTools;
using Fish_Tools.core.MiscTools;
using Fish_Tools.core.MiscTools.AccountChecker;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Fish_Tools
{
    internal static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        public static bool ApplicationConfigurationInitialize = false;
        public static bool IsTitleUpdated = false;
        private static string CurrentVersion = "2.4";
        private static readonly string VersionUrl = "https://raw.githubusercontent.com/official-notfishvr/Fish-Tools/refs/heads/main/version";
        public static string dataDir = "";
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        async static Task Main()
        {
            AllocConsole();
            await CheckForUpdates();
            MainMenu();
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
        public static void MainMenu()
        {
            if (!IsTitleUpdated)
            {
                Utils.NewThread(Update);
                IsTitleUpdated = true;
            }

            string dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataDirectory)) { Directory.CreateDirectory(dataDirectory); }
            dataDir = dataDirectory;
            string ResultDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Result");
            if (!Directory.Exists(ResultDirectory)) { Directory.CreateDirectory(ResultDirectory); }

            Logger Logger = new Logger();
            Console.Clear();
            Console.Title = "Fish Tools";
            Logger.PrintArt();

            Logger.WriteBarrierLine("1", "Bypass Tools");
            Logger.WriteBarrierLine("2", "File Management Tools");
            Logger.WriteBarrierLine("3", "Discord Tools");
            Logger.WriteBarrierLine("4", "Misc Tools");

            Console.Write("Select a category -> ");
            ConsoleKey categoryChoice = Console.ReadKey().Key;

            switch (categoryChoice)
            {
                case ConsoleKey.D1:
                    Console.Clear();
                    Logger.PrintArt();
                    Logger.WriteBarrierLine("1", "Double Counter Bypass");
                    Logger.WriteBarrierLine("2", "Linkvertise Bypass");
                    Logger.WriteBarrierLine("0", "Back");
                    Console.Write("-> ");
                    ConsoleKey bypassChoice = Console.ReadKey().Key;
                    switch (bypassChoice)
                    {
                        case ConsoleKey.D1:
                            Console.Clear();
                            Logger.PrintArt();
                            Logger.Info("Insert Verify Link:");
                            Console.Write("-> ");
                            string code = Console.ReadLine();
                            DoubleCounterBypass.Bypass(code, Logger);
                            break;
                        case ConsoleKey.D2:
                            Console.Clear();
                            Logger.PrintArt();
                            Logger.Info("Insert Linkvertise URL:");
                            Console.Write("-> ");
                            string url = Console.ReadLine();
                            LinkvertiseBypass linkvertise = new LinkvertiseBypass();
                            linkvertise.Bypass(new Uri(url), Logger);
                            Console.ReadKey();
                            break;
                        case ConsoleKey.D0:
                            Console.Clear();
                            MainMenu();
                            break;
                    }
                    break;
                case ConsoleKey.D2:
                    Console.Clear();
                    Logger.PrintArt();
                    Logger.WriteBarrierLine("1", "Windows Cleaner");
                    Logger.WriteBarrierLine("2", "Costura Decompressor");
                    Logger.WriteBarrierLine("3", "Anti Dump");
                    Logger.WriteBarrierLine("4", "Hide/Unhide File");
                    Logger.WriteBarrierLine("0", "Back");

                    Console.Write("-> ");
                    ConsoleKey fileMgmtChoice = Console.ReadKey().Key;
                    switch (fileMgmtChoice)
                    {
                        case ConsoleKey.D1:
                            Console.Clear();
                            Logger.PrintArt();
                            WindowsCleaner.WindowsCleanerMain(Logger);
                            break;
                        case ConsoleKey.D2:
                            Console.Clear();
                            Logger.PrintArt();
                            Console.Write("File Path -> ");
                            string path = Console.ReadLine();
                            if (Path.GetExtension(path) == ".exe" || Path.GetExtension(path) == ".dll")
                            {
                                Logger.Info($"Processing executable: {Path.GetFileName(path)}");
                                try
                                {
                                    var assembly = Assembly.LoadFrom(path);
                                    var extractor = new CosturaDecompressor(assembly);
                                    extractor.Run(Logger);
                                }
                                catch (Exception e)
                                {
                                    Logger.Error(e.Message);
                                }
                            }
                            if (path.EndsWith(".compressed"))
                            {
                                path.ProcessCompressedFile(Logger);
                            }
                            break;
                        case ConsoleKey.D3:
                            Console.Clear();
                            Logger.PrintArt();
                            Console.Write("File Path -> ");
                            string antiDumpPath = Console.ReadLine();
                            var fileName = Path.GetFileNameWithoutExtension(antiDumpPath) + "_antidump";
                            var fullFileName = fileName + Path.GetExtension(antiDumpPath);
                            Dictionary<uint, uint> offsets = new Dictionary<uint, uint>() { { 0xD0, 0x0 }, { 0xD4, 0x0 }, { 0xB4, 0x0 }, { 0xF8, 0x0 }, { 0x34, 0x0 } };
                            for (int i = 0; i < offsets.Count; i++)
                            {
                                var sizeBytes = BitConverter.GetBytes(offsets.Values.ToArray()[i]);
                                Array.Copy(sizeBytes, 0, File.ReadAllBytes(antiDumpPath), offsets.Keys.ToArray()[i], sizeBytes.Length);
                            }
                            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(antiDumpPath), fullFileName), File.ReadAllBytes(antiDumpPath));
                            Logger.Success($"Completed. output: {Path.Combine(Path.GetDirectoryName(antiDumpPath), fullFileName)}");
                            Console.ReadKey();
                            break;
                        case ConsoleKey.D4:
                            Console.Clear();
                            Logger.PrintArt();
                            Console.Write("File Path -> ");
                            string hideFilePath = Console.ReadLine();

                            Logger.WriteBarrierLine("1", "Hide File");
                            Logger.WriteBarrierLine("2", "UnHide File");
                            Console.Write("-> ");
                            ConsoleKey hideChoice = Console.ReadKey().Key;
                            if (hideChoice == ConsoleKey.D1)
                            {
                                DirectoryInfo file = new DirectoryInfo(hideFilePath);
                                file.Attributes |= FileAttributes.Hidden;
                                file.Attributes |= FileAttributes.System;
                            }
                            if (hideChoice == ConsoleKey.D2)
                            {
                                DirectoryInfo file = new DirectoryInfo(hideFilePath);
                                file.Attributes &= ~FileAttributes.Hidden;
                                file.Attributes &= ~FileAttributes.System;
                            }
                            Logger.Success($"Completed");
                            Console.ReadKey();
                            break;
                        case ConsoleKey.D0:
                            Console.Clear();
                            MainMenu();
                            break;
                    }
                    break;
                case ConsoleKey.D3:
                    Console.Clear();
                    Logger.PrintArt();
                    DiscordTools.DiscordToolsMenu(Logger);
                    break;
                case ConsoleKey.D4:
                    Console.Clear();
                    Logger.PrintArt();
                    Logger.WriteBarrierLine("1", "Auto Clicker");
                    Logger.WriteBarrierLine("2", "Account Checker");
                    Logger.WriteBarrierLine("3", "Combo Editer");
                    Logger.WriteBarrierLine("4", "Extract Emails");
                    Logger.WriteBarrierLine("0", "Back");
                    Console.Write("-> ");
                    ConsoleKey miscChoice = Console.ReadKey().Key;
                    switch (miscChoice)
                    {
                        case ConsoleKey.D1:
                            if (ApplicationConfigurationInitialize != true)
                            {
                                ApplicationConfiguration.Initialize();
                                ApplicationConfigurationInitialize = true;
                            }
                            Application.Run(new Auto_Clicker());
                            break;
                        case ConsoleKey.D2:
                            AccountChecker.Main(Logger);
                            break;
                        case ConsoleKey.D3:
                            Console.Clear();
                            Logger.PrintArt();
                            ComboEditor.Main(Logger);
                            break;
                        case ConsoleKey.D4:
                            Console.Clear();
                            Logger.PrintArt();
                            ExtractEmails.ExtractEmailsMain(Logger);
                            break;
                        case ConsoleKey.D0:
                            Console.Clear();
                            MainMenu();
                            break;
                    }
                    break;
            }
            MainMenu();
        }
        public static void Update()
        {
            int counter = 0;
            bool direction = true;

            string title = $"{Environment.UserName.ToLower()}@fish~tools$";
            while (IsTitleUpdated)
            {
                Thread.Sleep(105);

                if (counter == title.Length) { direction = false; }
                if (counter == 0) { direction = true; }

                counter = direction ? ++counter : --counter;
                string newTitle = counter == title.Length ? title[..counter] : string.Concat(title.AsSpan(0, counter), "_");
                Console.Title = newTitle;
                Utils.Wait(135);
            }
        }
        public async static Task CheckForUpdates() // not using Logger bc it gets made in MainMenu and i dont feel like moving stuff
        {
            try
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string exePath = Path.Combine(baseDirectory, "Fish Tools.exe");
                string dllPath = Path.Combine(baseDirectory, "Fish Tools.dll");
                string winRtDllPath = Path.Combine(baseDirectory, "WinRT.Runtime.dll");

                string renamedExePath = Path.Combine(baseDirectory, "Fish Tools.old.exe");
                string renamedDllPath = Path.Combine(baseDirectory, "Fish Tools.old.dll");
                string renamedWinRtDllPath = Path.Combine(baseDirectory, "WinRT.Runtime.old.dll");
                string newFileZipName = "Fish-Tools-Update.zip";

                using (WebClient client = new WebClient())
                {
                    Console.WriteLine("Checking for updates...");
                    string remoteVersion = await client.DownloadStringTaskAsync(VersionUrl);

                    if (!remoteVersion.Contains(CurrentVersion))
                    {
                        Console.Clear();
                        Console.WriteLine($"A new version is available: {remoteVersion.Trim()} (Current: {CurrentVersion})");

                        if (File.Exists(exePath)) File.Move(exePath, renamedExePath, true);
                        if (File.Exists(dllPath)) File.Move(dllPath, renamedDllPath, true);
                        if (File.Exists(winRtDllPath)) File.Move(winRtDllPath, renamedWinRtDllPath, true);

                        Console.WriteLine("Downloading the latest version...");
                        string latestDownloadUrl = $"https://github.com/official-notfishvr/Fish-Tools/releases/latest/download/Fish-Tools-V{remoteVersion.Trim()}.zip";
                        string tempZipPath = Path.Combine(baseDirectory, newFileZipName);

                        try
                        {
                            await client.DownloadFileTaskAsync(new Uri(latestDownloadUrl), tempZipPath);
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Failed to download the update. URL: {latestDownloadUrl}", e);
                        }

                        Console.WriteLine("Download complete! Extracting files...");
                        System.IO.Compression.ZipFile.ExtractToDirectory(tempZipPath, baseDirectory, true);
                        string batFilePath = Path.Combine(baseDirectory, "updateAndRestart.bat");
                        string batContent = @$"
@echo off
timeout /t 2 /nobreak
del ""{renamedExePath}""
del ""{renamedDllPath}""
del ""{renamedWinRtDllPath}""
del ""{tempZipPath}""
start ""{exePath}""
exit
";
                        File.WriteAllText(batFilePath, batContent);
                        System.Diagnostics.Process.Start(batFilePath);
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine($"You are using the latest version: {CurrentVersion}");
                    }
                }
            }
            catch (WebException webEx) when (webEx.Response is HttpWebResponse response)
            {
                Console.WriteLine($"Error checking for updates: {response.StatusCode} - {webEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for updates: {ex.Message}");
            }
        }
    }
}