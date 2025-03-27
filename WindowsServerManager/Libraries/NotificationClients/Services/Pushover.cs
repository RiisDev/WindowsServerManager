using System.Net;
using System.Text;
using MudBlazor;

namespace WindowsServerManager.Libraries.NotificationClients.Services
{
    public static class Pushover
    {
        private static readonly Dictionary<Severity, int> PushoverPriority = new()
        {
            { Severity.Success, 0},
            { Severity.Info, -1},
            { Severity.Normal, -1},
            { Severity.Warning, 1},
            { Severity.Error, 2}
        };

        // Who ever designed PushOver API should be beat
        public static async Task SendNotification(string pushoverToken, string pushoverUser, string pushoverDevice, string title, string description, Severity severity = Severity.Info)
        {
            StringBuilder postString = new();
            postString.Append($"token={pushoverToken}");
            postString.Append($"&user={pushoverUser}");

            if (!string.IsNullOrEmpty(pushoverDevice))
                postString.Append($"&device={pushoverDevice}");

            postString.Append($"&title={title}");
            postString.Append($"&message={description}");
            postString.Append($"&priority={PushoverPriority[severity]}");

            string postFinal = WebUtility.HtmlEncode(postString.ToString());

            Program.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
            await Program.HttpClient.PostAsync("https://api.pushover.net/1/messages.json", new StringContent(postFinal));
            Program.HttpClient.DefaultRequestHeaders.Clear();
        }
    }
}
