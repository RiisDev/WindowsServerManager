using System.Text.Json;

namespace WindowsServerManager.BugCheck
{
    internal class Program
    {
        public static LogService LogService = new($@"{AppDomain.CurrentDomain.BaseDirectory}\..\..\logs\bug-check-log_{DateTime.Now:yyyy-MM-dd}.txt");
        public static void Main(string[] args)
        {
            LogService.LogInformation("Application started");
            List<Win32.BugCheck> bugChecks = [];
            string[] dumps = Directory.GetFiles(@"C:\Windows\Minidump", "*.dmp", SearchOption.TopDirectoryOnly);
            LogService.LogInformation($"Found Files: {string.Join('|', dumps)}");

            foreach (string dump in dumps)
            {
                FileInfo info = new(dump);
                string tempFileName = $"{Path.GetTempPath()}{Guid.NewGuid().ToString()}.dmp";
                File.Copy(dump, tempFileName, true);

                Win32.BugCheck? bugCheck = Win32.AnalyzeKernelDump(tempFileName, info.CreationTime).Result;

                if (bugCheck is not null)
                    bugChecks.Add(bugCheck);

                File.Delete(tempFileName);
            }

            Console.WriteLine(JsonSerializer.Serialize(bugChecks));
        }
    }
}
