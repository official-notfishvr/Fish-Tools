/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.Misc_Tools
{
    internal class GetInfo : ITool
    {
        public string Name => "Get Info";
        public string Category => "Utility Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Extract and organize information from credential files";

        public void Main(Logger logger)
        {
            Console.Clear();
            logger.PrintArt();
            GetInfoMain(logger);
        }

        public static void GetInfoMain(Logger logger)
        {
            List<string> discordTokensInfo = new List<string>();
            Dictionary<string, List<string>> domainInfo = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Patreon Website Info",           new List<string>() },
                { "PayPal Website Info",            new List<string>() },
                { "Cloudflare Website Info",        new List<string>() },
                { "Live Website (Microsoft) Info",  new List<string>() },
                { "Discord Website Info",           new List<string>() },
                { "TikTok Website Info",            new List<string>() },
                { "Netflix Website Info",           new List<string>() },
                { "HBO Website Info",               new List<string>() },
                { "Hulu Website Info",              new List<string>() },
                { "Steam Website Info",             new List<string>() },
                { "Ubisoft Website Info",           new List<string>() }

            };

            Dictionary<string, HashSet<string>> domainBlocks = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Patreon Website Info",            new HashSet<string>() },
                { "PayPal Website Info",             new HashSet<string>() },
                { "Cloudflare Website Info",         new HashSet<string>() },
                { "Live Website (Microsoft) Info",   new HashSet<string>() },
                { "Discord Website Info",            new HashSet<string>() },
                { "TikTok Website Info",             new HashSet<string>() },
                { "Netflix Website Info",            new HashSet<string>() },
                { "HBO Website Info",                new HashSet<string>() },
                { "Hulu Website Info",               new HashSet<string>() },
                { "Steam Website Info",              new HashSet<string>() },
                { "Ubisoft Website Info",            new HashSet<string>() },
            };

            Dictionary<string, string> targetDomains = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "patreon.com",            "Patreon Website Info" },
                { "paypal.com",             "PayPal Website Info" },
                { "cloudflare.com",         "Cloudflare Website Info" },
                { "live.com",               "Live Website (Microsoft) Info" },
                { "discord.com",            "Discord Website Info" },
                { "tiktok.com",             "TikTok Website Info" },
                { "netflix.com",            "Netflix Website Info" },
                { "hbo.com",                "HBO Website Info" },
                { "hulu.com",               "Hulu Website Info" },
                { "steampowered.com",       "Steam Website Info" },
                { "ubisoft.com",            "Ubisoft Website Info" },
            };

            logger.Info("Do you want to format credentials as user:pass? (y/n):");
            Console.Write("-> ");
            string formatInput = Console.ReadLine()?.Trim().ToLower();
            bool useCompactFormat = formatInput == "y";

            logger.Info("Do you want to scan a folder? (y/n):");
            Console.Write("-> ");
            string folderInput = Console.ReadLine()?.Trim().ToLower();

            if (folderInput == "y")
            {
                logger.Info("Enter the folder path:");
                Console.Write("-> ");
                string folderPath = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                {
                    logger.Error("Invalid folder path.");
                    Console.ReadKey();
                    return;
                }

                var discordFiles = Directory.GetFiles(folderPath, "DiscordTokens.txt", SearchOption.AllDirectories).ToList();
                if (discordFiles.Count > 0)
                {
                    logger.Info($"Found {discordFiles.Count} DiscordTokens.txt file(s). Extracting Discord tokens...");
                    foreach (var file in discordFiles)
                    {
                        try
                        {
                            var lines = File.ReadAllLines(file);
                            discordTokensInfo.AddRange(lines.Where(line => !string.IsNullOrWhiteSpace(line)));
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Error processing file {file}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    logger.Info("No DiscordTokens.txt files found in the folder.");
                }

                var passwordFiles = Directory.GetFiles(folderPath, "All Passwords.txt", SearchOption.AllDirectories).ToList();
                if (passwordFiles.Count > 0)
                {
                    logger.Info($"Found {passwordFiles.Count} All Passwords.txt file(s). Extracting account info...");
                    foreach (var file in passwordFiles)
                    {
                        try
                        {
                            var lines = File.ReadAllLines(file);
                            for (int i = 0; i < lines.Length; i++)
                            {
                                if (lines[i].StartsWith("URL:", StringComparison.OrdinalIgnoreCase))
                                {
                                    string urlLineFull = lines[i].Trim();
                                    string urlValue = urlLineFull.Substring(4).Trim();
                                    bool matchFound = false;
                                    string matchedSection = string.Empty;

                                    if (Uri.TryCreate(urlValue, UriKind.Absolute, out Uri uri))
                                    {
                                        string host = uri.Host;
                                        foreach (var kvp in targetDomains)
                                        {
                                            if (host.EndsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                                            {
                                                matchFound = true;
                                                matchedSection = kvp.Value;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (var kvp in targetDomains)
                                        {
                                            if (urlValue.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                                            {
                                                matchFound = true;
                                                matchedSection = kvp.Value;
                                                break;
                                            }
                                        }
                                    }

                                    if (matchFound)
                                    {
                                        string userLine = (i + 1 < lines.Length) ? lines[i + 1].Trim() : "USER: Not Found";
                                        string passLine = (i + 2 < lines.Length) ? lines[i + 2].Trim() : "PASS: Not Found";
                                        string blockKey = urlLineFull + "|" + userLine + "|" + passLine;

                                        if (!domainBlocks[matchedSection].Contains(blockKey))
                                        {
                                            domainBlocks[matchedSection].Add(blockKey);

                                            if (useCompactFormat)
                                            {
                                                string username = userLine.StartsWith("USER:", StringComparison.OrdinalIgnoreCase) ? userLine.Substring(5).Trim() : userLine;
                                                string password = passLine.StartsWith("PASS:", StringComparison.OrdinalIgnoreCase) ? passLine.Substring(5).Trim() : passLine;
                                                domainInfo[matchedSection].Add($"{username}:{password}");
                                            }
                                            else
                                            {
                                                domainInfo[matchedSection].Add(urlLineFull);
                                                domainInfo[matchedSection].Add(userLine);
                                                domainInfo[matchedSection].Add(passLine);
                                                domainInfo[matchedSection].Add("");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Error processing file {file}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    logger.Info("No All Passwords.txt files found in the folder.");
                }
            }
            else
            {
                logger.Info("Enter the file path:");
                Console.Write("-> ");
                string filePath = Console.ReadLine()?.Trim().Replace("\"", "");

                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    logger.Error("Invalid file path.");
                    Console.ReadKey();
                    return;
                }

                string fileName = Path.GetFileName(filePath);
                try
                {
                    if (fileName.Equals("DiscordTokens.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        var lines = File.ReadAllLines(filePath);
                        discordTokensInfo.AddRange(lines.Where(line => !string.IsNullOrWhiteSpace(line)));
                    }
                    else if (fileName.Equals("All Passwords.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        var lines = File.ReadAllLines(filePath);
                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (lines[i].StartsWith("URL:", StringComparison.OrdinalIgnoreCase))
                            {
                                string urlLineFull = lines[i].Trim();
                                string urlValue = urlLineFull.Substring(4).Trim();
                                bool matchFound = false;
                                string matchedSection = string.Empty;

                                if (Uri.TryCreate(urlValue, UriKind.Absolute, out Uri uri))
                                {
                                    string host = uri.Host;
                                    foreach (var kvp in targetDomains)
                                    {
                                        if (host.EndsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                                        {
                                            matchFound = true;
                                            matchedSection = kvp.Value;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var kvp in targetDomains)
                                    {
                                        if (urlValue.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            matchFound = true;
                                            matchedSection = kvp.Value;
                                            break;
                                        }
                                    }
                                }

                                if (matchFound)
                                {
                                    string userLine = (i + 1 < lines.Length) ? lines[i + 1].Trim() : "USER: Not Found";
                                    string passLine = (i + 2 < lines.Length) ? lines[i + 2].Trim() : "PASS: Not Found";
                                    string blockKey = urlLineFull + "|" + userLine + "|" + passLine;

                                    if (!domainBlocks[matchedSection].Contains(blockKey))
                                    {
                                        domainBlocks[matchedSection].Add(blockKey);

                                        if (useCompactFormat)
                                        {
                                            string username = userLine.StartsWith("USER:", StringComparison.OrdinalIgnoreCase) ? userLine.Substring(5).Trim() : userLine;
                                            string password = passLine.StartsWith("PASS:", StringComparison.OrdinalIgnoreCase) ? passLine.Substring(5).Trim() : passLine;
                                            domainInfo[matchedSection].Add($"{username}:{password}");
                                        }
                                        else
                                        {
                                            domainInfo[matchedSection].Add(urlLineFull);
                                            domainInfo[matchedSection].Add(userLine);
                                            domainInfo[matchedSection].Add(passLine);
                                            domainInfo[matchedSection].Add("");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.Error("The specified file is not recognized. Please provide either DiscordTokens.txt or All Passwords.txt.");
                        Console.ReadKey();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error processing the file: {ex.Message}");
                    Console.ReadKey();
                    return;
                }
            }

            string resultDir = Path.Combine(Directory.GetCurrentDirectory(), "Result");
            if (!Directory.Exists(resultDir)) { Directory.CreateDirectory(resultDir); }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (discordTokensInfo.Any())
            {
                string discordFileName = $"DiscordTokens_{timestamp}.txt";
                string discordPath = Path.Combine(resultDir, discordFileName);
                try
                {
                    File.WriteAllLines(discordPath, discordTokensInfo);
                    logger.Info($"Discord tokens saved to {discordPath}");
                }
                catch (Exception ex)
                {
                    logger.Error($"Error saving Discord tokens file: {ex.Message}");
                }
            }

            foreach (var section in domainInfo)
            {
                if (section.Value.Any())
                {
                    string safeSectionName = section.Key.Replace(" ", "_");
                    string fileName = $"{safeSectionName}_{timestamp}.txt";
                    string savePath = Path.Combine(resultDir, fileName);
                    try
                    {
                        File.WriteAllLines(savePath, section.Value);
                        logger.Info($"{section.Key} saved to {savePath}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Error saving {section.Key} file: {ex.Message}");
                    }
                }
            }

            Console.ReadKey();
        }
    }
}