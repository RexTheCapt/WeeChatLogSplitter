namespace WeeChatLogSplitter
{
    internal class LogFile
    {
        public readonly string FullPath;
        public readonly string Server;
        public readonly string Channel;
        public readonly StreamReader ReadFileStream;

        public LogFile(string fullPath, string server, string channel, StreamReader readFileStream)
        {
            this.FullPath = fullPath;
            this.Server = server;
            this.Channel = channel;
            this.ReadFileStream = readFileStream;
        }
    }
}