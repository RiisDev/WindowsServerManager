using System.Diagnostics;
using System.Text;
using static System.Diagnostics.EventLogEntryType;

namespace WindowsServerManager.SoftwareUpdater
{
    public class LogService
    {
        public LogService(string logName)
        {
            LogName = logName;
            if (!Directory.Exists(Path.GetDirectoryName(LogName))) Directory.CreateDirectory(Path.GetDirectoryName(LogName)!);
            if (!File.Exists(LogName)) File.Create(LogName).Close();
            LogStream = new StreamWriter(new FileStream(LogName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.ASCII)
            {
                AutoFlush = true
            };

            try
            {
                if (!EventLog.SourceExists("Windows Server Manager - Soft Upd"))
                {
                    EventLog.CreateEventSource("Windows Server Manager - Soft Upd", "Application");
                }
            }
            catch {/**/}
        }

        internal static string LogName = null!;
        internal StreamWriter LogStream;

        internal string ParseMessage(string? message, string type)
        {
            return $"[{DateTime.Now}] {type} {(string.IsNullOrEmpty(message) ? "Undefined Message" : message)}";
        }

        public Task LogInformation(string? message)
        {
            LogToFile(message, "[INF]");
            LogToEventLog(message, Information);
            return Task.CompletedTask;
        }

        public Task LogError(string? message)
        {
            LogToFile(message, "[ERR]");
            LogToEventLog(message, Error);
            return Task.CompletedTask;
        }

        public Task LogWarning(string? message)
        {
            LogToFile(message, "[WARN]");
            LogToEventLog(message, Warning);
            return Task.CompletedTask;
        }

        public Task LogDebug(string? message)
        {
            LogToFile(message, "[DEBUG]");
            return Task.CompletedTask; // Debug logs can be written to file only
        }

        private void LogToFile(string? message, string type)
        {
            lock (LogStream) LogStream.WriteLine(ParseMessage(message, type));
        }

        private void LogToEventLog(string? message, EventLogEntryType entryType)
        {
            try
            {
                using EventLog eventLog = new("Application");
                eventLog.Source = "Windows Server Manager - Soft Upd";
                eventLog.WriteEntry(message, entryType);
            }
            catch {/**/}
        }
    }
}
