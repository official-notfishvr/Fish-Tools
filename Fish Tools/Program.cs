/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.Utils;
using AsmResolver.DotNet;
using System.Runtime.InteropServices;
using Fish_Tools.core.FileManagementTools;
using Fish_Tools.core.BypassTools;
using Fish_Tools.core.DiscordTools;
using Fish_Tools.core.MiscTools;
using Fish_Tools.core.MiscTools.AccountChecker;

namespace Fish_Tools
{
    internal static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        public static bool ApplicationConfigurationInitialize = false;


        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AllocConsole();
            MainMenu();
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
        public static void MainMenu()
        {
            string dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataDirectory)) { Directory.CreateDirectory(dataDirectory); }
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
                            linkvertise.Bypass(new Uri("https://linkvertise.com/106636/XRayUpdate"), Logger);
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
                                    var module = ModuleDefinition.FromFile(path);
                                    var extractor = new CosturaDecompressor(module);
                                    extractor.Run(Logger);
                                }
                                catch (Exception e) { Logger.Error(e.Message); }
                            }
                            if (path.EndsWith(".compressed")) { path.ProcessCompressedFile(Logger); }
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
                    //DiscordTools.DiscordToolsMenu(Logger);
                    Remove_Stuff.doit();
                    break;
                case ConsoleKey.D4:
                    Console.Clear();
                    Logger.PrintArt();
                    Logger.WriteBarrierLine("1", "Auto Clicker");
                    Logger.WriteBarrierLine("2", "Account Checker");
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
                        case ConsoleKey.D0:
                            Console.Clear();
                            MainMenu();
                            break;
                    }
                    break;
            }
            MainMenu();
        }
    }
}