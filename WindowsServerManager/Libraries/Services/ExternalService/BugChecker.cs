using System.Text.Json;

namespace WindowsServerManager.Libraries.Services.ExternalService
{
    public class BugCheckService(int updateDelay) : ServiceBase(updateDelay, "WindowsServerManager.BugCheck.exe")
    {
        public record BugCheck(DateTime Timestamp, string ProcessName, string ImageName, string Module, string BsodType, string Description);

        private volatile List<BugCheck>? _bugChecks = [];
        public List<BugCheck>? BugChecks => _bugChecks;

        protected override void BindOutput(string output) => _bugChecks = JsonSerializer.Deserialize<List<BugCheck>>(output);

        protected override bool ShouldRun()
        {
            EventViewerSettings? settings = Program.Settings?.EventViewerSettings;
            return settings is not null && (settings.Enabled ?? false) && (settings.ViewerOptions?.BugCheck ?? false);
        }

    }
}
