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
    internal class ComboEditor
    {
        private static bool _comboLoaded = false;
        private static string[] _combos;
        private static string _combosPath;

        public static void Main(Logger logger)
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
                    Main(logger);
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
                logger.WriteBarrierLine("0", "Save Combos");
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
                        Main(logger);
                        break;
                    case ConsoleKey.D2:

                        if (_combos != null) { for (int i = 0; i < _combos.Length; i++) { int index = _combos[i].IndexOf(" "); if (index != -1) { _combos[i] = _combos[i].Substring(0, index); } } }

                        Console.WriteLine();
                        logger.Success("Non email:pass parts removed and saved to output file.");
                        Console.ReadKey();
                        Main(logger);
                        break;
                    case ConsoleKey.D3:
                        string fileContent = File.ReadAllText(_combosPath);
                        string updatedContent = fileContent.Replace(".com", ".com:");
                        logger.Success("add ':' After email.");
                        Console.ReadKey();
                        Main(logger);
                        break;
                    case ConsoleKey.D0:
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
                            Main(logger);
                        }
                        else
                        {
                            logger.Error("No combos loaded or invalid path.");
                            Console.ReadKey();
                        }
                        break;
                    default:
                        logger.Warn("Invalid option selected.");
                        break;
                }
            }
        }
    }
}
