using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeeChatLogSplitter
{
    internal class Program
    {
        private static string logLocation = @"D:\Documents\IRC Logs";
        private readonly static string tmpLocation = "tmp\\";

        internal static void Main(string[] args)
        {
            if (Directory.Exists(tmpLocation))
                Directory.Delete(tmpLocation, true);

            Directory.CreateDirectory(tmpLocation);

            List<LogFile> logFiles = GetLogFiles(logLocation);

            ProcessLogFiles(logFiles);
        }

        private static void ProcessLogFiles(List<LogFile> logFiles)
        {
            StreamWriter? writer = null;
            string? writingPath = null;
            string lastWriteFile = "";
            int lineNr = 0;

            foreach (LogFile log in logFiles)
            {
                Console.WriteLine(log.FullPath);

                while (!log.ReadFileStream.EndOfStream)
                {
                    string? line = log.ReadFileStream.ReadLine();
                    lineNr++;

                    if (line == null) continue;

                    try
                    {
                        DateTime date = ToDateTime(line.Split(' ')[0]);
                        string writeDir = $"{tmpLocation}{log.Server}\\{log.Channel}";
                        string writeFile = $"{writeDir}\\{date.ToString("yyyy-MM-dd")}.weechatlog";

                        if (!Directory.Exists(writeDir))
                            Directory.CreateDirectory(writeDir);

                        #region Start writing to new location
                        if (writingPath == null || writer == null)
                        {
                            NewWriter(path: writeFile, writer, out writer);
                            writingPath = log.FullPath;
                        }
                        else
                        {
                            if (!log.FullPath.Equals(writingPath, StringComparison.OrdinalIgnoreCase) || !lastWriteFile.Equals(writeFile))
                            {
                                NewWriter(path: writeFile, writer, out writer);
                                writingPath = log.FullPath;
                            }
                        }
                        #endregion

                        writer.WriteLine(line);
                        lastWriteFile = writeFile;
                    }
                    catch (Exception ex)
                    {
                        ConsoleColor c = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write($"[{lineNr}] ");
                        Console.WriteLine(ex.Message);
                        Console.ForegroundColor = c;
                    }
                }

                #region Close log reader
                log.ReadFileStream.Close();
                log.ReadFileStream.Dispose();
                #endregion

                lineNr = 0;
            }
        }

        private static DateTime ToDateTime(string v)
        {
            string[] vs = v.Split('-');
            int year = int.Parse(vs[0]);
            int month = int.Parse(vs[1]);
            int day = int.Parse(vs[2]);

            return new DateTime(year, month, day);
        }

        private static void NewWriter(string path, StreamWriter? oldWriter, out StreamWriter newWriter)
        {
            if (path.Contains("D:\\Documents\\", StringComparison.OrdinalIgnoreCase)) throw new Exception("Do not write to documents!");

            if (oldWriter != null)
            {
                oldWriter.Flush();
                oldWriter.Close();
                oldWriter.Dispose();
            }

            newWriter = new StreamWriter(path);
        }

        private static List<LogFile> GetLogFiles(string logLocation)
        {
            List<LogFile> logFiles = new List<LogFile>();

            foreach (string dir in Directory.GetDirectories(logLocation))
                logFiles.AddRange(GetLogFiles(dir));

            foreach (string path in Directory.GetFiles(logLocation))
            {
                string name = GetFileName(path);

                if (IsBlacklisted(name)) continue;

                string[] split = name.Split('.');
                string server = split[1];
                string channel = split[2];

                logFiles.Add(new(path, server, channel, new StreamReader(path)));
            }

            return logFiles;
        }

        private static bool IsBlacklisted(string name)
        {
            return name.Contains("core.weechat") || name.Contains("%2A");
        }

        private static string GetFileName(string path)
        {
            string[] split = path.Split('\\');
            return split[^1];
        }
    }
}
