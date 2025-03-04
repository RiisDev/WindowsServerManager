using System.Diagnostics.CodeAnalysis;
using System.Management;
using static WindowsServerManager.Components.Libraries.Win32.Win32;

namespace WindowsServerManager.Components.Libraries.Win32
{
    public static class DriveHandler
    {

        [SuppressMessage("ReSharper", "PossibleInvalidCastExceptionInForeachLoop")]
        private static (string, string) GetDriveInfoFromLetter(string driveLetter)
        {
            try
            {
                using ManagementObjectSearcher partitionSearcher = new("ASSOCIATORS OF {Win32_LogicalDisk.DeviceID='" + driveLetter + "'} WHERE AssocClass=Win32_LogicalDiskToPartition");
                foreach (ManagementObject partition in partitionSearcher.Get())
                {
                    using ManagementObjectSearcher diskSearcher = new("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE AssocClass=Win32_DiskDriveToDiskPartition");
                    foreach (ManagementObject disk in diskSearcher.Get())
                    {
                        string? model = disk["Model"]?.ToString();
                        string? serialNumber = disk["SerialNumber"]?.ToString();

                        return (serialNumber ?? "", model ?? "");
                    }
                }
            }
            catch{/**/}

            return ("", "");
        }


        public static List<DriveInfo> GetDriveSpace()
        {
            List<DriveInfo> drives = [];

            foreach (string drive in Directory.GetLogicalDrives())
            {
                GetDiskFreeSpaceEx(drive, out ulong _, out ulong totalBytes, out ulong totalFreeBytes);
                ulong usedBytes = totalBytes - totalFreeBytes;

                string driveLetter = drive[..2];
               (string serialNumber, string physicalName) = GetDriveInfoFromLetter(driveLetter);

               drives.Add(new DriveInfo(drive, usedBytes, totalBytes, serialNumber.Trim(), physicalName.Trim()));
            }

            return drives;
        }
    }
}
