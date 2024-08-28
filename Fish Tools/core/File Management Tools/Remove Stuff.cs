/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
namespace Fish_Tools.core.FileManagementTools
{
    internal class Remove_Stuff
    {
        public static void doit()
        {
            string inputFilePath = "C:\\Users\\notfishvr\\Documents\\Outher\\VS\\C#\\Fish Tools\\bin\\test.txt";
            string outputFilePath = "C:\\Users\\notfishvr\\Documents\\Outher\\VS\\C#\\Fish Tools\\bin\\test2.txt";
            RemoveStuffDontNeed(inputFilePath, outputFilePath);
        }
        public static void RemoveStuffDontNeed(string inputFilePath, string outputFilePath)
        {
            string[] lines = File.ReadAllLines(inputFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                int index = lines[i].IndexOf(" ");
                if (index != -1)
                {
                    lines[i] = lines[i].Substring(0, index);
                }
            }
            File.WriteAllLines(outputFilePath, lines);
        }
    }
}
