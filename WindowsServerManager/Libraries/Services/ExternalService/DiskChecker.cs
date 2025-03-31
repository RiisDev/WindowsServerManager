using System.Text.Json;

namespace WindowsServerManager.Libraries.Services.ExternalService
{
    public class DiskCheckService(int updateDelay) : ServiceBase(updateDelay, "WindowsServerManager.DiskCheck.exe")
    {
        public record SmartData(string ModelName, string SerialNumber, string DriveLetter, string Size, string HealthStatus, string Temperature, long PowerOnHours, long PowerOnCount, long? ReAllocatedSectors = 0, long? UncorrectableSectors = 0, long? PendingSectors = 0, long? Reads = 0, long? Writes = 0, long? HeliumLevel = 0);
        private volatile List<SmartData>? _diskStats = [];
        public List<SmartData>? DiskStats => _diskStats;

        protected override void BindOutput(string output) => _diskStats = JsonSerializer.Deserialize<List<SmartData>>(output);

        protected override bool ShouldRun()
        {
            EventViewerSettings? settings = Program.Settings?.EventViewerSettings;
            return settings is not null && (settings.Enabled ?? false) && (settings.ViewerOptions?.Disk ?? false);
        }

    }

}
