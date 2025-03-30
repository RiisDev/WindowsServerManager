using System.Diagnostics;
using System.Text;

namespace WindowsServerManager.Libraries.Services
{
    public class LogService
    {
        private static DateTime _currentLogDate = DateTime.Now.Date;
        private static volatile StreamWriter _logStream = InitializeLogStream();

        private static StreamWriter InitializeLogStream()
        {
            string logFile = $@"{AppDomain.CurrentDomain.BaseDirectory}\logs\log_{DateTime.Now:yyyy-MM-dd}.txt";
            string? logDirectory = Path.GetDirectoryName(logFile);
            if (!Directory.Exists(logDirectory)) Directory.CreateDirectory(logDirectory!);
            if (!File.Exists(logFile)) File.Create(logFile).Close();

            return new StreamWriter(new FileStream(logFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.ASCII)
            {
                AutoFlush = true
            };
        }

        private static void CheckForNewLogFile()
        {
            DateTime now = DateTime.Now.Date;
            if (now <= _currentLogDate) return;
            lock (_logStream)
            {
                _logStream.Close();
                _currentLogDate = now;
                _logStream = InitializeLogStream();
            }
        }

        private static void LogToFile(string? message, string type)
        {
            CheckForNewLogFile();
            lock (_logStream) _logStream.WriteLine(ParseMessage(message, type));
        }

        private static string ParseMessage(string? message, string type)
        {
            return $"[{DateTime.Now}] {type} {(string.IsNullOrEmpty(message) ? "Undefined Message" : message)}";
        }

        public Task LogInformation(string? message)
        {
            LogToFile(message, "[INF]");
            LogToEventLog(message, EventLogEntryType.Information);
            return Task.CompletedTask;
        }

        public Task LogError(string? message)
        {
            LogToFile(message, "[ERR]");
            LogToEventLog(message, EventLogEntryType.Error);
            return Task.CompletedTask;
        }

        public Task LogWarning(string? message)
        {
            LogToFile(message, "[WARN]");
            LogToEventLog(message, EventLogEntryType.Warning);
            return Task.CompletedTask;
        }

        public Task LogDebug(string? message)
        {
            LogToFile(message, "[DEBUG]");
            return Task.CompletedTask;
        }

        private void LogToEventLog(string? message, EventLogEntryType entryType)
        {
            try
            {
                using EventLog eventLog = new("Application");
                eventLog.Source = "Windows Server Manager";
                eventLog.WriteEntry(message ?? "Undefined Message", entryType);
            }
            catch {/**/}
        }
    }

}
