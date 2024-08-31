/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.BypassTools;
using Fish_Tools.core.FileManagementTools;
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.MiscTools
{
    internal class ComboEditer
    {
        public static bool ComboLoaded = false;
        public static string[] Combos;
        public static void Main(Logger Logger)
        {
            if (!ComboLoaded)
            {
                Console.Clear();
                Console.Title = "Fish Tools";
                Logger.PrintArt();
                Logger.Info("Path to Combos");
                string combos = Console.ReadLine();
                Combos = File.ReadAllLines(combos);
                ComboLoaded = true;
            }
            else
            {
                Console.Clear();
                Console.Title = "Fish Tools";
                Logger.PrintArt();

                Logger.WriteBarrierLine("1", "");

                Console.Write("-> ");
                ConsoleKey Choice = Console.ReadKey().Key;

                switch (Choice)
                {
                    case ConsoleKey.D1:

                        break;
                }
            }
        }
    }
}