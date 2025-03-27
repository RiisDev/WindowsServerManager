using System;
using System.CommandLine;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WindowsServerManager.SoftwareUpdater
{
    internal static class StringExpand { public static string ToBase64(this string value) => Convert.ToBase64String(Encoding.ASCII.GetBytes(value)); }

    internal class Program
    {
        public static LogService LogService = new($@"{AppDomain.CurrentDomain.BaseDirectory}\..\..\logs\software_updater-log_{DateTime.Now:yyyy-MM-dd}.txt");
        private record ProgramUpdate(string Name, string CurrentVersion, string NewVersion, string Download);

        static async Task Main(string[] args)
        {
            if (Process.GetProcessesByName("WindowsServerManager.SoftwareUpdater").Length > 1) return;

            await LogService.LogInformation("Starting application...");

            RootCommand root = new("Software Updater CLI");

            Command checkCommand = new("check", "Checks for updates");

            Command updateCommand = new("update", "Initiates updates");

            checkCommand.SetHandler(async _ => await CheckForUpdatesHandle());

            updateCommand.SetHandler(DoUpdatesHandle);

            root.AddCommand(checkCommand);
            root.AddCommand(updateCommand);

            await root.InvokeAsync(args);
        }

        private static async Task DoUpdatesHandle() => await Console.Error.WriteLineAsync("Not implemented");

        private static async Task CheckForUpdatesHandle()
        {
            await LogService.LogInformation("Checking for updates...");
            List<ProgramUpdate> updates = [];
            string updatesFound = await Execution.ExecuteConHost(@$"C:\Users\Administrator\AppData\Local\Microsoft\WindowsApps\winget.exe upgrade --include-unknown --disable-interactivity --nowarn");
            await LogService.LogInformation($"Updates Found: {updatesFound}");

            string[] programUpdates = updatesFound
                .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Skip(2) // First 2 is just table indexers
                .ToArray();

            // This index is hell, but every other way I tried refused to work
            // Feel free to modify
            string titles = updatesFound.Split('\n').First()[updatesFound.IndexOf("Name", StringComparison.Ordinal)..].Trim();

            string nameExtract = titles[..titles.IndexOf("Id", StringComparison.Ordinal)];
            string idExtract = titles[titles.IndexOf("Id", StringComparison.Ordinal)..titles.IndexOf("Version", StringComparison.Ordinal)];
            string versionExtract = titles[titles.IndexOf("Version", StringComparison.Ordinal)..titles.IndexOf("Available", StringComparison.Ordinal)];
            string availableExtract = titles[titles.IndexOf("Available", StringComparison.Ordinal)..titles.IndexOf("Source", StringComparison.Ordinal)];

            int nameLength = nameExtract.Length - 1;
            int idLength = idExtract.Length - 1;
            int versionLength = versionExtract.Length - 1;
            int availableLength = availableExtract.Length - 1;

            foreach (string program in programUpdates)
            {
                Match match = Regex.Match(program, @$"^(.{{{nameLength}}})\s+(.{{{idLength}}})\s+(.{{{versionLength}}})\s+(.{{{availableLength}}})\s+(\S+)$");

                if (!match.Success) continue;

                string id = match.Groups[2].Value.Trim();
                string programInfo = await Execution.ExecuteConHost($"winget show \"{id}\"");
                string installerUrl;
                try
                {
                    installerUrl = programInfo.Split('\n').First(x => x.Contains("Installer Url"))[17..];
                }
                catch
                {
                    Debug.WriteLine(programInfo);
                    installerUrl = "N/A";
                }

                updates.Add(
                    new ProgramUpdate(
                        Name: match.Groups[1].Value.Trim(),
                        CurrentVersion: match.Groups[3].Value.Trim(),
                        NewVersion: match.Groups[4].Value.Trim(),
                        Download: installerUrl.Trim()
                    )
                );
            }

            Console.WriteLine(JsonSerializer.Serialize(updates));
        }
    }
}
