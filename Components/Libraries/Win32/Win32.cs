
using System.Runtime.InteropServices;
using System.Text;

namespace WindowsServerManager.Components.Libraries.Win32
{
    public record MotherboardInfo(string Manufacturer, string Product, string BiosVendor, string BiosVersion, string BiosReleaseDate);

    public record DriveInfo(string Drive, ulong UsedBytes, ulong TotalBytes, string SerialNumber, string PhysicalName)
    {
        public string? UsedSizeFormatted { get; set; }
        public string? TotalSizeFormatted { get; set; }
        public string? SizePercent { get; set; }
    };
    public record MemoryInfo(ulong TotalMemory, ulong AvailableMemory, ulong UsedMemory);

    public record MemoryStickInfo(string Slot, string SerialNumber, string Details);
    public record CpuInfo(string Name, string Hertz, string LogicalProcessors);
    public record CpuStats(float Usage);

    public static class Win32
    {
        [DllImport("kernel32")]
        public static extern UInt64 GetTickCount64();

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out nint phToken);

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int NetGetDCName(
            string serverName,
            string domainName,
            out nint domainController);

        [DllImport("netapi32.dll")]
        public static extern int NetApiBufferFree(nint buffer);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetDiskFreeSpaceEx(
            string lpDirectoryName,
            out ulong lpFreeBytesAvailable,
            out ulong lpTotalNumberOfBytes,
            out ulong lpTotalNumberOfFreeBytes);

    }
}
