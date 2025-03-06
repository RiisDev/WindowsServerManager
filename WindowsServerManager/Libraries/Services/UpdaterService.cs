using System.Diagnostics;
using System.Text.Json;

namespace WindowsServerManager.Libraries.Services
{
    public class UpdaterService
    {
        public record ProgramUpdate(string Name, string CurrentVersion, string NewVersion, string Download);
        public record SystemUpdate(string Name, string UpdateId, string Severity, string Mandatory);

        private readonly CancellationTokenSource _cts = new();

        private volatile bool _running;

        private volatile List<SystemUpdate>? _systemUpdates = [];
        private volatile List<ProgramUpdate>? _programUpdates = [];

        public bool Running => _running;

        public List<SystemUpdate>? SystemUpdates => _systemUpdates;
        public List<ProgramUpdate>? ProgramUpdates => _programUpdates;
        
        public UpdaterService() => Task.Run(RunBackgroundTask);

        private async Task RunBackgroundTask()
        {
            UpdateSettings? updateSettings = Program.Settings?.UpdateSettings;
            if (updateSettings is null) return;

            if (!(updateSettings.EnableSoftwareUpdateChecker ?? false) && !(updateSettings.EnableSystemUpdateChecker ?? false))
                return; // Disabled updates, do not run.

            int updateDelay = updateSettings.UpdateRecheckTimeMinutes ?? 30;

            while (!_cts.Token.IsCancellationRequested)
            {
                
                Interlocked.Exchange(ref _running, true);

                Process process = new()
                {
                    StartInfo =
                    {
                        Verb = "runas",
                        FileName = "WindowsServerManager.SystemUpdater.exe",
                        ArgumentList = { "check" },
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                if ((updateSettings.EnableSoftwareUpdateChecker ?? false) && (updateSettings.EnableSystemUpdateChecker ?? false))
                    process.StartInfo.ArgumentList.Add("--all");
                else if (updateSettings.EnableSystemUpdateChecker ?? false)
                    process.StartInfo.ArgumentList.Add("--system");
                else if (updateSettings.EnableSoftwareUpdateChecker ?? false) 
                    process.StartInfo.ArgumentList.Add("--programs");

                process.Start();

                await process.WaitForExitAsync(_cts.Token);

                Interlocked.Exchange(ref _running, false);


                if (File.Exists("sys-updates.json"))
                    Interlocked.Exchange(ref _systemUpdates, JsonSerializer.Deserialize<List<SystemUpdate>>(await File.ReadAllTextAsync("sys-updates.json")));
                else
                    Console.WriteLine("Failed to check for system updates, file not found");

                if (File.Exists("soft-updates.json"))
                    Interlocked.Exchange(ref _programUpdates, JsonSerializer.Deserialize<List<ProgramUpdate>>(await File.ReadAllTextAsync("soft-updates.json")));
                else
                    Console.WriteLine("Failed to check for system updates, file not found");

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(updateDelay), _cts.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        public void Stop() => _cts.Cancel();
    }
}
