using WindowsServerManager.Libraries.NotificationClients;
using Severity = MudBlazor.Severity;

namespace WindowsServerManager.Libraries.Services.ExternalService
{
    public class NotifierService
    {
        public SystemUpdaterService SystemUpdater = Program.SystemUpdaterService;
        public SoftwareUpdaterService SoftwareUpdater = Program.SoftwareUpdaterService;
        public BugCheckService BugChecks = Program.BugCheckService;
        public DiskCheckService DiskChecks = Program.DiskCheckService;

        private int _programUpdates = 0;
        private int _systemUpdates = 0;

        private int _bugChecks = 0;
        private int _diskChecks = 0;

        private async Task CheckUpdates()
        {
            if (SoftwareUpdater.ProgramUpdates is not null && SoftwareUpdater.ProgramUpdates.Count > 0)
            {
                _programUpdates = SoftwareUpdater.ProgramUpdates.Count;
            }

            if (SystemUpdater.SystemUpdates is not null && SystemUpdater.SystemUpdates.Count > 0)
            {
                _systemUpdates = SystemUpdater.SystemUpdates.Count;
            }

            if (_programUpdates > 0 || _systemUpdates > 0)
            {
                if (_programUpdates == SoftwareUpdater.OldSoftCount && _systemUpdates == SystemUpdater.OldSysCount) return;
                bool found = false;

                foreach (NotificationClient.Notification notificationData in Program.NotificationClient.Notifications)
                    if (notificationData.Text.Contains($" {_programUpdates} software updates, and {_systemUpdates}"))
                        found = true;

                if (!found)
                {
                    NotificationClient.Notification notification = new(Severity.Normal, "Updates Found", $"Update checker has found {_programUpdates} software updates, and {_systemUpdates} system updates.", DateTime.Now);
                    await Program.NotificationClient.Add(notification);
                }

                SoftwareUpdater.OldSoftCount = _programUpdates;
                SystemUpdater.OldSysCount = _systemUpdates;
            }
        }

        private async Task CheckSystemHealth()
        {
            if (BugChecks.BugChecks is not null && BugChecks.BugChecks.Count > 0)
            {
                _bugChecks = BugChecks.BugChecks.Count;
            }

            if (DiskChecks.DiskStats is not null && DiskChecks.DiskStats.Count > 0)
            {
                foreach (DiskCheckService.SmartData _ in DiskChecks.DiskStats.Where(data => !data.HealthStatus.Contains("Good")))
                    _diskChecks++;
            }

            if (_bugChecks > 0)
            {
                foreach (BugCheckService.BugCheck bugCheck in BugChecks.BugChecks!)
                {
                    bool exists = Program.NotificationClient.Notifications
                        .Any(notificationData =>
                            notificationData.Timestamp == bugCheck.Timestamp &&
                            notificationData.Text.Contains(bugCheck.Description) &&
                            notificationData.Title == "BugCheck Found");

                    if (exists) continue;

                    NotificationClient.Notification notification = new(
                        Severity.Warning,
                        "BugCheck Found",
                        $"System has detected a new BSOD, advise logs.\n\n{bugCheck.BsodType}\n{bugCheck.Description}",
                        bugCheck.Timestamp
                    );

                    await Program.NotificationClient.Add(notification);
                }
            }

            if (_diskChecks > 0)
            {
                NotificationClient.Notification notification = new(Severity.Error, "Disk Error Found", "System has detected a drive failure warning, please advise logs.", DateTime.Now);
                await Program.NotificationClient.Add(notification);
            }
        }

        private static readonly CancellationTokenSource CancellationTokenSource = new();
        private readonly CancellationToken _cancellationToken = CancellationTokenSource.Token;

        public NotifierService()
        {
            Task.Run(async () =>
            {
                await Task.Delay(30_000); // Wait before starting the notifier service to ensure other services are initialized
                while (!CancellationTokenSource.IsCancellationRequested)
                {
                    _ = Task.Run(CheckUpdates);
                    _ = Task.Run(CheckSystemHealth);
                    await Task.Delay(1000);
                }
            });
        }
    }
}
