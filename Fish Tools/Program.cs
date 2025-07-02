/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.Utils;
using Fish_Tools.core.FileManagementTools;
//using Fish_Tools.core.BypassTools;
using Fish_Tools.core.DiscordTools;
using Fish_Tools.core.MiscTools;
using Fish_Tools.core.MiscTools.AccountChecker;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using Fish_Tools.core.Misc_Tools;
using Fish_Tools.core.File_Management_Tools;
using System.Linq;

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

            ToolManager.Initialize();

            MainMenu();

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
            while (true)
            {
                Console.Clear();
                Console.Title = "Fish Tools";
                Logger.PrintArt();

                var categories = ToolManager.GetCategoriesOrdered();

                for (int i = 0; i < categories.Count; i++)
                {
                    string displayName = ToolManager.GetCategoryDisplayName(categories[i]);
                    Logger.WriteBarrierLine((i + 1).ToString(), displayName);
                }

                Logger.WriteBarrierLine("M", "Manage Tools");
                Logger.WriteBarrierLine("Q", "Quit");

                Console.Write("Select a category -> ");
                var keyInfo = Console.ReadKey();
                var input = keyInfo.KeyChar.ToString().ToUpper();

                if (input == "Q")
                {
                    Console.WriteLine();
                    Logger.Info("Exiting application. Goodbye!");
                    Thread.Sleep(1000);
                    Environment.Exit(0);
                }
                else if (input == "M")
                {
                    Console.WriteLine();
                    ManageToolsMenu(Logger);
                }
                else if (char.IsDigit(keyInfo.KeyChar))
                {
                    var restOfInput = "";
                    var startTime = DateTime.Now;

                    while ((DateTime.Now - startTime).TotalMilliseconds < 500)
                    {
                        if (Console.KeyAvailable)
                        {
                            var nextKey = Console.ReadKey(true);
                            if (char.IsDigit(nextKey.KeyChar))
                            {
                                restOfInput += nextKey.KeyChar;
                                Console.Write(nextKey.KeyChar);
                                startTime = DateTime.Now;
                            }
                            else if (nextKey.Key == ConsoleKey.Enter)
                            {
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        Thread.Sleep(10);
                    }

                    var fullInput = input + restOfInput;
                    Console.WriteLine();

                    if (int.TryParse(fullInput, out int selection) && selection >= 1 && selection <= categories.Count)
                    {
                        string selectedCategory = categories[selection - 1];
                        CategoryMenu(Logger, selectedCategory);
                    }
                    else
                    {
                        Logger.Error("Invalid selection. Please try again.");
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    Console.WriteLine();
                    Logger.Error("Invalid selection. Please try again.");
                    Thread.Sleep(1000);
                }
            }
        }
        private static void CategoryMenu(Logger Logger, string category)
        {
            while (true)
            {
                Console.Clear();
                Logger.PrintArt();

                var tools = ToolManager.GetEnabledToolsByCategory(category);

                if (tools.Count == 0)
                {
                    Logger.Warn($"No enabled tools found in {category}");
                    Logger.Info("Press any key to return to main menu...");
                    Console.ReadKey();
                    return;
                }

                for (int i = 0; i < tools.Count; i++)
                {
                    Logger.WriteBarrierLine((i + 1).ToString(), tools[i].Name);
                }

                Logger.WriteBarrierLine("0", "Back");
                Console.Write("-> ");
                var keyInfo = Console.ReadKey();
                var input = keyInfo.KeyChar.ToString();

                if (char.IsDigit(keyInfo.KeyChar))
                {
                    var restOfInput = "";
                    var startTime = DateTime.Now;

                    while ((DateTime.Now - startTime).TotalMilliseconds < 500)
                    {
                        if (Console.KeyAvailable)
                        {
                            var nextKey = Console.ReadKey(true);
                            if (char.IsDigit(nextKey.KeyChar))
                            {
                                restOfInput += nextKey.KeyChar;
                                Console.Write(nextKey.KeyChar);
                                startTime = DateTime.Now;
                            }
                            else if (nextKey.Key == ConsoleKey.Enter)
                            {
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        Thread.Sleep(10);
                    }

                    var fullInput = input + restOfInput;
                    Console.WriteLine();

                    if (fullInput == "0")
                    {
                        return;
                    }
                    else if (int.TryParse(fullInput, out int selection) && selection >= 1 && selection <= tools.Count)
                    {
                        var selectedTool = tools[selection - 1];
                        try
                        {
                            if (ToolManager.IsFormTool(selectedTool))
                            {
                                if (ApplicationConfigurationInitialize != true)
                                {
                                    ApplicationConfiguration.Initialize();
                                    ApplicationConfigurationInitialize = true;
                                }
                                selectedTool.Main(Logger);
                            }
                            else
                            {
                                selectedTool.Main(Logger);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Error running {selectedTool.Name}: {ex.Message}");
                            Logger.Info("Press any key to continue...");
                            Console.ReadKey();
                        }
                    }
                    else
                    {
                        Logger.Error("Invalid selection. Please try again.");
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    Logger.Error("Invalid selection. Please try again.");
                    Thread.Sleep(1000);
                }
            }
        }
        private static void ManageToolsMenu(Logger Logger)
        {
            while (true)
            {
                Console.Clear();
                Logger.PrintArt();

                Logger.WriteBarrierLine("1", "View All Tools");
                Logger.WriteBarrierLine("2", "Enable/Disable Tool");
                Logger.WriteBarrierLine("3", "Enable/Disable Category");
                Logger.WriteBarrierLine("4", "Enable All Tools");
                Logger.WriteBarrierLine("5", "Disable All Tools");
                Logger.WriteBarrierLine("0", "Back");

                Console.Write("-> ");
                var keyInfo = Console.ReadKey();
                var input = keyInfo.KeyChar.ToString();
                Console.WriteLine();

                switch (input)
                {
                    case "1":
                        ViewAllTools(Logger);
                        break;
                    case "2":
                        EnableDisableTool(Logger);
                        break;
                    case "3":
                        EnableDisableCategory(Logger);
                        break;
                    case "4":
                        EnableAllTools(Logger);
                        break;
                    case "5":
                        DisableAllTools(Logger);
                        break;
                    case "0":
                        return;
                    default:
                        Logger.Error("Invalid selection. Please try again.");
                        Thread.Sleep(1000);
                        break;
                }
            }
        }
        private static void ViewAllTools(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();

            var categories = ToolManager.GetCategoriesOrdered();
            foreach (var category in categories)
            {
                string displayName = ToolManager.GetCategoryDisplayName(category);
                Logger.Info($"=== {displayName} ===");
                var tools = ToolManager.GetToolsByCategory(category);
                foreach (var tool in tools)
                {
                    string status = tool.IsEnabled ? "[ENABLED]" : "[DISABLED]";
                    Logger.Write($"{status} {tool.Name}");
                    if (!string.IsNullOrEmpty(tool.Description))
                    {
                        Logger.Write($" - {tool.Description}");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }

            Logger.Info("Press any key to return to management menu...");
            Console.ReadKey();
        }
        private static void EnableDisableTool(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();

            var allTools = ToolManager.GetAllTools();
            if (allTools.Count == 0)
            {
                Logger.Warn("No tools found.");
                Logger.Info("Press any key to return to management menu...");
                Console.ReadKey();
                return;
            }

            Logger.Info("Available tools:");
            for (int i = 0; i < allTools.Count; i++)
            {
                string status = allTools[i].IsEnabled ? "[ENABLED]" : "[DISABLED]";
                Logger.WriteBarrierLine((i + 1).ToString(), $"{status} {allTools[i].Name} ({allTools[i].Category})");
            }

            Logger.WriteBarrierLine("0", "Back");
            Console.Write("Select tool to toggle -> ");
            var keyInfo = Console.ReadKey();
            var input = keyInfo.KeyChar.ToString();

            if (char.IsDigit(keyInfo.KeyChar))
            {
                var restOfInput = "";
                var startTime = DateTime.Now;

                while ((DateTime.Now - startTime).TotalMilliseconds < 500)
                {
                    if (Console.KeyAvailable)
                    {
                        var nextKey = Console.ReadKey(true);
                        if (char.IsDigit(nextKey.KeyChar))
                        {
                            restOfInput += nextKey.KeyChar;
                            Console.Write(nextKey.KeyChar);
                            startTime = DateTime.Now;
                        }
                        else if (nextKey.Key == ConsoleKey.Enter)
                        {
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    Thread.Sleep(10);
                }

                var fullInput = input + restOfInput;
                Console.WriteLine();

                if (fullInput == "0")
                {
                    return;
                }
                else if (int.TryParse(fullInput, out int selection) && selection >= 1 && selection <= allTools.Count)
                {
                    var tool = allTools[selection - 1];
                    tool.IsEnabled = !tool.IsEnabled;
                    ToolManager.SetToolEnabled(tool.Name, tool.IsEnabled);
                    string status = tool.IsEnabled ? "enabled" : "disabled";
                    Logger.Success($"{tool.Name} has been {status}.");
                }
                else
                {
                    Logger.Error("Invalid selection.");
                }
            }
            else
            {
                Logger.Error("Invalid selection.");
            }

            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }
        private static void EnableDisableCategory(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();

            var categories = ToolManager.GetCategoriesOrdered();
            if (categories.Count == 0)
            {
                Logger.Warn("No categories found.");
                Logger.Info("Press any key to return to management menu...");
                Console.ReadKey();
                return;
            }

            Logger.Info("Available categories:");
            for (int i = 0; i < categories.Count; i++)
            {
                var tools = ToolManager.GetToolsByCategory(categories[i]);
                int enabledCount = tools.Count(t => t.IsEnabled);
                string displayName = ToolManager.GetCategoryDisplayName(categories[i]);
                Logger.WriteBarrierLine((i + 1).ToString(), $"{displayName} ({enabledCount}/{tools.Count} enabled)");
            }

            Logger.WriteBarrierLine("0", "Back");
            Console.Write("Select category to toggle -> ");
            var keyInfo = Console.ReadKey();
            var input = keyInfo.KeyChar.ToString();

            if (char.IsDigit(keyInfo.KeyChar))
            {
                var restOfInput = "";
                var startTime = DateTime.Now;

                while ((DateTime.Now - startTime).TotalMilliseconds < 500)
                {
                    if (Console.KeyAvailable)
                    {
                        var nextKey = Console.ReadKey(true);
                        if (char.IsDigit(nextKey.KeyChar))
                        {
                            restOfInput += nextKey.KeyChar;
                            Console.Write(nextKey.KeyChar);
                            startTime = DateTime.Now;
                        }
                        else if (nextKey.Key == ConsoleKey.Enter)
                        {
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    Thread.Sleep(10);
                }

                var fullInput = input + restOfInput;
                Console.WriteLine();

                if (fullInput == "0")
                {
                    return;
                }
                else if (int.TryParse(fullInput, out int selection) && selection >= 1 && selection <= categories.Count)
                {
                    string category = categories[selection - 1];
                    var tools = ToolManager.GetToolsByCategory(category);
                    bool allEnabled = tools.All(t => t.IsEnabled);

                    ToolManager.SetCategoryEnabled(category, !allEnabled);

                    string status = allEnabled ? "disabled" : "enabled";
                    Logger.Success($"All tools in {category} have been {status}.");
                }
                else
                {
                    Logger.Error("Invalid selection.");
                }
            }
            else
            {
                Logger.Error("Invalid selection.");
            }

            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }
        private static void EnableAllTools(Logger Logger)
        {
            var allTools = ToolManager.GetAllTools();
            foreach (var tool in allTools)
            {
                ToolManager.SetToolEnabled(tool.Name, true);
            }
            Logger.Success("All tools have been enabled.");
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }
        private static void DisableAllTools(Logger Logger)
        {
            var allTools = ToolManager.GetAllTools();
            foreach (var tool in allTools)
            {
                ToolManager.SetToolEnabled(tool.Name, false);
            }
            Logger.Success("All tools have been disabled.");
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
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
        public async static Task CheckForUpdates()
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