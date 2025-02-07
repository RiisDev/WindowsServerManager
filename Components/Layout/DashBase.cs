using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WindowsServerManager.Components.Libraries.JwtHandler;
using WindowsServerManager.Components.Libraries.Services;

namespace WindowsServerManager.Components.Layout
{
    public class DashBase : ComponentBase
    {
        [Inject] public NavigationManager NavigationManager { get; set; } = default!;
        [Inject] public IClientIpAddressService ClientIpService { get; set; } = default!;
        [Inject] public IDialogService DialogService { get; set; } = default!;
        [Inject] public ISnackbar SnackbarService { get; set; } = default!;
        [Inject] public ILocalStorageService LocalStorageService { get; set; } = default!;

        public Authenticate Authenticate { get; set; } = default!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            Authenticate = new Authenticate(LocalStorageService, NavigationManager, SuccessCaller, FailureCaller);

            await Authenticate.AuthenticateWithReturn();

            await base.OnAfterRenderAsync(firstRender);
        }

        private Task FailureCaller()
        {
            //string path = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).AbsolutePath;

            //if (path != "/")
            //    NavigationManager.NavigateTo("/", true);

            return Task.CompletedTask;
        }

        private Task SuccessCaller(User? _)
        {
            string path = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).AbsolutePath;

            if (path == "/")
                NavigationManager.NavigateTo("/dashboard", true);

            return Task.CompletedTask;
        }

        public async Task CreateNewAuthorizedJwt(string username, string password, bool twoFactor) => await Authenticate.CreateAndAuthenticate(new User(username, password, twoFactor));

    }
}
