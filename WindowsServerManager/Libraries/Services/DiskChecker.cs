﻿using System.Diagnostics;
using System.Text.Json;

namespace WindowsServerManager.Libraries.Services
{
    public class DiskCheckService
    {
        public record SmartData(
            string ModelName,
            string SerialNumber,
            string DriveLetter,
            string Size,
            string HealthStatus,
            string Temperature,
            long PowerOnHours,
            long PowerOnCount,
            long? ReAllocatedSectors = 0,
            long? UncorrectableSectors = 0,
            long? PendingSectors = 0,
            long? Reads = 0,
            long? Writes = 0,
            long? HeliumLevel = 0);

        private volatile List<SmartData>? _diskStats = [];
        public List<SmartData>? DiskStats => _diskStats;

        private readonly CancellationTokenSource _cts = new();

        private volatile bool _running;

        public bool Running => _running;
        
        public DiskCheckService() => Task.Run(RunBackgroundTask);

        private async Task RunBackgroundTask()
        {
            EventViewerSettings? settings = Program.Settings?.EventViewerSettings;
            if (settings is null) return;

            if (!(settings.Enabled ?? false) || !(settings.ViewerOptions?.Disk ?? false))
                return; // Disabled updates, do not run.

            int updateDelay = settings.RecheckTimeMinutes ?? 60;

            while (!_cts.Token.IsCancellationRequested)
            {
                _running = true;
                Interlocked.Exchange(ref _running, true);

                Process process = new()
                {
                    StartInfo =
                    {
                        Verb = "runas",
                        FileName = $@"{AppDomain.CurrentDomain.BaseDirectory}runtimes\servermanager\WindowsServerManager.DiskCheck.exe",
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

                await Program.LogService.LogInformation($"Disk Checker: {systemOutput}");

                if (!string.IsNullOrEmpty(systemError))
                    await Program.LogService.LogError(systemError);

                Interlocked.Exchange(ref _running, false);

                _diskStats = JsonSerializer.Deserialize<List<SmartData>>(systemOutput);

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
