using System.Diagnostics;

namespace WindowsServerManager.Libraries.Services
{
    public class UpdaterService
    {
        private readonly CancellationTokenSource _cts = new();

        private volatile bool _running;

        public bool Running => _running;
        
        public UpdaterService() => Task.Run(RunBackgroundTask);

        private async Task RunBackgroundTask()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                Interlocked.Exchange(ref _running, true);

                Process process = new()
                {
                    StartInfo =
                    {
                        Verb = "runas",
                        FileName = "WindowsServerManager.SystemUpdater.exe",
                        ArgumentList = { "check", "--all" },
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                await process.WaitForExitAsync(_cts.Token);

                Interlocked.Exchange(ref _running, false);

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(30), _cts.Token);
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
