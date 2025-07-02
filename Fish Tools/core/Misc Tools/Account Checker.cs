/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.MiscTools.AccountChecker.Checkers;
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.MiscTools.AccountChecker
{
    internal class AccountChecker : ITool
    {
        public string Name => "Account Checker";
        public string Category => "Misc Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Check account validity for various services";

        public void Main(Logger Logger)
        {
            Console.Clear();
            Console.Title = "Fish Tools";
            Logger.PrintArt();
            Logger.WriteBarrierLine("1", "Steam");
            Logger.WriteBarrierLine("2", "mega.nz");
            Logger.WriteBarrierLine("3", "Discord");
            Logger.WriteBarrierLine("4", "Hulu");
            Logger.WriteBarrierLine("0", "Back");

            Console.Write("-> ");
            ConsoleKey Choice = Console.ReadKey().Key;

            switch (Choice)
            {
                case ConsoleKey.D1:
                    new SteamAccountChecker().Main(Logger);
                    break;
                case ConsoleKey.D2:
                    new MegaAccountChecker().Main(Logger);
                    break;
                case ConsoleKey.D3:
                    new DiscordAccountChecker().Main(Logger);
                    break;
                case ConsoleKey.D4:
                    new HuluAccountChecker().Main(Logger);
                    break;
                case ConsoleKey.D0:
                    return;
            }
        }
    }
}
