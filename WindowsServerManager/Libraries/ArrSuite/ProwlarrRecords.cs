using System.Text.Json.Serialization;

namespace WindowsServerManager.Libraries.ArrSuite.Prowlarr;
public record Host(
    [property: JsonPropertyName("host")] string HostData,
    [property: JsonPropertyName("numberOfQueries")] int NumberOfQueries,
    [property: JsonPropertyName("numberOfGrabs")] int NumberOfGrabs
);

public record Indexer(
    [property: JsonPropertyName("indexerId")] int IndexerId,
    [property: JsonPropertyName("indexerName")] string IndexerName,
    [property: JsonPropertyName("averageResponseTime")] int AverageResponseTime,
    [property: JsonPropertyName("averageGrabResponseTime")] int AverageGrabResponseTime,
    [property: JsonPropertyName("numberOfQueries")] int NumberOfQueries,
    [property: JsonPropertyName("numberOfGrabs")] int NumberOfGrabs,
    [property: JsonPropertyName("numberOfRssQueries")] int NumberOfRssQueries,
    [property: JsonPropertyName("numberOfAuthQueries")] int NumberOfAuthQueries,
    [property: JsonPropertyName("numberOfFailedQueries")] int NumberOfFailedQueries,
    [property: JsonPropertyName("numberOfFailedGrabs")] int NumberOfFailedGrabs,
    [property: JsonPropertyName("numberOfFailedRssQueries")] int NumberOfFailedRssQueries,
    [property: JsonPropertyName("numberOfFailedAuthQueries")] int NumberOfFailedAuthQueries
);

public record ProwlarrStats(
    [property: JsonPropertyName("indexers")] IReadOnlyList<Indexer> Indexers,
    [property: JsonPropertyName("userAgents")] IReadOnlyList<UserAgent> UserAgents,
    [property: JsonPropertyName("hosts")] IReadOnlyList<Host> Hosts
);

public record UserAgent(
    [property: JsonPropertyName("userAgent")] string UserAgentData,
    [property: JsonPropertyName("numberOfQueries")] int NumberOfQueries,
    [property: JsonPropertyName("numberOfGrabs")] int NumberOfGrabs
);

