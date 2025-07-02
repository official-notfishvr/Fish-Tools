using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.File_Management_Tools
{
    internal class DuplicateFinder : ITool
    {
        public string Name => "Duplicate Finder";
        public string Category => "File Management Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Find and manage duplicate files in directories";

        public void Main(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();
            
            Logger.Info("Duplicate Finder - File Duplicate Detection Tool");
            Logger.Info("This tool finds duplicate files in a directory and allows you to manage them.");
            
            Console.Write("Enter directory path to scan: ");
            string directoryPath = Console.ReadLine();
            
            if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            {
                Logger.Error("Invalid directory path specified.");
                Logger.Info("Press any key to return to menu...");
                Console.ReadKey();
                return;
            }
            
            Console.Write("Include subdirectories? (y/n, default y): ");
            string includeSubdirs = Console.ReadLine();
            bool recursive = string.IsNullOrEmpty(includeSubdirs) || includeSubdirs.ToLower() == "y";
            
            Logger.Info($"Scanning directory: {directoryPath}");
            if (recursive) Logger.Info("Including subdirectories...");
            
            var duplicates = FindDuplicates(directoryPath, recursive, Logger);
            
            if (duplicates.Count > 0)
            {
                DisplayDuplicates(duplicates, Logger);
                ManageDuplicates(duplicates, Logger);
            }
            else
            {
                Logger.Success("No duplicate files found!");
            }
            
            Logger.Info("Press any key to return to menu...");
            Console.ReadKey();
        }

        private Dictionary<string, List<string>> FindDuplicates(string directoryPath, bool recursive, Logger Logger)
        {
            var fileHashes = new Dictionary<string, List<string>>();
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            
            try
            {
                var files = Directory.GetFiles(directoryPath, "*.*", searchOption);
                Logger.Info($"Found {files.Length} files to analyze...");
                
                int processed = 0;
                foreach (var file in files)
                {
                    try
                    {
                        string hash = CalculateFileHash(file);
                        
                        if (!fileHashes.ContainsKey(hash))
                        {
                            fileHashes[hash] = new List<string>();
                        }
                        fileHashes[hash].Add(file);
                        
                        processed++;
                        if (processed % 100 == 0)
                        {
                            Logger.Write($"Processed {processed}/{files.Length} files...");
                            Console.WriteLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"Could not process file {file}: {ex.Message}");
                    }
                }
                
                Logger.Success($"Analysis complete! Processed {processed} files.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error scanning directory: {ex.Message}");
                return new Dictionary<string, List<string>>();
            }
            
            return fileHashes.Where(kvp => kvp.Value.Count > 1)
                           .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private string CalculateFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private void DisplayDuplicates(Dictionary<string, List<string>> duplicates, Logger Logger)
        {
            Logger.Success($"Found {duplicates.Count} groups of duplicate files:");
            Console.WriteLine();
            
            int groupIndex = 1;
            foreach (var group in duplicates)
            {
                Logger.Info($"Group {groupIndex}:");
                long fileSize = new FileInfo(group.Value[0]).Length;
                Logger.Write($"  Size: {FormatFileSize(fileSize)} | Count: {group.Value.Count} files");
                Console.WriteLine();
                
                foreach (var file in group.Value)
                {
                    Logger.Write($"    {file}");
                    Console.WriteLine();
                }
                Console.WriteLine();
                groupIndex++;
            }
        }

        private void ManageDuplicates(Dictionary<string, List<string>> duplicates, Logger Logger)
        {
            Logger.Info("Duplicate Management Options:");
            Logger.WriteBarrierLine("1", "Delete all duplicates (keep first file)");
            Logger.WriteBarrierLine("2", "Move duplicates to separate folder");
            Logger.WriteBarrierLine("3", "Generate report only");
            Logger.WriteBarrierLine("0", "Skip management");
            
            Console.Write("Select option: ");
            var input = Console.ReadKey();
            Console.WriteLine();
            
            switch (input.Key)
            {
                case ConsoleKey.D1:
                    DeleteDuplicates(duplicates, Logger);
                    break;
                case ConsoleKey.D2:
                    MoveDuplicates(duplicates, Logger);
                    break;
                case ConsoleKey.D3:
                    GenerateReport(duplicates, Logger);
                    break;
                case ConsoleKey.D0:
                    Logger.Info("Skipping duplicate management.");
                    break;
                default:
                    Logger.Error("Invalid option selected.");
                    break;
            }
        }

        private void DeleteDuplicates(Dictionary<string, List<string>> duplicates, Logger Logger)
        {
            Logger.Warn("This will delete all duplicate files, keeping only the first file in each group.");
            Console.Write("Are you sure? (y/n): ");
            string confirm = Console.ReadLine();
            
            if (confirm?.ToLower() != "y")
            {
                Logger.Info("Operation cancelled.");
                return;
            }
            
            int deletedCount = 0;
            foreach (var group in duplicates.Values)
            {
                for (int i = 1; i < group.Count; i++)
                {
                    try
                    {
                        File.Delete(group[i]);
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Could not delete {group[i]}: {ex.Message}");
                    }
                }
            }
            
            Logger.Success($"Successfully deleted {deletedCount} duplicate files.");
        }

        private void MoveDuplicates(Dictionary<string, List<string>> duplicates, Logger Logger)
        {
            Console.Write("Enter destination folder for duplicates: ");
            string destFolder = Console.ReadLine();
            
            if (string.IsNullOrEmpty(destFolder))
            {
                Logger.Error("Invalid destination folder.");
                return;
            }
            
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }
            
            int movedCount = 0;
            foreach (var group in duplicates.Values)
            {
                for (int i = 1; i < group.Count; i++)
                {
                    try
                    {
                        string fileName = Path.GetFileName(group[i]);
                        string destPath = Path.Combine(destFolder, fileName);
                        
                        int counter = 1;
                        while (File.Exists(destPath))
                        {
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                            string extension = Path.GetExtension(fileName);
                            destPath = Path.Combine(destFolder, $"{nameWithoutExt}_{counter}{extension}");
                            counter++;
                        }
                        
                        File.Move(group[i], destPath);
                        movedCount++;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Could not move {group[i]}: {ex.Message}");
                    }
                }
            }
            
            Logger.Success($"Successfully moved {movedCount} duplicate files to {destFolder}.");
        }

        private void GenerateReport(Dictionary<string, List<string>> duplicates, Logger Logger)
        {
            string reportPath = Path.Combine(Program.dataDir, "duplicate_report.txt");
            
            try
            {
                using (var writer = new StreamWriter(reportPath))
                {
                    writer.WriteLine("Duplicate Files Report");
                    writer.WriteLine("Generated: " + DateTime.Now);
                    writer.WriteLine();
                    
                    long totalSize = 0;
                    int totalDuplicates = 0;
                    
                    foreach (var group in duplicates)
                    {
                        long fileSize = new FileInfo(group.Value[0]).Length;
                        totalSize += fileSize * (group.Value.Count - 1);
                        totalDuplicates += group.Value.Count - 1;
                        
                        writer.WriteLine($"Group - Size: {FormatFileSize(fileSize)} | Count: {group.Value.Count} files");
                        foreach (var file in group.Value)
                        {
                            writer.WriteLine($"  {file}");
                        }
                        writer.WriteLine();
                    }
                    
                    writer.WriteLine($"Summary:");
                    writer.WriteLine($"Total duplicate files: {totalDuplicates}");
                    writer.WriteLine($"Total space that could be saved: {FormatFileSize(totalSize)}");
                }
                
                Logger.Success($"Report generated: {reportPath}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Could not generate report: {ex.Message}");
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }
    }
} 