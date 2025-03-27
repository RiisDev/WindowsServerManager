using MudBlazor;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using WindowsServerManager.Libraries.NotificationClients.Services;
using WindowsServerManager.Libraries.Utilities.Expanders;

namespace WindowsServerManager.Libraries.NotificationClients
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class NotificationClient
    {
        private readonly Settings _settings;

        private volatile List<Notification> _notifications;

        public NotificationClient(Settings settings)
        {
            _settings = settings;
            try
            {
                _notifications = JsonSerializer.Deserialize<List<Notification>>(File.ReadAllText("notifications.json").FromBase64()) ?? [];
            }
            catch
            {
                _notifications = [];
            }
        }

        public ReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

        public record Notification(Severity Severity,string Title, string Text, DateTime Timestamp)
        {
            public bool Sent { get; set; }
            public bool Displayed { get; set; }

            public Guid Id = Guid.NewGuid();
        };
        
        public delegate void NotificationEvent(Notification notification);
        public event NotificationEvent? OnNotificationReceived;
        public event NotificationEvent? OnNotificationRemoved;

        public async Task Add(Notification notification)
        {
            _notifications.Add(notification);
            OnNotificationReceived?.Invoke(notification);
            await SendNotification(notification);
            WriteDatabase();
        }

        public void Remove(Notification notification)
        {
            Notification? note = _notifications.FirstOrDefault(x => x.Id == notification.Id);
            if (note is null) return;

            _notifications.Remove(note);
            OnNotificationRemoved?.Invoke(note);

            WriteDatabase();
        }

        public void Clear()
        {
            _notifications.Clear();
            WriteDatabase();
        }

        private async Task SendNotification(Notification notification)
        {
            Notification? note = _notifications.FirstOrDefault(x => x.Id == notification.Id);
            if (note is null) return;
            if (note.Sent) return;

            note.Sent = true;

            DiscordSettings? discordSettings = _settings.NotificationSettings.Discord;
            EmailSettings? emailSetting = _settings.NotificationSettings.Email;
            PushoverSettings? pushoverSettings = _settings.NotificationSettings.Pushover;

            try { 
                if (discordSettings is not null && (discordSettings?.WebhookEnabled ?? false))
                    await Task.Run(async () => await DiscordWebhookService.Send(note.Text, note.Title, note.Severity,
                        discordSettings?.RolePing ?? "", note.Timestamp, discordSettings?.WebhookUrl!));
            }catch {/**/}
            try {
                if (emailSetting is not null && (emailSetting?.EmailEnabled ?? false))
                    await Task.Run(async () => await MailService.SendEmailAsync(emailSetting.SmtpUsername!, emailSetting.SmtpPassword!, int.Parse(emailSetting.SmtpPort!), emailSetting.SmtpUsername!, emailSetting.SmtpHost!, emailSetting.EmailTo!, $"Windows Server Manager - {note.Title}", note.Text));
            }catch {/**/}
            try {
                if (pushoverSettings is not null && (pushoverSettings.PushoverEnabled ?? false))
                    await Task.Run(async () => await Pushover.SendNotification(pushoverSettings.PushoverToken!, pushoverSettings.PushoverUser!, pushoverSettings.PushoverDevice!, note.Title, note.Text, note.Severity));
            }catch { /**/ }

        }
        
        public void DisplayNotification(Notification notification, ISnackbar snackBar)
        {
            Notification? note = _notifications.FirstOrDefault(x => x.Id == notification.Id);
            if (note is null) return;
            if (note.Displayed) return;

            snackBar.Add(note.Text, note.Severity);

            note.Displayed = true;
        }

        private void WriteDatabase() => File.WriteAllText("notifications.json", JsonSerializer.Serialize(_notifications).ToBase64());
    }
}
