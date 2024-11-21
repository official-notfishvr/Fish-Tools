using System;
using System.IO;
using System.Security.Cryptography;

namespace Fish_Tools.core.File_Management_Tools
{
    internal class FishHash
    {
        // Method to compare two folders and copy files with different hashes
        public void CompareFolders(string folder1, string folder2)
        {
            // Ensure folder paths exist
            if (!Directory.Exists(folder1) || !Directory.Exists(folder2))
            {
                Console.WriteLine("One or both of the directories do not exist.");
                return;
            }

            // Create a new folder named "NotSameHash"
            string notSameHashFolder = Path.Combine(folder1, "NotSameHash");
            Directory.CreateDirectory(notSameHashFolder);

            // Get all files in the first folder
            string[] filesFolder1 = Directory.GetFiles(folder1);

            foreach (string filePath1 in filesFolder1)
            {
                // Get the file name from folder 1
                string fileName = Path.GetFileName(filePath1);

                // Check if the same file exists in folder 2
                string filePath2 = Path.Combine(folder2, fileName);
                if (File.Exists(filePath2))
                {
                    // Compare the hash of the files
                    string hash1 = GetFileHash(filePath1);
                    string hash2 = GetFileHash(filePath2);

                    // If hashes are different, copy the file to "NotSameHash"
                    if (hash1 != hash2)
                    {
                        Console.WriteLine($"File {fileName} has a different hash. Copying to NotSameHash folder.");
                        File.Copy(filePath1, Path.Combine(notSameHashFolder, fileName), true);
                    }
                }
                else
                {
                    // If the file doesn't exist in folder 2, copy it
                    Console.WriteLine($"File {fileName} does not exist in {folder2}. Copying to NotSameHash folder.");
                    File.Copy(filePath1, Path.Combine(notSameHashFolder, fileName), true);
                }
            }
        }

        // Helper method to compute the hash of a file
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
