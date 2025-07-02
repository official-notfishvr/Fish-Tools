/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using System;
using System.IO;
using System.Linq;
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.FileManagementTools
{
    internal class FileOrganizer : ITool
    {
        public string Name => "File Organizer";
        public string Category => "File Management Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Automatically organize files by type, date, or other criteria";

        public void Main(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();
            
            Logger.Info("File Organizer - Automatic File Organization Tool");
            Logger.Info("Organize files automatically by various criteria.");
            
            while (true)
            {
                Console.WriteLine();
                Logger.Info("Organization Options:");
                Logger.WriteBarrierLine("1", "Organize by File Type");
                Logger.WriteBarrierLine("2", "Organize by Date");
                Logger.WriteBarrierLine("3", "Organize by Size");
                Logger.WriteBarrierLine("4", "Organize Downloads Folder");
                Logger.WriteBarrierLine("5", "Clean Desktop");
                Logger.WriteBarrierLine("0", "Back to Menu");
                
                Console.Write("Select option: ");
                var input = Console.ReadKey();
                Console.WriteLine();
                
                switch (input.Key)
                {
                    case ConsoleKey.D1:
                        OrganizeByFileType(Logger);
                        break;
                    case ConsoleKey.D2:
                        OrganizeByDate(Logger);
                        break;
                    case ConsoleKey.D3:
                        OrganizeBySize(Logger);
                        break;
                    case ConsoleKey.D4:
                        OrganizeDownloadsFolder(Logger);
                        break;
                    case ConsoleKey.D5:
                        CleanDesktop(Logger);
                        break;
                    case ConsoleKey.D0:
                        return;
                    default:
                        Logger.Error("Invalid option selected.");
                        break;
                }
            }
        }

        private void OrganizeByFileType(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== Organize by File Type ===");
            
            Console.Write("Enter directory path to organize: ");
            string dirPath = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(dirPath))
            {
                Logger.Error("No directory path provided.");
                return;
            }
            
            if (!Directory.Exists(dirPath))
            {
                Logger.Error("Directory not found.");
                return;
            }
            
            try
            {
                var files = Directory.GetFiles(dirPath, "*", SearchOption.TopDirectoryOnly);
                int organizedCount = 0;
                
                Logger.Info($"Found {files.Length} files to organize...");
                
                foreach (var file in files)
                {
                    try
                    {
                        string extension = Path.GetExtension(file).ToLower();
                        string category = GetFileCategory(extension);
                        string categoryPath = Path.Combine(dirPath, category);
                        
                        if (!Directory.Exists(categoryPath))
                        {
                            Directory.CreateDirectory(categoryPath);
                        }
                        
                        string fileName = Path.GetFileName(file);
                        string destinationPath = Path.Combine(categoryPath, fileName);
                        
                        // Handle duplicate files
                        if (File.Exists(destinationPath))
                        {
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(file);
                            string newFileName = $"{nameWithoutExt}_copy{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                            destinationPath = Path.Combine(categoryPath, newFileName);
                        }
                        
                        File.Move(file, destinationPath);
                        organizedCount++;
                        
                        Logger.Write($"Moved: {fileName} -> {category}/");
                        Console.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to organize {Path.GetFileName(file)}: {ex.Message}");
                    }
                }
                
                Logger.Success($"Organization completed! {organizedCount}/{files.Length} files organized.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Organization failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void OrganizeByDate(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== Organize by Date ===");
            
            Console.Write("Enter directory path to organize: ");
            string dirPath = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(dirPath))
            {
                Logger.Error("No directory path provided.");
                return;
            }
            
            if (!Directory.Exists(dirPath))
            {
                Logger.Error("Directory not found.");
                return;
            }
            
            Console.Write("Organize by (1=Creation Date, 2=Modified Date, 3=Access Date): ");
            var dateChoice = Console.ReadKey();
            Console.WriteLine();
            
            try
            {
                var files = Directory.GetFiles(dirPath, "*", SearchOption.TopDirectoryOnly);
                int organizedCount = 0;
                
                Logger.Info($"Found {files.Length} files to organize...");
                
                foreach (var file in files)
                {
                    try
                    {
                        DateTime fileDate;
                        
                        switch (dateChoice.Key)
                        {
                            case ConsoleKey.D1:
                                fileDate = File.GetCreationTime(file);
                                break;
                            case ConsoleKey.D2:
                                fileDate = File.GetLastWriteTime(file);
                                break;
                            case ConsoleKey.D3:
                                fileDate = File.GetLastAccessTime(file);
                                break;
                            default:
                                fileDate = File.GetLastWriteTime(file);
                                break;
                        }
                        
                        string yearMonth = fileDate.ToString("yyyy-MM");
                        string categoryPath = Path.Combine(dirPath, yearMonth);
                        
                        if (!Directory.Exists(categoryPath))
                        {
                            Directory.CreateDirectory(categoryPath);
                        }
                        
                        string fileName = Path.GetFileName(file);
                        string destinationPath = Path.Combine(categoryPath, fileName);
                        
                        if (File.Exists(destinationPath))
                        {
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(file);
                            string extension = Path.GetExtension(file);
                            string newFileName = $"{nameWithoutExt}_copy{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                            destinationPath = Path.Combine(categoryPath, newFileName);
                        }
                        
                        File.Move(file, destinationPath);
                        organizedCount++;
                        
                        Logger.Write($"Moved: {fileName} -> {yearMonth}/");
                        Console.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to organize {Path.GetFileName(file)}: {ex.Message}");
                    }
                }
                
                Logger.Success($"Organization completed! {organizedCount}/{files.Length} files organized.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Organization failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void OrganizeBySize(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== Organize by Size ===");
            
            Console.Write("Enter directory path to organize: ");
            string dirPath = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(dirPath))
            {
                Logger.Error("No directory path provided.");
                return;
            }
            
            if (!Directory.Exists(dirPath))
            {
                Logger.Error("Directory not found.");
                return;
            }
            
            try
            {
                var files = Directory.GetFiles(dirPath, "*", SearchOption.TopDirectoryOnly)
                    .Select(f => new FileInfo(f))
                    .OrderBy(f => f.Length)
                    .ToList();
                
                int organizedCount = 0;
                
                Logger.Info($"Found {files.Count} files to organize...");
                
                foreach (var fileInfo in files)
                {
                    try
                    {
                        string category = GetSizeCategory(fileInfo.Length);
                        string categoryPath = Path.Combine(dirPath, category);
                        
                        if (!Directory.Exists(categoryPath))
                        {
                            Directory.CreateDirectory(categoryPath);
                        }
                        
                        string fileName = fileInfo.Name;
                        string destinationPath = Path.Combine(categoryPath, fileName);
                        
                        if (File.Exists(destinationPath))
                        {
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                            string extension = Path.GetExtension(fileName);
                            string newFileName = $"{nameWithoutExt}_copy{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                            destinationPath = Path.Combine(categoryPath, newFileName);
                        }
                        
                        File.Move(fileInfo.FullName, destinationPath);
                        organizedCount++;
                        
                        Logger.Write($"Moved: {fileName} ({FormatFileSize(fileInfo.Length)}) -> {category}/");
                        Console.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to organize {fileInfo.Name}: {ex.Message}");
                    }
                }
                
                Logger.Success($"Organization completed! {organizedCount}/{files.Count} files organized.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Organization failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void OrganizeDownloadsFolder(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== Organize Downloads Folder ===");
            
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            
            if (!Directory.Exists(downloadsPath))
            {
                Logger.Error("Downloads folder not found.");
                return;
            }
            
            Logger.Info($"Organizing Downloads folder: {downloadsPath}");
            Logger.Warn("This will organize all files in your Downloads folder. Continue? (y/n)");
            
            var input = Console.ReadKey();
            Console.WriteLine();
            
            if (input.Key != ConsoleKey.Y)
            {
                return;
            }
            
            try
            {
                var files = Directory.GetFiles(downloadsPath, "*", SearchOption.TopDirectoryOnly);
                int organizedCount = 0;
                
                Logger.Info($"Found {files.Length} files to organize...");
                
                foreach (var file in files)
                {
                    try
                    {
                        string extension = Path.GetExtension(file).ToLower();
                        string category = GetFileCategory(extension);
                        string categoryPath = Path.Combine(downloadsPath, category);
                        
                        if (!Directory.Exists(categoryPath))
                        {
                            Directory.CreateDirectory(categoryPath);
                        }
                        
                        string fileName = Path.GetFileName(file);
                        string destinationPath = Path.Combine(categoryPath, fileName);
                        
                        if (File.Exists(destinationPath))
                        {
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(file);
                            string newFileName = $"{nameWithoutExt}_copy{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                            destinationPath = Path.Combine(categoryPath, newFileName);
                        }
                        
                        File.Move(file, destinationPath);
                        organizedCount++;
                        
                        Logger.Write($"Moved: {fileName} -> {category}/");
                        Console.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to organize {Path.GetFileName(file)}: {ex.Message}");
                    }
                }
                
                Logger.Success($"Downloads organization completed! {organizedCount}/{files.Length} files organized.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Organization failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void CleanDesktop(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== Clean Desktop ===");
            
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            
            if (!Directory.Exists(desktopPath))
            {
                Logger.Error("Desktop folder not found.");
                return;
            }
            
            Logger.Info($"Cleaning Desktop: {desktopPath}");
            Logger.Warn("This will organize all files on your Desktop. Continue? (y/n)");
            
            var input = Console.ReadKey();
            Console.WriteLine();
            
            if (input.Key != ConsoleKey.Y)
            {
                return;
            }
            
            try
            {
                var files = Directory.GetFiles(desktopPath, "*", SearchOption.TopDirectoryOnly);
                int organizedCount = 0;
                
                Logger.Info($"Found {files.Length} files to organize...");
                
                foreach (var file in files)
                {
                    try
                    {
                        string extension = Path.GetExtension(file).ToLower();
                        string category = GetFileCategory(extension);
                        string categoryPath = Path.Combine(desktopPath, category);
                        
                        if (!Directory.Exists(categoryPath))
                        {
                            Directory.CreateDirectory(categoryPath);
                        }
                        
                        string fileName = Path.GetFileName(file);
                        string destinationPath = Path.Combine(categoryPath, fileName);
                        
                        if (File.Exists(destinationPath))
                        {
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(file);
                            string newFileName = $"{nameWithoutExt}_copy{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                            destinationPath = Path.Combine(categoryPath, newFileName);
                        }
                        
                        File.Move(file, destinationPath);
                        organizedCount++;
                        
                        Logger.Write($"Moved: {fileName} -> {category}/");
                        Console.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to organize {Path.GetFileName(file)}: {ex.Message}");
                    }
                }
                
                Logger.Success($"Desktop cleaning completed! {organizedCount}/{files.Length} files organized.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Desktop cleaning failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private string GetFileCategory(string extension)
        {
            return extension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".tiff" or ".svg" or ".webp" => "Images",
                ".mp4" or ".avi" or ".mkv" or ".mov" or ".wmv" or ".flv" or ".webm" or ".m4v" => "Videos",
                ".mp3" or ".wav" or ".flac" or ".aac" or ".ogg" or ".wma" or ".m4a" => "Audio",
                ".pdf" or ".doc" or ".docx" or ".txt" or ".rtf" or ".odt" => "Documents",
                ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" => "Archives",
                ".exe" or ".msi" or ".dmg" or ".deb" or ".rpm" => "Executables",
                ".iso" or ".img" or ".dmg" => "Disk Images",
                ".torrent" => "Torrents",
                ".lnk" => "Shortcuts",
                _ => "Other"
            };
        }

        private string GetSizeCategory(long fileSize)
        {
            return fileSize switch
            {
                < 1024 * 1024 => "Small (< 1MB)",
                < 10 * 1024 * 1024 => "Medium (1-10MB)",
                < 100 * 1024 * 1024 => "Large (10-100MB)",
                _ => "Very Large (> 100MB)"
            };
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
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