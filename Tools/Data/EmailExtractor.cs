using System.Text.RegularExpressions;

namespace FishTools.App;

internal sealed class EmailExtractor : ITool
{
    private static readonly Regex EmailIdRegex = new(@"[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public string Id => "email-extractor";
    public string Name => "Email Extractor";
    public string Category => ToolCategories.DataText;
    public string Description => "Harvest unique email addresses from files or entire directory trees.";

    public Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        var source = ConsoleUi.ShowMenu("Choose input source", ["Single Text File", "Recursive Folder Scan", "Back"]);
        if (source == 2)
            return Task.CompletedTask;

        IEnumerable<string> files = source == 0 ? [ConsoleUi.ExistingFilePrompt("Target File")] : Helpers.SafeEnumerateFiles(ConsoleUi.ExistingDirectoryPrompt("Target Directory"), "*", SearchOption.AllDirectories);

        var filter = ConsoleUi.Prompt("Extension filter (e.g. .txt,.json), blank for all", "");
        var extensions = filter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (extensions.Count > 0)
            files = files.Where(f => extensions.Contains(Path.GetExtension(f)));

        var emailsFound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in files)
        {
            try
            {
                var content = File.ReadAllText(file);
                foreach (Match m in EmailIdRegex.Matches(content))
                    emailsFound.Add(m.Value);
            }
            catch { }
        }

        ConsoleUi.ResetScreen(Name);
        ConsoleUi.Success($"Harvesting Complete. Unique addresses: {emailsFound.Count}");
        foreach (var email in emailsFound.Take(50))
            ConsoleUi.Info(email);

        if (emailsFound.Count > 0 && ConsoleUi.Confirm("Save harvested list to Results?"))
        {
            var report = context.CreateReportPath("email_extract");
            File.WriteAllLines(report, emailsFound.OrderBy(e => e, StringComparer.OrdinalIgnoreCase));
            ConsoleUi.Success($"List saved to {report}");
        }

        ConsoleUi.Pause();
        return Task.CompletedTask;
    }
}
