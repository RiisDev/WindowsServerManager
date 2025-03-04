using Microsoft.Win32;

namespace WindowsServerManager.Libraries.Win32.Logistics
{
    public static class MotherboardHandler
    {
        public static MotherboardInfo GetMotherboardInfo()
        {
            string biosRoot = @"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\BIOS";

            string manufacturer = Registry.GetValue(biosRoot, "BaseBoardManufacturer", "N/A")?.ToString() ?? "N/A";
            string product = Registry.GetValue(biosRoot, "BaseBoardProduct", "N/A")?.ToString() ?? "N/A";
            string biosVendor = Registry.GetValue(biosRoot, "BIOSVendor", "N/A")?.ToString() ?? "N/A";
            string biosVersion = Registry.GetValue(biosRoot, "BIOSVersion", "N/A")?.ToString() ?? "N/A";
            string biosReleaseDate = Registry.GetValue(biosRoot, "BIOSReleaseDate", "N/A")?.ToString() ?? "N/A";

            return new MotherboardInfo(manufacturer, product, biosVendor, biosVersion, biosReleaseDate);
        }
    }
}
