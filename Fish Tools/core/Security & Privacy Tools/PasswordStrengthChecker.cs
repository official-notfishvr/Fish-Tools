/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.Utils;
using System.Text.RegularExpressions;

namespace Fish_Tools.core.SecurityTools
{
    internal class PasswordStrengthChecker : ITool
    {
        public string Name => "Password Strength Checker";
        public string Category => "Security & Privacy Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Analyze password strength and security";

        public void Main(Logger logger)
        {
            logger.Info("Password Strength Checker");
            logger.Info("Enter passwords to check their strength (type 'exit' to quit)");

            while (true)
            {
                Console.Write("\nEnter password to check: ");
                string password = Console.ReadLine() ?? "";

                if (password.ToLower() == "exit")
                    break;

                if (string.IsNullOrEmpty(password))
                {
                    logger.Warn("Please enter a password");
                    continue;
                }

                AnalyzePassword(password, logger);
            }

            logger.Info("Returning to main menu...");
        }

        private void AnalyzePassword(string password, Logger logger)
        {
            logger.Info($"\n=== Password Analysis: {new string('*', Math.Min(password.Length, 10))} ===");

            logger.Info($"Length: {password.Length} characters");
            logger.Info($"Character types used:");

            bool hasLower = password.Any(char.IsLower);
            bool hasUpper = password.Any(char.IsUpper);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            logger.Write($"  - Lowercase letters: {(hasLower ? "✓" : "✗")}");
            logger.Write($"  - Uppercase letters: {(hasUpper ? "✓" : "✗")}");
            logger.Write($"  - Numbers: {(hasDigit ? "✓" : "✗")}");
            logger.Write($"  - Special characters: {(hasSpecial ? "✓" : "✗")}");

            int strength = CalculateStrength(password);
            string strengthLevel = GetStrengthLevel(strength);
            string strengthColor = GetStrengthColor(strength);

            logger.Info($"\nStrength Score: {strength}/100");
            logger.Write($"Strength Level: {strengthLevel}");

            double entropy = CalculateEntropy(password);
            logger.Info($"Entropy: {entropy:F1} bits");

            string crackTime = EstimateCrackTime(strength, password.Length);
            logger.Info($"Estimated time to crack: {crackTime}");

            logger.Info("\n=== Recommendations ===");
            var recommendations = GetRecommendations(password, hasLower, hasUpper, hasDigit, hasSpecial);
            foreach (var rec in recommendations)
            {
                logger.Write($"• {rec}");
            }

            CheckCommonPatterns(password, logger);
        }

        private int CalculateStrength(string password)
        {
            int score = 0;

            score += Math.Min(password.Length * 4, 40);

            if (password.Any(char.IsLower)) score += 10;
            if (password.Any(char.IsUpper)) score += 10;
            if (password.Any(char.IsDigit)) score += 10;
            if (password.Any(c => !char.IsLetterOrDigit(c))) score += 15;

            if (Regex.IsMatch(password, @"(.)\1{2,}")) score -= 10;
            if (Regex.IsMatch(password, @"(abc|bcd|cde|def|efg|fgh|ghi|hij|ijk|jkl|klm|lmn|mno|nop|opq|pqr|qrs|rst|stu|tuv|uvw|vwx|wxy|xyz)", RegexOptions.IgnoreCase)) score -= 15;
            if (Regex.IsMatch(password, @"(123|234|345|456|567|678|789|012)")) score -= 15;
            if (Regex.IsMatch(password, @"(qwerty|asdfgh|zxcvbn)", RegexOptions.IgnoreCase)) score -= 20;

            return Math.Max(0, Math.Min(100, score));
        }

        private string GetStrengthLevel(int strength)
        {
            if (strength >= 80) return "Very Strong";
            if (strength >= 60) return "Strong";
            if (strength >= 40) return "Moderate";
            if (strength >= 20) return "Weak";
            return "Very Weak";
        }

        private string GetStrengthColor(int strength)
        {
            if (strength >= 80) return "Green";
            if (strength >= 60) return "Blue";
            if (strength >= 40) return "Yellow";
            if (strength >= 20) return "Orange";
            return "Red";
        }

        private double CalculateEntropy(string password)
        {
            int charsetSize = 0;
            if (password.Any(char.IsLower)) charsetSize += 26;
            if (password.Any(char.IsUpper)) charsetSize += 26;
            if (password.Any(char.IsDigit)) charsetSize += 10;
            if (password.Any(c => !char.IsLetterOrDigit(c))) charsetSize += 32;

            return Math.Log(Math.Pow(charsetSize, password.Length), 2);
        }

        private string EstimateCrackTime(int strength, int length)
        {
            if (strength >= 80) return "Centuries";
            if (strength >= 60) return "Years";
            if (strength >= 40) return "Months";
            if (strength >= 20) return "Days";
            return "Hours or less";
        }

        private List<string> GetRecommendations(string password, bool hasLower, bool hasUpper, bool hasDigit, bool hasSpecial)
        {
            var recommendations = new List<string>();

            if (password.Length < 12)
                recommendations.Add("Make password at least 12 characters long");

            if (!hasLower)
                recommendations.Add("Add lowercase letters");

            if (!hasUpper)
                recommendations.Add("Add uppercase letters");

            if (!hasDigit)
                recommendations.Add("Add numbers");

            if (!hasSpecial)
                recommendations.Add("Add special characters (!@#$%^&*)");

            if (password.Length < 8)
                recommendations.Add("Password is too short - increase length");

            if (Regex.IsMatch(password, @"(.)\1{2,}"))
                recommendations.Add("Avoid repeated characters");

            if (Regex.IsMatch(password, @"(password|123456|qwerty)", RegexOptions.IgnoreCase))
                recommendations.Add("Avoid common passwords");

            return recommendations;
        }

        private void CheckCommonPatterns(string password, Logger logger)
        {
            logger.Info("\n=== Pattern Analysis ===");

            var commonPatterns = new Dictionary<string, string>
            {
                { @"(.)\1{2,}", "Repeated characters detected" },
                { @"(abc|bcd|cde|def|efg|fgh|ghi|hij|ijk|jkl|klm|lmn|mno|nop|opq|pqr|qrs|rst|stu|tuv|uvw|vwx|wxy|xyz)", "Sequential letters detected" },
                { @"(123|234|345|456|567|678|789|012)", "Sequential numbers detected" },
                { @"(qwerty|asdfgh|zxcvbn)", "Keyboard patterns detected" },
                { @"(password|pass|admin|user|login)", "Common words detected" }
            };

            foreach (var pattern in commonPatterns)
            {
                if (Regex.IsMatch(password, pattern.Key, RegexOptions.IgnoreCase))
                {
                    logger.Warn($"⚠ {pattern.Value}");
                }
            }
        }
    }
} 