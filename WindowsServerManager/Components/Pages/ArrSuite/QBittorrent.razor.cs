using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using MudBlazor;
using WindowsServerManager.Libraries;

namespace WindowsServerManager.Components.Pages.ArrSuite;

public partial class QBittorrent : IDisposable
{
    private readonly List<string> _categoryFilters = [];

    private readonly Dictionary<string, string> _statusFilters = new(StringComparer.OrdinalIgnoreCase)
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

    private readonly Dictionary<string, string> _statusIcons = new(StringComparer.OrdinalIgnoreCase)
    {
        { "forcedDL", CustomIcons.QBit.ForceStart },
        { "moving", CustomIcons.QBit.Moving },
        { "uploading", CustomIcons.QBit.Seeding },
        { "pausedUP", CustomIcons.QBit.Completed },

        { "queuedUP", CustomIcons.QBit.Stalled },
        { "pausedDL", CustomIcons.QBit.Stalled },
        { "queuedDL", CustomIcons.QBit.Stalled },
        { "forcedUP", CustomIcons.QBit.StalledUploading },
        { "stalledUP", CustomIcons.QBit.StalledUploading },
        { "stalledDL", CustomIcons.QBit.StalledDownloading },
        { "downloading", CustomIcons.QBit.Download },

        { "checkingDL", CustomIcons.QBit.Loading },
        { "checkingResumeData", CustomIcons.QBit.Loading },
        { "allocating",  CustomIcons.QBit.Loading  },
        { "metaDL", CustomIcons.QBit.Loading },
        { "checkingUP", CustomIcons.QBit.Loading },

        { "missingFiles", CustomIcons.QBit.ExitqBittorrent },
        { "error", CustomIcons.QBit.Errored },
        { "unknown", CustomIcons.QBit.Errored },
        { "unknow", CustomIcons.QBit.Errored } // Pulled from their docs, not sure if real or not
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
        public double Progress { get; set; }
    }

    private bool _firstLoad = true; // Inverse to work with Loader component.

    private readonly List<Torrent> _torrents = [];

    private readonly CancellationTokenSource _cts = new();

    private readonly HttpClient _httpClient = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
        UseCookies = true,
        CookieContainer = new CookieContainer(),
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        AutomaticDecompression = DecompressionMethods.All,
    });

    private async Task GetCategories()
    {
        string data = await _httpClient.GetStringAsync($"{Program.Settings?.ArrSuite.QBitTorrent?.Url}api/v2/torrents/categories");
        using JsonDocument doc = JsonDocument.Parse(data);
        foreach (JsonProperty property in doc.RootElement.EnumerateObject().Where(property => !string.IsNullOrWhiteSpace(property.Name)).Where(property => !_categoryFilters.Contains(property.Name, StringComparer.OrdinalIgnoreCase)))
            _categoryFilters.Add(property.Name);
    }

    private async Task GetTorrents()
    {
        string torrentUrl = $"{Program.Settings?.ArrSuite.QBitTorrent?.Url}api/v2/torrents/info";

        Uri currentUri = new (NavigationManager.Uri);
        if (QueryHelpers.ParseQuery(currentUri.Query).TryGetValue("status", out StringValues statusData))
        {
            if (_categoryFilters.Contains(WebUtility.HtmlDecode(statusData)))
            {
                torrentUrl += "?category=" + WebUtility.UrlEncode(statusData);
            }
            else if (_statusFilters.ContainsKey(statusData!))
            {
                torrentUrl += "?filter=" + WebUtility.UrlEncode(statusData.ToString().ToLowerInvariant());
            }
        }
        else
            torrentUrl += "?filter=all";
        
        string torrentData = await _httpClient.GetStringAsync(torrentUrl);
        List<TorrentRootData>? torrentRootData = JsonSerializer.Deserialize<List<TorrentRootData>>(torrentData);
        if (torrentRootData is null) return;
        _torrents.Clear();
        _torrents.AddRange(torrentRootData.Select(torrent => new Torrent(torrent.State, torrent.Name, torrent.Size.ToString()!, torrent.NumSeeds.ToString()!, torrent.NumLeechs.ToString()!, torrent.Dlspeed.ToString()!, torrent.Upspeed.ToString()!, torrent.Eta.ToString()!, torrent.Ratio.ToString()!, torrent.Popularity.ToString()!, torrent.Category, torrent.Tags, DateTimeOffset.FromUnixTimeSeconds(torrent.AddedOn ?? 0).ToString("yyyy-MM-dd HH:mm:ss")) { Progress = torrent.Progress }));
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (!NavigationManager.Uri.Contains("?status")) NavigationManager.NavigateTo("/arr/qbittorrent?status=All");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", Program.Settings?.ArrSuite.QBitTorrent?.Url);
        await _httpClient.PostAsync($"{Program.Settings?.ArrSuite.QBitTorrent?.Url}api/v2/auth/login", new StringContent($"username={Program.Settings?.ArrSuite.QBitTorrent?.Username}&password={Program.Settings?.ArrSuite.QBitTorrent?.Password}", Encoding.UTF8, "application/x-www-form-urlencoded"));

        NavigationManager.LocationChanged += (_, _) => Task.Run(GetTorrents);

        _ = Task.Run(async () =>
        {
            while (!_cts.IsCancellationRequested)
            {
                await GetCategories();
                await GetTorrents();
                _firstLoad = false;
                await InvokeAsync(StateHasChanged);
                await Task.Delay(1000);
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
        [property: JsonPropertyName("added_on")] long? AddedOn,
        [property: JsonPropertyName("amount_left")] long? AmountLeft,
        [property: JsonPropertyName("auto_tmm")] bool? AutoTmm,
        [property: JsonPropertyName("availability")] double? Availability,
        [property: JsonPropertyName("category")] string Category,
        [property: JsonPropertyName("comment")] string Comment,
        [property: JsonPropertyName("completed")] long? Completed,
        [property: JsonPropertyName("completion_on")] long? CompletionOn,
        [property: JsonPropertyName("content_path")] string ContentPath,
        [property: JsonPropertyName("dl_limit")] long? DlLimit,
        [property: JsonPropertyName("dlspeed")] long? Dlspeed,
        [property: JsonPropertyName("download_path")] string DownloadPath,
        [property: JsonPropertyName("downloaded")] long? Downloaded,
        [property: JsonPropertyName("downloaded_session")] long? DownloadedSession,
        [property: JsonPropertyName("eta")] long? Eta,
        [property: JsonPropertyName("f_l_piece_prio")] bool? FLPiecePrio,
        [property: JsonPropertyName("force_start")] bool? ForceStart,
        [property: JsonPropertyName("has_metadata")] bool? HasMetadata,
        [property: JsonPropertyName("hash")] string Hash,
        [property: JsonPropertyName("inactive_seeding_time_limit")] long? InactiveSeedingTimeLimit,
        [property: JsonPropertyName("infohash_v1")] string InfohashV1,
        [property: JsonPropertyName("infohash_v2")] string InfohashV2,
        [property: JsonPropertyName("last_activity")] long? LastActivity,
        [property: JsonPropertyName("magnet_uri")] string MagnetUri,
        [property: JsonPropertyName("max_inactive_seeding_time")] long? MaxInactiveSeedingTime,
        [property: JsonPropertyName("max_ratio")] double? MaxRatio,
        [property: JsonPropertyName("max_seeding_time")] long? MaxSeedingTime,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("num_complete")] long? NumComplete,
        [property: JsonPropertyName("num_incomplete")] long? NumIncomplete,
        [property: JsonPropertyName("num_leechs")] long? NumLeechs,
        [property: JsonPropertyName("num_seeds")] long? NumSeeds,
        [property: JsonPropertyName("popularity")] double? Popularity,
        [property: JsonPropertyName("priority")] long? Priority,
        [property: JsonPropertyName("private")] bool? Private,
        [property: JsonPropertyName("progress")] double Progress,
        [property: JsonPropertyName("ratio")] double? Ratio,
        [property: JsonPropertyName("ratio_limit")] double? RatioLimit,
        [property: JsonPropertyName("reannounce")] long? Reannounce,
        [property: JsonPropertyName("root_path")] string RootPath,
        [property: JsonPropertyName("save_path")] string SavePath,
        [property: JsonPropertyName("seeding_time")] long? SeedingTime,
        [property: JsonPropertyName("seeding_time_limit")] long? SeedingTimeLimit,
        [property: JsonPropertyName("seen_complete")] long? SeenComplete,
        [property: JsonPropertyName("seq_dl")] bool? SeqDl,
        [property: JsonPropertyName("size")] long? Size,
        [property: JsonPropertyName("state")] string State,
        [property: JsonPropertyName("super_seeding")] bool? SuperSeeding,
        [property: JsonPropertyName("tags")] string Tags,
        [property: JsonPropertyName("time_active")] long? TimeActive,
        [property: JsonPropertyName("total_size")] long? TotalSize,
        [property: JsonPropertyName("tracker")] string Tracker,
        [property: JsonPropertyName("trackers_count")] long? TrackersCount,
        [property: JsonPropertyName("up_limit")] long? UpLimit,
        [property: JsonPropertyName("uploaded")] long? Uploaded,
        [property: JsonPropertyName("uploaded_session")] long? UploadedSession,
        [property: JsonPropertyName("upspeed")] long? Upspeed
    );
}