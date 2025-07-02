/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using System.Reflection;
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.FileManagementTools
{
    internal class CosturaDecompressor : ITool
    {
        public string Name => "Costura Decompressor";
        public string Category => "File Management Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Extract embedded resources from Costura-compressed assemblies";

        private readonly Assembly _assembly;
        private readonly string _outputPath;

        public CosturaDecompressor(Assembly assembly)
        {
            _assembly = assembly;
            _outputPath = assembly.GetOutputPath();
        }

        public void Main(Logger logger)
        {
            Console.Clear();
            logger.PrintArt();
            
            logger.Info("Costura Decompressor");
            logger.Info("This tool extracts embedded resources from Costura-compressed assemblies.");
            
            Console.Write("Enter the path to the assembly file (.exe or .dll): ");
            string assemblyPath = Console.ReadLine();
            
            if (string.IsNullOrEmpty(assemblyPath) || !File.Exists(assemblyPath))
            {
                logger.Error("Invalid file path or file does not exist.");
                logger.Info("Press any key to return to menu...");
                Console.ReadKey();
                return;
            }
            
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var decompressor = new CosturaDecompressor(assembly);
                decompressor.Run(logger);
            }
            catch (Exception ex)
            {
                logger.Error($"Error processing assembly: {ex.Message}");
            }
            
            logger.Info("Press any key to return to menu...");
            Console.ReadKey();
        }

        public void Run(Logger logger)
        {
            if (ExtractResources(out var resources, logger))
            {
                SaveResources(resources, logger);
                return;
            }

            logger.Error("Could not find or extract costura embedded resources");
        }

        private bool ExtractResources(out Dictionary<byte[], string> resources, Logger logger)
        {
            resources = new Dictionary<byte[], string>();

            var manifestResourceNames = _assembly.GetManifestResourceNames();
            if (manifestResourceNames.Length == 0) return false;

            foreach (var resourceName in manifestResourceNames)
            {
                if (!resourceName.StartsWith("costura.") || !resourceName.EndsWith(".compressed")) continue;

                string name = resourceName.Substring(8, resourceName.LastIndexOf(".compressed") - 8);

                using (var stream = _assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) continue;
                    byte[] data;
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        data = memoryStream.ToArray();
                    }

                    resources.Add(data.Decompress(logger), name);
                    logger.Success($"Extracted costura resource {name}");
                }
            }

            return resources.Count != 0;
        }

        private void SaveResources(Dictionary<byte[], string> extractedResources, Logger logger)
        {
            if (!Directory.Exists(_outputPath)) Directory.CreateDirectory(_outputPath);

            foreach (var entry in extractedResources)
            {
                string fullPath = Path.Combine(_outputPath, entry.Value);
                File.WriteAllBytes(fullPath, entry.Key);
            }

            logger.Info($"Saved {extractedResources.Count} extracted resources to {_outputPath}");
        }
    }
}
