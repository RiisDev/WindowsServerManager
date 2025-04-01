using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WindowsServerManager.Libraries;

namespace WindowsServerManager.Components.Pages.ArrSuite;

public partial class QBittorrent : IDisposable
{
    private readonly List<string> _categoryFilters = ["TV-Sonnar", "Radarr", "Nyaa"];

    private readonly Dictionary<string, string> _statusFilters = new()
    {
        { "All", CustomIcons.QBit.All },
        { "Downloading", CustomIcons.QBit.Download },
        { "Seeding", CustomIcons.QBit.Seeding },
        { "Completed", CustomIcons.QBit.Completed },
        { "Running", CustomIcons.QBit.Start },
        { "Stopped", CustomIcons.QBit.Stopped },
        { "Active", CustomIcons.QBit.Active },
        { "Inactive", CustomIcons.QBit.Inactive },
        { "Stalled", CustomIcons.QBit.Stalled },
        { "Errored", CustomIcons.QBit.Errored },
    };

    public record Torrent(
        string Status,
        string Name,
        string Size,
        string Seeds,
        string Peers,
        string DownSpeed,
        string UpSpeed,
        string Eta,
        string Ratio,
        string Popularity,
        string Category,
        string Tags,
        string AddedOn
    )
    {
        public string Progress { get; set; } = "0%";
    }

    private List<Torrent> _torrents = [];

    private readonly CancellationTokenSource _cts = new();

    private readonly HttpClient _httpClient = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
        UseCookies = true,
        CookieContainer = new CookieContainer(),
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        AutomaticDecompression = DecompressionMethods.All,
    });

    private async Task<List<Torrent>> GetTorrents()
    {
        List<Torrent> torrents = [];
        string torrentData = await _httpClient.GetStringAsync($"{Program.Settings?.ArrSuite.QBitTorrent?.Url}api/v2/torrents/info?filter=all");

        List<TorrentRootData>? torrentRootData = JsonSerializer.Deserialize<List<TorrentRootData>>(torrentData);
        if (torrentRootData is null) return torrents;

        torrents.AddRange(torrentRootData.Select(torrent => new Torrent(torrent.State, torrent.Name, torrent.Size.ToString()!, torrent.NumSeeds.ToString()!, torrent.NumLeechs.ToString()!, torrent.Dlspeed.ToString()!, torrent.Upspeed.ToString()!, torrent.Eta.ToString()!, torrent.Ratio.ToString()!, torrent.Popularity.ToString()!, torrent.Category, torrent.Tags, DateTimeOffset.FromUnixTimeSeconds(torrent.AddedOn ?? 0).ToString("yyyy-MM-dd HH:mm:ss")) { Progress = torrent.Progress.ToString("P1") }));

        return torrents;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", Program.Settings?.ArrSuite.QBitTorrent?.Url);
        await _httpClient.PostAsync($"{Program.Settings?.ArrSuite.QBitTorrent?.Url}api/v2/auth/login", new StringContent($"username={Program.Settings?.ArrSuite.QBitTorrent?.Username}&password={Program.Settings?.ArrSuite.QBitTorrent?.Password}", Encoding.UTF8, "application/x-www-form-urlencoded"));

        _ = Task.Run(async () =>
        {
            while (!_cts.IsCancellationRequested)
            {
                _torrents = await GetTorrents();
                await InvokeAsync(StateHasChanged);
                await Task.Delay(1500);
            }
        });

        await base.OnAfterRenderAsync(firstRender);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _httpClient.Dispose();
    }

    public record TorrentRootData(
        [property: JsonPropertyName("added_on")] int? AddedOn,
        [property: JsonPropertyName("amount_left")] int? AmountLeft,
        [property: JsonPropertyName("auto_tmm")] bool? AutoTmm,
        [property: JsonPropertyName("availability")] double? Availability,
        [property: JsonPropertyName("category")] string Category,
        [property: JsonPropertyName("comment")] string Comment,
        [property: JsonPropertyName("completed")] long? Completed,
        [property: JsonPropertyName("completion_on")] int? CompletionOn,
        [property: JsonPropertyName("content_path")] string ContentPath,
        [property: JsonPropertyName("dl_limit")] int? DlLimit,
        [property: JsonPropertyName("dlspeed")] int? Dlspeed,
        [property: JsonPropertyName("download_path")] string DownloadPath,
        [property: JsonPropertyName("downloaded")] long? Downloaded,
        [property: JsonPropertyName("downloaded_session")] long? DownloadedSession,
        [property: JsonPropertyName("eta")] int? Eta,
        [property: JsonPropertyName("f_l_piece_prio")] bool? FLPiecePrio,
        [property: JsonPropertyName("force_start")] bool? ForceStart,
        [property: JsonPropertyName("has_metadata")] bool? HasMetadata,
        [property: JsonPropertyName("hash")] string Hash,
        [property: JsonPropertyName("inactive_seeding_time_limit")] int? InactiveSeedingTimeLimit,
        [property: JsonPropertyName("infohash_v1")] string InfohashV1,
        [property: JsonPropertyName("infohash_v2")] string InfohashV2,
        [property: JsonPropertyName("last_activity")] int? LastActivity,
        [property: JsonPropertyName("magnet_uri")] string MagnetUri,
        [property: JsonPropertyName("max_inactive_seeding_time")] int? MaxInactiveSeedingTime,
        [property: JsonPropertyName("max_ratio")] double? MaxRatio,
        [property: JsonPropertyName("max_seeding_time")] int? MaxSeedingTime,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("num_complete")] int? NumComplete,
        [property: JsonPropertyName("num_incomplete")] int? NumIncomplete,
        [property: JsonPropertyName("num_leechs")] int? NumLeechs,
        [property: JsonPropertyName("num_seeds")] int? NumSeeds,
        [property: JsonPropertyName("popularity")] int? Popularity,
        [property: JsonPropertyName("priority")] int? Priority,
        [property: JsonPropertyName("private")] bool? Private,
        [property: JsonPropertyName("progress")] double Progress,
        [property: JsonPropertyName("ratio")] int? Ratio,
        [property: JsonPropertyName("ratio_limit")] int? RatioLimit,
        [property: JsonPropertyName("reannounce")] int? Reannounce,
        [property: JsonPropertyName("root_path")] string RootPath,
        [property: JsonPropertyName("save_path")] string SavePath,
        [property: JsonPropertyName("seeding_time")] int? SeedingTime,
        [property: JsonPropertyName("seeding_time_limit")] int? SeedingTimeLimit,
        [property: JsonPropertyName("seen_complete")] int? SeenComplete,
        [property: JsonPropertyName("seq_dl")] bool? SeqDl,
        [property: JsonPropertyName("size")] long? Size,
        [property: JsonPropertyName("state")] string State,
        [property: JsonPropertyName("super_seeding")] bool? SuperSeeding,
        [property: JsonPropertyName("tags")] string Tags,
        [property: JsonPropertyName("time_active")] int? TimeActive,
        [property: JsonPropertyName("total_size")] long? TotalSize,
        [property: JsonPropertyName("tracker")] string Tracker,
        [property: JsonPropertyName("trackers_count")] int? TrackersCount,
        [property: JsonPropertyName("up_limit")] int? UpLimit,
        [property: JsonPropertyName("uploaded")] int? Uploaded,
        [property: JsonPropertyName("uploaded_session")] int? UploadedSession,
        [property: JsonPropertyName("upspeed")] int? Upspeed
    );
}