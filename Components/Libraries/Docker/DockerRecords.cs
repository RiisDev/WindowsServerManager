using System.Text.Json.Serialization;

namespace WindowsServerManager.Components.Libraries.Docker;

public record BlkioStats(
    [property: JsonPropertyName("io_service_bytes_recursive")] IReadOnlyList<object> IoServiceBytesRecursive,
    [property: JsonPropertyName("io_serviced_recursive")] IReadOnlyList<object> IoServicedRecursive,
    [property: JsonPropertyName("io_queue_recursive")] IReadOnlyList<object> IoQueueRecursive,
    [property: JsonPropertyName("io_service_time_recursive")] IReadOnlyList<object> IoServiceTimeRecursive,
    [property: JsonPropertyName("io_wait_time_recursive")] IReadOnlyList<object> IoWaitTimeRecursive,
    [property: JsonPropertyName("io_merged_recursive")] IReadOnlyList<object> IoMergedRecursive,
    [property: JsonPropertyName("io_time_recursive")] IReadOnlyList<object> IoTimeRecursive,
    [property: JsonPropertyName("sectors_recursive")] IReadOnlyList<object> SectorsRecursive
);

public record CpuStats(
    [property: JsonPropertyName("cpu_usage")] CpuUsage CpuUsage,
    [property: JsonPropertyName("system_cpu_usage")] long? SystemCpuUsage,
    [property: JsonPropertyName("online_cpus")] int? OnlineCpus,
    [property: JsonPropertyName("throttling_data")] ThrottlingData ThrottlingData
);

public record CpuUsage(
    [property: JsonPropertyName("total_usage")] long? TotalUsage,
    [property: JsonPropertyName("percpu_usage")] IReadOnlyList<long?> PercpuUsage,
    [property: JsonPropertyName("usage_in_kernelmode")] int? UsageInKernelmode,
    [property: JsonPropertyName("usage_in_usermode")] long? UsageInUsermode
);

public record Eth0(
    [property: JsonPropertyName("rx_bytes")] int? RxBytes,
    [property: JsonPropertyName("rx_packets")] int? RxPackets,
    [property: JsonPropertyName("rx_errors")] int? RxErrors,
    [property: JsonPropertyName("rx_dropped")] int? RxDropped,
    [property: JsonPropertyName("tx_bytes")] int? TxBytes,
    [property: JsonPropertyName("tx_packets")] int? TxPackets,
    [property: JsonPropertyName("tx_errors")] int? TxErrors,
    [property: JsonPropertyName("tx_dropped")] int? TxDropped
);

public record MemoryStats(
    [property: JsonPropertyName("usage")] int? Usage,
    [property: JsonPropertyName("max_usage")] int? MaxUsage,
    [property: JsonPropertyName("stats")] Stats Stats,
    [property: JsonPropertyName("limit")] long? Limit
);

public record Networks(
    [property: JsonPropertyName("eth0")] Eth0 Eth0
);

public record PidsStats(
    [property: JsonPropertyName("current")] int? Current
);

public record PrecpuStats(
    [property: JsonPropertyName("cpu_usage")] CpuUsage CpuUsage,
    [property: JsonPropertyName("system_cpu_usage")] long? SystemCpuUsage,
    [property: JsonPropertyName("online_cpus")] int? OnlineCpus,
    [property: JsonPropertyName("throttling_data")] ThrottlingData ThrottlingData
);

public record StatApiReturn(
    [property: JsonPropertyName("read")] string Read,
    [property: JsonPropertyName("preread")] string Preread,
    [property: JsonPropertyName("pids_stats")] PidsStats PidsStats,
    [property: JsonPropertyName("blkio_stats")] BlkioStats BlkioStats,
    [property: JsonPropertyName("num_procs")] int? NumProcs,
    [property: JsonPropertyName("storage_stats")] StorageStats StorageStats,
    [property: JsonPropertyName("cpu_stats")] CpuStats CpuStats,
    [property: JsonPropertyName("precpu_stats")] PrecpuStats PrecpuStats,
    [property: JsonPropertyName("memory_stats")] MemoryStats MemoryStats,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("networks")] Networks Networks
);

public record Stats(
    [property: JsonPropertyName("active_anon")] int? ActiveAnon,
    [property: JsonPropertyName("active_file")] int? ActiveFile,
    [property: JsonPropertyName("cache")] int? Cache,
    [property: JsonPropertyName("dirty")] int? Dirty,
    [property: JsonPropertyName("hierarchical_memory_limit")] long? HierarchicalMemoryLimit,
    [property: JsonPropertyName("hierarchical_memsw_limit")] long? HierarchicalMemswLimit,
    [property: JsonPropertyName("inactive_anon")] int? InactiveAnon,
    [property: JsonPropertyName("inactive_file")] int? InactiveFile,
    [property: JsonPropertyName("mapped_file")] int? MappedFile,
    [property: JsonPropertyName("pgfault")] int? Pgfault,
    [property: JsonPropertyName("pgmajfault")] int? Pgmajfault,
    [property: JsonPropertyName("pgpgin")] int? Pgpgin,
    [property: JsonPropertyName("pgpgout")] int? Pgpgout,
    [property: JsonPropertyName("rss")] int? Rss,
    [property: JsonPropertyName("rss_huge")] int? RssHuge,
    [property: JsonPropertyName("total_active_anon")] int? TotalActiveAnon,
    [property: JsonPropertyName("total_active_file")] int? TotalActiveFile,
    [property: JsonPropertyName("total_cache")] int? TotalCache,
    [property: JsonPropertyName("total_dirty")] int? TotalDirty,
    [property: JsonPropertyName("total_inactive_anon")] int? TotalInactiveAnon,
    [property: JsonPropertyName("total_inactive_file")] int? TotalInactiveFile,
    [property: JsonPropertyName("total_mapped_file")] int? TotalMappedFile,
    [property: JsonPropertyName("total_pgfault")] int? TotalPgfault,
    [property: JsonPropertyName("total_pgmajfault")] int? TotalPgmajfault,
    [property: JsonPropertyName("total_pgpgin")] int? TotalPgpgin,
    [property: JsonPropertyName("total_pgpgout")] int? TotalPgpgout,
    [property: JsonPropertyName("total_rss")] int? TotalRss,
    [property: JsonPropertyName("total_rss_huge")] int? TotalRssHuge,
    [property: JsonPropertyName("total_unevictable")] int? TotalUnevictable,
    [property: JsonPropertyName("total_writeback")] int? TotalWriteback,
    [property: JsonPropertyName("unevictable")] int? Unevictable,
    [property: JsonPropertyName("writeback")] int? Writeback
);

public record StorageStats();

public record ThrottlingData(
    [property: JsonPropertyName("periods")] int? Periods,
    [property: JsonPropertyName("throttled_periods")] int? ThrottledPeriods,
    [property: JsonPropertyName("throttled_time")] int? ThrottledTime
);

public record CpuInternalStats(string Cpu, string MemPercent, string MemUsage, string NetIo, string Pid);

public record DockerStat(
    bool Active,
    string Name,
    string ContainerId,
    string Image,
    string Ports,
    string LastStarted
)
{
    public string Cpu { get; set; } = "0%";
    public string MemPercent { get; set; } = "0%";
    public string MemUsage { get; set; } = "--";
    public string NetIo { get; set; } = "--";
    public string Pid { get; set; } = "0";
};
