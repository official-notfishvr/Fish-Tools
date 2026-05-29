using System.Text.RegularExpressions;

namespace FishTools.App;

internal sealed class Il2CppCleanerTool : ITool
{
    public string Id => "il2cpp-cleaner";
    public string Name => "il2cpp.h File Cleaner";
    public string Category => ToolCategories.Operations;
    public string Description => "Cleans up an il2cpp.h file by fixing macros and reserved keywords.";

    public Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        
        string filePath = ConsoleUi.PromptRequired("Enter path to il2cpp.h file");
        filePath = filePath.Trim('"', '\'');
        
        string absolutePath = Path.GetFullPath(filePath);
        if (!File.Exists(absolutePath))
        {
            ConsoleUi.Error("File not found: " + absolutePath);
            ConsoleUi.Pause();
            return Task.CompletedTask;
        }

        ConsoleUi.Info("Reading " + absolutePath);
        string content = File.ReadAllText(absolutePath);

        ConsoleUi.Info("Cleaning...");

        content = content.Replace("intptr_t HKEY_CLASSES_ROOT;", "intptr_t _HKEY_CLASSES_ROOT;");
        content = content.Replace("intptr_t HKEY_CURRENT_USER;", "intptr_t _HKEY_CURRENT_USER;");
        content = content.Replace("intptr_t HKEY_LOCAL_MACHINE;", "intptr_t _HKEY_LOCAL_MACHINE;");
        content = content.Replace("intptr_t HKEY_USERS;", "intptr_t _HKEY_USERS;");
        content = content.Replace("intptr_t HKEY_PERFORMANCE_DATA;", "intptr_t _HKEY_PERFORMANCE_DATA;");
        content = content.Replace("intptr_t HKEY_CURRENT_CONFIG;", "intptr_t _HKEY_CURRENT_CONFIG;");
        content = content.Replace("intptr_t HKEY_DYN_DATA;", "intptr_t _HKEY_DYN_DATA;");

        content = Regex.Replace(content, @"\* stdout;", "* _stdout;");
        content = Regex.Replace(content, @"\* stderr;", "* _stderr;");
        content = Regex.Replace(content, @"\* stdin;", "* _stdin;");

        content = Regex.Replace(content, @"\* interface;", "* _interface;");

        content = Regex.Replace(content, @"\* DELETE;", "* _DELETE;");
        content = Regex.Replace(content, @"\* IN;", "* _IN;");
        content = Regex.Replace(content, @"\* NULL;", "* _NULL;");
        
        content = Regex.Replace(content, @"\* FAR;", "* _FAR;");
        content = Regex.Replace(content, @"\* NEAR;", "* _NEAR;");
        content = Regex.Replace(content, @"\* OUT;", "* _OUT;");
        
        content = content.Replace("int32_t __int32;", "int32_t m_int32;");
        content = content.Replace("int32_t _int32;", "int32_t m_int32;");

        ConsoleUi.Info("Writing output...");
        File.WriteAllText(absolutePath, content);
        ConsoleUi.Success("il2cpp.h successfully cleaned.");
        
        ConsoleUi.Pause();
        return Task.CompletedTask;
    }
}
