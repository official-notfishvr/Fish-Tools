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
using static Fish_Tools.core.Utils.Utils.Config;
using System.Text;
using System.Net;
using static Fish_Tools.core.Utils.Utils;

namespace Fish_Tools.core.DiscordTools
{
    internal class DiscordTools
    {
        public static DiscordSocketClient client = new DiscordSocketClient();
        public static string token;
        public static string username;
        public static List<BotGuild> BotGuilds = new List<BotGuild>();
        public struct BotGuild
        {
            public DiscordGuild Guild { get; }
            public ulong Id => Guild.Id;
            public BotGuild(DiscordGuild gld) { Guild = gld; }
            public override string ToString() { return Guild.Name; }
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
            if (string.IsNullOrEmpty(token))
            {
                Console.Clear();
                Console.Title = "Fish Tools";
                Logger.PrintArt();
                Logger.Write("Put In Token");
                Console.Write("-> ");
                token = Console.ReadLine();
                Login(token, Logger);
                Console.ReadKey();
            }

            Console.Clear();
            Console.Title = "Fish Tools";
            Logger.PrintArt();
            Logger.WriteBarrierLine("1", "Scrape Groups");
            Logger.WriteBarrierLine("2", "Messages");
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
    }
}
