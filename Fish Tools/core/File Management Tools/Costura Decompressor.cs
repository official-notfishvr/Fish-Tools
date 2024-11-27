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
    internal class CosturaDecompressor
    {
        private readonly Assembly _assembly;

        private readonly string _outputPath;

        public CosturaDecompressor(Assembly assembly)
        {
            _assembly = assembly;
            _outputPath = assembly.GetOutputPath();
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
