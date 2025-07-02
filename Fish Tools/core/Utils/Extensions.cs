/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using System.IO.Compression;
using System.Reflection;

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

        public static string GetOutputPath(this Assembly assembly)
        {
            if (assembly.Location == null) return null;
            string name = Path.GetFileName(assembly.Location);
            return Path.Combine(assembly.Location.Remove(assembly.Location.Length - name.Length), $"{assembly.GetName().Name}-decompressed-resources\\");
        }

        public static byte[] Decompress(this byte[] data, Logger logger)
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

        public static void ProcessCompressedFile(this string inputFile, Logger logger)
        {
            string outputFileName = inputFile.Replace("costura.", string.Empty);
            outputFileName = outputFileName.Replace(".compressed", string.Empty);
            using (var bufferStream = File.OpenRead(inputFile))
            {
                File.WriteAllBytes(outputFileName, bufferStream.DecompressResource());
            }
            logger.Success($"Decompressed costura file: {inputFile}");
        }
    }
}
