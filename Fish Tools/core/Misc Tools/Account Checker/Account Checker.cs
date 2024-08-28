/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.MiscTools.AccountChecker.Checkers;
using Fish_Tools.core.MiscTools.AccountChecker.Checkers.Steam;
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.MiscTools.AccountChecker
{
    internal class AccountChecker
    {
        public static void Main(Logger Logger)
        {
            Console.Clear();
            Console.Title = "Fish Tools";
            Logger.PrintArt();
            Logger.WriteBarrierLine("1", "Steam");
            Logger.WriteBarrierLine("2", "mega.nz");
            Logger.WriteBarrierLine("3", "Discord");
            Logger.WriteBarrierLine("4", "Hulu");

            Console.Write("-> ");
            ConsoleKey Choice = Console.ReadKey().Key;

            switch (Choice)
            {
                case ConsoleKey.D1:
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Settings.mw = new MainWindow();
                    Application.Run(Settings.mw);
                    break;
                case ConsoleKey.D2:
                    MegaAccountChecker.Main(Logger);
                    break;
                case ConsoleKey.D3:
                    DiscordAccountChecker.Main(Logger);
                    break;
                case ConsoleKey.D4:
                    HuluAccountChecker.Main(Logger);
                    break;
            }
        }
    }
}
