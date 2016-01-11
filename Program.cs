using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace MigHelper
{
    public class Config
    {
        public string Path { get; set; }
        public string OutputPath { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var config = Args.Configuration.Configure<Config>().CreateAndBind(args);

            PromptPath(config);
            PromptOutputPath(config);

            DirectoryInfo dir = new DirectoryInfo(config.Path);
            var orderedDirectories = dir.GetDirectories("*", SearchOption.AllDirectories).OrderBy(x => x.FullName);
            var orderedFiles = dir.GetFiles("*", SearchOption.AllDirectories).OrderBy(x => x.FullName);
            
            string backupFilesPattern = "^" + config.Path.Replace(@"\", @"\\") + @"\\+{0}Backup Files[^\\]*\\*$";
            var backupFilesDirectories = orderedDirectories.Where(x => Regex.IsMatch(x.FullName, string.Format(backupFilesPattern, @"Backup Set[^\\]*\\")))
                                        .ToList();

            var selectedPoint = PromptSelectRestorePoint(backupFilesDirectories);
            var selectedPoints = backupFilesDirectories.ToList();
            selectedPoints = selectedPoints.GetRange(0, selectedPoints.IndexOf(selectedPoint) + 1);

            Console.Clear();

            IEnumerable<FileInfo> backupFilesArchives = Enumerable.Empty<FileInfo>();
            //get all selected zip files
            foreach (var item in selectedPoints)
            {
                var pattern = item.FullName.Replace(@"\", @"\\") + @"\\Backup files \d+.zip$";
                backupFilesArchives = backupFilesArchives
                                          .Concat(orderedFiles.Where(x => Regex.IsMatch(x.FullName, pattern)));
            }

            int count = 0;
            foreach (FileInfo e in backupFilesArchives)
            {
                DrawTextProgressBar(++count, backupFilesArchives.Count());
                StaticWrite(e.FullName, 1);

                using (ZipArchive archive = ZipFile.OpenRead(e.FullName))
                {
                    ExtractToDirectory(archive, config.OutputPath, true);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to end:");
            Console.ReadKey();

        }

        private static void PromptPath(Config config, int line = 0, string error = null)
        {
            do
            {
                if (string.IsNullOrWhiteSpace(config.Path) || !string.IsNullOrWhiteSpace(error))
                {
                    config.Path = PromptLine("Enter the path to MIG to load.", line, error);
                    error = null;
                }

                if (!Directory.Exists(config.Path))
                {
                    error = "The directory does not exist.";
                }
            } while (!string.IsNullOrWhiteSpace(error));
        }

        private static void PromptOutputPath(Config config, int line = 0, string error = null)
        {
            do
            {
                if (string.IsNullOrWhiteSpace(config.OutputPath) || !string.IsNullOrWhiteSpace(error))
                {
                    config.OutputPath = PromptLine("Enter the output path.", line, error);
                    error = null;
                }

                if (!Directory.Exists(config.OutputPath))
                {
                    error = "The directory does not exist.";
                }
            } while (!string.IsNullOrWhiteSpace(error));
        }

        private static string PromptLine(string message, int line = 0, string error = null)
        {
            StaticWrite(message, line);
            string value;
            do
            {
                int currentLine = line;
                WriteError(error, ++currentLine);
                Console.CursorTop = ++currentLine;
                value = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(value))
                {
                    error = "Enter a value.";
                    continue;
                }

                error = null;

            } while (!string.IsNullOrWhiteSpace(error));
            Console.Clear();
            return value;
        }

        ///http://stackoverflow.com/a/14795752
        private static void ExtractToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }

            int count = 0;
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                string directory = Path.GetDirectoryName(completeFileName);

                DrawTextProgressBar(count++, archive.Entries.Count, 2);
                StaticWrite(file.FullName, 3);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (file.Name != "" && !Regex.IsMatch(file.FullName, @"^.*Zone\.Identifier$"))
                {
                    file.ExtractToFile(completeFileName, true);
                }
            }
        }

        private static DirectoryInfo PromptSelectRestorePoint(IList<DirectoryInfo> entries, string error = null)
        {
            Console.CursorTop = 0;
            Console.CursorLeft = 0;

            Console.WriteLine("Restore Points Found: " + entries.Count());
            Console.WriteLine("Select a point to restore from:");

            int backupCount = 0;
            foreach (var item in entries)
            {
                Console.WriteLine("[" + backupCount++ + "] " + item.FullName);
            }

            int errorPosition = entries.Count() + 2;
            string selectedRestorePoint;
            int selectedRestorePointIndex;

            do
            {
                WriteError(error, errorPosition);
                selectedRestorePoint = Console.ReadLine();
                
                if (!int.TryParse(selectedRestorePoint, out selectedRestorePointIndex))
                {
                    error = "You must select a number";
                    continue;
                }

                if (selectedRestorePointIndex >= entries.Count())
                {
                    error = "Select a valid option.";
                    continue;
                }

                error = null;
            } while (!string.IsNullOrWhiteSpace(error));

            return entries.ToList()[selectedRestorePointIndex];
        }

        private static void WriteError(string error, int errorPosition = 0, params object[] args)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            StaticWrite(error, errorPosition);
            Console.ForegroundColor = originalColor;
        }

        //http://stackoverflow.com/questions/24918768/progress-bar-in-console-application
        private static void DrawTextProgressBar(int progress, int total, int lineNumber = 0)
        {
            Console.CursorTop = lineNumber;

            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = 32;
            Console.Write("]"); //end
            Console.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i < onechunk * progress; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw unfilled part
            for (int i = position; i <= 31; i++)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        }

        private static void StaticWrite(string message, int lineNumber, params object[] args)
        {
            //draw empty progress bar
            Console.CursorTop = lineNumber;
            Console.CursorLeft = 0;

            message = message ?? string.Empty;
            if (message.Length > Console.WindowWidth)
            {
                message = message.Substring(0, Console.WindowWidth - 4) + " ...";
            }

            //get total lines to clear 
            int clearWidth = message.Length < Console.WindowWidth ? Console.WindowWidth - message.Length : 0;
            Console.Write(message + new string(' ', clearWidth), args);
        }
    }
}
