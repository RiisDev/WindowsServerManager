using System.Text.Json.Serialization;

namespace WindowsServerManager.Libraries.ArrSuite.Radarr;

public record AlternateTitle(
    [property: JsonPropertyName("sourceType")] string SourceType,
    [property: JsonPropertyName("movieMetadataId")] int MovieMetadataId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("id")] int Id
);

public record Collection(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("tmdbId")] int TmdbId
);

public record Image(
    [property: JsonPropertyName("coverType")] string CoverType,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("remoteUrl")] string RemoteUrl
);

public record Imdb(
    [property: JsonPropertyName("votes")] int Votes,
    [property: JsonPropertyName("value")] double Value,
    [property: JsonPropertyName("type")] string Type
);

public record Language(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name
);

public record MediaInfo(
    [property: JsonPropertyName("audioBitrate")] int AudioBitrate,
    [property: JsonPropertyName("audioChannels")] double AudioChannels,
    [property: JsonPropertyName("audioCodec")] string AudioCodec,
    [property: JsonPropertyName("audioLanguages")] string AudioLanguages,
    [property: JsonPropertyName("audioStreamCount")] int AudioStreamCount,
    [property: JsonPropertyName("videoBitDepth")] int VideoBitDepth,
    [property: JsonPropertyName("videoBitrate")] int VideoBitrate,
    [property: JsonPropertyName("videoCodec")] string VideoCodec,
    [property: JsonPropertyName("videoFps")] double VideoFps,
    [property: JsonPropertyName("videoDynamicRange")] string VideoDynamicRange,
    [property: JsonPropertyName("videoDynamicRangeType")] string VideoDynamicRangeType,
    [property: JsonPropertyName("resolution")] string Resolution,
    [property: JsonPropertyName("runTime")] string RunTime,
    [property: JsonPropertyName("scanType")] string ScanType,
    [property: JsonPropertyName("subtitles")] string Subtitles
);

public record Metacritic(
    [property: JsonPropertyName("votes")] int Votes,
    [property: JsonPropertyName("value")] int Value,
    [property: JsonPropertyName("type")] string Type
);

public record MovieFile(
    [property: JsonPropertyName("movieId")] int MovieId,
    [property: JsonPropertyName("relativePath")] string RelativePath,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("size")] object Size,
    [property: JsonPropertyName("dateAdded")] DateTime DateAdded,
    [property: JsonPropertyName("edition")] string Edition,
    [property: JsonPropertyName("languages")] IReadOnlyList<Language> Languages,
    [property: JsonPropertyName("quality")] Quality Quality,
    [property: JsonPropertyName("customFormatScore")] int CustomFormatScore,
    [property: JsonPropertyName("indexerFlags")] int IndexerFlags,
    [property: JsonPropertyName("mediaInfo")] MediaInfo MediaInfo,
    [property: JsonPropertyName("qualityCutoffNotMet")] bool QualityCutoffNotMet,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("sceneName")] string SceneName,
    [property: JsonPropertyName("releaseGroup")] string ReleaseGroup,
    [property: JsonPropertyName("originalFilePath")] string OriginalFilePath
);

public record OriginalLanguage(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name
);

public record Quality(
    [property: JsonPropertyName("quality")] Quality QualityData,
    [property: JsonPropertyName("revision")] Revision Revision,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("resolution")] int Resolution,
    [property: JsonPropertyName("modifier")] string Modifier
);

public record Ratings(
    [property: JsonPropertyName("imdb")] Imdb Imdb,
    [property: JsonPropertyName("tmdb")] Tmdb Tmdb,
    [property: JsonPropertyName("metacritic")] Metacritic Metacritic,
    [property: JsonPropertyName("rottenTomatoes")] RottenTomatoes RottenTomatoes,
    [property: JsonPropertyName("trakt")] Trakt Trakt
);

public record Revision(
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("real")] int Real,
    [property: JsonPropertyName("isRepack")] bool IsRepack
);

public record RadarrRoot(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("originalTitle")] string OriginalTitle,
    [property: JsonPropertyName("originalLanguage")] OriginalLanguage OriginalLanguage,
    [property: JsonPropertyName("alternateTitles")] IReadOnlyList<AlternateTitle> AlternateTitles,
    [property: JsonPropertyName("secondaryYearSourceId")] int SecondaryYearSourceId,
    [property: JsonPropertyName("sortTitle")] string SortTitle,
    [property: JsonPropertyName("sizeOnDisk")] object SizeOnDisk,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("overview")] string Overview,
    [property: JsonPropertyName("inCinemas")] DateTime InCinemas,
    [property: JsonPropertyName("physicalRelease")] DateTime PhysicalRelease,
    [property: JsonPropertyName("digitalRelease")] DateTime DigitalRelease,
    [property: JsonPropertyName("releaseDate")] DateTime ReleaseDate,
    [property: JsonPropertyName("images")] IReadOnlyList<Image> Images,
    [property: JsonPropertyName("website")] string Website,
    [property: JsonPropertyName("year")] int Year,
    [property: JsonPropertyName("youTubeTrailerId")] string YouTubeTrailerId,
    [property: JsonPropertyName("studio")] string Studio,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("qualityProfileId")] int QualityProfileId,
    [property: JsonPropertyName("hasFile")] bool HasFile,
    [property: JsonPropertyName("movieFileId")] int MovieFileId,
    [property: JsonPropertyName("monitored")] bool Monitored,
    [property: JsonPropertyName("minimumAvailability")] string MinimumAvailability,
    [property: JsonPropertyName("isAvailable")] bool IsAvailable,
    [property: JsonPropertyName("folderName")] string FolderName,
    [property: JsonPropertyName("runtime")] int Runtime,
    [property: JsonPropertyName("cleanTitle")] string CleanTitle,
    [property: JsonPropertyName("imdbId")] string ImdbId,
    [property: JsonPropertyName("tmdbId")] int TmdbId,
    [property: JsonPropertyName("titleSlug")] string TitleSlug,
    [property: JsonPropertyName("rootFolderPath")] string RootFolderPath,
    [property: JsonPropertyName("certification")] string Certification,
    [property: JsonPropertyName("genres")] IReadOnlyList<string> Genres,
    [property: JsonPropertyName("tags")] IReadOnlyList<int> Tags,
    [property: JsonPropertyName("added")] DateTime Added,
    [property: JsonPropertyName("ratings")] Ratings Ratings,
    [property: JsonPropertyName("movieFile")] MovieFile MovieFile,
    [property: JsonPropertyName("collection")] Collection Collection,
    [property: JsonPropertyName("popularity")] double Popularity,
    [property: JsonPropertyName("statistics")] Statistics Statistics,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("lastSearchTime")] DateTime? LastSearchTime,
    [property: JsonPropertyName("secondaryYear")] int? SecondaryYear
);

public record RottenTomatoes(
    [property: JsonPropertyName("votes")] int Votes,
    [property: JsonPropertyName("value")] int Value,
    [property: JsonPropertyName("type")] string Type
);

public record Statistics(
    [property: JsonPropertyName("movieFileCount")] int MovieFileCount,
    [property: JsonPropertyName("sizeOnDisk")] object SizeOnDisk,
    [property: JsonPropertyName("releaseGroups")] IReadOnlyList<string> ReleaseGroups
);

public record Tmdb(
    [property: JsonPropertyName("votes")] int Votes,
    [property: JsonPropertyName("value")] double Value,
    [property: JsonPropertyName("type")] string Type
);

public record Trakt(
    [property: JsonPropertyName("votes")] int Votes,
    [property: JsonPropertyName("value")] double Value,
    [property: JsonPropertyName("type")] string Type
);

