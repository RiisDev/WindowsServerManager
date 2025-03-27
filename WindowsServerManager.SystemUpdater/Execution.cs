using System.Diagnostics;

namespace WindowsServerManager.SystemUpdater
{
    public static class Execution
    {
        public static async Task<string> ExecutePowerShell(string command) => await ExecuteConHost($"powershell \"{command}\"");

        public static async Task<string> ExecuteConHost(string command)
        {
            await Program.LogService.LogInformation($"Process Start with arguments: cmd.exe /C {command}");
            return await Task.Run(async () =>
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

                string shellReturn = await process.StandardOutput.ReadToEndAsync();
                string shellError = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                Program.LogService?.LogInformation($"Process Return: {shellReturn.ToBase64()}");

                if (!string.IsNullOrEmpty(shellError))
                    Program.LogService?.LogError($"Process Error Return: {shellError}");

                return shellReturn;
            });
        }
    }
}
