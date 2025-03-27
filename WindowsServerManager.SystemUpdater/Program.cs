using System.CommandLine;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WindowsServerManager.SystemUpdater
{
    internal static class StringExpand { public static string ToBase64(this string value) => Convert.ToBase64String(Encoding.ASCII.GetBytes(value)); }
    internal class Program
    {
        public static LogService LogService = new($@"{AppDomain.CurrentDomain.BaseDirectory}\..\..\logs\system_updater-log_{DateTime.Now:yyyy-MM-dd}.txt");
        private record SystemUpdate(string Name, string UpdateId, string Severity, string Mandatory);

        static async Task Main(string[] args)
        {
            if (Process.GetProcessesByName("WindowsServerManager.SystemUpdater").Length > 1) return;

            await LogService.LogInformation("Starting application...");
            RootCommand root = new ("System Updater CLI");

            Command checkCommand = new("check", "Checks for updates");
            Command updateCommand = new("update", "Initiates updates");

            checkCommand.SetHandler(async _ => await CheckForUpdatesHandle());
            
            updateCommand.SetHandler(DoUpdatesHandle);

            root.AddCommand(checkCommand);
            root.AddCommand(updateCommand);

            await root.InvokeAsync(args);
        }

        private static async Task DoUpdatesHandle() => await Console.Error.WriteLineAsync("Not yet implemented.");

        private static async Task CheckForUpdatesHandle()
        {
            await LogService.LogInformation("Checking for updates...");
            List<SystemUpdate> systemUpdates = [];
            string updatesFound = await Execution.ExecutePowerShell("(((New-Object -ComObject Microsoft.Update.Session).CreateUpdateSearcher()).Search('IsInstalled=0').Updates) | Select-Object Title, @{Name='KBArticleIDs'; Expression={($_.KBArticleIDs -join ', ')}}, MsrcSeverity, IsMandatory | Out-String -Width 999");
            await LogService.LogInformation($"Updates Found: {updatesFound}");
            string[] programUpdates = updatesFound
                .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Skip(2) // First 2 is just table indexers
                .ToArray();

            string titles = updatesFound.Split('\n')[1];

            if (string.IsNullOrEmpty(titles)) goto Finished;

            string nameExtract = titles[..titles.IndexOf("KBArticleIDs", StringComparison.Ordinal)]; // Only title will be longer than the title
            int nameLength = nameExtract.Length - 1;

            systemUpdates.AddRange(
                programUpdates.Select(
                        program => Regex.
                            Match(program, @$"^(.{{{nameLength}}})\s(.{{12}})\s(.{{12}})\s(.{{11}})$")).
                    Select(
                        match => new SystemUpdate(
                            match.Groups[1].Value.Trim(),
                            match.Groups[2].Value.Trim(),
                            match.Groups[3].Value.Trim(),
                            match.Groups[4].Value.Trim()
                        )
                    )
            );

            Finished:

            Console.WriteLine(JsonSerializer.Serialize(systemUpdates));
        }
    }
}
