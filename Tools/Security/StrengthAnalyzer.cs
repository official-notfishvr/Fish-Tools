using System.Text.RegularExpressions;
using FishConsole = Fish.Console.FishConsole;

namespace FishTools.App;

internal sealed class StrengthAnalyzer : ITool
{
    public string Id => "strength-analyzer";
    public string Name => "Password Strength Analyzer";
    public string Category => ToolCategories.Security;
    public string Description => "Rate passwords for entropy, common patterns, and brute-force resistance.";

    public Task RunAsync(ToolContext context)
    {
        while (true)
        {
            ConsoleUi.ResetScreen(Name);
            var password = ConsoleUi.Prompt("Enter a password to analyze (leave blank to go back)");
            if (string.IsNullOrEmpty(password))
            {
                return Task.CompletedTask;
            }

            var analysis = Analyze(password);
            ConsoleUi.ResetScreen(Name);
            ConsoleUi.Info($"Length: {password.Length}");
            ConsoleUi.Info($"Score: {analysis.Score}/100");
            ConsoleUi.Info($"Entropy: {analysis.Entropy:0.0} bits");
            ConsoleUi.Info($"Strength: {analysis.Label}");
            FishConsole.WriteLine();
            ConsoleUi.Section("Findings");
            foreach (var finding in analysis.Findings)
            {
                ConsoleUi.Info($"- {finding}");
            }

            FishConsole.WriteLine();
            ConsoleUi.Section("Recommendations");
            foreach (var recommendation in analysis.Recommendations)
            {
                ConsoleUi.Info($"- {recommendation}");
            }

            ConsoleUi.Pause();
        }
    }

    private static (int Score, double Entropy, string Label, List<string> Findings, List<string> Recommendations) Analyze(string password)
    {
        var findings = new List<string>();
        var recommendations = new List<string>();
        var score = 0;

        var hasLower = password.Any(char.IsLower);
        var hasUpper = password.Any(char.IsUpper);
        var hasDigit = password.Any(char.IsDigit);
        var hasSymbol = password.Any(c => !char.IsLetterOrDigit(c));
        var charset = (hasLower ? 26 : 0) + (hasUpper ? 26 : 0) + (hasDigit ? 10 : 0) + (hasSymbol ? 33 : 0);
        var entropy = charset == 0 ? 0 : Math.Log2(Math.Pow(charset, password.Length));

        score += Math.Min(password.Length * 4, 40);
        score += hasLower ? 10 : 0;
        score += hasUpper ? 10 : 0;
        score += hasDigit ? 10 : 0;
        score += hasSymbol ? 15 : 0;
        score += password.Length >= 16 ? 10 : 0;

        if (Regex.IsMatch(password, @"(.)\1{2,}"))
        {
            score -= 10;
            findings.Add("Contains repeated characters.");
        }

        if (Regex.IsMatch(password, @"(123|234|345|456|567|678|789|890|abc|bcd|cde|qwerty|password)", RegexOptions.IgnoreCase))
        {
            score -= 20;
            findings.Add("Contains common sequences or dictionary words.");
        }

        if (password.Length < 12)
            recommendations.Add("Increase length to at least 12 characters.");
        if (!hasLower)
            recommendations.Add("Include lowercase letters.");
        if (!hasUpper)
            recommendations.Add("Include uppercase letters.");
        if (!hasDigit)
            recommendations.Add("Include numeric digits.");
        if (!hasSymbol)
            recommendations.Add("Include special symbols.");
        if (recommendations.Count == 0)
            recommendations.Add("Excellent! No obvious weaknesses found.");
        if (findings.Count == 0)
            findings.Add("No repeated or common patterns detected.");

        score = Math.Clamp(score, 0, 100);
        var label = score switch
        {
            >= 85 => "Very strong",
            >= 65 => "Strong",
            >= 45 => "Moderate",
            >= 25 => "Weak",
            _ => "Very weak",
        };

        return (score, entropy, label, findings, recommendations);
    }
}
