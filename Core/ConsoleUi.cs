using Fish.Console;
using FishConsole = Fish.Console.FishConsole;

namespace FishTools.App;

internal static class ConsoleUi
{
    public static void ResetScreen(string? subtitle = null, ConsoleColor? subtitleColor = ConsoleColor.Cyan)
    {
        FishConsole.Clear();
        FishConsole.SetTitle("Fish Tools");
    }

    public static void Section(string title)
    {
        FishConsole.WriteLine(ConsoleColor.Yellow, $"> {title}");
        FishConsole.WriteThinSeparator(Math.Max(32, title.Length + 10), color: ConsoleColor.DarkGray);
    }

    public static void Info(string message) => FishConsole.WriteLine(ConsoleColor.Gray, $"  {message}");

    public static void Success(string message) => FishConsole.WriteLine(ConsoleColor.Green, $"  [✓] {message}");

    public static void Warning(string message) => FishConsole.WriteLine(ConsoleColor.Yellow, $"  [!] {message}");

    public static void Error(string message) => FishConsole.WriteLine(ConsoleColor.Red, $"  [X] {message}");

    public static string Prompt(string message, string defaultValue = "") => FishConsole.ShowPrompt($"  {message}", defaultValue);

    public static string PromptRequired(string message)
    {
        while (true)
        {
            var value = Prompt(message).Trim();
            if (!string.IsNullOrEmpty(value))
                return value;
            Error("A value is required.");
        }
    }

    public static int PromptInt(string message, int defaultValue, int minValue = int.MinValue, int maxValue = int.MaxValue)
    {
        while (true)
        {
            var raw = Prompt(message, defaultValue.ToString());
            if (int.TryParse(raw, out var value) && value >= minValue && value <= maxValue)
                return value;
            Error($"Enter a number between {minValue} and {maxValue}.");
        }
    }

    public static bool Confirm(string message) => Fish.Console.FishConsole.ShowYesNo($"  {message}");

    public static void Pause(string message = "Press any key to continue...")
    {
        FishConsole.WriteLine();
        FishConsole.WriteLine(ConsoleColor.DarkGray, $"  {message}");
        FishConsole.ReadKey(true);
    }

    public static int ShowMenu(string title, IEnumerable<string> options)
    {
        var menuOptions = options.ToArray();
        var selected = 0;
        var startY = FishConsole.Position.Y;
        var width = Math.Max(40, FishConsole.WindowWidth - 4);
        var totalLines = 2 + menuOptions.Length;

        while (true)
        {
            for (var lineOffset = 0; lineOffset < totalLines; lineOffset++)
            {
                FishConsole.WriteAt(new string(' ', FishConsole.WindowWidth), 0, startY + lineOffset);
            }

            WriteMenuLine(2, startY, $"┌─┤ {title} ├─" + new string('─', Math.Max(0, width - title.Length - 10)) + "┐", ConsoleColor.DarkGray, width);

            for (var i = 0; i < menuOptions.Length; i++)
            {
                var isSelected = i == selected;
                var prefix = isSelected ? "  > " : "    ";
                var color = isSelected ? ConsoleColor.Cyan : ConsoleColor.White;
                var text = menuOptions[i];

                FishConsole.WriteAt(prefix, 2, startY + 1 + i, color);
                FishConsole.WriteAt(text, 6, startY + 1 + i, color);
            }

            WriteMenuLine(2, startY + 1 + menuOptions.Length, "└" + new string('─', width - 2) + "┘", ConsoleColor.DarkGray, width);

            var key = FishConsole.ReadKeyEx(true);
            if (key.Key == ConsoleKey.UpArrow)
                selected = (selected - 1 + menuOptions.Length) % menuOptions.Length;
            else if (key.Key == ConsoleKey.DownArrow)
                selected = (selected + 1) % menuOptions.Length;
            else if (key.Key == ConsoleKey.Enter)
            {
                FishConsole.Move(0, startY + totalLines + 1);
                return selected;
            }
            else if (char.IsDigit(key.KeyChar))
            {
                var choice = key.KeyChar - '0' - 1;
                if (choice >= 0 && choice < menuOptions.Length)
                {
                    FishConsole.Move(0, startY + totalLines + 1);
                    return choice;
                }
            }
        }
    }

    private static void WriteMenuLine(int x, int y, string text, ConsoleColor color, int width)
    {
        if (text.Length > width)
        {
            text = text[..width];
        }

        FishConsole.WriteAt(text.PadRight(width), x, y, color);
    }

    public static void WriteSafeScope()
    {
        Section("Included");
        Info("File cleanup, organization, duplicate detection, hashing, password tools, file encryption, system info, speed tests, data conversion, and email extraction.");
        FishConsole.WriteLine();
    }

    public static string ExistingDirectoryPrompt(string message, string defaultValue = "")
    {
        while (true)
        {
            var path = Helpers.NormalizePath(Prompt(message, defaultValue));
            if (Directory.Exists(path))
            {
                return path;
            }

            Error("Directory not found.");
        }
    }

    public static string ExistingFilePrompt(string message, string defaultValue = "")
    {
        while (true)
        {
            var path = Helpers.NormalizePath(Prompt(message, defaultValue));
            if (File.Exists(path))
            {
                return path;
            }

            Error("File not found.");
        }
    }
}
