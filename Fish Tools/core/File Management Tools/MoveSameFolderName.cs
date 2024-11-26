// not goaana add 
// whos goaana use this
// dont feel like to make 2
/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
/*
using System;
using System.IO;

namespace Fish_Tools.core.FileManagementTools
{
    internal class MoveSameFolderName
    {
        public static void MoveFolders(string folder1Path, string folder2Path)
        {
            if (!Directory.Exists(folder1Path))
            {
                Console.WriteLine($"Folder 1 does not exist: {folder1Path}");
                return;
            }
            if (!Directory.Exists(folder2Path))
            {
                Console.WriteLine($"Folder 2 does not exist: {folder2Path}");
                return;
            }

            string[] folder1SubDirs = Directory.GetDirectories(folder1Path);
            string[] folder2SubDirs = Directory.GetDirectories(folder2Path);

            foreach (string folder2SubDir in folder2SubDirs)
            {
                string folder2Name = Path.GetFileName(folder2SubDir);
                foreach (string folder1SubDir in folder1SubDirs)
                {
                    string folder1Name = Path.GetFileName(folder1SubDir);

                    if (folder1Name.Equals(folder2Name, StringComparison.OrdinalIgnoreCase))
                    {
                        string destinationPath = Path.Combine(folder1Path, folder2Name);
                        try
                        {
                            Directory.Move(folder2SubDir, destinationPath);
                            Console.WriteLine($"Moved folder: {folder2Name} from {folder2Path} to {folder1Path}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to move {folder2Name}: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}
*/