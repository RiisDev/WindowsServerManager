using System.Diagnostics;

namespace WindowsServerManager.Libraries.Win32.Shell
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
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.Verb = "runas";

                process.Start();

                using StreamReader reader = process.StandardOutput;
                using StreamReader errorReader = process.StandardError;

                string shellReturn = reader.ReadToEnd();
                string shellError = errorReader.ReadToEnd();

                Program.LogService.LogInformation($"Process Return: {shellReturn}");
                Program.LogService.LogError($"Process Error Return: {shellReturn}");

                return shellReturn;
            });
        }
    }
}
