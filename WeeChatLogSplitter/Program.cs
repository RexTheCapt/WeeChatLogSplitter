using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeeChatLogSplitter
{
    internal class Program
    {
        private static string logLocations = @"D:\Documents\IRC Logs";
        private static DateTime lastPrint;
        
        internal static void Main(string[] args)
        {
            string tmpLocation = "tmp\\";
            if (Directory.Exists(tmpLocation))
                Directory.Delete(tmpLocation, true);

            Directory.CreateDirectory(tmpLocation);

            List<LogFile> logFiles = new();
            foreach (string file in Directory.GetFiles(logLocations))
            {
                LogFile logFile = new();
                FileInfo oldInfo = new FileInfo(file);
                File.Copy(file, $"{tmpLocation}{oldInfo.Name}", false);
                string newPath = $"{tmpLocation}{oldInfo.Name}";
                FileInfo newInfo = new FileInfo(newPath);
                logFile.FileInfo = newInfo;

                List<LogEntry> logEntries = new();
                using (var reader = new StreamReader(newInfo.FullName))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        LogEntry entry = new LogEntry()
                        {
                            Date = ToDateTime(line.Split(' ')[0]),
                            Text = line
                        };
                        logEntries.Add(entry);
                    }
                }

                logFile.LogEntries = logEntries;
                logFiles.Add(logFile);
            }

            for (int fileIndex = 0; fileIndex < logFiles.Count; fileIndex++)
            {
                LogFile? logFile = logFiles[fileIndex];
                for (int entryIndex = 0; entryIndex < logFile.LogEntries.Count; entryIndex++)
                {
                    if (lastPrint.AddSeconds(1) < DateTime.Now)
                    {
                        Console.WriteLine($"Working on file {fileIndex + 1:000}/{logFiles.Count:000} entry {entryIndex + 1:00000}/{logFile.LogEntries.Count:00000} ({GetPercentage(n1: logFiles.Count, n2: fileIndex)} | {GetPercentage(logFile.LogEntries.Count, entryIndex)})");
                        lastPrint = DateTime.Now;
                    }

                    LogEntry? entry = logFile.LogEntries[entryIndex];
                    int[] splitDate = new int[]
                    {
                        entry.Date.Year,
                        entry.Date.Month,
                    };

                    string dir = $"{tmpLocation}";

                    foreach (var v in splitDate)
                    {
                        dir += $"{v.ToString("00")}\\";
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                    }

                    dir += $"{logFile.FileInfo.Name}";

                    using (var writer = new StreamWriter(dir, true))
                    {
                        writer.WriteLine(entry.Text);
                    }
                }
            }
        }

        private static string GetPercentage(int n1, int n2)
        {
            if (n1 == 0 || n2 == 0)
                return "000%";
            float p = (float)n2 / n1 * 100;
            return $"{p:000}%";
        }

        private static DateTime ToDateTime(string v)
        {
            string[] vs = v.Split('-');
            int year = int.Parse(vs[0]);
            int month = int.Parse(vs[1]);
            int day = int.Parse(vs[2]);

            return new DateTime(year, month, day);
        }

        private class LogFile
        {
            public List<LogEntry> LogEntries = new();
            public FileInfo FileInfo;

            public override string ToString()
            {
                return FileInfo.FullName;
            }
        }

        private class LogEntry
        {
            public DateTime Date { get; set; }
            public string Text { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}
