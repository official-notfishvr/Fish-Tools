namespace FishTools.App;

internal sealed class AttributeSwitcher : ITool
{
    public string Id => "attribute-switcher";
    public string Name => "Attribute Switcher";
    public string Category => ToolCategories.Operations;
    public string Description => "Easily hide or unhide files and folders by toggling system attributes.";

    public Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        var rawPath = ConsoleUi.PromptRequired("File or folder path");
        var path = Helpers.NormalizePath(rawPath);

        if (!File.Exists(path) && !Directory.Exists(path))
        {
            ConsoleUi.Error("Path not found.");
            ConsoleUi.Pause();
            return Task.CompletedTask;
        }

        ConsoleUi.ResetScreen(Name);
        ConsoleUi.Info(path);
        var choice = ConsoleUi.ShowMenu("Choose an action", ["Hide path", "Unhide path", "Back"]);
        if (choice == 2)
        {
            return Task.CompletedTask;
        }

        try
        {
            var attributes = File.GetAttributes(path);

            if (choice == 0)
            {
                attributes |= FileAttributes.Hidden;
                attributes |= FileAttributes.System;
                File.SetAttributes(path, attributes);
                ConsoleUi.Success("Path was hidden.");
            }
            else
            {
                attributes &= ~FileAttributes.Hidden;
                attributes &= ~FileAttributes.System;
                File.SetAttributes(path, attributes);
                ConsoleUi.Success("Path was unhidden.");
            }

            ConsoleUi.Info(path);
        }
        catch (Exception ex)
        {
            ConsoleUi.Error(ex.Message);
        }

        ConsoleUi.Pause();
        return Task.CompletedTask;
    }
}
