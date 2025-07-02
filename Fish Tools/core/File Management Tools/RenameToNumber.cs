/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.File_Management_Tools
{
    internal class RenameToNumber : ITool
    {
        public string Name => "Rename To Number";
        public string Category => "File Management Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Rename files to sequential numbers in directories";

        public void Main(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();
            
            Logger.Info("Choose renaming method:");
            Logger.WriteBarrierLine("1", "Rename all files globally with sequential numbers");
            Logger.WriteBarrierLine("2", "Rename files in each folder separately (each folder starts from 1)");
            Logger.WriteBarrierLine("0", "Back");
            
            Console.Write("-> ");
            var input = Console.ReadKey();
            ConsoleKey choice = input.Key;
            Console.WriteLine();
            
            if (choice == ConsoleKey.D0)
            {
                return;
            }
            
            Console.Write("Enter the root directory path: ");
            string rootPath = Console.ReadLine();

            if (choice == ConsoleKey.D1)
            {
                RenameAllFilesToNumbers(rootPath, Logger);
            }
            else if (choice == ConsoleKey.D2)
            {
                RenameFilesInEachFolderSeparately(rootPath, Logger);
            }
            else
            {
                Logger.Error("Invalid choice.");
            }
        }

        public static void RenameAllFilesToNumbers(string rootPath, Logger Logger = null)
        {
            if (!Directory.Exists(rootPath))
            {
                Logger?.Error($"Directory does not exist: {rootPath}");
                return;
            }

            try
            {
                var allFiles = Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories)
                                      .OrderBy(f => f)
                                      .ToList();

                Logger?.Info($"Found {allFiles.Count} files total.");

                var filesToRename = allFiles.Where(file => !StartsWithNumber(Path.GetFileNameWithoutExtension(file)))
                                          .ToList();

                Logger?.Info($"Found {filesToRename.Count} files to rename (excluding files that already start with numbers).");

                for (int i = 0; i < filesToRename.Count; i++)
                {
                    string currentFile = filesToRename[i];
                    string directory = Path.GetDirectoryName(currentFile);
                    string extension = Path.GetExtension(currentFile);

                    string newFileName = $"{i + 1}{extension}";
                    string newFilePath = Path.Combine(directory, newFileName);

                    int counter = 1;
                    while (File.Exists(newFilePath) && newFilePath != currentFile)
                    {
                        newFileName = $"{i + 1}_{counter}{extension}";
                        newFilePath = Path.Combine(directory, newFileName);
                        counter++;
                    }

                    if (currentFile != newFilePath)
                    {
                        File.Move(currentFile, newFilePath);
                        Logger?.Success($"Renamed: {Path.GetFileName(currentFile)} -> {newFileName}");
                    }
                }

                Logger?.Success("File renaming completed successfully!");
            }
            catch (Exception ex)
            {
                Logger?.Error($"Error occurred: {ex.Message}");
            }
        }

        public static void RenameFilesInEachFolderSeparately(string rootPath, Logger Logger = null)
        {
            if (!Directory.Exists(rootPath))
            {
                Logger?.Error($"Directory does not exist: {rootPath}");
                return;
            }

            try
            {
                RenameFilesInDirectory(rootPath, Logger);
                Logger?.Success("File renaming completed successfully!");
            }
            catch (Exception ex)
            {
                Logger?.Error($"Error occurred: {ex.Message}");
            }
        }

        private static void RenameFilesInDirectory(string directoryPath, Logger Logger = null)
        {
            var allFiles = Directory.GetFiles(directoryPath)
                                  .OrderBy(f => f)
                                  .ToList();

            var filesToRename = allFiles.Where(file => !StartsWithNumber(Path.GetFileNameWithoutExtension(file)))
                                      .ToList();

            Logger?.Info($"Found {filesToRename.Count} files to rename in {directoryPath} (excluding files that already start with numbers).");

            for (int i = 0; i < filesToRename.Count; i++)
            {
                string currentFile = filesToRename[i];
                string directory = Path.GetDirectoryName(currentFile);
                string extension = Path.GetExtension(currentFile);

                string newFileName = $"{i + 1}{extension}";
                string newFilePath = Path.Combine(directory, newFileName);

                int counter = 1;
                while (File.Exists(newFilePath) && newFilePath != currentFile)
                {
                    newFileName = $"{i + 1}_{counter}{extension}";
                    newFilePath = Path.Combine(directory, newFileName);
                    counter++;
                }

                if (currentFile != newFilePath)
                {
                    File.Move(currentFile, newFilePath);
                    Logger?.Success($"Renamed: {Path.GetFileName(currentFile)} -> {newFileName} in {directoryPath}");
                }
            }

            var subdirectories = Directory.GetDirectories(directoryPath);
            foreach (string subdir in subdirectories)
            {
                RenameFilesInDirectory(subdir, Logger);
            }
        }

        private static bool StartsWithNumber(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return false;

            return char.IsDigit(filename[0]);
        }

        public static void Main()
        {
            Console.WriteLine("Choose renaming method:");
            Console.WriteLine("1. Rename all files globally with sequential numbers");
            Console.WriteLine("2. Rename files in each folder separately (each folder starts from 1)");

            string choice = Console.ReadLine();

            Console.Write("Enter the root directory path: ");
            string rootPath = Console.ReadLine();

            if (choice == "1")
            {
                RenameAllFilesToNumbers(rootPath);
            }
            else if (choice == "2")
            {
                RenameFilesInEachFolderSeparately(rootPath);
            }
            else
            {
                Console.WriteLine("Invalid choice.");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}