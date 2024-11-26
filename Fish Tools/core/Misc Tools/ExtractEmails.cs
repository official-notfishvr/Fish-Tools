/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Fish_Tools.core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fish_Tools.core.Misc_Tools
{
    internal class ExtractEmails
    {
        public static void ExtractEmailsMain(Logger logger)
        {
            logger.Info("Do you want to scan a folder? (y/n):");
            Console.Write("-> ");
            string folderInput = Console.ReadLine()?.Trim().ToLower();

            if (folderInput == "y")
            {
                logger.Info("Enter the folder path:");
                Console.Write("-> ");
                string folderPath = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                {
                    logger.Error("Invalid folder path.");
                    Console.ReadKey();
                    return;
                }

                var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories).Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".json", StringComparison.OrdinalIgnoreCase)).ToList();

                if (files.Count == 0)
                {
                    logger.Error("No .txt or .json files found in the folder.");
                    Console.ReadKey();
                    return;
                }

                logger.Info($"Found {files.Count} file(s). Extracting emails...");

                List<string> allEmails = new List<string>();

                foreach (var file in files)
                {
                    List<string> emails = ExtractEmailsFromFile(file);
                    allEmails.AddRange(emails);
                }

                if (allEmails.Any())
                {
                    logger.Success($"Found {allEmails.Count} email(s):");

                    //foreach (string email in allEmails) { logger.Success(email); }

                    logger.Info("Do you want to save the emails to a file? (y/n):");
                    Console.Write("-> ");
                    string input = Console.ReadLine()?.Trim().ToLower();

                    if (input == "y")
                    {
                        string outputPath = Path.Combine(folderPath, "ExtractedEmails.txt");
                        File.WriteAllLines(outputPath, allEmails.Distinct());
                        logger.Success($"Emails saved to: {outputPath}");
                        Console.ReadKey();
                    }
                }
                else
                {
                    logger.Error("No emails were found.");
                    Console.ReadKey();
                }
            }
            else
            {
                logger.Info("Enter the file path (.txt or .json):");
                Console.Write("-> ");
                string filePath = Console.ReadLine()?.Trim();
                filePath = filePath.Replace("\"", "");

                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    logger.Error("Invalid file path.");
                    Console.ReadKey();
                    return;
                }

                List<string> emails = new List<string>();

                try
                {
                    string extension = Path.GetExtension(filePath);
                    switch (extension.ToLower())
                    {
                        case ".txt":
                        case ".json":
                            emails = ExtractEmailsFromFile(filePath);
                            break;

                        default:
                            logger.Error("Unsupported file format. Please provide a .txt or .json file.");
                            Console.ReadKey();
                            return;
                    }

                    if (emails.Any())
                    {
                        logger.Success($"Found {emails.Count} email(s):");

                        //foreach (string email in emails) { logger.Success(email); }

                        logger.Info("Do you want to save the emails to a file? (y/n):");
                        Console.Write("-> ");
                        string input = Console.ReadLine()?.Trim().ToLower();

                        if (input == "y")
                        {
                            string outputPath = Path.Combine(Path.GetDirectoryName(filePath) ?? "", "ExtractedEmails.txt");
                            File.WriteAllLines(outputPath, emails);
                            logger.Success($"Emails saved to: {outputPath}");
                            Console.ReadKey();
                        }
                    }
                    else
                    {
                        logger.Error("No emails were found.");
                        Console.ReadKey();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"An error occurred: {ex.Message}");
                    Console.ReadKey();
                }
            }
        }
        private static List<string> ExtractEmailsFromFile(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                return ExtractEmailsFromContent(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {filePath}: {ex.Message}");
                return new List<string>();
            }
        }
        private static List<string> ExtractEmailsFromContent(string content)
        {
            Regex emailRegex = new Regex(@"[a-zA-Z0-9._%+-]+@(yahoo\.com|outlook\.com|icloud\.com|live\.com|proton\.me|protonmail\.com|simail\.info|gmail\.com|hotmail\.fr|hotmail\.com)", RegexOptions.IgnoreCase);
            MatchCollection matches = emailRegex.Matches(content);

            return matches
                .Cast<Match>()
                .Select(m => m.Value)
                .Distinct()
                .ToList();
        }
    }
}
