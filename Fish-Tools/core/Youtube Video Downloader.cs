using Fish_Tools.core.Utils;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Fish_Tools.core
{
    internal class YVD
    {
        public YVD() { }

        public static void Main(Logger Logger)
        {
            try
            {
                if (!File.Exists("youtube-dl.exe"))
                {
                    Logger.Write("A required dependency has been found, Will Start Download Now...");
                    StartDownload("https://drive.usercontent.google.com/download?id=1ecakVNbQECFq092t0VanaLzoWeDhmlBp&export=download&authuser=0&confirm=t&uuid=11d77ebc-1616-4ef3-af28-e58c43ffc112&at=APZUnTWryDqgWRO5NqItB3Ih0L8g:1719477914208", "youtube-dl.exe", Logger);
                }

                if (!File.Exists("ffmpeg.exe"))
                {
                    Logger.Write("A required dependency has been found, Will Start Download Now...");
                    StartDownload("https://drive.usercontent.google.com/download?id=1I0CCiznF_UFzkuluMAZdRtAOCfVJ-SNq&export=download&authuser=0&confirm=t&uuid=c7a332de-3913-4676-839e-e4bffa0c3a81&at=APZUnTWPbHYgn0Z3cX10pMNgX_Nn:1719477750530", "ffmpeg.exe", Logger);
                }

                if (File.Exists("ffmpeg.exe") && File.Exists("youtube-dl.exe"))
                {
                    Logger.WriteBarrierLine("1", "Text File");
                    Logger.WriteBarrierLine("2", "Playlist URL");
                    Logger.WriteBarrierLine("3", "Download By Artist");
                    Logger.WriteBarrierLine("4", "URL");
                    Console.Write("-> ");

                    ConsoleKey choice2 = Console.ReadKey().Key;

                    if (choice2 == ConsoleKey.D1)
                    {
                        Console.Clear();
                        Logger.PrintArt();
                        HandleTextFileDownload(Logger);
                    }
                    else if (choice2 == ConsoleKey.D2)
                    {
                        Console.Clear();
                        Logger.PrintArt();
                        HandlePlaylistDownload(Logger);
                    }
                    else if (choice2 == ConsoleKey.D3)
                    {
                        Console.Clear();
                        Logger.PrintArt();
                        HandleArtistDownload(Logger);
                    }
                    else if (choice2 == ConsoleKey.D4)
                    {
                        Console.Clear();
                        Logger.PrintArt();
                        HandleURLDownload(Logger);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write($"An error occurred: {ex.Message}");
            }
        }
        private static void HandleTextFileDownload(Logger Logger)
        {
            Logger.Write("What Text File Would You Like To Load? (Put Full Path)");
            Console.Write("-> ");
            string URLFullPath = Console.ReadLine();
            if (string.IsNullOrEmpty(URLFullPath) || !File.Exists(URLFullPath))
            {
                Logger.Write("Invalid file path provided.");
                return;
            }

            string[] VideoURLs = File.ReadAllLines(URLFullPath);
            Console.Clear();
            Logger.Write("Where Would You Like To Save The Videos Too (PUT FULL FOLDER PATH)");
            Console.Write("-> ");
            string FullVideoSavePath = Console.ReadLine();
            if (string.IsNullOrEmpty(FullVideoSavePath) || !Directory.Exists(FullVideoSavePath))
            {
                Logger.Write("Invalid directory path provided.");
                return;
            }

            Console.Clear();
            Logger.Write("Would You Like To Download Video Or MP3?");
            Logger.WriteBarrierLine("1", "Video");
            Logger.WriteBarrierLine("2", "MP3");
            Console.Write("-> ");
            string WhatType = Console.ReadLine();
            Console.Clear();

            foreach (string line in VideoURLs)
            {
                StartDownloadProcess(line, FullVideoSavePath, WhatType, Logger);
            }
        }
        private static void HandlePlaylistDownload(Logger Logger)
        {
            Logger.Write("What Is The Playlist URL? (ONLY ID) -- EXAMPLE PL3oW2tjiIxvQW6c-4Iry8Bpp3QId40S5S");
            Console.Write("-> ");
            string PlayListURLFull = Console.ReadLine();
            if (string.IsNullOrEmpty(PlayListURLFull))
            {
                Logger.Write("Invalid playlist URL provided.");
                return;
            }

            Console.Clear();
            Logger.Write("Where Would You Like To Save The Videos Too (PUT FULL FOLDER PATH)");
            Console.Write("-> ");
            string FullVideoSavePath = Console.ReadLine();
            if (string.IsNullOrEmpty(FullVideoSavePath) || !Directory.Exists(FullVideoSavePath))
            {
                Logger.Write("Invalid directory path provided.");
                return;
            }

            Console.Clear();
            Logger.Write("Would You Like To Download Video Or MP3?");
            Logger.WriteBarrierLine("1", "Video");
            Logger.WriteBarrierLine("2", "MP3");
            Console.Write("-> ");
            string WhatType = Console.ReadLine();
            Console.Clear();

            string arguments = WhatType == "1"
                ? $"https://www.youtube.com/playlist?list={PlayListURLFull} --yes-playlist -f best --audio-format best -o {FullVideoSavePath}/%(title)s.%(ext)s"
                : $"https://www.youtube.com/playlist?list={PlayListURLFull} --yes-playlist -f best --audio-format mp3 -x -o {FullVideoSavePath}/%(title)s.%(ext)s";

            StartProcess("youtube-dl.exe", arguments, Logger);
        }
        private static void HandleArtistDownload(Logger Logger)
        {
            Logger.Write("Enter The Name Of An Artist");
            Console.Write("-> ");
            string SongNameDownload = Console.ReadLine();
            if (string.IsNullOrEmpty(SongNameDownload))
            {
                Logger.Write("Invalid artist name provided.");
                return;
            }

            Console.Clear();
            Logger.Write("How Many Videos Would You Like To Download?");
            Console.Write("-> ");
            string Amount = Console.ReadLine();
            if (string.IsNullOrEmpty(Amount) || !int.TryParse(Amount, out int videoAmount))
            {
                Logger.Write("Invalid amount provided.");
                return;
            }

            Console.Clear();
            string FullVideoSavePath = "Downloads";
            if (!Directory.Exists(FullVideoSavePath))
            {
                Directory.CreateDirectory(FullVideoSavePath);
            }
            Logger.Write("Would You Like To Download Video Or MP3?");
            Logger.WriteBarrierLine("1", "Video");
            Logger.WriteBarrierLine("2", "MP3");
            Console.Write("-> ");
            string WhatType = Console.ReadLine();
            Console.Clear();

            string arguments = WhatType == "1"
                ? $"ytsearch{videoAmount}:{SongNameDownload} -f best -o {FullVideoSavePath}/%(title)s.%(ext)s"
                : $"ytsearch{videoAmount}:{SongNameDownload} -f best --audio-format mp3 -x -o {FullVideoSavePath}/%(title)s.%(ext)s";

            StartProcess("youtube-dl.exe", arguments, Logger);
        }
        private static void HandleURLDownload(Logger Logger)
        {
            Logger.Write("Link?");
            Console.Write("-> ");
            string link = Console.ReadLine();
            if (string.IsNullOrEmpty(link))
            {
                Logger.Write("Invalid URL.");
                return;
            }

            Console.Clear();
            string FullVideoSavePath = "Downloads";
            if (!Directory.Exists(FullVideoSavePath)) { Directory.CreateDirectory(FullVideoSavePath); }
            Logger.Write("Would You Like To Download Video Or MP3?");
            Logger.WriteBarrierLine("1", "Video");
            Logger.WriteBarrierLine("2", "MP3");
            Console.Write("-> ");
            string WhatType = Console.ReadLine();
            Console.Clear();

            StartDownloadProcess(link, FullVideoSavePath, WhatType, Logger);
        }
        private static void StartDownload(string DownloadURL, string FileName, Logger Logger)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += Client_DownloadProgressChanged;
                client.DownloadFileCompleted += (sender, e) => Client_DownloadFileCompleted(sender, e, Logger);
                client.DownloadFileAsync(new Uri(DownloadURL), FileName);
            }
        }
        private static void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e, Logger Logger)
        {
            if (e.Error != null)
            {
                Logger.Write($"Error downloading file: {e.Error.Message}");
                return;
            }
            Logger.Write("Finished Downloading Dependencies...");
            Console.Clear();
            Logger.Write("Please Reopen The tool...");
        }
        private static void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double percentage = e.BytesReceived / (double)e.TotalBytesToReceive * 100;
            Console.Title = $"{(int)percentage} Percent Completed....";
        }
        private static void StartDownloadProcess(string url, string savePath, string whatType, Logger Logger)
        {
            string arguments = whatType == "1"
                ? $"{url} -f best -o {savePath}/%(title)s.%(ext)s"
                : $"{url} -f best --audio-format mp3 -x -o {savePath}/%(title)s.%(ext)s";

            StartProcess("youtube-dl.exe", arguments, Logger);
        }
        private static void StartProcess(string fileName, string arguments, Logger Logger)
        {
            try
            {
                Process DProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        UseShellExecute = false,
                        CreateNoWindow = false
                    }
                };
                DProcess.Start();
            }
            catch (Win32Exception ex)
            {
                Logger.Write($"Error starting process: {ex.Message}");
            }
        }
    }
}
