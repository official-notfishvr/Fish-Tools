/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using System.Reflection;
using System.Text.Json;

namespace Fish_Tools.core.Utils
{

    public static class ToolManager
    {
        private static List<ITool> _tools = new();
        private static Dictionary<string, List<ITool>> _toolsByCategory = new();
        private static string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "tools_config.json");
        

        public static void Initialize()
        {
            DiscoverTools();
            LoadToolConfigurations();
        }
        

        private static void DiscoverTools()
        {
            _tools.Clear();
            _toolsByCategory.Clear();
            

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    var toolTypes = assembly.GetTypes()
                        .Where(t => typeof(ITool).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                        .ToList();
                    
                    foreach (var toolType in toolTypes)
                    {
                        try
                        {
                            var tool = (ITool)Activator.CreateInstance(toolType);
                            _tools.Add(tool);
                            

                            if (!_toolsByCategory.ContainsKey(tool.Category))
                            {
                                _toolsByCategory[tool.Category] = new List<ITool>();
                            }
                            _toolsByCategory[tool.Category].Add(tool);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to instantiate tool {toolType.Name}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to process assembly {assembly.FullName}: {ex.Message}");
                }
            }
        }
        

        public static List<string> GetCategories()
        {
            return _toolsByCategory.Keys.ToList();
        }
        

        public static List<ITool> GetToolsByCategory(string category)
        {
            return _toolsByCategory.ContainsKey(category) ? _toolsByCategory[category] : new List<ITool>();
        }
        

        public static List<ITool> GetAllTools()
        {
            return _tools.ToList();
        }
        

        public static void SetToolEnabled(string toolName, bool enabled)
        {
            var tool = _tools.FirstOrDefault(t => t.Name == toolName);
            if (tool != null)
            {
                tool.IsEnabled = enabled;
                SaveToolConfigurations();
            }
        }
        

        public static void SetCategoryEnabled(string category, bool enabled)
        {
            if (_toolsByCategory.ContainsKey(category))
            {
                foreach (var tool in _toolsByCategory[category])
                {
                    tool.IsEnabled = enabled;
                }
                SaveToolConfigurations();
            }
        }
        

        private static void LoadToolConfigurations()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    var config = JsonSerializer.Deserialize<Dictionary<string, bool>>(json);
                    
                    if (config != null)
                    {
                        foreach (var kvp in config)
                        {
                            var tool = _tools.FirstOrDefault(t => t.Name == kvp.Key);
                            if (tool != null)
                            {
                                tool.IsEnabled = kvp.Value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load tool configurations: {ex.Message}");
            }
        }
        

        private static void SaveToolConfigurations()
        {
            try
            {
                var config = _tools.ToDictionary(t => t.Name, t => t.IsEnabled);
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                
                var directory = Path.GetDirectoryName(_configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save tool configurations: {ex.Message}");
            }
        }
        

        public static List<ITool> GetEnabledToolsByCategory(string category)
        {
            return GetToolsByCategory(category).Where(t => t.IsEnabled).ToList();
        }


        public static bool IsFormTool(ITool tool)
        {
            return tool.GetType().IsSubclassOf(typeof(System.Windows.Forms.Form));
        }
        

        public static List<ITool> GetAllEnabledTools()
        {
            return _tools.Where(t => t.IsEnabled).ToList();
        }
    }
} 