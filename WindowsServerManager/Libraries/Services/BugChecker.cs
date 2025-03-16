using System.Diagnostics;
using System.Text.Json;

namespace WindowsServerManager.Libraries.Services
{
    public class BugCheckService
    {
        public record BugCheck(DateTime Timestamp, string ProcessName, string ImageName, string Module, string BsodType, string Description);

        private volatile List<BugCheck>? _bugChecks = [];
        public List<BugCheck>? BugChecks => _bugChecks;

        private readonly CancellationTokenSource _cts = new();

        private volatile bool _running;

        public bool Running => _running;
        
        public BugCheckService() => Task.Run(RunBackgroundTask);

        private async Task RunBackgroundTask()
        {
            EventViewerSettings? settings = Program.Settings?.EventViewerSettings;
            if (settings is null) return;

            if (!(settings.Enabled ?? false) || (settings.ViewerOptions?.BugCheck ?? false))
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
                        FileName = "WindowsServerManager.BugCheck.exe",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();

                await process.WaitForExitAsync(_cts.Token);

                Interlocked.Exchange(ref _running, false);

                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                _bugChecks = JsonSerializer.Deserialize<List<BugCheck>>(output);

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
