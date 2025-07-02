/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.FileManagementTools
{
    internal class FileEncryptor : ITool
    {
        public string Name => "File Encryptor";
        public string Category => "Security & Privacy Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Encrypt and decrypt files using AES encryption with password protection";

        public void Main(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();
            
            Logger.Info("File Encryptor - Secure File Encryption Tool");
            Logger.Info("Encrypt and decrypt files using AES-256 encryption.");
            
            while (true)
            {
                Console.WriteLine();
                Logger.Info("Encryption Options:");
                Logger.WriteBarrierLine("1", "Encrypt File");
                Logger.WriteBarrierLine("2", "Decrypt File");
                Logger.WriteBarrierLine("3", "Encrypt Directory");
                Logger.WriteBarrierLine("4", "Decrypt Directory");
                Logger.WriteBarrierLine("0", "Back to Menu");
                
                Console.Write("Select option: ");
                var input = Console.ReadKey();
                Console.WriteLine();
                
                switch (input.Key)
                {
                    case ConsoleKey.D1:
                        EncryptFile(Logger);
                        break;
                    case ConsoleKey.D2:
                        DecryptFile(Logger);
                        break;
                    case ConsoleKey.D3:
                        EncryptDirectory(Logger);
                        break;
                    case ConsoleKey.D4:
                        DecryptDirectory(Logger);
                        break;
                    case ConsoleKey.D0:
                        return;
                    default:
                        Logger.Error("Invalid option selected.");
                        break;
                }
            }
        }

        private void EncryptFile(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== File Encryption ===");
            
            Console.Write("Enter file path to encrypt: ");
            string filePath = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Logger.Error("No file path provided.");
                return;
            }
            
            if (!File.Exists(filePath))
            {
                Logger.Error("File not found.");
                return;
            }
            
            Console.Write("Enter password: ");
            string password = ReadPassword();
            
            if (string.IsNullOrWhiteSpace(password))
            {
                Logger.Error("Password is required.");
                return;
            }
            
            Console.Write("Confirm password: ");
            string confirmPassword = ReadPassword();
            
            if (password != confirmPassword)
            {
                Logger.Error("Passwords do not match.");
                return;
            }
            
            try
            {
                string encryptedPath = filePath + ".encrypted";
                EncryptFile(filePath, encryptedPath, password, Logger);
                
                Logger.Success($"File encrypted successfully!");
                Logger.Write($"Original: {filePath}");
                Console.WriteLine();
                Logger.Write($"Encrypted: {encryptedPath}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Logger.Error($"Encryption failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void DecryptFile(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== File Decryption ===");
            
            Console.Write("Enter encrypted file path: ");
            string filePath = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Logger.Error("No file path provided.");
                return;
            }
            
            if (!File.Exists(filePath))
            {
                Logger.Error("File not found.");
                return;
            }
            
            Console.Write("Enter password: ");
            string password = ReadPassword();
            
            if (string.IsNullOrWhiteSpace(password))
            {
                Logger.Error("Password is required.");
                return;
            }
            
            try
            {
                string decryptedPath = filePath.Replace(".encrypted", ".decrypted");
                if (decryptedPath == filePath)
                {
                    decryptedPath = filePath + ".decrypted";
                }
                
                DecryptFile(filePath, decryptedPath, password, Logger);
                
                Logger.Success($"File decrypted successfully!");
                Logger.Write($"Encrypted: {filePath}");
                Console.WriteLine();
                Logger.Write($"Decrypted: {decryptedPath}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Logger.Error($"Decryption failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void EncryptDirectory(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== Directory Encryption ===");
            
            Console.Write("Enter directory path to encrypt: ");
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
            
            Console.Write("Enter password: ");
            string password = ReadPassword();
            
            if (string.IsNullOrWhiteSpace(password))
            {
                Logger.Error("Password is required.");
                return;
            }
            
            Console.Write("Confirm password: ");
            string confirmPassword = ReadPassword();
            
            if (password != confirmPassword)
            {
                Logger.Error("Passwords do not match.");
                return;
            }
            
            try
            {
                var files = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
                int encryptedCount = 0;
                
                Logger.Info($"Found {files.Length} files to encrypt...");
                
                foreach (var file in files)
                {
                    try
                    {
                        string encryptedPath = file + ".encrypted";
                        EncryptFile(file, encryptedPath, password, Logger);
                        encryptedCount++;
                        
                        Logger.Write($"Encrypted: {Path.GetFileName(file)}");
                        Console.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to encrypt {Path.GetFileName(file)}: {ex.Message}");
                    }
                }
                
                Logger.Success($"Directory encryption completed! {encryptedCount}/{files.Length} files encrypted.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Directory encryption failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void DecryptDirectory(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== Directory Decryption ===");
            
            Console.Write("Enter directory path to decrypt: ");
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
            
            Console.Write("Enter password: ");
            string password = ReadPassword();
            
            if (string.IsNullOrWhiteSpace(password))
            {
                Logger.Error("Password is required.");
                return;
            }
            
            try
            {
                var files = Directory.GetFiles(dirPath, "*.encrypted", SearchOption.AllDirectories);
                int decryptedCount = 0;
                
                Logger.Info($"Found {files.Length} encrypted files...");
                
                foreach (var file in files)
                {
                    try
                    {
                        string decryptedPath = file.Replace(".encrypted", ".decrypted");
                        DecryptFile(file, decryptedPath, password, Logger);
                        decryptedCount++;
                        
                        Logger.Write($"Decrypted: {Path.GetFileName(file)}");
                        Console.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to decrypt {Path.GetFileName(file)}: {ex.Message}");
                    }
                }
                
                Logger.Success($"Directory decryption completed! {decryptedCount}/{files.Length} files decrypted.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Directory decryption failed: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void EncryptFile(string inputFile, string outputFile, string password, Logger Logger)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                
                byte[] salt = new byte[16];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(salt);
                }
                
                var key = new Rfc2898DeriveBytes(password, salt, 10000);
                aes.Key = key.GetBytes(32);
                aes.GenerateIV();
                
                using (FileStream fsCrypt = new FileStream(outputFile, FileMode.Create))
                {
                    fsCrypt.Write(salt, 0, salt.Length);
                    fsCrypt.Write(aes.IV, 0, aes.IV.Length);
                    
                    using (CryptoStream cs = new CryptoStream(fsCrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
                    {
                        byte[] buffer = new byte[1048576];
                        int read;
                        
                        while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            cs.Write(buffer, 0, read);
                        }
                    }
                }
            }
        }

        private void DecryptFile(string inputFile, string outputFile, string password, Logger Logger)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                
                using (FileStream fsCrypt = new FileStream(inputFile, FileMode.Open))
                {
                    byte[] salt = new byte[16];
                    fsCrypt.Read(salt, 0, salt.Length);
                    
                    byte[] iv = new byte[16];
                    fsCrypt.Read(iv, 0, iv.Length);
                    
                    var key = new Rfc2898DeriveBytes(password, salt, 10000);
                    aes.Key = key.GetBytes(32);
                    aes.IV = iv;
                    
                    using (CryptoStream cs = new CryptoStream(fsCrypt, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (FileStream fsOut = new FileStream(outputFile, FileMode.Create))
                    {
                        byte[] buffer = new byte[1048576];
                        int read;
                        
                        while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fsOut.Write(buffer, 0, read);
                        }
                    }
                }
            }
        }

        private string ReadPassword()
        {
            var password = new StringBuilder();
            ConsoleKeyInfo key;
            
            do
            {
                key = Console.ReadKey(true);
                
                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Length--;
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
            } while (key.Key != ConsoleKey.Enter);
            
            Console.WriteLine();
            return password.ToString();
        }
    }
} 