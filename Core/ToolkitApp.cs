namespace FishTools.App;

internal static class ToolkitApp
{
    public static async Task RunAsync()
    {
        var paths = new AppPaths(AppContext.BaseDirectory);
        paths.EnsureCreated();

        var settings = AppSettingsStore.Load(paths.SettingsFilePath);
        var context = new ToolContext(paths, settings);
        var tools = ToolCatalog.Create().OrderBy(t => ToolCategories.GetOrder(t.Category)).ThenBy(t => t.Name).ToArray();

        while (true)
        {
            ConsoleUi.ResetScreen("Main Menu", ConsoleColor.Green);

            var categoryGroups = tools.Where(settings.IsEnabled).GroupBy(t => t.Category).OrderBy(g => ToolCategories.GetOrder(g.Key)).ToList();

            var categoryOptions = categoryGroups.Select(g => $"{g.Key} ({g.Count()})").ToList();

            var menuOptions = new List<string>(categoryOptions) { "Search for a tool", "Manage tool library", "About rebuild", "Exit" };

            var selected = ConsoleUi.ShowMenu("Select a section", menuOptions);

            if (selected < categoryGroups.Count)
            {
                await ShowCategoryAsync(categoryGroups[selected].Key, tools, context);
                continue;
            }

            var offset = selected - categoryGroups.Count;
            switch (offset)
            {
                case 0:
                    await SearchAsync(tools, context);
                    break;
                case 1:
                    ShowManageTools(tools, context);
                    break;
                case 2:
                    ShowAbout();
                    break;
                default:
                    return;
            }
        }
    }

    private static async Task SearchAsync(IReadOnlyList<ITool> tools, ToolContext context)
    {
        ConsoleUi.ResetScreen("Search Tools");
        var query = ConsoleUi.Prompt("Enter tool name or description").Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(query))
            return;

        var matches = tools.Where(t => t.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || t.Description.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        if (matches.Count == 0)
        {
            ConsoleUi.Warning("No tools matched your search.");
            ConsoleUi.Pause();
            return;
        }

        var options = matches.Select(t => $"{t.Name} [{t.Category}]").Append("Back").ToArray();
        var selected = ConsoleUi.ShowMenu("Search Results", options);

        if (selected < matches.Count)
        {
            var tool = matches[selected];
            if (!context.Settings.IsEnabled(tool))
            {
                ConsoleUi.Warning("This tool is currently disabled in settings.");
                if (ConsoleUi.Confirm("Enable and run it?"))
                {
                    context.Settings.SetEnabled(tool, true);
                }
                else
                    return;
            }

            await RunToolSafeAsync(tool, context);
        }
    }

    private static async Task RunToolSafeAsync(ITool tool, ToolContext context)
    {
        try
        {
            await tool.RunAsync(context);
        }
        catch (Exception ex)
        {
            ConsoleUi.ResetScreen(tool.Name, ConsoleColor.Red);
            ConsoleUi.Error("An error occurred during execution:");
            ConsoleUi.Info(ex.Message);
            ConsoleUi.Pause();
        }
    }

    private static async Task ShowCategoryAsync(string category, IReadOnlyList<ITool> tools, ToolContext context)
    {
        while (true)
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
            ConsoleUi.ResetScreen(tool.Name, ConsoleColor.Magenta);
            ConsoleUi.Section("Description");
            ConsoleUi.Info(tool.Description);
            Fish.Console.FishConsole.WriteLine();

            if (ConsoleUi.Confirm("Launch this tool?"))
            {
                await RunToolSafeAsync(tool, context);
            }
        }
    }

    private static void ShowManageTools(IReadOnlyList<ITool> tools, ToolContext context)
    {
        while (true)
        {
            ConsoleUi.ResetScreen("Management Console", ConsoleColor.Yellow);
            ConsoleUi.Info("Configure your active toolset.");
            ConsoleUi.Info($"Enabled: {tools.Count(context.Settings.IsEnabled)} / {tools.Count}");
            Fish.Console.FishConsole.WriteLine();

            var sortedTools = tools.OrderBy(t => ToolCategories.GetOrder(t.Category)).ThenBy(t => t.Name).ToList();

            var options = sortedTools
                .Select(t =>
                {
                    var status = context.Settings.IsEnabled(t) ? "[ON]" : "[OFF]";
                    return $"{status, -5} {t.Name, -24} ({t.Category})";
                })
                .Append("Enable All Tools")
                .Append("Disable All Tools")
                .Append("Back")
                .ToArray();

            var selected = ConsoleUi.ShowMenu("Library Configuration", options);

            if (selected < sortedTools.Count)
            {
                var tool = sortedTools[selected];
                context.Settings.SetEnabled(tool, !context.Settings.IsEnabled(tool));
                continue;
            }

            var offset = selected - sortedTools.Count;
            if (offset == 0)
                context.Settings.EnableAll(tools);
            else if (offset == 1)
                context.Settings.DisableAll(tools);
            else
                return;
        }
    }

    private static void ShowAbout()
    {
        ConsoleUi.ResetScreen("System Manifesto", ConsoleColor.White);
        ConsoleUi.Section("The Rebuild");
        ConsoleUi.Info("This suite represents a full architectural redesign of Fish Tools.");
        ConsoleUi.Info("Focus: Portability, modularity, and high-performance I/O.");
        Fish.Console.FishConsole.WriteLine();
        ConsoleUi.WriteSafeScope();
        ConsoleUi.Pause();
    }
}
