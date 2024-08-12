using System.IO.Compression;
using System.IO;

namespace Fish_Tools.core.Utils
{
    public static class Extensions
    {
        private static byte[] DecompressResource(this Stream input)
        {
            using (var output = new MemoryStream())
            {
                using (var deflateStream = new DeflateStream(input, CompressionMode.Decompress))
                {
                    deflateStream.CopyTo(output);
                    return output.ToArray();
                }
            }
        }

        public static string GetOutputPath(this AsmResolver.DotNet.ModuleDefinition module)
        {
            if (module.FilePath == null) return null;
            if (module.Assembly == null) return null;
            string name = Path.GetFileName(module.FilePath);
            return Path.Combine(module.FilePath.Remove(module.FilePath.Length - name.Length), $@"{module.Assembly.Name}-decompressed-resources\");
        }

        public static byte[] Decompress(this byte[] data, Logger Logger)
        {
            using (var input = new MemoryStream(data))
            {
                using (var output = new MemoryStream())
                {
                    using (var deflateStream = new DeflateStream(input, CompressionMode.Decompress))
                    {
                        deflateStream.CopyTo(output);
                        return output.ToArray();
                    }
                }
            }
        }

        public static void ProcessCompressedFile(this string inputFile, Logger Logger)
        {
            string outputFileName = inputFile.Replace("costura.", null);
            outputFileName = outputFileName.Replace(".compressed", null);
            using (var bufferStream = File.OpenRead(inputFile)) File.WriteAllBytes(outputFileName, bufferStream.DecompressResource());
            Logger.Success($"Decompressed costura file: {inputFile}");
        }
    }
}
