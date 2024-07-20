using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AsmResolver.DotNet;
using AsmResolver.IO;
using Fish_Tools.core;
using Fish_Tools.core.Utils;

namespace Fish_Tools
{
    public class Program
    {
        static void Main(string[] args) { MainMenu(); }
        public static void MainMenu()
        {
            Logger Logger = new Logger();
            Console.Clear();
            Console.Title = "Fish Tools";
            Logger.PrintArt();
            Logger.WriteBarrierLine("1", "Double Counter Bypass");
            Logger.WriteBarrierLine("2", "File Clean Up");
            Logger.WriteBarrierLine("3", "Costura Decompressor");
            Logger.WriteBarrierLine("4", "Anti Dump");
            Logger.WriteBarrierLine("5", "Hide File");
            Console.Write("-> ");
            ConsoleKey choice = Console.ReadKey().Key;
            if (choice == ConsoleKey.D1) 
            {
                Console.Clear();
                Logger.PrintArt();
                Logger.Info("Insert Verify Link:");
                string code = Console.ReadLine();
                DCB.Bypass(code, Logger);
            }
            if (choice == ConsoleKey.D2)
            {
                Console.Clear();
                Logger.PrintArt();
                FCU.CleanUpDirectories(Logger);
            }
            if (choice == ConsoleKey.D3)
            {
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
                        var extractor = new CD(module);
                        extractor.Run(Logger);
                    }
                    catch (Exception e) { Logger.Error(e.Message); }
                }
                if (path.EndsWith(".compressed")) { path.ProcessCompressedFile(Logger); }
            }
            if (choice == ConsoleKey.D4)
            {
                Console.Clear();
                Logger.PrintArt();
                Console.Write("File Path -> ");
                string path = Console.ReadLine();
                var fileName = Path.GetFileNameWithoutExtension(path) + "_antidump";
                var fullFileName = fileName + Path.GetExtension(path);
                Dictionary<uint, uint> offsets = new Dictionary<uint, uint>() { { 0xD0, 0x0 }, { 0xD4, 0x0 }, { 0xB4, 0x0 }, { 0xF8, 0x0 }, { 0x34, 0x0 } };
                for (int i = 0; i < offsets.Count; i++)
                {
                    var sizeBytes = BitConverter.GetBytes(offsets.Values.ToArray()[i]);
                    Array.Copy(sizeBytes, 0, File.ReadAllBytes(path), offsets.Keys.ToArray()[i], sizeBytes.Length);
                }
                File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(path), fullFileName), File.ReadAllBytes(path));
                Logger.Success($"Completed. output: {Path.Combine(Path.GetDirectoryName(path), fullFileName)}");
                Console.ReadKey();
            }
            if (choice == ConsoleKey.D5)
            {
                Console.Clear();
                Logger.PrintArt();
                Console.Write("File Path -> ");
                string path = Console.ReadLine();

                Logger.WriteBarrierLine("1", "Hide File");
                Logger.WriteBarrierLine("2", "UnHide File");

                ConsoleKey choice2 = Console.ReadKey().Key;
                if (choice2 == ConsoleKey.D1) 
                {
                    DirectoryInfo file = new DirectoryInfo(path);
                    file.Attributes |= FileAttributes.Hidden;
                    file.Attributes |= FileAttributes.System;
                }
                if (choice2 == ConsoleKey.D2)
                {
                    DirectoryInfo file = new DirectoryInfo(path);
                    file.Attributes |= FileAttributes.Normal;
                }
                Logger.Success($"Completed");
                Console.ReadKey();
            }
            MainMenu();
        }
    }
}
