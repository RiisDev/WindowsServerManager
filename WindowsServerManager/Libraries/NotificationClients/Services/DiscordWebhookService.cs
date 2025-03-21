using System.Text.Json.Serialization;
using MudBlazor;
using WindowsServerManager.Libraries.Utilities.Expanders;

namespace WindowsServerManager.Libraries.NotificationClients.Services
{
    public static class DiscordWebhookService
    {
        private record Embed(
            [property: JsonPropertyName("title")] string? Title,
            [property: JsonPropertyName("description")] string Description, [property: JsonPropertyName("color")] int? Color,
            [property: JsonPropertyName("footer")] Footer Footer,
            [property: JsonPropertyName("timestamp")] string? Timestamp
        );
        public record Footer(
            [property: JsonPropertyName("text")] string Text
        );

        private record EmbedRoot(
            [property: JsonPropertyName("content")] string? Content,
            [property: JsonPropertyName("embeds")] IReadOnlyList<Embed> Embeds,
            [property: JsonPropertyName("attachments")] IReadOnlyList<object> Attachments
        );

        private static readonly Dictionary<Severity, int> SeverityColors = new ()
        {
            { Severity.Info, 5814783},
            { Severity.Normal, 2930402},

            { Severity.Error, 16721446},
            { Severity.Success, 2555773},
            { Severity.Warning, 14974753}
        };

        public static async Task Send(string text, string title, Severity severity, string rolePing, DateTime timeStamp, string webhook)
        {
            EmbedRoot embedData = new(
                Content: rolePing.IsNullOrEmpty() ? null : rolePing.Contains("<@") ? rolePing : $"<@{rolePing}>",
                Embeds:
                [
                    new Embed(
                        Title: $"Windows Server Manager - {title}",
                        Description: text,
                        Color: SeverityColors[severity],
                        Footer: new Footer("Windows Server Manager"),
                        Timestamp: timeStamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    )
                ],
                Attachments: []
            );
            Program.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            await Program.HttpClient.PostAsync(webhook, JsonContent.Create(embedData));
            Program.HttpClient.DefaultRequestHeaders.Remove("Content-Type");
        }
    }
}