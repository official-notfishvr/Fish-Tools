using System.IO.Compression;
using System.Reflection;

namespace FishTools.App;

internal sealed class ResourceExtractor : ITool
{
    public string Id => "resource-extractor";
    public string Name => "Costura Resource Extractor";
    public string Category => ToolCategories.Analysis;
    public string Description => "Extract Costura-compressed embedded resources (DLLs) from .NET assemblies.";

    public Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        ConsoleUi.Info("Use this on assemblies you are authorized to inspect.");
        var assemblyPath = ConsoleUi.ExistingFilePrompt("Assembly path (.exe or .dll)");
        var outputRoot = Helpers.EnsureUniquePath(Path.Combine(context.Paths.ResultsDirectory, $"{Path.GetFileNameWithoutExtension(assemblyPath)}-costura"));
        ConsoleUi.ResetScreen(Name);

        var extracted = 0;

        try
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            Directory.CreateDirectory(outputRoot);

            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (!resourceName.StartsWith("costura.", StringComparison.OrdinalIgnoreCase) || !resourceName.EndsWith(".compressed", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream is null)
                {
                    continue;
                }

                var name = resourceName["costura.".Length..resourceName.LastIndexOf(".compressed", StringComparison.OrdinalIgnoreCase)];
                var outputPath = Path.Combine(outputRoot, name);
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

                using var deflate = new DeflateStream(stream, CompressionMode.Decompress);
                using var output = File.Create(outputPath);
                deflate.CopyTo(output);
                extracted++;
            }
        }
        catch (Exception ex)
        {
            ConsoleUi.Error(ex.Message);
            ConsoleUi.Pause();
            return Task.CompletedTask;
        }

        if (extracted == 0)
        {
            ConsoleUi.Warning("No Costura-compressed resources were found.");
        }
        else
        {
            ConsoleUi.Success($"Extracted {extracted} resources to {outputRoot}");
        }

        ConsoleUi.Pause();
        return Task.CompletedTask;
    }
}
