using System.Diagnostics;

namespace WindowsServerManager.Libraries.Services.ExternalService
{
    public abstract class ServiceBase
    {
        private static readonly CancellationTokenSource CancellationTokenSource = new();
        private readonly CancellationToken _cancellationToken = CancellationTokenSource.Token;

        public bool Running => _running;
        private volatile bool _running;
        private readonly int _updateDelay;
        private readonly string _runtime;
        private readonly string[] _arguments;

        protected abstract void BindOutput(string output);

        protected abstract bool ShouldRun();
        
        protected ServiceBase(int updateDelay, string runtime, params string[] arguments)
        {
            _updateDelay = updateDelay;
            _runtime = runtime;
            _arguments = arguments;

            Task.Run(RunAsync);
        }

        public async Task RunAsync()
        {

#if DEBUG // I do not care to run these all the time
            return;
#else
            if (!ShouldRun())
                return;
#endif

            while (!_cancellationToken.IsCancellationRequested)
            {
                Interlocked.Exchange(ref _running, true);

                Process process = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        Verb = "runas",
                        FileName = $@"{AppDomain.CurrentDomain.BaseDirectory}runtimes\servermanager\{_runtime}",
                        Arguments = string.Join(" ", _arguments),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                string systemOutput = await process.StandardOutput.ReadToEndAsync(_cancellationToken);
                string systemError = await process.StandardError.ReadToEndAsync(_cancellationToken);

                await process.WaitForExitAsync(_cancellationToken);

                await Program.LogService.LogInformation($"{_runtime[..^4]} Checker: {systemOutput}");

                BindOutput(systemOutput);

                if (!string.IsNullOrEmpty(systemError))
                    await Program.LogService.LogError(systemError);

                Interlocked.Exchange(ref _running, false);

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(_updateDelay), _cancellationToken);
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

        public void Stop() => CancellationTokenSource.Cancel();
    }
}
