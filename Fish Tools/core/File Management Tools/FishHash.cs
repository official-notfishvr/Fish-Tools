/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using System;
using System.IO;
using System.Security.Cryptography;
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.File_Management_Tools
{
    internal class FishHash : ITool
    {
        public string Name => "Fish Hash";
        public string Category => "File Management Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Compare folders and copy files with different hashes";

        public void Main(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();
            
            Logger.Info("Fish Hash - Folder Comparison Tool");
            Logger.Info("This tool compares two folders and copies files with different hashes to a 'NotSameHash' folder.");
            
            Console.Write("Enter path to first folder: ");
            string folder1 = Console.ReadLine();
            
            Console.Write("Enter path to second folder: ");
            string folder2 = Console.ReadLine();
            
            if (string.IsNullOrEmpty(folder1) || string.IsNullOrEmpty(folder2))
            {
                Logger.Error("Invalid folder paths.");
                Logger.Info("Press any key to return to menu...");
                Console.ReadKey();
                return;
            }
            
            CompareFolders(folder1, folder2, Logger);
            
            Logger.Info("Press any key to return to menu...");
            Console.ReadKey();
        }

        public void CompareFolders(string folder1, string folder2, Logger Logger = null)
        {
            if (!Directory.Exists(folder1) || !Directory.Exists(folder2))
            {
                Logger?.Error("One or both of the directories do not exist.");
                return;
            }

            string notSameHashFolder = Path.Combine(folder1, "NotSameHash");
            Directory.CreateDirectory(notSameHashFolder);

            string[] filesFolder1 = Directory.GetFiles(folder1);

            foreach (string filePath1 in filesFolder1)
            {
                string fileName = Path.GetFileName(filePath1);
                string filePath2 = Path.Combine(folder2, fileName);
                if (File.Exists(filePath2))
                {
                    string hash1 = GetFileHash(filePath1);
                    string hash2 = GetFileHash(filePath2);

                    if (hash1 != hash2)
                    {
                        Logger?.Info($"File {fileName} has a different hash. Copying to NotSameHash folder.");
                        File.Copy(filePath1, Path.Combine(notSameHashFolder, fileName), true);
                    }
                }
                else
                {
                    Logger?.Info($"File {fileName} does not exist in {folder2}. Copying to NotSameHash folder.");
                    File.Copy(filePath1, Path.Combine(notSameHashFolder, fileName), true);
                }
            }
            
            Logger?.Success("Folder comparison completed!");
        }

        private string GetFileHash(string filePath)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    byte[] fileHash = sha256.ComputeHash(fileStream);
                    return BitConverter.ToString(fileHash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
