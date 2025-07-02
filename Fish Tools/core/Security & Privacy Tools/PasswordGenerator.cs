/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using System;
using System.Linq;
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.MiscTools
{
    internal class PasswordGenerator : ITool
    {
        public string Name => "Password Generator";
        public string Category => "Security & Privacy Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Generate secure passwords with various options";

        public void Main(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();
            
            Logger.Info("Password Generator - Secure Password Creation Tool");
            Logger.Info("This tool generates secure passwords with customizable options.");
            
            while (true)
            {
                Console.WriteLine();
                Logger.Info("Password Generation Options:");
                Logger.WriteBarrierLine("1", "Generate Single Password");
                Logger.WriteBarrierLine("2", "Generate Multiple Passwords");
                Logger.WriteBarrierLine("3", "Generate Passphrase");
                Logger.WriteBarrierLine("0", "Back to Menu");
                
                Console.Write("Select option: ");
                var input = Console.ReadKey();
                Console.WriteLine();
                
                switch (input.Key)
                {
                    case ConsoleKey.D1:
                        GenerateSinglePassword(Logger);
                        break;
                    case ConsoleKey.D2:
                        GenerateMultiplePasswords(Logger);
                        break;
                    case ConsoleKey.D3:
                        GeneratePassphrase(Logger);
                        break;
                    case ConsoleKey.D0:
                        return;
                    default:
                        Logger.Error("Invalid option selected.");
                        break;
                }
            }
        }

        private void GenerateSinglePassword(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== Single Password Generation ===");
            
            var options = GetPasswordOptions(Logger);
            if (options == null) return;
            
            string password = GeneratePassword(options);
            
            Console.WriteLine();
            Logger.Success("Generated Password:");
            Logger.Write($"Password: {password}");
            Console.WriteLine();
            Logger.Write($"Length: {password.Length} characters");
            Console.WriteLine();
            Logger.Write($"Strength: {CalculatePasswordStrength(password)}");
            Console.WriteLine();
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void GenerateMultiplePasswords(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== Multiple Password Generation ===");
            
            Console.Write("Enter number of passwords to generate (default 10): ");
            string countStr = Console.ReadLine();
            int count = string.IsNullOrEmpty(countStr) ? 10 : int.Parse(countStr);
            
            if (count <= 0 || count > 100)
            {
                Logger.Error("Please enter a number between 1 and 100.");
                return;
            }
            
            var options = GetPasswordOptions(Logger);
            if (options == null) return;
            
            Console.WriteLine();
            Logger.Info($"Generating {count} passwords...");
            Console.WriteLine();
            
            for (int i = 1; i <= count; i++)
            {
                string password = GeneratePassword(options);
                Logger.Write($"{i:D2}. {password}");
                Console.WriteLine();
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void GeneratePassphrase(Logger Logger)
        {
            Console.WriteLine();
            Logger.Info("=== Passphrase Generation ===");
            
            Console.Write("Enter number of words (default 4): ");
            string wordCountStr = Console.ReadLine();
            int wordCount = string.IsNullOrEmpty(wordCountStr) ? 4 : int.Parse(wordCountStr);
            
            if (wordCount <= 0 || wordCount > 20)
            {
                Logger.Error("Please enter a number between 1 and 20.");
                return;
            }
            
            Console.Write("Include numbers? (y/n, default y): ");
            string includeNumbers = Console.ReadLine();
            bool numbers = string.IsNullOrEmpty(includeNumbers) || includeNumbers.ToLower() == "y";
            
            Console.Write("Include special characters? (y/n, default y): ");
            string includeSpecial = Console.ReadLine();
            bool special = string.IsNullOrEmpty(includeSpecial) || includeSpecial.ToLower() == "y";
            
            string passphrase = GeneratePassphrase(wordCount, numbers, special);
            
            Console.WriteLine();
            Logger.Success("Generated Passphrase:");
            Logger.Write($"Passphrase: {passphrase}");
            Console.WriteLine();
            Logger.Write($"Length: {passphrase.Length} characters");
            Console.WriteLine();
            Logger.Write($"Strength: {CalculatePasswordStrength(passphrase)}");
            Console.WriteLine();
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private PasswordOptions GetPasswordOptions(Logger Logger)
        {
            Console.Write("Enter password length (default 16): ");
            string lengthStr = Console.ReadLine();
            int length = string.IsNullOrEmpty(lengthStr) ? 16 : int.Parse(lengthStr);
            
            if (length <= 0 || length > 100)
            {
                Logger.Error("Please enter a length between 1 and 100.");
                return null;
            }
            
            Console.Write("Include uppercase letters? (y/n, default y): ");
            string includeUpper = Console.ReadLine();
            bool uppercase = string.IsNullOrEmpty(includeUpper) || includeUpper.ToLower() == "y";
            
            Console.Write("Include lowercase letters? (y/n, default y): ");
            string includeLower = Console.ReadLine();
            bool lowercase = string.IsNullOrEmpty(includeLower) || includeLower.ToLower() == "y";
            
            Console.Write("Include numbers? (y/n, default y): ");
            string includeNumbers = Console.ReadLine();
            bool numbers = string.IsNullOrEmpty(includeNumbers) || includeNumbers.ToLower() == "y";
            
            Console.Write("Include special characters? (y/n, default y): ");
            string includeSpecial = Console.ReadLine();
            bool special = string.IsNullOrEmpty(includeSpecial) || includeSpecial.ToLower() == "y";
            
            Console.Write("Exclude similar characters (l, 1, I, O, 0)? (y/n, default n): ");
            string excludeSimilar = Console.ReadLine();
            bool exclude = !string.IsNullOrEmpty(excludeSimilar) && excludeSimilar.ToLower() == "y";
            
            return new PasswordOptions
            {
                Length = length,
                IncludeUppercase = uppercase,
                IncludeLowercase = lowercase,
                IncludeNumbers = numbers,
                IncludeSpecial = special,
                ExcludeSimilar = exclude
            };
        }

        private string GeneratePassword(PasswordOptions options)
        {
            var random = new Random();
            var chars = new System.Text.StringBuilder();
            
            if (options.IncludeUppercase)
            {
                chars.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                if (options.ExcludeSimilar)
                {
                    chars.Replace("I", "").Replace("O", "");
                }
            }
            
            if (options.IncludeLowercase)
            {
                chars.Append("abcdefghijklmnopqrstuvwxyz");
                if (options.ExcludeSimilar)
                {
                    chars.Replace("l", "");
                }
            }
            
            if (options.IncludeNumbers)
            {
                chars.Append("0123456789");
                if (options.ExcludeSimilar)
                {
                    chars.Replace("1", "").Replace("0", "");
                }
            }
            
            if (options.IncludeSpecial)
            {
                chars.Append("!@#$%^&*()_+-=[]{}|;:,.<>?");
            }
            
            if (chars.Length == 0)
            {
                chars.Append("abcdefghijklmnopqrstuvwxyz");
            }
            
            var charArray = chars.ToString().ToCharArray();
            var password = new char[options.Length];
            
            for (int i = 0; i < options.Length; i++)
            {
                password[i] = charArray[random.Next(charArray.Length)];
            }
            
            return new string(password);
        }

        private string GeneratePassphrase(int wordCount, bool includeNumbers, bool includeSpecial)
        {
            var words = new[]
            {
                "apple", "banana", "cherry", "dragon", "eagle", "forest", "garden", "house", "island", "jungle",
                "knight", "lemon", "mountain", "ocean", "planet", "queen", "river", "sunset", "tiger", "umbrella",
                "village", "window", "yellow", "zebra", "anchor", "bridge", "castle", "diamond", "elephant", "flower",
                "guitar", "hammer", "iceberg", "jacket", "kangaroo", "lighthouse", "moonlight", "notebook", "orange", "penguin",
                "quilt", "rainbow", "sailboat", "telescope", "umbrella", "volcano", "waterfall", "xylophone", "yacht", "zeppelin"
            };
            
            var random = new Random();
            var passphrase = new System.Text.StringBuilder();
            
            for (int i = 0; i < wordCount; i++)
            {
                if (i > 0)
                {
                    if (includeSpecial && random.Next(2) == 0)
                    {
                        passphrase.Append("!@#$%^&*"[random.Next(8)]);
                    }
                    else
                    {
                        passphrase.Append(" ");
                    }
                }
                
                passphrase.Append(words[random.Next(words.Length)]);
                
                if (includeNumbers && random.Next(3) == 0)
                {
                    passphrase.Append(random.Next(10, 100));
                }
            }
            
            return passphrase.ToString();
        }

        private string CalculatePasswordStrength(string password)
        {
            int score = 0;
            
            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;
            if (password.Length >= 16) score++;
            
            if (password.Any(char.IsUpper)) score++;
            if (password.Any(char.IsLower)) score++;
            if (password.Any(char.IsDigit)) score++;
            if (password.Any(c => !char.IsLetterOrDigit(c))) score++;
            
            return score switch
            {
                0 or 1 => "Very Weak",
                2 => "Weak",
                3 => "Fair",
                4 => "Good",
                5 => "Strong",
                6 => "Very Strong",
                _ => "Excellent"
            };
        }

        private class PasswordOptions
        {
            public int Length { get; set; }
            public bool IncludeUppercase { get; set; }
            public bool IncludeLowercase { get; set; }
            public bool IncludeNumbers { get; set; }
            public bool IncludeSpecial { get; set; }
            public bool ExcludeSimilar { get; set; }
        }
    }
}