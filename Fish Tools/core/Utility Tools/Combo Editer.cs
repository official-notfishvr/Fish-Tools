/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/

using Fish_Tools.core.Utils;
using System;
using System.IO;
using System.Linq;

namespace Fish_Tools.core.MiscTools
{
    internal class ComboEditor : ITool
    {
        public string Name => "Combo Editor";
        public string Category => "Utility Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Edit and format combo lists (email:password)";

        private static bool _comboLoaded = false;
        private static string[] _combos;
        private static string _combosPath;

        public void Main(Logger logger)
        {
            ComboEditorMain(logger);
        }

        public static void ComboEditorMain(Logger logger)
        {
            Console.Clear();
            Console.Title = "Fish Tools";
            logger.PrintArt();

            if (!_comboLoaded)
            {
                logger.Info("Path to Combos:");
                _combosPath = Console.ReadLine();

                if (File.Exists(_combosPath))
                {
                    _combos = File.ReadAllLines(_combosPath);
                    _comboLoaded = true;
                    ComboEditorMain(logger);
                }
                else
                {
                    logger.Error("File not found. Please check the path and try again.");
                    Console.ReadKey();
                    return;
                }
            }
            else
            {
                Console.Clear();
                Console.Title = "Fish Tools";
                logger.PrintArt();
                logger.WriteBarrierLine("1", "Remove Spaces");
                logger.WriteBarrierLine("2", "Remove That Is Not email:pass");
                logger.WriteBarrierLine("3", "Add ':' After email");
                logger.WriteBarrierLine("4", "Remove Duplicates");
                logger.WriteBarrierLine("5", "Sort Combos");
                logger.WriteBarrierLine("6", "Count Combos");
                logger.WriteBarrierLine("7", "Show Sample Combos");
                logger.WriteBarrierLine("9", "Save Combos");
                logger.WriteBarrierLine("0", "Back");
                Console.Write("-> ");

                ConsoleKey Choice = Console.ReadKey().Key;
                switch (Choice)
                {
                    case ConsoleKey.D1:
                        _combos = _combos
                            .Select(line => line.Trim())
                            .Where(line => !string.IsNullOrWhiteSpace(line))
                            .ToArray();
                        Console.WriteLine();
                        logger.Success("Spaces removed from combos.");
                        Console.ReadKey();
                        ComboEditorMain(logger);
                        break;
                    case ConsoleKey.D2:

                        if (_combos != null) { for (int i = 0; i < _combos.Length; i++) { int index = _combos[i].IndexOf(" "); if (index != -1) { _combos[i] = _combos[i].Substring(0, index); } } }

                        Console.WriteLine();
                        logger.Success("Non email:pass parts removed and saved to output file.");
                        Console.ReadKey();
                        ComboEditorMain(logger);
                        break;
                    case ConsoleKey.D3:
                        string fileContent = File.ReadAllText(_combosPath);
                        string updatedContent = fileContent.Replace(".com", ".com:");
                        logger.Success("add ':' After email.");
                        Console.ReadKey();
                        ComboEditorMain(logger);
                        break;
                    case ConsoleKey.D4:
                        if (_combos != null)
                        {
                            int before = _combos.Length;
                            _combos = _combos.Distinct().ToArray();
                            int after = _combos.Length;
                            Console.WriteLine();
                            logger.Success($"Removed duplicates. {before - after} duplicates removed.");
                        }
                        else
                        {
                            logger.Error("No combos loaded.");
                        }
                        Console.ReadKey();
                        ComboEditorMain(logger);
                        break;
                    case ConsoleKey.D5:
                        if (_combos != null)
                        {
                            _combos = _combos.OrderBy(x => x).ToArray();
                            Console.WriteLine();
                            logger.Success("Combos sorted alphabetically.");
                        }
                        else
                        {
                            logger.Error("No combos loaded.");
                        }
                        Console.ReadKey();
                        ComboEditorMain(logger);
                        break;
                    case ConsoleKey.D6:
                        if (_combos != null)
                        {
                            logger.Info($"Total combos loaded: {_combos.Length}");
                        }
                        else
                        {
                            logger.Error("No combos loaded.");
                        }
                        Console.ReadKey();
                        ComboEditorMain(logger);
                        break;
                    case ConsoleKey.D7:
                        if (_combos != null && _combos.Length > 0)
                        {
                            logger.Info("Sample combos:");
                            int count = Math.Min(5, _combos.Length);
                            for (int i = 0; i < count; i++)
                            {
                                Console.WriteLine($"{i + 1}: {_combos[i]}");
                            }
                        }
                        else
                        {
                            logger.Error("No combos loaded.");
                        }
                        Console.ReadKey();
                        ComboEditorMain(logger);
                        break;
                    case ConsoleKey.D9:
                        if (!string.IsNullOrEmpty(_combosPath) && _combos != null)
                        {
                            string directory = Path.GetDirectoryName(_combosPath);
                            string fileName = Path.GetFileName(_combosPath);
                            string newFileName = "new_" + fileName;
                            string newFilePath = Path.Combine(directory, newFileName);

                            File.WriteAllLines(newFilePath, _combos);
                            Console.WriteLine();
                            logger.Success("Combos saved successfully.");
                            Console.ReadKey();
                            ComboEditorMain(logger);
                        }
                        else
                        {
                            logger.Error("No combos loaded or invalid path.");
                            Console.ReadKey();
                        }
                        break;
                    case ConsoleKey.D0:
                        return;
                    default:
                        logger.Warn("Invalid option selected.");
                        break;
                }
            }
        }


    }
}
