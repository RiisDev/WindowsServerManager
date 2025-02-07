using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace WindowsServerManager.Components.Libraries.JwtHandler
{
    public class JwtService(string secretKey, string issuer, string audience)
    {
        public string GenerateToken<T>(T record)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.UTF8.GetBytes(secretKey);
            string recordData = JsonSerializer.Serialize(record);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity([
                    new Claim("userAuth", recordData)
                ]),
                Expires = DateTime.UtcNow.AddDays(3),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal? GetPrincipal(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new();
                JwtSecurityToken? jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null || !ValidateToken(jwtToken))
                    return null;

                return new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public T? GetJwtRecord<T>(string token) where T : class
        {
            ClaimsPrincipal? principal = GetPrincipal(token);
            return principal == null ? null : JsonSerializer.Deserialize<T>(principal.Claims.First().Value);
        }

        private bool ValidateToken(JwtSecurityToken token)
        {
            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };

            try
            {
                new JwtSecurityTokenHandler().ValidateToken(token.RawData, tokenValidationParameters, out SecurityToken _);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
