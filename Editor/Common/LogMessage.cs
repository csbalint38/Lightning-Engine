using Editor.Common.Enums;
using System.IO;

namespace Editor.Common
{
    public class LogMessage(LogLevel logLevel, string message, string file, string caller, int line)
    {
        public DateTime Time { get; } = DateTime.Now;
        public LogLevel LogLevel { get; } = logLevel;
        public string Message { get; } = message;
        public string File { get; } = Path.GetFileName(file);
        public string Caller { get; } = caller;
        public int Line { get; } = line;
        public string MetaData => $"{File}: {Caller}({Line})";
    }
}
