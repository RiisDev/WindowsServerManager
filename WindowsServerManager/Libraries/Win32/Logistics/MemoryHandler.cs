using System.Diagnostics.CodeAnalysis;
using System.Management;
using System.Runtime.InteropServices;
using static WindowsServerManager.Libraries.Win32.Win32;

namespace WindowsServerManager.Libraries.Win32.Logistics
{
    public static class MemoryHandler
    {
        public static MemoryInfo FakeMemoryInfo() => new(0,0, 0);

        public static MemoryInfo GetMemoryUsage()
        {
            MEMORYSTATUSEX memoryStatus = new()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX))
            };

            if (!GlobalMemoryStatusEx(ref memoryStatus)) return new MemoryInfo(0, 0, 0);

            ulong totalMemory = memoryStatus.ullTotalPhys;
            ulong availableMemory = memoryStatus.ullAvailPhys;
            ulong usedMemory = totalMemory - availableMemory;
            return new MemoryInfo(totalMemory, availableMemory, usedMemory);
        }

        [SuppressMessage("ReSharper", "PossibleInvalidCastExceptionInForeachLoop")]
        public static List<MemoryStickInfo> GetSticks()
        {
            List<MemoryStickInfo> sticks = [];

            ManagementObjectSearcher searcher = new("SELECT * FROM Win32_PhysicalMemory");

            foreach (ManagementObject obj in searcher.Get())
            {
                string slot = obj["DeviceLocator"]?.ToString() ?? "N/A";
                string bankLabel = obj["BankLabel"]?.ToString() ?? "N/A";
                string partNumber = obj["PartNumber"]?.ToString() ?? "N/A";
                string manufacturer = obj["Manufacturer"]?.ToString() ?? "N/A";
                ulong capacityBytes = Convert.ToUInt64(obj["Capacity"]);
                int speed = Convert.ToInt32(obj["Speed"]);
                string capacityGb = capacityBytes / (1024 * 1024 * 1024) + " GB";

                sticks.Add(new MemoryStickInfo($"{slot.Insert(5, bankLabel[^1].ToString())}", $"{partNumber}", $"{manufacturer} {capacityGb} {speed}MHz"));
            }

            return sticks;
        }
    }
}
