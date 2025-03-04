using System.Net;
using Blazored.LocalStorage;
using Microsoft.Extensions.Hosting.WindowsServices;
using MudBlazor.Services;
using WindowsServerManager.Components;
using WindowsServerManager.Libraries.Services;

namespace WindowsServerManager
{
    public class Program
    {
        public static string Version = "Beta 1.0.0";
        public static DateTime StartTime = DateTime.Now;
        public static LogService LogService = new($@"{AppDomain.CurrentDomain.BaseDirectory}\logs\log_{DateTime.Now:yyyy-MM-dd}.txt");

        public static HttpClient HttpClient = new(new HttpClientHandler()
        {
            AllowAutoRedirect = true,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            AutomaticDecompression = DecompressionMethods.All
        });

        public static void Main(string[] args)
        {
            LogService.LogInformation("Application started");
            WebApplicationOptions webApplicationOptions = new()
            {
                ContentRootPath = AppContext.BaseDirectory,
                Args = args,
                ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName
            };
            WebApplicationBuilder builder = WebApplication.CreateBuilder(webApplicationOptions);

            LogService.LogInformation("Building services...");
            // Add services to the container.
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();
            builder.Services.AddHttpClient();
            HttpContextExtensions.AddHttpContextAccessor(builder.Services); // Yuck but doesn't have relation errors
            builder.Services.AddControllers();
            builder.Services.AddMudServices();
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddSingleton<UpdaterService>();
            
            if (WindowsServiceHelpers.IsWindowsService()) builder.Services.AddWindowsService();

            WebApplication app = builder.Build();
            
            LogService.LogInformation("Building app...");

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            app.MapControllers();
            app.UseStaticFiles();
            app.UseAntiforgery();
            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
            app.UseHttpContext();
            
            LogService.LogInformation("Running web server...");
            
            app.Run();
        }
    }
}