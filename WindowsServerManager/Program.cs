using System.Net;
using Blazored.LocalStorage;
using Microsoft.Extensions.Hosting.WindowsServices;
using MudBlazor.Services;
using WindowsServerManager.Components;
using WindowsServerManager.Components.Libraries.Services;

namespace WindowsServerManager
{
    public class Program
    {
        public static string Version = "Beta 1.0.0";
        public static DateTime StartTime = DateTime.Now;
        public static LogService LogService = new($@"{AppDomain.CurrentDomain.BaseDirectory}\logs\log_{DateTime.Now:yyyy-MM-dd}.txt");

        public static string JwtSecret = Environment.GetEnvironmentVariable("JwtSecret") ?? "TEST_SECRET"; // ?? throw new Exception("JwtSecret not found in environment variables");
        public static string JwtIssuer = Environment.GetEnvironmentVariable("JwtIssuer") ?? "TEST_ISSUER"; //?? throw new Exception("JwtIssuer not found in environment variables");

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
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllers();

            builder.Services.AddMudServices();

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddSingleton<IClientIpAddressService, ClientIpAddressService>();
            
            if (WindowsServiceHelpers.IsWindowsService()) builder.Services.AddWindowsService();

            WebApplication app = builder.Build();

            //app.UseMiddleware<IpCheckMiddleware>(); // Allows only local connections

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

    public record DatabaseConnectionDetails(string Username, string Password, string Database, IPAddress Address);

    public class IpCheckMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            string? remoteIp = context.Connection.RemoteIpAddress?.ToString();

            if (context.Request.Path.StartsWithSegments("/api"))
            {
                await Program.LogService.LogInformation($"({context.Response.StatusCode}) API request from {remoteIp} via: {context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}");
            }

            await next(context);
        }
    }

    public class BetterContext
    {
        private static IHttpContextAccessor? _httpContextAccessor;

        public static HttpContext? Current => _httpContextAccessor?.HttpContext;

        public static string AppBaseUrl => $"{Current?.Request.Scheme}://{Current?.Request.Host}{Current?.Request.PathBase}";

        internal static void Configure(IHttpContextAccessor? contextAccessor) => _httpContextAccessor = contextAccessor;
    }

    public static class HttpContextExtensions
    {
        public static void AddHttpContextAccessor(this IServiceCollection services) => services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        public static IApplicationBuilder UseHttpContext(this IApplicationBuilder app)
        {
            BetterContext.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
            return app;
        }
    }
}