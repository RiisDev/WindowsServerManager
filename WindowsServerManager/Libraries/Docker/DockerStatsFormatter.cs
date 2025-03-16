using System.Globalization;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace WindowsServerManager.Libraries.Docker
{
    public static class DockerStatsFormatter
    {
        public static CpuInternalStats FormatStats(StatApiReturn jsonData)
        {
            string cpuPerc = CalculateCpuPercentage(jsonData);
            string memPerc = CalculateMemoryPercentage(jsonData);
            string memUsage = FormatMemoryUsage(jsonData);
            string netIo = FormatNetIo(jsonData);
            string pids = jsonData?.PidsStats?.Current?.ToString() ?? "0";

            return new CpuInternalStats(cpuPerc, memPerc, memUsage, netIo, pids);
        }

        private static string CalculateCpuPercentage(StatApiReturn? jsonData)
        {
            try
            {
                ulong curTotalUsage = jsonData?.CpuStats?.CpuUsage?.TotalUsage ?? 0;
                ulong preTotalUsage = jsonData?.PrecpuStats?.CpuUsage?.TotalUsage ?? 0;

                ulong curSystemUsage = jsonData?.CpuStats?.SystemCpuUsage ?? 0;
                ulong preSystemUsage = jsonData?.PrecpuStats?.SystemCpuUsage ?? 0;

                long opus = jsonData?.CpuStats?.OnlineCpus ?? 0;

                double cpuDelta = curTotalUsage - preTotalUsage + .0;
                double systemCpuDelta = curSystemUsage - preSystemUsage + .0;

                double cpuUsage = cpuDelta / systemCpuDelta * opus * 100.0;

                return $"{cpuUsage:F2}%";
            }
            catch (Exception ex)
            {
                Program.LogService.LogError(ex.ToString());
                return "0%";
            }
        }

        private static string CalculateMemoryPercentage(StatApiReturn? jsonData)
        {
            try
            {
                long usedMemory = jsonData?.MemoryStats?.Usage ?? 0;
                ulong memoryLimit = jsonData?.MemoryStats?.Limit ?? 0;

                double used = usedMemory + .0;
                double limit = memoryLimit + .0;

                double percentage = used / limit * 100;

                return $"{percentage:F2}%";
            }
            catch (Exception ex)
            {
                Program.LogService.LogError(ex.ToString());
                return "0%";
            }
        }

        private static string FormatMemoryUsage(StatApiReturn? jsonData)
        {
            try
            {
                long usedMemory = jsonData?.MemoryStats.Usage ?? 0;
                ulong memoryLimit = jsonData?.MemoryStats?.Limit ?? 0;

                string usage = FormatBytes(usedMemory);
                string limit = FormatBytes(memoryLimit);

                return $"{usage} / {limit}";
            }
            catch (Exception ex)
            {
                Program.LogService.LogError(ex.ToString());
                return "0B / 0B";
            }
        }

        private static string FormatBytes(double bytes)
        {
            const double kb = 1024;
            const double mb = kb * 1024;
            const double gb = mb * 1024;

            if (bytes < kb)
                return $"{bytes:F2}B";
            return bytes switch
            {
                < mb => $"{bytes / kb:F2}kB",
                < gb => $"{bytes / mb:F2}MiB",
                _ => $"{bytes / gb:F2}GiB"
            };
        }

        private static string FormatNetIo(StatApiReturn? jsonData)
        {
            try
            {
                long currentRxBytes = jsonData?.Networks?.Eth0?.RxBytes ?? 0;
                long previousRxBytes = jsonData?.Networks?.Eth0?.RxBytes ?? 0;
                long currentTxBytes = jsonData?.Networks?.Eth0?.TxBytes ?? 0;
                long previousTxBytes = jsonData?.Networks?.Eth0?.TxBytes ?? 0;

                // Compute time difference in seconds
                DateTime currentTime = DateTime.Parse(jsonData?.Read ?? DateTime.Now.ToString(CultureInfo.InvariantCulture));
                DateTime previousTime = DateTime.Parse(jsonData?.Preread ?? DateTime.Now.ToString(CultureInfo.InvariantCulture));
                double timeDelta = (currentTime - previousTime).TotalSeconds;

                if (timeDelta <= 0)
                    return "0B / 0B";

                // Compute throughput (bytes per second)
                double rxRate = (currentRxBytes - previousRxBytes) / timeDelta;
                double txRate = (currentTxBytes - previousTxBytes) / timeDelta;

                return $"{rxRate}B / {txRate}B";
            }
            catch (Exception ex)
            {
                Program.LogService.LogError(ex.ToString());
                return "0B / 0B";
            }

        }
    }

}
