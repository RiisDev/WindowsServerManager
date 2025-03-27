using System;
using System.Diagnostics;
using System.Text.Json;
using WindowsServerManager.Libraries.Utilities.Expanders;

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
        
        public int OldSysCount { get; set; }
        public int OldSoftCount { get; set; }

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
            await Program.LogService.LogInformation($@"{AppDomain.CurrentDomain.BaseDirectory}runtimes\servermanager\WindowsServerManager.SoftwareUpdater.exe");
            await Program.LogService.LogInformation($@"{AppDomain.CurrentDomain.BaseDirectory}runtimes\servermanager\WindowsServerManager.SystemUpdater.exe");
            while (!_cts.Token.IsCancellationRequested)
            {
                _running = true;
                try
                {

                    Process process = new()
                    {

                        StartInfo =
                        {
                            Verb = "runas",
                            FileName = $@"{AppDomain.CurrentDomain.BaseDirectory}runtimes\servermanager\WindowsServerManager.SoftwareUpdater.exe",
                            ArgumentList = { "check" },
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();

                    await process.WaitForExitAsync(_cts.Token);

                    string softwareOutput = await process.StandardOutput.ReadToEndAsync();
                    string softwareError = await process.StandardError.ReadToEndAsync();

                    await process.WaitForExitAsync();

                    await Program.LogService.LogInformation($"Updater (Software): {softwareOutput}");
                    await Program.LogService.LogError(softwareError);

                    _programUpdates = JsonSerializer.Deserialize<List<ProgramUpdate>>(softwareOutput);

                    process = new Process
                    {
                        StartInfo =
                        {
                            Verb = "runas",
                            FileName = $@"{AppDomain.CurrentDomain.BaseDirectory}runtimes\servermanager\WindowsServerManager.SystemUpdater.exe",
                            ArgumentList = { "check" },
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();

                    await process.WaitForExitAsync(_cts.Token);

                    string systemOutput = await process.StandardOutput.ReadToEndAsync();
                    string systemError = await process.StandardError.ReadToEndAsync();

                    await process.WaitForExitAsync();

                    await Program.LogService.LogInformation($"Updater (System): {systemOutput}");

                    if (!string.IsNullOrEmpty(systemError))
                        await Program.LogService.LogError(systemError);

                    _systemUpdates = JsonSerializer.Deserialize<List<SystemUpdate>>(systemOutput);
                }
                catch (Exception ex)
                {
                    await Program.LogService.LogError(ex.ToString());
                }

                _running = false;


                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(updateDelay), _cts.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    await Program.LogService.LogError(ex.ToString());
                }
            }
        }

        public void Stop() => _cts.Cancel();
    }
}
