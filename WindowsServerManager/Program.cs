using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Hosting.WindowsServices;
using MudBlazor.Services;
using WindowsServerManager.Components;
using WindowsServerManager.Libraries.NotificationClients;
using WindowsServerManager.Libraries.Services;
using WindowsServerManager.Libraries.Services.ExternalService;
using WindowsServerManager.Libraries.Utilities.Expanders;
#pragma warning disable CA2211

namespace WindowsServerManager
{
    public class Program
    {
        public static BugCheckService BugCheckService { get; private set; } = null!;
        public static SystemUpdaterService SystemUpdaterService { get; private set; } = null!;
        public static SoftwareUpdaterService SoftwareUpdaterService { get; private set; } = null!;
        public static DiskCheckService DiskCheckService { get; private set; } = null!;

        public static NotificationClient NotificationClient { get; private set; } = null!;
        public static Settings? Settings { get; private set; }
        public static string Version = "Beta 1.0.0";
        public static DateTime StartTime = DateTime.Now;
        public static LogService LogService = new();

        public static HttpClient HttpClient = new(new HttpClientHandler
        {
            AllowAutoRedirect = true,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            AutomaticDecompression = DecompressionMethods.All
        });

        public static void Main(string[] args)
        {
            LogService.LogInformation("Application started");

            AppDomain.CurrentDomain.UnhandledException += (_, exception) => LogService.LogError(exception.ExceptionObject.ToString()!);

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                foreach (Process process in Process.GetProcesses().Where(x=> (x.ProcessName.Contains("WindowsServerManager.") && x.MainModule!.FileName.Contains(AppDomain.CurrentDomain.BaseDirectory))))
                    process.Kill(true);
            };

            LogService.LogInformation("Checking settings file");
            if (File.Exists("settings.json"))
                Settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText("settings.json"));
            else
            {
                // Generate empty settings file
                LogService.LogInformation("File doesn't exist, creating empty settings");
                Settings = new Settings(new UpdateSettings(), new VmSettings(), new EventViewerSettings(), new ArrSuite(new SonarrSettings(), new RadarrSettings(), new ProwlarrSettings(), new QBitTorrentSettings(), new BazaarSettings(), new WhisparrSettings()), new NotificationSettings(new DiscordSettings(), new PushoverSettings(), new EmailSettings()));
                File.WriteAllText("settings.json", Json.Serialize(Settings));
            }


            BugCheckService = new BugCheckService(Settings?.EventViewerSettings.RecheckTimeMinutes ?? 60);
            DiskCheckService = new DiskCheckService(Settings?.EventViewerSettings.RecheckTimeMinutes ?? 60);
            SystemUpdaterService = new SystemUpdaterService(Settings?.UpdateSettings.UpdateRecheckTimeMinutes ?? 60);
            SoftwareUpdaterService = new SoftwareUpdaterService(Settings?.UpdateSettings.UpdateRecheckTimeMinutes ?? 60);

            if (!File.Exists("notifications.json"))
                File.WriteAllText("notifications.json", "[]".ToBase64());

            NotificationClient = new NotificationClient(Settings!);

            NotifierService _ = new(); // Starts the actual notifier service which will run in the background instead of in the DashLayout

            WebApplicationOptions webApplicationOptions = new()
            {
                ContentRootPath = AppContext.BaseDirectory,
                Args = args,
                ApplicationName = Process.GetCurrentProcess().ProcessName
            };
            WebApplicationBuilder builder = WebApplication.CreateBuilder(webApplicationOptions);

            LogService.LogInformation("Building services...");
            // Add services to the container.
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();
            builder.Services.AddHttpClient();
            builder.Services.AddControllers();
            builder.Services.AddMudServices();
            builder.Services.AddBlazoredLocalStorage();
            
            LogService.LogInformation("Checking if launched as windows service");
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

            app.UseExceptionHandler(errorHandler => errorHandler.Run(async context => await LogService.LogError(context.Features.Get<IExceptionHandlerFeature>()?.Error.ToString())));

            app.MapControllers();
            app.UseStaticFiles();
            app.UseAntiforgery();
            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
            
            LogService.LogInformation("Running web server...");
            
            app.Run();
        }
    }

    public record UpdateSettings(int? UpdateRecheckTimeMinutes = 30, bool? EnableSystemUpdateChecker = true, bool? EnableSoftwareUpdateChecker = true, bool EnabledSystemUpdateNotification = false, bool EnableSoftwareUpdateNotification = false);
    public record VmSettings(bool? EnableHyperVManagement = true);

    public record EventViewerOptions(bool BugCheck = false, bool Disk = false, bool BugCheckNotification = false, bool DiskNotification = false);
    public record EventViewerSettings(bool? Enabled = true, int? RecheckTimeMinutes = 60, EventViewerOptions? ViewerOptions = default);

    public record SonarrSettings(bool? Enabled = false, string? Url = "", string? ApiKey = "");
    public record RadarrSettings(bool? Enabled = false, string? Url = "", string? ApiKey = "");
    public record ProwlarrSettings(bool? Enabled = false, string? Url = "", string? ApiKey = "");
    public record QBitTorrentSettings(bool? Enabled = false, string? Url = "", string? Username = "", string? Password = "");
    public record BazaarSettings(bool? Enabled = false, string? Url = "", string? ApiKey = "");
    public record WhisparrSettings(bool? Enabled = false, string? Url = "", string? ApiKey = "");
    public record ArrSuite(SonarrSettings? Sonarr = default, RadarrSettings? Radarr = default, ProwlarrSettings? Prowlarr = default, QBitTorrentSettings? QBitTorrent = default, BazaarSettings? Bazaar = default, WhisparrSettings? Whisparr = default);
    
    public record DiscordSettings(bool? WebhookEnabled = true, string? WebhookUrl = "", string? RolePing = "", bool? SoftwarePing = true, bool? SystemPing = true, bool? SmartPing = true, bool? BugCheckPing = true);
    public record PushoverSettings(bool? PushoverEnabled = true, string? PushoverToken = "", string? PushoverUser = "", string? PushoverDevice = "");
    public record EmailSettings(bool? EmailEnabled = true, string? EmailTo = "", string? SmtpSender = "", string? SmtpUsername = "", string? SmtpPassword = "", string? SmtpHost = "", string? SmtpPort = "");
    public record NotificationSettings(DiscordSettings? Discord = default, PushoverSettings? Pushover = default, EmailSettings? Email = default);


    public record Settings(UpdateSettings UpdateSettings, VmSettings VmSettings, EventViewerSettings EventViewerSettings, ArrSuite ArrSuite, NotificationSettings NotificationSettings);

}