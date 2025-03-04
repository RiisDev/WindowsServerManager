using System.Text.Json.Serialization;
namespace WindowsServerManager.Components.Libraries.ArrSuite.Sonarr;
public record AlternateTitle(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("seasonNumber")] int? SeasonNumber,
    [property: JsonPropertyName("sceneSeasonNumber")] int? SceneSeasonNumber,
    [property: JsonPropertyName("comment")] string Comment,
    [property: JsonPropertyName("sceneOrigin")] string SceneOrigin
);

public record Image(
    [property: JsonPropertyName("coverType")] string CoverType,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("remoteUrl")] string RemoteUrl
);

public record OriginalLanguage(
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("name")] string Name
);

public record Ratings(
    [property: JsonPropertyName("votes")] int? Votes,
    [property: JsonPropertyName("value")] double? Value
);

public record SonarrRoot(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("alternateTitles")] IReadOnlyList<AlternateTitle> AlternateTitles,
    [property: JsonPropertyName("sortTitle")] string SortTitle,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("ended")] bool? Ended,
    [property: JsonPropertyName("overview")] string Overview,
    [property: JsonPropertyName("previousAiring")] DateTime? PreviousAiring,
    [property: JsonPropertyName("network")] string Network,
    [property: JsonPropertyName("airTime")] string AirTime,
    [property: JsonPropertyName("images")] IReadOnlyList<Image> Images,
    [property: JsonPropertyName("originalLanguage")] OriginalLanguage OriginalLanguage,
    [property: JsonPropertyName("seasons")] IReadOnlyList<Season> Seasons,
    [property: JsonPropertyName("year")] int? Year,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("qualityProfileId")] int? QualityProfileId,
    [property: JsonPropertyName("seasonFolder")] bool? SeasonFolder,
    [property: JsonPropertyName("monitored")] bool? Monitored,
    [property: JsonPropertyName("monitorNewItems")] string MonitorNewItems,
    [property: JsonPropertyName("useSceneNumbering")] bool? UseSceneNumbering,
    [property: JsonPropertyName("runtime")] int? Runtime,
    [property: JsonPropertyName("tvdbId")] int? TvdbId,
    [property: JsonPropertyName("tvRageId")] int? TvRageId,
    [property: JsonPropertyName("tvMazeId")] int? TvMazeId,
    [property: JsonPropertyName("tmdbId")] int? TmdbId,
    [property: JsonPropertyName("firstAired")] DateTime? FirstAired,
    [property: JsonPropertyName("lastAired")] DateTime? LastAired,
    [property: JsonPropertyName("seriesType")] string SeriesType,
    [property: JsonPropertyName("cleanTitle")] string CleanTitle,
    [property: JsonPropertyName("imdbId")] string ImdbId,
    [property: JsonPropertyName("titleSlug")] string TitleSlug,
    [property: JsonPropertyName("rootFolderPath")] string RootFolderPath,
    [property: JsonPropertyName("genres")] IReadOnlyList<string> Genres,
    [property: JsonPropertyName("tags")] IReadOnlyList<object> Tags,
    [property: JsonPropertyName("added")] DateTime? Added,
    [property: JsonPropertyName("ratings")] Ratings Ratings,
    [property: JsonPropertyName("statistics")] Statistics Statistics,
    [property: JsonPropertyName("languageProfileId")] int? LanguageProfileId,
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("certification")] string Certification,
    [property: JsonPropertyName("nextAiring")] DateTime? NextAiring
);

public record Season(
    [property: JsonPropertyName("seasonNumber")] int? SeasonNumber,
    [property: JsonPropertyName("monitored")] bool? Monitored,
    [property: JsonPropertyName("statistics")] Statistics Statistics
);

public record Statistics(
    [property: JsonPropertyName("episodeFileCount")] int? EpisodeFileCount,
    [property: JsonPropertyName("episodeCount")] int? EpisodeCount,
    [property: JsonPropertyName("totalEpisodeCount")] int? TotalEpisodeCount,
    [property: JsonPropertyName("sizeOnDisk")] object SizeOnDisk,
    [property: JsonPropertyName("releaseGroups")] IReadOnlyList<string> ReleaseGroups,
    [property: JsonPropertyName("percentOfEpisodes")] double? PercentOfEpisodes,
    [property: JsonPropertyName("previousAiring")] DateTime? PreviousAiring,
    [property: JsonPropertyName("nextAiring")] DateTime? NextAiring,
    [property: JsonPropertyName("seasonCount")] int? SeasonCount
);

