using System.Diagnostics;

namespace WindowsServerManager.SystemUpdater
{
    public static class Execution
    {
        public static async Task<string> ExecutePowerShell(string command) => await ExecuteConHost($"powershell \"{command}\"");

        public static async Task<string> ExecuteConHost(string command)
        {
            return await Task.Run(() =>
            {
                using Process process = new();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/C {command}";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.Verb = "runas";

                process.Start();

                using StreamReader reader = process.StandardOutput;
                return reader.ReadToEnd();
            });
        }
    }
}
