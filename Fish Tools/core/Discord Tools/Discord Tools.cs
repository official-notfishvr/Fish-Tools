using Fish_Tools.core.Utils;
using Fish_Tools;
using Discord.Gateway;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json.Linq;
using Discord;
using Newtonsoft.Json;

namespace Fish_Tools.core.DiscordTools
{
    internal class DiscordTools
    {
        public static DiscordSocketClient client = new DiscordSocketClient();
        public static string token;
        public static void Login(string token, Logger Logger)
        {
            try
            {
                client.OnLoggedIn += (sender, args) => client_OnLoggedIn(sender, args, Logger);
                client.OnLoggedOut += (sender, args) => client_OnLoggedOut(sender, args, Logger);

                client.Login(token);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                token = "";
            }
        }
        private static void client_OnLoggedIn(DiscordSocketClient client, LoginEventArgs args, Logger Logger)
        {
            try
            {
                Logger.Success("Username: " + client.User.Username);
                Logger.Success("Logged In");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error during OnLoggedIn: {ex.Message}");
                token = "";
            }
        }
        private static void client_OnLoggedOut(DiscordSocketClient client, LogoutEventArgs args, Logger Logger) { }
        public static async Task DiscordToolsMenu(Logger Logger)
        {
            if (token == "" || token == null)
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
            Console.Write("-> ");
            ConsoleKey choice = Console.ReadKey().Key;
            switch (choice)
            {
                case ConsoleKey.D1:
                    Console.Clear();
                    Console.WriteLine();
                    ScrapeGroups(Logger);
                    break;
            }
            Console.WriteLine();
            DiscordToolsMenu(Logger);
        }
        public static async void ScrapeGroups(Logger logger)
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
                logger.Write("\r\nScraping groups...");

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
                    if (!Directory.Exists(dataDirectory)) { Directory.CreateDirectory(dataDirectory); }
                    string json = JsonConvert.SerializeObject(groupInfoList, Formatting.Indented);
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "/Data/", "groupInfo.json");
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