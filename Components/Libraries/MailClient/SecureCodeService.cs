using System.Security.Cryptography;

namespace WindowsServerManager.Components.Libraries.MailClient;

public interface ISecureCodeService
{
    string GenerateSecureCode();
}

public class SecureCodeService : ISecureCodeService
{
    public string GenerateSecureCode()
    {
        byte[] randomBytes = new byte[4];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        int randomValue = BitConverter.ToInt32(randomBytes, 0) & 0x7FFFFFFF;
        int secureCode = randomValue % 900000 + 100000;

        return secureCode.ToString("D6");
    }
}