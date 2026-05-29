using FishTools.App;

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    await RunAsync(cts.Token);
}
catch (OperationCanceledException) { }

internal static partial class Program
{
    public static async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var paths = new AppPaths(AppContext.BaseDirectory);
        paths.EnsureCreated();

        var settings = AppSettingsStore.Load(paths.SettingsFilePath);
        var context = new ToolContext(paths, settings);
        var tools = ToolCatalog.Create().OrderBy(t => ToolCategories.GetOrder(t.Category)).ThenBy(t => t.Name).ToArray();

        while (!cancellationToken.IsCancellationRequested)
        {
            ConsoleUi.ResetScreen("Main Menu", ConsoleColor.Green);

            var categoryGroups = tools.Where(settings.IsEnabled).GroupBy(t => t.Category).OrderBy(g => ToolCategories.GetOrder(g.Key)).ToList();

            var menuOptions = categoryGroups.Select(g => $"{g.Key}  ({g.Count()})").Append("Search for a tool").Append("Manage tool library").Append("Exit").ToList();

            var selected = ConsoleUi.ShowMenu("Select a section", menuOptions);

            if (selected < categoryGroups.Count)
            {
                await ShowCategoryAsync(categoryGroups[selected].Key, tools, context, cancellationToken);
                continue;
            }

            var offset = selected - categoryGroups.Count;
            switch (offset)
            {
                case 0:
                    await SearchAsync(tools, context, cancellationToken);
                    break;
                case 1:
                    ShowManageTools(tools, context);
                    break;
                default:
                    return;
            }
        }
    }

    private static async Task ShowCategoryAsync(string category, IReadOnlyList<ITool> tools, ToolContext context, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ConsoleUi.ResetScreen(category, ConsoleColor.Cyan);

            var categoryTools = tools.Where(t => t.Category == category && context.Settings.IsEnabled(t)).OrderBy(t => t.Name).ToArray();

            if (categoryTools.Length == 0)
            {
                ConsoleUi.Warning("No tools are currently enabled in this category.");
                ConsoleUi.Pause();
                return;
            }

            var options = categoryTools.Select(t => t.Name).Append("Go back").ToArray();
            var selected = ConsoleUi.ShowMenu("Available Tools", options);

            if (selected == categoryTools.Length)
                return;

            var tool = categoryTools[selected];
            await ShowToolDetailAsync(tool, context, cancellationToken);
        }
    }

    private static async Task ShowToolDetailAsync(ITool tool, ToolContext context, CancellationToken cancellationToken)
    {
        ConsoleUi.ResetScreen(tool.Name, ConsoleColor.Magenta);
        ConsoleUi.Section("Description");
        ConsoleUi.Info(tool.Description);
        Fish.Console.FishConsole.WriteLine();

        if (!ConsoleUi.Confirm("Launch this tool?"))
            return;

        await RunToolSafeAsync(tool, context, cancellationToken);
    }

    private static async Task RunToolSafeAsync(ITool tool, ToolContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await tool.RunAsync(context);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            ConsoleUi.ResetScreen(tool.Name, ConsoleColor.Red);
            ConsoleUi.Error("An error occurred during execution:");
            ConsoleUi.Info(ex.Message);
            ConsoleUi.Pause();
        }
    }

    private static async Task SearchAsync(IReadOnlyList<ITool> tools, ToolContext context, CancellationToken cancellationToken)
    {
        ConsoleUi.ResetScreen("Search Tools");
        var query = ConsoleUi.Prompt("Enter tool name or description").Trim();

        if (string.IsNullOrEmpty(query))
            return;

        var matches = tools.Where(t => t.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || t.Description.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        if (matches.Count == 0)
        {
            ConsoleUi.Warning("No tools matched your search.");
            ConsoleUi.Pause();
            return;
        }

        var options = matches.Select(t => $"{t.Name}  [{t.Category}]").Append("Back").ToList();
        var selected = ConsoleUi.ShowMenu($"Results for \"{query}\"", options);

        if (selected >= matches.Count)
            return;

        var tool = matches[selected];

        if (!context.Settings.IsEnabled(tool))
        {
            ConsoleUi.Warning("This tool is currently disabled.");
            if (!ConsoleUi.Confirm("Enable and run it?"))
                return;

            context.Settings.SetEnabled(tool, true);
        }

        await ShowToolDetailAsync(tool, context, cancellationToken);
    }

    private static void ShowManageTools(IReadOnlyList<ITool> tools, ToolContext context)
    {
        while (true)
        {
            ConsoleUi.ResetScreen("Manage Tool Library", ConsoleColor.Yellow);

            var enabledCount = tools.Count(context.Settings.IsEnabled);
            ConsoleUi.Info($"Enabled: {enabledCount} / {tools.Count}");
            Fish.Console.FishConsole.WriteLine();

            var sortedTools = tools.OrderBy(t => ToolCategories.GetOrder(t.Category)).ThenBy(t => t.Name).ToList();

            var options = sortedTools
                .Select(t =>
                {
                    var status = context.Settings.IsEnabled(t) ? "[ON] " : "[OFF]";
                    return $"{status}  {t.Name, -24}  {t.Category}";
                })
                .Append("Enable all")
                .Append("Disable all")
                .Append("Back")
                .ToList();

            var selected = ConsoleUi.ShowMenu("Toggle tools", options);

            if (selected < sortedTools.Count)
            {
                var tool = sortedTools[selected];
                context.Settings.SetEnabled(tool, !context.Settings.IsEnabled(tool));
                continue;
            }

            var offset = selected - sortedTools.Count;
            switch (offset)
            {
                case 0:
                    context.Settings.EnableAll(tools);
                    break;
                case 1:
                    context.Settings.DisableAll(tools);
                    break;
                default:
                    return;
            }
        }
    }
}
