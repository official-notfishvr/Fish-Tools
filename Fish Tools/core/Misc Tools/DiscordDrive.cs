// i cant get this shit to work it just keeps on crashing and idk why if you know how to fix it go to https://github.com/official-notfishvr/Fish-Tools/issues and say how too
/*
using System.Security.Cryptography;
using System.Text.Json;
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.Misc_Tools
{
    internal class DiscordDrive
    {
        private static HttpClient httpClient = new HttpClient();
        private static string WebhookFile = Program.dataDir + "\\WebhookURL.txt";
        private static string WebhookURL = "";
        private static string discordDriveDir = Program.dataDir + "\\DiscordDrive";

        public async static Task DiscordDriveMain(Logger Logger)
        {
            try
            {
                Logger.PrintArt();
                Logger.Info("Choose an option:");
                Logger.WriteBarrierLine("1", "Upload a file");
                Logger.WriteBarrierLine("2", "Download a file");
                Logger.WriteBarrierLine("3", "Hash a file");
                Console.Write("-> ");

                string option = Console.ReadLine();
                switch (option)
                {
                    case "1":
                        {
                            if (File.Exists(WebhookFile))
                            {
                                WebhookURL = File.ReadAllText(WebhookFile);
                            }
                            else
                            {
                                Console.Clear();
                                Logger.PrintArt();
                                Logger.Info("Enter Discord Webhook URL:");
                                Console.Write("-> ");
                                WebhookURL = Console.ReadLine();
                                File.WriteAllText(WebhookFile, WebhookURL);
                            }

                            Logger.Debug("Uploading File to " + WebhookURL);
                            Logger.Info("Enter Discord Webhook URL:");
                            Console.Write("-> ");
                            Logger.Info("File Path");
                            Console.Write("-> ");
                            string filename = Console.ReadLine();
                            filename = filename.Replace("\"", "");
                            if (filename != null)
                            {
                                DiscordFile file = UploadFile(File.OpenRead(filename), Path.GetFileName(filename), Logger);
                                if (file.Chunks.Count > 0)
                                {
                                    Directory.CreateDirectory($"{discordDriveDir}\\Uploaded");
                                    string filejson = JsonSerializer.Serialize(file, new JsonSerializerOptions() { WriteIndented = true });
                                    File.WriteAllText(Path.Combine($"{discordDriveDir}\\Uploaded", file.FileName + ".json"), filejson);
                                }
                            }
                        }
                        break;
                    case "2":
                        {
                            Logger.Info("File Path [look in ]");
                            Console.Write("-> ");
                            string filename = Console.ReadLine();
                            filename = filename.Replace("\"", "");
                            if (filename != null)
                            {
                                DiscordFile fileToDownload = JsonSerializer.Deserialize<DiscordFile>(File.ReadAllText(filename))!;
                                Logger.Debug("Loaded file: " + fileToDownload.FileName + " with " + fileToDownload.Chunks.Count + " chunks. " + fileToDownload.FileSize + " bytes.");
                                await DownloadFile(fileToDownload, Logger);
                                string downloadedFile = Path.Combine($"{discordDriveDir}\\Downloaded", fileToDownload.FileName);
                                if (File.Exists(downloadedFile))
                                {
                                    string hash = Hashing.SHA256CheckSum(downloadedFile);
                                    Console.WriteLine("SHA256 Checksum: " + hash);
                                    if (hash == fileToDownload.SHA256)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Logger.Success("File checksums match!");
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Logger.Error("File checksums DO NOT MATCH!");
                                    }
                                    Console.ResetColor();
                                }
                            }
                        }

                        break;
                    case "3":
                        {
                            Logger.Info("File Path");
                            Console.Write("-> ");
                            string filename = Console.ReadLine();
                            filename = filename.Replace("\"", "");
                            if (filename != null)
                            {
                                string hash = Hashing.SHA256CheckSum(filename);
                                Logger.Info("SHA256 Checksum: " + hash);
                            }
                            break;
                        }
                    default:
                        Logger.Error("Invalid option.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.ReadKey();
            }
        }
        private static DiscordFile UploadFile(Stream fileStream, string FileName, Logger Logger)
        {
            DiscordFile file = new();
            Logger.Debug($"Uploading {FileName}");
            file.FileName = FileName;
            file.FileSize = fileStream.Length;
            file.SHA256 = Hashing.SHA256CheckSum(fileStream);
            var chunks = SplitStream(fileStream, 1000000 * 8);
            Logger.Debug($"Filesize is {file.FileSize} bytes with {chunks.Count} chunks");
            Logger.Debug($"SHA256 Checksum: {file.SHA256}");

            for (int i = 0; i < chunks.Count; i++)
            {
                MultipartFormDataContent form = new MultipartFormDataContent
        {
            { new ByteArrayContent(chunks[i], 0, chunks[i].Length), "file1", (i + 1).ToString() }
        };

                if (i == 0)
                {
                    string payload = "{ \"embeds\": [ { \"title\": \"File Upload: %filename% | %filesize% Bytes | %chunks% Chunk(s)\", \"color\": 2913013 } ] }";
                    payload = payload.Replace("%filename%", file.FileName);
                    payload = payload.Replace("%filesize%", file.FileSize.ToString());
                    payload = payload.Replace("%chunks%", chunks.Count().ToString());
                    form.Add(new StringContent(payload), "payload_json");
                }

                try
                {
                    HttpResponseMessage response = await httpClient.PostAsync(WebhookURL, form);
                    if (response.IsSuccessStatusCode)
                    {
                        MessageModal.Root msg = JsonSerializer.Deserialize<MessageModal.Root>(await response.Content.ReadAsStringAsync());
                        Logger.Success("Successfully uploaded chunk " + (i + 1) + "/" + chunks.Count);
                        string url = msg.attachments[0].url;

                        if (i == 0)
                        {
                            string result = url.Substring(0, url.LastIndexOf('/'));
                            result = result.Substring(0, result.LastIndexOf('/') + 1);
                            file.BaseURL = result;
                        }
                        string[] urlsplit = url.Split('/');
                        string Route = urlsplit[5] + "/" + urlsplit[6];
                        file.Chunks.Add(new Chunk(i, Route));
                    }
                    else
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Logger.Error($"Failed to upload chunk {i}, error code {response.StatusCode}. Response body: {responseBody}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error while uploading chunk {i}: {ex.Message}");
                }
            }
            return file;
        }
        private async static Task DownloadFile(DiscordFile discordFile, Logger Logger)
        {
            Directory.CreateDirectory($"{discordDriveDir}\\Downloaded");
            using FileStream newFile = File.Create(Path.Combine($"{discordDriveDir}\\Downloaded", discordFile.FileName));
            foreach (Chunk chunk in discordFile.Chunks.OrderBy(x => x.PartNumber))
            {
                byte[] curChunk = httpClient.GetByteArrayAsync(discordFile.BaseURL + chunk.Route).GetAwaiter().GetResult();
                Logger.Debug("Downloaded chunk " + (chunk.PartNumber + 1) + "/" + discordFile.Chunks.Count);
                newFile.Write(curChunk, 0, curChunk.Length);
            }

            newFile.Flush();
        }
        private static List<byte[]> SplitStream(Stream stream, int chunkSize)
        {
            List<byte[]> result = new List<byte[]>();

            for (int i = 0; i < stream.Length; i += chunkSize)
            {
                stream.Position = i;
                if (i + chunkSize > stream.Length)
                {
                    byte[] chunk = new byte[stream.Length - i];

                    stream.Read(chunk, 0, (int)(stream.Length - i));
                    result.Add(chunk);
                }
                else
                {
                    byte[] chunk = new byte[chunkSize];
                    stream.Read(chunk, 0, chunkSize);
                    result.Add(chunk);
                }
            }
            return result;
        }
        internal static class Hashing
        {
            public static string SHA256CheckSum(Stream stream)
            {
                using (SHA256 SHA256 = SHA256Managed.Create())
                {
                    string result = "";
                    foreach (var hash in SHA256.ComputeHash(stream))
                    {
                        result += hash.ToString("x2");
                    }

                    return result;
                }
            }
            public static string SHA256CheckSum(string filePath)
            {
                using (SHA256 SHA256 = SHA256Managed.Create())
                {
                    using (FileStream fileStream = File.OpenRead(filePath))
                    {
                        string result = "";
                        foreach (var hash in SHA256.ComputeHash(fileStream))
                        {
                            result += hash.ToString("x2");
                        }
                        return result;
                    }
                }
            }
        }
        internal sealed class DiscordFile
        {
            public long FileSize { get; set; }
            public string FileName { get; set; }
            public string BaseURL { get; set; }
            public string SHA256 { get; set; }
            public List<Chunk> Chunks { get; set; } = new();
        }
        internal sealed class Chunk
        {
            public int PartNumber { get; set; }
            public string Route { get; set; }
            public Chunk(int PartNumber, string Route)
            {
                this.PartNumber = PartNumber;
                this.Route = Route;
            }
        }
        internal class MessageModal
        {
            public class Root
            {
                public string id { get; set; }
                public int type { get; set; }
                public string content { get; set; }
                public string channel_id { get; set; }
                public Author author { get; set; }
                public Attachment[] attachments { get; set; }
                public object[] embeds { get; set; }
                public object[] mentions { get; set; }
                public object[] mention_roles { get; set; }
                public bool pinned { get; set; }
                public bool mention_everyone { get; set; }
                public bool tts { get; set; }
                public DateTime timestamp { get; set; }
                public object edited_timestamp { get; set; }
                public int flags { get; set; }
                public object[] components { get; set; }
                public string webhook_id { get; set; }
            }
            public class Author
            {
                public bool bot { get; set; }
                public string id { get; set; }
                public string username { get; set; }
                public object avatar { get; set; }
                public string discriminator { get; set; }
            }
            public class Attachment
            {
                public string id { get; set; }
                public string filename { get; set; }
                public int size { get; set; }
                public string url { get; set; }
                public string proxy_url { get; set; }
            }
        }
    }
}
*/