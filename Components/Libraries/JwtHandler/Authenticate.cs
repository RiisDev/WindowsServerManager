using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace WindowsServerManager.Components.Libraries.JwtHandler
{
    public record User(string Username, string Password, bool twoFactor);

    public interface IAuthenticate
    {
        Task DeAuthenticate(string redirectUrl);
        Task AuthenticateWithReturn();

        Task CreateJwt(User user);
        Task CreateAndAuthenticate(User user);
    }

    public class Authenticate(ILocalStorageService localStorage, NavigationManager navigationManager, Func<User?, Task> successCaller, Func<Task> failureCaller) : IDisposable, IAuthenticate
    {
        internal ILocalStorageService LocalStorage { get; } = localStorage;
        internal NavigationManager NavigationManager { get; } = navigationManager;
        internal Func<User?, Task> SuccessCaller { get; } = successCaller;
        internal Func<Task> FailureCaller { get; } = failureCaller;
        internal HttpClient HttpClient { get; } = new(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        });

        public async Task AuthenticateWithReturn()
        {
            JwtService jwt = new(Program.JwtSecret, Program.JwtIssuer, BetterContext.AppBaseUrl);

            string? jwtData = await LocalStorage.GetItemAsStringAsync("local-user");
            if (jwtData == null)
            {
                await FailureCaller();
                return;
            }

            User? authData = jwt.GetJwtRecord<User>(jwtData);
            if (authData == null)
            {
                await LocalStorage.RemoveItemAsync("local-user");
                await FailureCaller();
                return;
            }

            await SuccessCaller(authData);
        }

        public async Task CreateJwt(User user)
        {
            await LocalStorage.RemoveItemAsync("local-user");

            JwtService jwt = new(
                secretKey: Program.JwtSecret,
                issuer: Program.JwtIssuer,
                audience: BetterContext.AppBaseUrl
            );
            string jwtData = jwt.GenerateToken(record: user);

            await LocalStorage.SetItemAsStringAsync(key: "local-user", data: jwtData);
        }

        public async Task CreateAndAuthenticate(User user)
        {
            await CreateJwt(user);

            await AuthenticateWithReturn();
        }

        public async Task DeAuthenticate(string redirectUrl)
        {
            await LocalStorage.RemoveItemAsync("local-user");
            NavigationManager.NavigateTo(redirectUrl);
        }

        public void Dispose() => HttpClient.Dispose();
    }
}
