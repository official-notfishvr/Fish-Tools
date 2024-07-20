using System;
using System.Drawing;
using Console = ConsoleLib.Console;

namespace Fish_Tools.core.Utils
{
    public interface ILogger
    {
        void Error(string message);

        void Info(string message);

        void Warn(string message);

        void Debug(string message);

        void Success(string message);

        void Write(string message);

        void WriteBarrierLine(string num, string message);

        void PrintArt();
    }
    public class Logger : ILogger
    {
        private string currentTime = DateTime.Now.ToString("T");
        public void Write(string message)
        {
            Console.Write("[", Color.White);
            Console.Write(currentTime, Color.Aqua);
            Console.Write("] ", Color.White);
            Console.Write(message + "\n", Color.BlueViolet);
        }
        public void Debug(string message)
        {
            Console.Write("[", Color.White);
            Console.Write(currentTime, Color.Aqua);
            Console.Write("]", Color.White);
            Console.Write("[", Color.White);
            Console.Write("*", Color.BlueViolet);
            Console.Write("] ", Color.White);
            Console.Write(message + "\n", Color.BlueViolet);
        }
        public void Error(string message)
        {
            Console.Write("[", Color.White);
            Console.Write(currentTime, Color.Aqua);
            Console.Write("]", Color.White);
            Console.Write("[", Color.White);
            Console.Write("!", Color.Red);
            Console.Write("] ", Color.White);
            Console.Write(message + "\n", Color.Red);
        }
        public void Info(string message)
        {
            Console.Write("[", Color.White);
            Console.Write(currentTime, Color.Aqua);
            Console.Write("]", Color.White);
            Console.Write("[", Color.White);
            Console.Write("^", Color.DodgerBlue);
            Console.Write("] ", Color.White);
            Console.Write(message + "\n", Color.DodgerBlue);
        }
        public void Success(string message)
        {
            Console.Write("[", Color.White);
            Console.Write(currentTime, Color.Aqua);
            Console.Write("]", Color.White);
            Console.Write("[", Color.White);
            Console.Write("+", Color.LimeGreen);
            Console.Write("] ", Color.White);
            Console.Write(message + "\n", Color.LimeGreen);
        }
        public void Warn(string message)
        {
            Console.Write("[", Color.White);
            Console.Write(currentTime, Color.Aqua);
            Console.Write("]", Color.White);
            Console.Write("[", Color.White);
            Console.Write("!", Color.HotPink);
            Console.Write("] ", Color.White);
            Console.Write(message + "\n", Color.HotPink);
        }
        public void WriteBarrierLine(string num, string message)
        {
            Console.Write("[", Color.White);
            Console.Write(num, Color.Aqua);
            Console.Write("] ", Color.White);
            Console.Write(message + "\n", Color.HotPink);
        }
        public void PrintArt()
        {
            string[] asciiDesign =
            {
                "",
                "████████╗░█████╗░░█████╗░██╗░░░░░░██████╗",
                "╚══██╔══╝██╔══██╗██╔══██╗██║░░░░░██╔════╝",
                "░░░██║░░░██║░░██║██║░░██║██║░░░░░╚█████╗░",
                "░░░██║░░░██║░░██║██║░░██║██║░░░░░░╚═══██╗",
                "░░░██║░░░╚█████╔╝╚█████╔╝███████╗██████╔╝",
                "░░░╚═╝░░░░╚════╝░░╚════╝░╚══════╝╚═════╝░",
                "",
            };

            Array.ForEach(asciiDesign, line => Console.WriteLine(line.PadLeft((Console.WindowWidth - line.Length) / 2 + line.Length), Color.Aqua));
        }
    }
}
