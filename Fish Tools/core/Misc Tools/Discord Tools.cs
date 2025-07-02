/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.Utils;
using Discord.Gateway;
using Discord;
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

        public static DiscordSocketClient client = new DiscordSocketClient();
        public static string token;
        public static string username;
        public static List<BotGuild> BotGuilds = new List<BotGuild>();
        public static DiscordUser usr = new DiscordUser();
        public struct BotGuild
        {
            public DiscordGuild Guild { get; }
            public ulong Id => Guild.Id;
            public BotGuild(DiscordGuild gld) { Guild = gld; }
            public override string ToString() { return Guild.Name; }
        }

        public void Main(Logger Logger)
        {
            DiscordToolsMenu(Logger).Wait();
        }

        public static void Login(string token, Logger Logger)
        {
            try
            {
                client.OnLoggedIn += (sender, args) => client_OnLoggedIn(sender, args, Logger);
                client.OnLoggedOut += (sender, args) => client_OnLoggedOut(sender, args, Logger);
                client.OnGuildUpdated += (sender, args) => client_OnGuildUpdated(sender, args, Logger);

                client.Login(token);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                token = "";
                username = "";
            }
        }
        private static void client_OnLoggedIn(DiscordSocketClient client, LoginEventArgs args, Logger Logger)
        {
            try
            {
                Logger.Success("Username: " + client.User.Username);
                username = client.User.Username;
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
            }
            catch (Exception ex)
            {
                Logger.Error($"Error during OnLoggedIn: {ex.Message}");
                token = "";
            }
        }
        public static Task client_OnGuildUpdated(DiscordClient sender, GuildEventArgs e, Logger Logger)
        {
            Logger.Write($"Guild available: {e.Guild.Name} (ID: {e.Guild.Id})");
            BotGuilds.Add(new BotGuild(e.Guild));
            return Task.CompletedTask;
        }
        private static void client_OnLoggedOut(DiscordSocketClient client, LogoutEventArgs args, Logger Logger) { }
        public static async Task DiscordToolsMenu(Logger Logger)
        {
            LoadConfig();

            if (!string.IsNullOrEmpty(Config.Data?.Discord?.Token))
            {
                token = Config.Data.Discord.Token;
                Login(token, Logger);
            }
            else
            {
                Console.Clear();
                Console.Title = "Fish Tools";
                Logger.PrintArt();
                Logger.Write("Put In Token");
                Console.Write("-> ");
                token = Console.ReadLine();
                Login(token, Logger);
            }

            Console.Clear();
            Console.Title = "Fish Tools";
            Logger.PrintArt();
            Logger.WriteBarrierLine("1", "Scrape Groups");
            Logger.WriteBarrierLine("2", "Messages");
            Logger.WriteBarrierLine("3", "Discord UserLookUp");
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
                        using (WebClient web = new WebClient())
                        {
                            web.Headers.Add(HttpRequestHeader.Authorization, "Bot " + GetToken());
                            string data = web.DownloadString($"https://discord.com/api/v8/users/{UserID}");

                            DiscordUser usr = System.Text.Json.JsonSerializer.Deserialize<DiscordUser>(data);
                            usr.avatar = $"https://cdn.discordapp.com/avatars/{usr.id}/{usr.avatar}";
                            usr.ConvertedFlags = CalculateUserFlags(usr.public_flags);
                            Logger.Success($"ID: {usr.id}");
                            Logger.Success($"Username: {usr.username}#{usr.discriminator}");
                            Logger.Success($"User Flags: {usr.ConvertedFlags}");
                            Console.ReadKey();
                        }
                    }
                    else
                    {
                        Logger.Error("That isn't a valid Discord User ID!");
                    }
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
                Logger.WriteBarrierLine("2", "Send Webook Message");
                Logger.WriteBarrierLine("3", "Delete Webook");
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

                        HttpClient client = new HttpClient();
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Add("Authorization", token);

                        for (int i = 0; i < threadCount; i++)
                        {
                            var data = new { session_id = "", content = $"{message}" };
                            var response = await client.PostAsJsonAsync($"https://discord.com/api/v9/channels/{channelId}/messages", data);

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

                var groups = await client.GetPrivateChannelsAsync();
                Console.WriteLine();
                logger.Write("Scraping groups...");

                List<object> groupInfoList = new List<object>();

                foreach (var group in groups)
                {
                    if (group.Type == ChannelType.Group)
                    {
                        string groupName = string.IsNullOrEmpty(group.Name) ? "No Name" : group.Name;
                        logger.Warn($"Group ID: {group.Id}");
                        logger.Warn($"Group Name: {groupName}");

                        groupInfoList.Add(new { GroupId = group.Id, GroupName = groupName });
                    }
                }

                if (groupInfoList.Count > 0)
                {
                    string dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                    if (!Directory.Exists(dataDirectory)) { Directory.CreateDirectory(dataDirectory);}
                    string json = JsonConvert.SerializeObject(groupInfoList, Formatting.Indented);
                    string filePath = Path.Combine(dataDirectory, "groupInfo.json");
                    File.WriteAllText(filePath, json);
                }
                else { logger.Write("No group channels found."); }
            }
            catch (Exception ex) { logger.Error($"Error while scraping groups: {ex.Message}"); }

            Console.ReadKey();
            await DiscordToolsMenu(logger);
        }
        #region Userid Look up
        public static string GetToken()
        {
            string tkns = "eyJ0b2tlbnMiOlsiT0RVeE1ETTNOVFk1TkRRNE9EQTBNelV5LllMeWNnQS5XdU1xaUR6d1lBZnBQMm9tVmM1aEZEcV9PbDQiLCJPRFV4TURRMU56a3hOREF4TVRFMU5qVTQuWUx5a0tBLmxyOEIxaXJncW15dWwyQ0t3LWNtVkhKbjdlbyIsIk9EVXhNRFExT1RFNU5qazNNREV3TmpnNC5ZTHlrUmcuZ0IwWlBhaDhtdGl2ZnBjaVRRbUdQbWdjVTBNIl19";
            string[] tokens = System.Text.Json.JsonSerializer.Deserialize<Token>(Base64Decode(tkns)).tokens;

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
