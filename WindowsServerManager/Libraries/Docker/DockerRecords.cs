using System.Text.Json.Serialization;

namespace WindowsServerManager.Libraries.Docker;

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
    [property: JsonPropertyName("system_cpu_usage")] ulong? SystemCpuUsage,
    [property: JsonPropertyName("online_cpus")] long? OnlineCpus,
    [property: JsonPropertyName("throttling_data")] ThrottlingData ThrottlingData
);

public record CpuUsage(
    [property: JsonPropertyName("total_usage")] ulong? TotalUsage,
    [property: JsonPropertyName("percpu_usage")] IReadOnlyList<ulong?> PercpuUsage,
    [property: JsonPropertyName("usage_in_kernelmode")] long? UsageInKernelmode,
    [property: JsonPropertyName("usage_in_usermode")] ulong? UsageInUsermode
);

public record Eth0(
    [property: JsonPropertyName("rx_bytes")] long? RxBytes,
    [property: JsonPropertyName("rx_packets")] long? RxPackets,
    [property: JsonPropertyName("rx_errors")] long? RxErrors,
    [property: JsonPropertyName("rx_dropped")] long? RxDropped,
    [property: JsonPropertyName("tx_bytes")] long? TxBytes,
    [property: JsonPropertyName("tx_packets")] long? TxPackets,
    [property: JsonPropertyName("tx_errors")] long? TxErrors,
    [property: JsonPropertyName("tx_dropped")] long? TxDropped
);

public record MemoryStats(
    [property: JsonPropertyName("usage")] long? Usage,
    [property: JsonPropertyName("max_usage")] long? MaxUsage,
    [property: JsonPropertyName("stats")] Stats Stats,
    [property: JsonPropertyName("limit")] ulong? Limit
);

public record Networks(
    [property: JsonPropertyName("eth0")] Eth0 Eth0
);

public record PidsStats(
    [property: JsonPropertyName("current")] long? Current
);

public record PrecpuStats(
    [property: JsonPropertyName("cpu_usage")] CpuUsage CpuUsage,
    [property: JsonPropertyName("system_cpu_usage")] ulong? SystemCpuUsage,
    [property: JsonPropertyName("online_cpus")] long? OnlineCpus,
    [property: JsonPropertyName("throttling_data")] ThrottlingData ThrottlingData
);

public record StatApiReturn(
    [property: JsonPropertyName("read")] string Read,
    [property: JsonPropertyName("preread")] string Preread,
    [property: JsonPropertyName("pids_stats")] PidsStats PidsStats,
    [property: JsonPropertyName("blkio_stats")] BlkioStats BlkioStats,
    [property: JsonPropertyName("num_procs")] long? NumProcs,
    [property: JsonPropertyName("storage_stats")] StorageStats StorageStats,
    [property: JsonPropertyName("cpu_stats")] CpuStats CpuStats,
    [property: JsonPropertyName("precpu_stats")] PrecpuStats PrecpuStats,
    [property: JsonPropertyName("memory_stats")] MemoryStats MemoryStats,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("networks")] Networks Networks
);

public record Stats(
    [property: JsonPropertyName("active_anon")] long? ActiveAnon,
    [property: JsonPropertyName("active_file")] long? ActiveFile,
    [property: JsonPropertyName("cache")] long? Cache,
    [property: JsonPropertyName("dirty")] long? Dirty,
    [property: JsonPropertyName("hierarchical_memory_limit")] ulong? HierarchicalMemoryLimit,
    [property: JsonPropertyName("hierarchical_memsw_limit")] ulong? HierarchicalMemswLimit,
    [property: JsonPropertyName("inactive_anon")] long? InactiveAnon,
    [property: JsonPropertyName("inactive_file")] long? InactiveFile,
    [property: JsonPropertyName("mapped_file")] long? MappedFile,
    [property: JsonPropertyName("pgfault")] long? Pgfault,
    [property: JsonPropertyName("pgmajfault")] long? Pgmajfault,
    [property: JsonPropertyName("pgpgin")] long? Pgpgin,
    [property: JsonPropertyName("pgpgout")] long? Pgpgout,
    [property: JsonPropertyName("rss")] long? Rss,
    [property: JsonPropertyName("rss_huge")] long? RssHuge,
    [property: JsonPropertyName("total_active_anon")] long? TotalActiveAnon,
    [property: JsonPropertyName("total_active_file")] long? TotalActiveFile,
    [property: JsonPropertyName("total_cache")] long? TotalCache,
    [property: JsonPropertyName("total_dirty")] long? TotalDirty,
    [property: JsonPropertyName("total_inactive_anon")] long? TotalInactiveAnon,
    [property: JsonPropertyName("total_inactive_file")] long? TotalInactiveFile,
    [property: JsonPropertyName("total_mapped_file")] long? TotalMappedFile,
    [property: JsonPropertyName("total_pgfault")] long? TotalPgfault,
    [property: JsonPropertyName("total_pgmajfault")] long? TotalPgmajfault,
    [property: JsonPropertyName("total_pgpgin")] long? TotalPgpgin,
    [property: JsonPropertyName("total_pgpgout")] long? TotalPgpgout,
    [property: JsonPropertyName("total_rss")] long? TotalRss,
    [property: JsonPropertyName("total_rss_huge")] long? TotalRssHuge,
    [property: JsonPropertyName("total_unevictable")] long? TotalUnevictable,
    [property: JsonPropertyName("total_writeback")] long? TotalWriteback,
    [property: JsonPropertyName("unevictable")] long? Unevictable,
    [property: JsonPropertyName("writeback")] long? Writeback
);

public record StorageStats();

public record ThrottlingData(
    [property: JsonPropertyName("periods")] long? Periods,
    [property: JsonPropertyName("throttled_periods")] long? ThrottledPeriods,
    [property: JsonPropertyName("throttled_time")] long? ThrottledTime
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
