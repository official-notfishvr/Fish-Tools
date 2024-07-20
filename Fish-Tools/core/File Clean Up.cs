using Fish_Tools.core.Utils;
using System;
using System.IO;

namespace Fish_Tools.core
{
    internal class FCU
    {
        private static readonly string[] DirectoriesToClean =
        {
            $@"C:\Users\{Environment.UserName}\Documents\Outher\VS\C#",
            $@"C:\Users\{Environment.UserName}\Documents\Outher\VS\JS & html"
        };

        private static readonly string[] FoldersToDelete = { ".vs", "bin", "obj", "node_modules", "packages" };
        private static readonly string[] FilesToDelete = { "package.json", "package-lock.json" };

        public static void CleanUpDirectories(Logger Logger)
        {
            foreach (var dir in DirectoriesToClean)
            {
                if (Directory.Exists(dir)) { CleanUpDirectory(dir, Logger); }
                else { Logger.Info($"Directory not found: {dir}"); }
            }
        }
        private static void CleanUpDirectory(string dir, Logger Logger)
        {
            foreach (var folder in FoldersToDelete)
            {
                string folderPath = Path.Combine(dir, folder);
                if (Directory.Exists(folderPath)) { DeleteDirectory(folderPath, Logger); }
                else { Logger.Info($"Folder not found: {folderPath}, searching recursively..."); SearchAndDeleteFolder(dir, folder, Logger); }
            }
            foreach (var file in FilesToDelete) { DeleteFileInDirectory(dir, file, Logger); }
        }
        private static void SearchAndDeleteFolder(string parentDir, string folderName, Logger Logger)
        {
            var directories = Directory.GetDirectories(parentDir, folderName, SearchOption.AllDirectories);
            foreach (var dir in directories) { DeleteDirectory(dir, Logger); }
        }
        private static void DeleteDirectory(string path, Logger Logger)
        {
            try
            {
                Directory.Delete(path, true);
                Logger.Success($"Deleted: {path}");
            }
            catch (Exception ex) { Logger.Error($"Error deleting {path}: {ex.Message}"); }
        }
        private static void DeleteFileInDirectory(string dir, string fileName, Logger Logger)
        {
            try
            {
                var files = Directory.GetFiles(dir, fileName, SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    File.Delete(file);
                    Logger.Success($"Deleted file: {file}");
                }
            }
            catch (Exception ex) { Logger.Error($"Error deleting file {fileName}: {ex.Message}"); }
        }
    }
}
