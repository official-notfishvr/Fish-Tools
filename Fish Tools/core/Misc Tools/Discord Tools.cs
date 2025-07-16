/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.Utils;
using Newtonsoft.Json;
using System.Net.Http.Json;
using static Fish_Tools.core.Utils.Settings.Config;
using System.Text;
using System.Net;
using static Fish_Tools.core.Utils.Utils;
using static Fish_Tools.core.Utils.Settings;
using System.Windows.Forms;
using System.Reflection.Emit;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Fish_Tools.core.DiscordTools
{
    internal class DiscordTools : ITool
    {
        public string Name => "Discord Tools";
        public string Category => "Misc Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Discord automation and management tools";

        public static string token;
        public static string username;
        public static string userId;
        public static bool isLoggedIn = false;
        public static HttpClient httpClient = new HttpClient();

        public void Main(Logger Logger)
        {
            DiscordToolsMenu(Logger).Wait();
        }

        public static async Task<bool> Login(string token, Logger Logger)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", token);
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                var response = await httpClient.GetAsync("https://discord.com/api/v9/users/@me");
                
                if (response.IsSuccessStatusCode)
                {
                    var userData = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<DiscordUser>(userData);
                    
                    username = user.username;
                    userId = user.id;
                    isLoggedIn = true;
                    
                    Logger.Success($"Username: {username}");
                    Logger.Success("Logged In");
                    
                    Config.ConfigData configData = new Config.ConfigData
                    {
                        Discord = new Config.DiscordConfig
                        {
                            Username = username,
                            Token = token,
                        },
                    };

                    Config.SaveConfig(configData);
                    return true;
                }
                else
                {
                    Logger.Error($"Login failed: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception err)
            {
                Logger.Error($"Login error: {err.Message}");
                token = "";
                username = "";
                isLoggedIn = false;
                return false;
            }
        }

        public static async Task DiscordToolsMenu(Logger Logger)
        {
            LoadConfig();

            if (!string.IsNullOrEmpty(Config.Data?.Discord?.Token))
            {
                token = Config.Data.Discord.Token;
                await Login(token, Logger);
            }
            else
            {
                Console.Clear();
                Console.Title = "Fish Tools";
                Logger.PrintArt();
                Logger.Write("Put In Token");
                Console.Write("-> ");
                token = Console.ReadLine();
                await Login(token, Logger);
            }

            Console.Clear();
            Console.Title = "Fish Tools";
            Logger.PrintArt();
            Logger.WriteBarrierLine("1", "Scrape Groups");
            Logger.WriteBarrierLine("2", "Messages");
            Logger.WriteBarrierLine("3", "Discord UserLookUp");
            Logger.WriteBarrierLine("4", "Get User Info");
            Logger.WriteBarrierLine("5", "Get Guilds");
            Logger.WriteBarrierLine("0", "Back");
            Console.Write("-> ");
            ConsoleKey choice = Console.ReadKey().Key;
            switch (choice)
            {
                case ConsoleKey.D1:
                    Console.Clear();
                    Console.WriteLine();
                    await ScrapeGroups(Logger);
                    break;
                case ConsoleKey.D2:
                    Console.Clear();
                    Console.WriteLine();
                    await SendMessage(Logger);
                    break;
                case ConsoleKey.D3:
                    Console.Clear();
                    Logger.PrintArt();
                    Logger.Info("User Id? to look up");
                    Console.Write("-> ");
                    string UserID = Console.ReadLine();
                    if (ulong.TryParse(UserID, out _))
                    {
                        await LookupUser(UserID, Logger);
                    }
                    else
                    {
                        Logger.Error("That isn't a valid Discord User ID!");
                    }
                    break;
                case ConsoleKey.D4:
                    Console.Clear();
                    Logger.PrintArt();
                    await GetCurrentUserInfo(Logger);
                    break;
                case ConsoleKey.D5:
                    Console.Clear();
                    Logger.PrintArt();
                    await GetGuilds(Logger);
                    break;
                case ConsoleKey.D0:
                    return;
            }
            Console.WriteLine();
            await DiscordToolsMenu(Logger);
        }

        public static async Task SendMessage(Logger Logger)
        {
            try
            {
                Console.Clear();
                Console.Title = "Fish Tools";
                Logger.PrintArt();
                Logger.WriteBarrierLine("1", "Send Message With Token");
                Logger.WriteBarrierLine("2", "Send Webhook Message");
                Logger.WriteBarrierLine("3", "Delete Webhook");
                Console.Write("-> ");
                ConsoleKey choice = Console.ReadKey().Key;
                switch (choice)
                {
                    case ConsoleKey.D1:
                        Console.WriteLine();
                        Logger.Write("Enter the channel ID:");
                        string channelId = Console.ReadLine();

                        Logger.Write("Enter the message to send:");
                        string message = Console.ReadLine();

                        Logger.Write("Enter the thread count:");
                        string threadInput = Console.ReadLine();

                        if (!int.TryParse(threadInput, out int threadCount)) { Logger.Error("Invalid thread count. Please enter a valid number."); return; }

                        for (int i = 0; i < threadCount; i++)
                        {
                            var data = new { content = $"{message}" };
                            var response = await httpClient.PostAsJsonAsync($"https://discord.com/api/v9/channels/{channelId}/messages", data);

                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine();
                                Logger.Success($"Success -> {message.Substring(0, Math.Min(20, message.Length))} -> {username}");
                            }
                            else
                            {
                                var responseBody = await response.Content.ReadAsStringAsync();
                                if (responseBody.StartsWith("{\"captcha_key\"")) { Logger.Error($"Error -> {message.Substring(0, Math.Min(20, message.Length))} -> {username} ({response.StatusCode}) (Captcha)"); }
                                else if (responseBody.StartsWith("{\"message\": \"401: Unauthorized\"")) { Logger.Error($"Error -> {message.Substring(0, Math.Min(20, message.Length))} -> {username} ({response.StatusCode}) (Unauthorized)"); }
                                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests) { Logger.Error($"Error -> {message.Substring(0, Math.Min(20, message.Length))} -> {username} ({response.StatusCode}) (Too Many Requests)"); }
                                else if (responseBody.Contains("\"code\": 50001")) { Logger.Error($"Error -> {message.Substring(0, Math.Min(20, message.Length))} -> {username} ({response.StatusCode}) (No Access)"); }
                                else if (responseBody.Contains("Cloudflare")) { Logger.Error($"Error -> {message.Substring(0, Math.Min(20, message.Length))} -> {username} ({response.StatusCode}) (CloudFlare Blocked)"); }
                                else { Logger.Error($"Error -> {message.Substring(0, Math.Min(20, message.Length))} -> {username} ({response.StatusCode}) ({responseBody})"); }
                            }
                        }
                        break;
                    case ConsoleKey.D2:
                        Console.WriteLine();
                        Logger.Write("Enter the Webhook Url:");
                        string webhookUrl = Console.ReadLine();

                        Logger.Write("Enter the message to send:");
                        string WebhookMessage = Console.ReadLine();

                        Logger.Write("Enter the thread count:");
                        string threadInput2 = Console.ReadLine();

                        if (!int.TryParse(threadInput2, out int threadCount2)) { Logger.Error("Invalid thread count. Please enter a valid number."); return; }

                        for (int i = 0; i < threadCount2; i++)
                        {
                            using (var httpClient = new HttpClient())
                            {
                                var payload = new
                                {
                                    content = WebhookMessage,
                                    embeds = (object)null,
                                    attachments = new object[] { }
                                };

                                var content = new System.Net.Http.StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                                var response = await httpClient.PostAsync(webhookUrl, content);
                                if (response.IsSuccessStatusCode) { Logger.Success($"Success -> {WebhookMessage.Substring(0, Math.Min(20, WebhookMessage.Length))} -> {username}"); }
                                else { Logger.Error($"Failed to send message. Status Code: {response.StatusCode}"); }
                            }
                        }
                        break;
                    case ConsoleKey.D3:
                        Console.WriteLine();
                        Logger.Write("Enter the Webhook Url:");
                        string webhookUrl2 = Console.ReadLine();

                        var request2 = WebRequest.Create(webhookUrl2);
                        request2.Method = "DELETE";
                        var response2 = (HttpWebResponse)request2.GetResponse();

                        Logger.Success("\nWebhook has been deleted!");
                        break;
                }
            }
            catch (Exception e) { Logger.Error($"Error: {e}"); }
        }

        public static async Task ScrapeGroups(Logger logger)
        {
            Console.Clear();
            Console.Title = "Fish Tools";
            logger.PrintArt();

            Console.WriteLine();

            try
            {
                Console.Clear();
                Console.WriteLine();

                logger.Write("Scraping groups...");

                var response = await httpClient.GetAsync("https://discord.com/api/v9/users/@me/channels");
                
                if (response.IsSuccessStatusCode)
                {
                    var channelsData = await response.Content.ReadAsStringAsync();
                    var channels = JsonConvert.DeserializeObject<List<DiscordChannel>>(channelsData);
                    
                    List<object> groupInfoList = new List<object>();

                    foreach (var channel in channels)
                    {
                        if (channel.type == 3) // Group DM type
                        {
                            string groupName = string.IsNullOrEmpty(channel.name) ? "No Name" : channel.name;
                            logger.Warn($"Group ID: {channel.id}");
                            logger.Warn($"Group Name: {groupName}");

                            groupInfoList.Add(new { GroupId = channel.id, GroupName = groupName });
                        }
                    }

                    if (groupInfoList.Count > 0)
                    {
                        string dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                        if (!Directory.Exists(dataDirectory)) { Directory.CreateDirectory(dataDirectory);}
                        string json = JsonConvert.SerializeObject(groupInfoList, Formatting.Indented);
                        string filePath = Path.Combine(dataDirectory, "groupInfo.json");
                        File.WriteAllText(filePath, json);
                        logger.Success($"Saved {groupInfoList.Count} groups to groupInfo.json");
                    }
                    else { logger.Write("No group channels found."); }
                }
                else
                {
                    logger.Error($"Failed to get channels: {response.StatusCode}");
                }
            }
            catch (Exception ex) { logger.Error($"Error while scraping groups: {ex.Message}"); }

            Console.ReadKey();
            await DiscordToolsMenu(logger);
        }

        public static async Task GetCurrentUserInfo(Logger Logger)
        {
            try
            {
                var response = await httpClient.GetAsync("https://discord.com/api/v9/users/@me");
                
                if (response.IsSuccessStatusCode)
                {
                    var userData = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<DiscordUser>(userData);
                    
                    Logger.Success($"ID: {user.id}");
                    Logger.Success($"Username: {user.username}#{user.discriminator}");
                    Logger.Success($"Email: {user.email}");
                    Logger.Success($"Phone: {user.phone}");
                    Logger.Success($"Verified: {user.verified}");
                    Logger.Success($"MFA Enabled: {user.mfa_enabled}");
                    Logger.Success($"Locale: {user.locale}");
                    Logger.Success($"Nitro Type: {user.premium_type}");
                    Logger.Success($"Flags: {user.flags}");
                    
                    if (!string.IsNullOrEmpty(user.avatar))
                    {
                        user.avatar = $"https://cdn.discordapp.com/avatars/{user.id}/{user.avatar}";
                        Logger.Success($"Avatar: {user.avatar}");
                    }
                }
                else
                {
                    Logger.Error($"Failed to get user info: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting user info: {ex.Message}");
            }
            
            Console.ReadKey();
        }

        public static async Task GetGuilds(Logger Logger)
        {
            try
            {
                var response = await httpClient.GetAsync("https://discord.com/api/v9/users/@me/guilds");
                
                if (response.IsSuccessStatusCode)
                {
                    var guildsData = await response.Content.ReadAsStringAsync();
                    var guilds = JsonConvert.DeserializeObject<List<DiscordGuild>>(guildsData);
                    
                    Logger.Success($"Found {guilds.Count} guilds:");
                    Console.WriteLine();
                    
                    foreach (var guild in guilds)
                    {
                        Logger.Write($"Guild: {guild.name} (ID: {guild.id})");
                        Logger.Write($"Owner: {guild.owner}");
                        Logger.Write($"Permissions: {guild.permissions}");
                        Console.WriteLine();
                    }
                }
                else
                {
                    Logger.Error($"Failed to get guilds: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting guilds: {ex.Message}");
            }
            
            Console.ReadKey();
        }

        #region Userid Look up
        public static async Task LookupUser(string userId, Logger Logger)
        {
            try
            {
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add(HttpRequestHeader.Authorization, "Bot " + GetToken());
                    string data = web.DownloadString($"https://discord.com/api/v8/users/{userId}");

                    DiscordUser usr = JsonConvert.DeserializeObject<DiscordUser>(data);
                    usr.avatar = $"https://cdn.discordapp.com/avatars/{usr.id}/{usr.avatar}";
                    usr.ConvertedFlags = CalculateUserFlags(usr.public_flags);
                    Logger.Success($"ID: {usr.id}");
                    Logger.Success($"Username: {usr.username}#{usr.discriminator}");
                    Logger.Success($"User Flags: {usr.ConvertedFlags}");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error looking up user: {ex.Message}");
                Console.ReadKey();
            }
        }

        public static string GetToken()
        {
            string tkns = "eyJ0b2tlbnMiOlsiT0RVeE1ETTNOVFk1TkRRNE9EQTBNelV5LllMeWNnQS5XdU1xaUR6d1lBZnBQMm9tVmM1aEZEcV9PbDQiLCJPRFV4TURRMU56a3hOREF4TVRFMU5qVTQuWUx5a0tBLmxyOEIxaXJncW15dWwyQ0t3LWNtVkhKbjdlbyIsIk9EVXhNRFExT1RFNU5qazNNREV3TmpnNC5ZTHlrUmcuZ0IwWlBhaDhtdGl2ZnBjaVRRbUdQbWdjVTBNIl19";
            string[] tokens = JsonConvert.DeserializeObject<Token>(Base64Decode(tkns)).tokens;

            return tokens[new Random().Next(0, tokens.Length)];
        }
        
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        
        public class Token
        {
            public string[] tokens { get; set; }
        }
        
        public class DiscordUser
        {
            public string id { get; set; }
            public string username { get; set; }
            public string avatar { get; set; }
            public string discriminator { get; set; }
            public int public_flags { get; set; }
            public string ConvertedFlags { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public bool verified { get; set; }
            public bool mfa_enabled { get; set; }
            public string locale { get; set; }
            public int premium_type { get; set; }
            public int flags { get; set; }
        }

        public class DiscordChannel
        {
            public string id { get; set; }
            public int type { get; set; }
            public string name { get; set; }
        }

        public class DiscordGuild
        {
            public string id { get; set; }
            public string name { get; set; }
            public bool owner { get; set; }
            public string permissions { get; set; }
        }
        
        private static string CalculateUserFlags(int number)
        {
            // https://discord.com/developers/docs/resources/user#user-object-user-flags
            switch (number)
            {
                case 1 << 0: return "Discord Employee";
                case 1 << 1: return "Partnered Server Owner";
                case 1 << 2: return "HypeSquad Events";
                case 1 << 3: return "Bug Hunter Level 1";
                case 1 << 6: return "House Bravery";
                case 1 << 7: return "House Brilliance";
                case 1 << 8: return "House Balance";
                case 1 << 9: return "Early Supporter";
                case 1 << 10: return "Team User";
                case 1 << 14: return "Bug Hunter Level 2";
                case 1 << 16: return "Verified Bot";
                case 1 << 17: return "Early Verified Bot Developer";
                case 1 << 18: return "Discord Certified Moderator";
                default: return "None";
            }
        }
        #endregion
    }
}
