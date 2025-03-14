using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Text.RegularExpressions.Regex;

#pragma warning disable SYSLIB1045

namespace WindowsServerManager.DiskCheck
{
    internal class Program
    {
        private record SmartData(
            string ModelName,
            string SerialNumber,
            string DriveLetter,
            string Size,
            string HealthStatus,
            string Temperature,
            long PowerOnHours,
            long PowerOnCount,
            long? ReAllocatedSectors = 0,
            long? UncorrectableSectors = 0,
            long? PendingSectors = 0,
            long? Reads = 0,
            long? Writes = 0,
            long? HeliumLevel = 0);
        
        private static string ExtractValue(string input, [StringSyntax(StringSyntaxAttribute.Regex)] string pattern)
        {
            Match match = Match(input, pattern);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        private static long? ParseNullableInt(string input, NumberStyles style = NumberStyles.None) => long.TryParse(input, style, CultureInfo.InvariantCulture, out long result) ? result : (long?)null;

        private static List<SmartData> GetDiskStats(string data)
        {
            List<SmartData> smartData = [];

            string[] drivesData = data.Split("Model : ").Skip(1).ToArray();

            smartData.AddRange(from drive in drivesData
                let modelName = drive.Split('\n').First().Trim()
                let serialNumber = ExtractValue(drive, "Serial Number : (.*)")
                let driveLetter = ExtractValue(drive, "Drive Letter : (.*)")
                let size = ExtractValue(drive, "Disk Size : (.*)")
                let healthStatus = ExtractValue(drive, "Health Status : (.*)")
                let temperature = ExtractValue(drive, "Temperature : (.*)")
                let powerOnHours = ParseNullableInt(ExtractValue(drive, "Power On Hours : (.*) hours")) ?? 0
                let powerOnCount = ParseNullableInt(ExtractValue(drive, "Power On Count : (.*) count")) ?? 0
                let reAllocatedSectors = ParseNullableInt(ExtractValue(drive, "([0-9A-F]+) Reallocated Sectors Count"), NumberStyles.HexNumber)
                let uncorrectableSectors = ParseNullableInt(ExtractValue(drive, "([0-9A-F]+) Uncorrectable Sector Count"), NumberStyles.HexNumber)
                let pendingSectors = ParseNullableInt(ExtractValue(drive, "([0-9A-F]+) Current Pending Sector Count"), NumberStyles.HexNumber)
                let reads = ParseNullableInt(ExtractValue(drive, "Host Reads : (.*) "))
                let writes = ParseNullableInt(ExtractValue(drive, "Host Writes : (.*) "))
                let heliumLevel = ParseNullableInt(ExtractValue(drive, "([0-9A-F]+) Current Helium Level"), NumberStyles.HexNumber)
                select new SmartData(modelName, serialNumber, driveLetter, size, healthStatus, temperature,
                    powerOnHours, powerOnCount, reAllocatedSectors, uncorrectableSectors, pendingSectors, reads, writes,
                    heliumLevel));

            return smartData;
        }

        static void Main(string[] args)
        {
            string crystalDirectory = $@"{AppDomain.CurrentDomain.BaseDirectory}runtimes\crystaldiskinfo";
            string diskInfoPath = $"{crystalDirectory}\\DiskInfo.txt";

            if (File.Exists(diskInfoPath)) File.Delete(diskInfoPath);

            ProcessStartInfo startInfo = new()
            {
                FileName = $@"{AppDomain.CurrentDomain.BaseDirectory}runtimes\crystaldiskinfo\DiskInfo64.exe",
                CreateNoWindow = true,
                Arguments = "/CopyExit",
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Minimized
            };

            using Process? diskInfo = Process.Start(startInfo);

            if (diskInfo is null) throw new ApplicationException("Failed to create DiskInfo64.exe process.");

            diskInfo.WaitForExit();

            if (!File.Exists(diskInfoPath)) throw new FileNotFoundException("Failed to find DiskInfo.txt");

            Console.WriteLine(JsonSerializer.Serialize(GetDiskStats(File.ReadAllText(diskInfoPath))));
        }
    }
}
