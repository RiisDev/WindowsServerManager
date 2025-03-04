using System.Diagnostics;
using Microsoft.Win32;

namespace WindowsServerManager.Libraries.Win32.Logistics
{
    public static class CpuHandler
    {
        public static CpuInfo GetCpuInfo()
        {
            string processorRoot = @"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0";

            string processorName = Registry.GetValue(processorRoot, "ProcessorNameString", "N/A")?.ToString() ?? "N/A";
            string baseSpeed = ((int)Registry.GetValue(processorRoot, "~MHz", 0)!).ToString();
            string logicalProcessors = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment", "NUMBER_OF_PROCESSORS", 0)!.ToString() ?? "N/A";

            return new CpuInfo(processorName, baseSpeed, logicalProcessors);
        }

        public static CpuStats GetCpuStats()
        {
            using PerformanceCounter cpuCounter = new("Processor Information", "% Processor Utility", "_Total");
            cpuCounter.NextValue();
            Thread.Sleep(1000);
            float actualPercent = cpuCounter.NextValue() / 100;
            return new CpuStats(actualPercent);
        }

        public static CpuStats FakeStats() => new(0);
        public static CpuInfo FakeInfo() => new("Generic CPU", "1000", "1");
    }
}
