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
            List<string> autofillInfo = new List<string>();
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
                { "Ubisoft Website Info",           new List<string>() },
                { "Google Website Info",            new List<string>() },
                { "Instagram Website Info",         new List<string>() },
                { "YouCan Shop Website Info",       new List<string>() },
                { "Samsung Website Info",           new List<string>() },
                { "Roblox Website Info",            new List<string>() },
                { "Massar Service Website Info",    new List<string>() },
                { "Facebook Website Info",          new List<string>() },
                { "Twitter Website Info",           new List<string>() },
                { "YouTube Website Info",           new List<string>() },
                { "Reddit Website Info",            new List<string>() },
                { "LinkedIn Website Info",          new List<string>() },
                { "GitHub Website Info",            new List<string>() },
                { "Amazon Website Info",            new List<string>() },
                { "eBay Website Info",              new List<string>() },
                { "AliExpress Website Info",        new List<string>() },
                { "Shopify Website Info",           new List<string>() },
                { "WooCommerce Website Info",       new List<string>() },
                { "WordPress Website Info",         new List<string>() },
                { "Spotify Website Info",           new List<string>() },
                { "Apple Website Info",             new List<string>() },
                { "Microsoft Website Info",         new List<string>() },
                { "Adobe Website Info",             new List<string>() },
                { "Dropbox Website Info",           new List<string>() },
                { "Google Drive Website Info",      new List<string>() },
                { "OneDrive Website Info",          new List<string>() },
                { "Telegram Website Info",          new List<string>() },
                { "WhatsApp Website Info",          new List<string>() },
                { "Snapchat Website Info",          new List<string>() },
                { "Twitch Website Info",            new List<string>() },
                { "Epic Games Website Info",        new List<string>() },
                { "Origin Website Info",            new List<string>() },
                { "Battle.net Website Info",        new List<string>() },
                { "GOG Website Info",               new List<string>() },
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
                { "Google Website Info",             new HashSet<string>() },
                { "Instagram Website Info",          new HashSet<string>() },
                { "YouCan Shop Website Info",        new HashSet<string>() },
                { "Samsung Website Info",            new HashSet<string>() },
                { "Roblox Website Info",             new HashSet<string>() },
                { "Massar Service Website Info",     new HashSet<string>() },
                { "Facebook Website Info",           new HashSet<string>() },
                { "Twitter Website Info",            new HashSet<string>() },
                { "YouTube Website Info",            new HashSet<string>() },
                { "Reddit Website Info",             new HashSet<string>() },
                { "LinkedIn Website Info",           new HashSet<string>() },
                { "GitHub Website Info",             new HashSet<string>() },
                { "Amazon Website Info",             new HashSet<string>() },
                { "eBay Website Info",               new HashSet<string>() },
                { "AliExpress Website Info",         new HashSet<string>() },
                { "Shopify Website Info",            new HashSet<string>() },
                { "WooCommerce Website Info",        new HashSet<string>() },
                { "WordPress Website Info",          new HashSet<string>() },
                { "Spotify Website Info",            new HashSet<string>() },
                { "Apple Website Info",              new HashSet<string>() },
                { "Microsoft Website Info",          new HashSet<string>() },
                { "Adobe Website Info",              new HashSet<string>() },
                { "Dropbox Website Info",            new HashSet<string>() },
                { "Google Drive Website Info",       new HashSet<string>() },
                { "OneDrive Website Info",           new HashSet<string>() },
                { "Telegram Website Info",           new HashSet<string>() },
                { "WhatsApp Website Info",           new HashSet<string>() },
                { "Snapchat Website Info",           new HashSet<string>() },
                { "Twitch Website Info",             new HashSet<string>() },
                { "Epic Games Website Info",         new HashSet<string>() },
                { "Origin Website Info",             new HashSet<string>() },
                { "Battle.net Website Info",         new HashSet<string>() },
                { "GOG Website Info",                new HashSet<string>() },
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
                { "google.com",             "Google Website Info" },
                { "instagram.com",          "Instagram Website Info" },
                { "youcan.shop",            "YouCan Shop Website Info" },
                { "samsung.com",            "Samsung Website Info" },
                { "roblox.client",          "Roblox Website Info" },
                { "men.gov.ma",             "Massar Service Website Info" },
                { "facebook.com",           "Facebook Website Info" },
                { "twitter.com",            "Twitter Website Info" },
                { "youtube.com",            "YouTube Website Info" },
                { "reddit.com",             "Reddit Website Info" },
                { "linkedin.com",           "LinkedIn Website Info" },
                { "github.com",             "GitHub Website Info" },
                { "amazon.com",             "Amazon Website Info" },
                { "ebay.com",               "eBay Website Info" },
                { "aliexpress.com",         "AliExpress Website Info" },
                { "shopify.com",            "Shopify Website Info" },
                { "woocommerce.com",        "WooCommerce Website Info" },
                { "wordpress.com",          "WordPress Website Info" },
                { "spotify.com",            "Spotify Website Info" },
                { "apple.com",              "Apple Website Info" },
                { "microsoft.com",          "Microsoft Website Info" },
                { "adobe.com",              "Adobe Website Info" },
                { "dropbox.com",            "Dropbox Website Info" },
                { "drive.google.com",       "Google Drive Website Info" },
                { "onedrive.com",           "OneDrive Website Info" },
                { "telegram.org",           "Telegram Website Info" },
                { "whatsapp.com",           "WhatsApp Website Info" },
                { "snapchat.com",           "Snapchat Website Info" },
                { "twitch.tv",              "Twitch Website Info" },
                { "epicgames.com",          "Epic Games Website Info" },
                { "origin.com",             "Origin Website Info" },
                { "battle.net",             "Battle.net Website Info" },
                { "gog.com",                "GOG Website Info" },
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

                var discordTokensFiles = Directory.GetFiles(folderPath, "DiscordTokens.txt", SearchOption.AllDirectories).ToList();
                var discordFiles = Directory.GetFiles(folderPath, "Discord.txt", SearchOption.AllDirectories).ToList();
                var allDiscordFiles = discordTokensFiles.Concat(discordFiles).ToList();

                if (allDiscordFiles.Count > 0)
                {
                    logger.Info($"Found {allDiscordFiles.Count} Discord file(s) ({discordTokensFiles.Count} DiscordTokens.txt, {discordFiles.Count} Discord.txt). Extracting Discord tokens...");
                    foreach (var file in allDiscordFiles)
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
                    logger.Info("No Discord files (DiscordTokens.txt or Discord.txt) found in the folder.");
                }

                var passwordFiles = Directory.GetFiles(folderPath, "All Passwords.txt", SearchOption.AllDirectories).ToList();
                var passwordsFiles = Directory.GetFiles(folderPath, "Passwords.txt", SearchOption.AllDirectories).ToList();
                var allPasswordFiles = passwordFiles.Concat(passwordsFiles).ToList();

                List<string> filesToProcess = new List<string>();

                if (allPasswordFiles.Count > 0)
                {
                    logger.Info($"Found {allPasswordFiles.Count} password file(s) ({passwordFiles.Count} All Passwords.txt, {passwordsFiles.Count} Passwords.txt). Processing password files only.");
                    filesToProcess = allPasswordFiles;
                }
                else
                {
                    var autofillFolders = Directory.GetDirectories(folderPath, "Autofill", SearchOption.AllDirectories).ToList();
                    logger.Info($"Found {autofillFolders.Count} Autofill folder(s)");
                    var autofillFiles = new List<string>();
                    foreach (var autofillFolder in autofillFolders)
                    {
                        try
                        {
                            var filesInAutofill = Directory.GetFiles(autofillFolder, "*.txt", SearchOption.TopDirectoryOnly).ToList();
                            logger.Info($"Found {filesInAutofill.Count} .txt files in {autofillFolder}");
                            autofillFiles.AddRange(filesInAutofill);
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Error accessing Autofill folder {autofillFolder}: {ex.Message}");
                        }
                    }

                    if (autofillFiles.Count > 0)
                    {
                        logger.Info($"No password files found. Processing {autofillFiles.Count} autofill files instead.");
                        filesToProcess = autofillFiles;
                    }
                }

                if (filesToProcess.Count > 0)
                {
                    logger.Info($"Processing {filesToProcess.Count} file(s). Extracting account info...");
                    foreach (var file in filesToProcess)
                    {
                        logger.Info($"Processing file: {file}");
                        try
                        {
                            var lines = File.ReadAllLines(file);

                            bool isAutofillFormat = lines.Any(line => line.Trim().StartsWith("Name:")) &&
                                                   lines.Any(line => line.Trim().StartsWith("Value:"));

                            if (isAutofillFormat)
                            {
                                logger.Info($"Processing Autofill file: {file}");

                                for (int i = 0; i < lines.Length; i++)
                                {
                                    string line = lines[i].Trim();
                                    if (line.StartsWith("Name:"))
                                    {
                                        string name = line.Substring(5).Trim();
                                        if (i + 1 < lines.Length)
                                        {
                                            string nextLine = lines[i + 1].Trim();
                                            if (nextLine.StartsWith("Value:"))
                                            {
                                                string value = nextLine.Substring(6).Trim();
                                                logger.Info($"Processing entry: {name} = {value}");

                                                if (name.Equals("username", StringComparison.OrdinalIgnoreCase) ||
                                                    name.Equals("global_name", StringComparison.OrdinalIgnoreCase) ||
                                                    name.Equals("new-username", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    if (useCompactFormat)
                                                    {
                                                        autofillInfo.Add(value);
                                                        logger.Info($"Added username entry: {value}");
                                                    }
                                                    else
                                                    {
                                                        autofillInfo.Add($"Username: {value}");
                                                        autofillInfo.Add("");
                                                        logger.Info($"Added username entry: {value}");
                                                    }
                                                }
                                                else if (name.Equals("identifier", StringComparison.OrdinalIgnoreCase) && value.Contains("@"))
                                                {
                                                    if (useCompactFormat)
                                                    {
                                                        autofillInfo.Add(value);
                                                        logger.Info($"Added email entry: {value}");
                                                    }
                                                    else
                                                    {
                                                        autofillInfo.Add($"Email: {value}");
                                                        autofillInfo.Add("");
                                                        logger.Info($"Added email entry: {value}");
                                                    }
                                                }
                                                else if (name.Equals("userFullName", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    if (useCompactFormat)
                                                    {
                                                        autofillInfo.Add($"Name:{value}");
                                                        logger.Info($"Added full name entry: {value}");
                                                    }
                                                    else
                                                    {
                                                        autofillInfo.Add($"Full Name: {value}");
                                                        autofillInfo.Add("");
                                                        logger.Info($"Added full name entry: {value}");
                                                    }
                                                }
                                                else if (name.Equals("userBirthDate.year", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    if (useCompactFormat)
                                                    {
                                                        autofillInfo.Add($"BirthYear:{value}");
                                                        logger.Info($"Added birth year entry: {value}");
                                                    }
                                                    else
                                                    {
                                                        autofillInfo.Add($"Birth Year: {value}");
                                                        autofillInfo.Add("");
                                                        logger.Info($"Added birth year entry: {value}");
                                                    }
                                                }
                                                else if (name.Equals("businessName", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    if (useCompactFormat)
                                                    {
                                                        autofillInfo.Add($"Business:{value}");
                                                        logger.Info($"Added business name entry: {value}");
                                                    }
                                                    else
                                                    {
                                                        autofillInfo.Add($"Business Name: {value}");
                                                        autofillInfo.Add("");
                                                        logger.Info($"Added business name entry: {value}");
                                                    }
                                                }
                                                else if (name.Equals("workspaceName", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    if (useCompactFormat)
                                                    {
                                                        autofillInfo.Add($"Workspace:{value}");
                                                        logger.Info($"Added workspace name entry: {value}");
                                                    }
                                                    else
                                                    {
                                                        autofillInfo.Add($"Workspace Name: {value}");
                                                        autofillInfo.Add("");
                                                        logger.Info($"Added workspace name entry: {value}");
                                                    }
                                                }
                                                else if (!string.IsNullOrEmpty(value) && !value.StartsWith("http") && !value.Contains("=") && value.Length > 2)
                                                {
                                                    if (useCompactFormat)
                                                    {
                                                        autofillInfo.Add($"{name}:{value}");
                                                        logger.Info($"Added other entry: {name}:{value}");
                                                    }
                                                    else
                                                    {
                                                        autofillInfo.Add($"{name}: {value}");
                                                        autofillInfo.Add("");
                                                        logger.Info($"Added other entry: {name}:{value}");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
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
                                            string username = "Not Found";
                                            string password = "Not Found";

                                            for (int j = i + 1; j < Math.Min(i + 4, lines.Length); j++)
                                            {
                                                string checkLine = lines[j].Trim();
                                                if (checkLine.StartsWith("USER:", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    username = checkLine.Substring(5).Trim();
                                                }
                                                else if (checkLine.StartsWith("PASS:", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    password = checkLine.Substring(5).Trim();
                                                }
                                                else if (checkLine.StartsWith("Username:", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    username = checkLine.Substring(9).Trim();
                                                }
                                                else if (checkLine.StartsWith("Password:", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    password = checkLine.Substring(9).Trim();
                                                }
                                            }

                                            string blockKey = urlLineFull + "|" + username + "|" + password;

                                            if (!domainBlocks[matchedSection].Contains(blockKey))
                                            {
                                                domainBlocks[matchedSection].Add(blockKey);

                                                if (useCompactFormat)
                                                {
                                                    domainInfo[matchedSection].Add($"{username}:{password}");
                                                }
                                                else
                                                {
                                                    domainInfo[matchedSection].Add(urlLineFull);
                                                    domainInfo[matchedSection].Add($"Username: {username}");
                                                    domainInfo[matchedSection].Add($"Password: {password}");
                                                    domainInfo[matchedSection].Add("");
                                                }
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
                    logger.Info("No password files (All Passwords.txt or files in Autofill folders) found in the folder.");
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
                    if (fileName.Equals("DiscordTokens.txt", StringComparison.OrdinalIgnoreCase) || fileName.Equals("Discord.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        var lines = File.ReadAllLines(filePath);
                        discordTokensInfo.AddRange(lines.Where(line => !string.IsNullOrWhiteSpace(line)));
                    }
                    else if (fileName.Equals("All Passwords.txt", StringComparison.OrdinalIgnoreCase) || fileName.Equals("Passwords.txt", StringComparison.OrdinalIgnoreCase))
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
                                    string username = "Not Found";
                                    string password = "Not Found";

                                    for (int j = i + 1; j < Math.Min(i + 4, lines.Length); j++)
                                    {
                                        string checkLine = lines[j].Trim();
                                        if (checkLine.StartsWith("USER:", StringComparison.OrdinalIgnoreCase))
                                        {
                                            username = checkLine.Substring(5).Trim();
                                        }
                                        else if (checkLine.StartsWith("PASS:", StringComparison.OrdinalIgnoreCase))
                                        {
                                            password = checkLine.Substring(5).Trim();
                                        }
                                        else if (checkLine.StartsWith("Username:", StringComparison.OrdinalIgnoreCase))
                                        {
                                            username = checkLine.Substring(9).Trim();
                                        }
                                        else if (checkLine.StartsWith("Password:", StringComparison.OrdinalIgnoreCase))
                                        {
                                            password = checkLine.Substring(9).Trim();
                                        }
                                    }

                                    string blockKey = urlLineFull + "|" + username + "|" + password;

                                    if (!domainBlocks[matchedSection].Contains(blockKey))
                                    {
                                        domainBlocks[matchedSection].Add(blockKey);

                                        if (useCompactFormat)
                                        {
                                            domainInfo[matchedSection].Add($"{username}:{password}");
                                        }
                                        else
                                        {
                                            domainInfo[matchedSection].Add(urlLineFull);
                                            domainInfo[matchedSection].Add($"Username: {username}");
                                            domainInfo[matchedSection].Add($"Password: {password}");
                                            domainInfo[matchedSection].Add("");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.Error("The specified file is not recognized. Please provide either DiscordTokens.txt, Discord.txt, or All Passwords.txt.");
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

            List<string> allResults = new List<string>();

            if (discordTokensInfo.Any())
            {
                allResults.Add("=== DISCORD TOKENS ===");
                allResults.AddRange(discordTokensInfo);
                allResults.Add("");
            }

            List<string> allPasswordsInfo = new List<string>();
            foreach (var section in domainInfo)
            {
                if (section.Value.Any())
                {
                    allPasswordsInfo.AddRange(section.Value);
                }
            }

            allPasswordsInfo.AddRange(autofillInfo);

            if (allPasswordsInfo.Any())
            {
                allResults.Add("=== PASSWORDS ===");
                allResults.AddRange(allPasswordsInfo);
            }

            if (allResults.Any())
            {
                string resultFileName = $"Results_{timestamp}.txt";
                string resultPath = Path.Combine(resultDir, resultFileName);
                try
                {
                    File.WriteAllLines(resultPath, allResults);
                    logger.Info($"All results saved to {resultPath}");
                    logger.Info($"Total lines saved: {allResults.Count}");
                    logger.Info($"Discord tokens: {discordTokensInfo.Count}");
                    logger.Info($"Autofill entries: {autofillInfo.Count}");
                    logger.Info($"Password entries: {allPasswordsInfo.Count}");
                }
                catch (Exception ex)
                {
                    logger.Error($"Error saving results file: {ex.Message}");
                }
            }
            else
            {
                logger.Info("No results to save.");
            }

            Console.ReadKey();
        }
    }
}