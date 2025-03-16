using static WindowsServerManager.Libraries.Win32.Win32;

namespace WindowsServerManager.Libraries.Win32.Logistics
{
    public static class LoginHandler
    {
        public static bool DoesDomainExist(string domainName)
        {
            bool validDomain = NetGetDCName(null!, domainName, out nint domainController) == 0;
#pragma warning disable CA1806
            NetApiBufferFree(domainController);
#pragma warning restore CA1806
            return validDomain;
        }

        public static bool ValidUserLogin(string username, string password, string domain = ".")
        {
            if (string.IsNullOrEmpty(username)) { return false; }
            if (string.IsNullOrEmpty(password)) { return false; }
            if (!DoesDomainExist(domain)) { return false; }

            return LogonUser(username, domain, password, 2, 0, out nint _);
        }

    }
}
