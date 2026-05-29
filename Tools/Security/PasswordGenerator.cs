using System.Security.Cryptography;

namespace FishTools.App;

internal sealed class PasswordGenerator : ITool
{
    private static readonly string[] WordList = ["anchor", "forest", "signal", "ember", "planet", "tiger", "lantern", "harbor", "silver", "meadow", "rocket", "garden", "matrix", "velvet", "pioneer", "thunder", "canvas", "falcon", "summit", "ocean"];

    public string Id => "password-generator";
    public string Name => "Password Generator";
    public string Category => ToolCategories.Security;
    public string Description => "Generate cryptographically secure passwords or passphrases.";

    public Task RunAsync(ToolContext context)
    {
        while (true)
        {
            ConsoleUi.ResetScreen(Name);
            var choice = ConsoleUi.ShowMenu("Choose an action", ["Single password", "Batch passwords", "Passphrase", "Back"]);
            if (choice == 3)
            {
                return Task.CompletedTask;
            }

            if (choice == 2)
            {
                GeneratePassphrase();
                continue;
            }

            var count = choice == 0 ? 1 : ConsoleUi.PromptInt("How many passwords", 10, 1, 100);
            var length = ConsoleUi.PromptInt("Length", 20, 8, 256);
            var includeUpper = ConsoleUi.Confirm("Include uppercase?");
            var includeLower = ConsoleUi.Confirm("Include lowercase?");
            var includeDigits = ConsoleUi.Confirm("Include digits?");
            var includeSymbols = ConsoleUi.Confirm("Include symbols?");
            var excludeAmbiguous = ConsoleUi.Confirm("Exclude similar characters like O/0/I/l?");

            var passwords = Enumerable.Range(0, count).Select(_ => GeneratePassword(length, includeUpper, includeLower, includeDigits, includeSymbols, excludeAmbiguous)).ToArray();

            ConsoleUi.ResetScreen(Name);
            foreach (var password in passwords)
            {
                ConsoleUi.Success(password);
            }

            ConsoleUi.Pause();
        }
    }

    private static string GeneratePassword(int length, bool includeUpper, bool includeLower, bool includeDigits, bool includeSymbols, bool excludeAmbiguous)
    {
        var pools = new List<string>();
        if (includeUpper)
            pools.Add(excludeAmbiguous ? "ABCDEFGHJKLMNPQRSTUVWXYZ" : "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        if (includeLower)
            pools.Add(excludeAmbiguous ? "abcdefghijkmnopqrstuvwxyz" : "abcdefghijklmnopqrstuvwxyz");
        if (includeDigits)
            pools.Add(excludeAmbiguous ? "23456789" : "0123456789");
        if (includeSymbols)
            pools.Add("!@#$%^&*()-_=+[]{};:,.?/|");

        if (pools.Count == 0)
        {
            pools.Add("abcdefghijklmnopqrstuvwxyz");
        }

        var allCharacters = string.Concat(pools);
        var chars = new char[length];

        for (var i = 0; i < pools.Count && i < length; i++)
        {
            chars[i] = pools[i][RandomNumberGenerator.GetInt32(pools[i].Length)];
        }

        for (var i = pools.Count; i < length; i++)
        {
            chars[i] = allCharacters[RandomNumberGenerator.GetInt32(allCharacters.Length)];
        }

        return new string(chars.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
    }

    private static void GeneratePassphrase()
    {
        var words = ConsoleUi.PromptInt("Word count", 4, 3, 10);
        var delimiter = ConsoleUi.Prompt("Delimiter", "-");
        var includeDigits = ConsoleUi.Confirm("Append two digits?");

        var phrase = string.Join(delimiter, Enumerable.Range(0, words).Select(_ => WordList[RandomNumberGenerator.GetInt32(WordList.Length)]));
        if (includeDigits)
        {
            phrase += delimiter + RandomNumberGenerator.GetInt32(10, 100);
        }

        ConsoleUi.ResetScreen("Passphrase");
        ConsoleUi.Success(phrase);
        ConsoleUi.Pause();
    }
}
