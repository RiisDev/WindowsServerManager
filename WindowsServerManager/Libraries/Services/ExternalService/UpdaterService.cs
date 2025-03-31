using System.Text.Json;

namespace WindowsServerManager.Libraries.Services.ExternalService
{
    public class SystemUpdaterService(int updateDelay) : ServiceBase(updateDelay, "WindowsServerManager.SystemUpdater.exe", "check")
    {
        public List<SystemUpdate>? SystemUpdates => _systemUpdates;
        private volatile List<SystemUpdate>? _systemUpdates = [];

        public int OldSysCount { get; set; }
        public record SystemUpdate(string Name, string UpdateId, string Severity, string Mandatory);

        protected override void BindOutput(string output) => _systemUpdates = JsonSerializer.Deserialize<List<SystemUpdate>>(output);

        protected override bool ShouldRun()
        {
            UpdateSettings? updateSettings = Program.Settings?.UpdateSettings;
            return updateSettings is not null && (updateSettings.EnableSystemUpdateChecker ?? false);
        }
    }

    public class SoftwareUpdaterService(int updateDelay) : ServiceBase(updateDelay, "WindowsServerManager.SoftwareUpdater.exe", "check")
    {
        public record ProgramUpdate(string Name, string CurrentVersion, string NewVersion, string Download);
        
        private volatile List<ProgramUpdate>? _programUpdates = [];
        public List<ProgramUpdate>? ProgramUpdates => _programUpdates;

        public int OldSoftCount { get; set; }

        protected override void BindOutput(string output) => _programUpdates = JsonSerializer.Deserialize<List<ProgramUpdate>>(output);

        protected override bool ShouldRun()
        {
            UpdateSettings? updateSettings = Program.Settings?.UpdateSettings;
            return updateSettings is not null && (updateSettings.EnableSoftwareUpdateChecker ?? false);
        }
    }
}
