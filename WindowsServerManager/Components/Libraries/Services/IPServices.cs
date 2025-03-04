namespace WindowsServerManager.Components.Libraries.Services
{
    public interface IClientIpAddressService
    {
        string? GetClientIpAddress();
    }

    public class ClientIpAddressService(IHttpContextAccessor httpContextAccessor) : IClientIpAddressService
    {
        public string? GetClientIpAddress()
        {
            HttpContext? context = httpContextAccessor.HttpContext;

            if (context == null) return null;

            string? forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            switch (string.IsNullOrEmpty(forwardedHeader))
            {
                case false:
                    {
                        string? ipAddress = forwardedHeader?.Split(',').FirstOrDefault();
                        return ipAddress;
                    }
                default:
                    return context.Connection?.RemoteIpAddress?.ToString();
            }
        }
    }

}
