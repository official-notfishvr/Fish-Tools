using System.Security.Cryptography;
using System.Text;

namespace FishTools.App;

internal sealed class AesEncryptor : ITool
{
    private static readonly byte[] Header = Encoding.ASCII.GetBytes("FTOOLS1");

    public string Id => "aes-encryptor";
    public string Name => "AES File Encryptor";
    public string Category => ToolCategories.Security;
    public string Description => "Encrypt and decrypt files or directories using AES-256 (CBC) with PBKDF2.";

    public Task RunAsync(ToolContext context)
    {
        while (true)
        {
            ConsoleUi.ResetScreen(Name);
            var choice = ConsoleUi.ShowMenu("Choose an action", ["Encrypt file", "Decrypt file", "Encrypt folder (batch)", "Decrypt folder (batch)", "Back"]);
            if (choice == 4)
            {
                return Task.CompletedTask;
            }

            try
            {
                switch (choice)
                {
                    case 0:
                        EncryptFileFlow();
                        break;
                    case 1:
                        DecryptFileFlow();
                        break;
                    case 2:
                        EncryptDirectoryFlow();
                        break;
                    case 3:
                        DecryptDirectoryFlow();
                        break;
                }
            }
            catch (Exception ex)
            {
                ConsoleUi.Error(ex.Message);
                ConsoleUi.Pause();
            }
        }
    }

    private static void EncryptFileFlow()
    {
        var path = ConsoleUi.ExistingFilePrompt("File to encrypt");
        var password = ConsoleUi.PromptRequired("Password");
        var outputPath = Helpers.EnsureUniquePath(path + ".ftenc");
        ConsoleUi.ResetScreen("AES Encryptor");
        EncryptFile(path, outputPath, password);
        ConsoleUi.Success($"Encrypted successfully to {outputPath}");
        ConsoleUi.Pause();
    }

    private static void DecryptFileFlow()
    {
        var path = ConsoleUi.ExistingFilePrompt("Encrypted file");
        var password = ConsoleUi.PromptRequired("Password");
        var outputPath = Helpers.EnsureUniquePath(path.EndsWith(".ftenc", StringComparison.OrdinalIgnoreCase) ? path[..^6] : path + ".decrypted");

        ConsoleUi.ResetScreen("AES Encryptor");
        DecryptFile(path, outputPath, password);
        ConsoleUi.Success($"Decrypted successfully to {outputPath}");
        ConsoleUi.Pause();
    }

    private static void EncryptDirectoryFlow()
    {
        var path = ConsoleUi.ExistingDirectoryPrompt("Folder to encrypt");
        var password = ConsoleUi.PromptRequired("Password");
        var files = Helpers.SafeEnumerateFiles(path, "*", SearchOption.AllDirectories).ToArray();
        var count = 0;
        ConsoleUi.ResetScreen("AES Encryptor");

        foreach (var file in files)
        {
            if (file.EndsWith(".ftenc", StringComparison.OrdinalIgnoreCase))
                continue;
            EncryptFile(file, file + ".ftenc", password);
            count++;
        }

        ConsoleUi.Success($"Batch encryption complete. Encrypted {count} files.");
        ConsoleUi.Pause();
    }

    private static void DecryptDirectoryFlow()
    {
        var path = ConsoleUi.ExistingDirectoryPrompt("Folder to decrypt");
        var password = ConsoleUi.PromptRequired("Password");
        var files = Helpers.SafeEnumerateFiles(path, "*.ftenc", SearchOption.AllDirectories).ToArray();
        var count = 0;
        ConsoleUi.ResetScreen("AES Encryptor");

        foreach (var file in files)
        {
            var outputPath = Helpers.EnsureUniquePath(file[..^6]);
            DecryptFile(file, outputPath, password);
            count++;
        }

        ConsoleUi.Success($"Batch decryption complete. Decrypted {count} files.");
        ConsoleUi.Pause();
    }

    private static void EncryptFile(string inputPath, string outputPath, string password)
    {
        var plaintext = File.ReadAllBytes(inputPath);
        var salt = RandomNumberGenerator.GetBytes(16);
        var iv = RandomNumberGenerator.GetBytes(16);

        using var derive = new Rfc2898DeriveBytes(password, salt, 200_000, HashAlgorithmName.SHA256);
        var key = derive.GetBytes(32);
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var output = File.Create(outputPath);
        output.Write(Header);
        output.Write(salt);
        output.Write(iv);

        using var crypto = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
        crypto.Write(plaintext);
        crypto.FlushFinalBlock();
    }

    private static void DecryptFile(string inputPath, string outputPath, string password)
    {
        using var input = File.OpenRead(inputPath);
        var header = new byte[Header.Length];
        input.ReadExactly(header);
        if (!header.SequenceEqual(Header))
        {
            throw new InvalidDataException("Unsupported file header format.");
        }

        var salt = new byte[16];
        var iv = new byte[16];
        input.ReadExactly(salt);
        input.ReadExactly(iv);

        using var derive = new Rfc2898DeriveBytes(password, salt, 200_000, HashAlgorithmName.SHA256);
        var key = derive.GetBytes(32);
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var crypto = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var output = File.Create(outputPath);
        crypto.CopyTo(output);
    }
}
