using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WindowsServerManager.SystemUpdater
{
    internal class Program
    {
        record ProgramUpdate(string Name, string CurrentVersion, string NewVersion, string Download);

        record SystemUpdate(string Name, string UpdateId, string Severity, string Mandatory);

        static async Task Main(string[] args)
        {
            RootCommand root = new ("System Updater CLI");

            Option<bool> programsOption = new Option<bool>("--programs", "Program updates");
            Option<bool> systemOption = new Option<bool>("--system", "System updates");

            Command checkCommand = new("check", "Checks for updates");
            checkCommand.AddOption(programsOption);
            checkCommand.AddOption(systemOption);

            Command updateCommand = new("update", "Initiates updates");
            updateCommand.AddOption(programsOption);
            updateCommand.AddOption(systemOption);

            checkCommand.SetHandler(async (programs, system) => await CheckForUpdatesHandle(programs, system), programsOption, systemOption);
            updateCommand.SetHandler(DoUpdatesHandle, programsOption, systemOption);

            root.AddCommand(checkCommand);
            root.AddCommand(updateCommand);

            await root.InvokeAsync(args);
        }

        private static void DoUpdatesHandle(bool programs, bool system)
        {
            if (!programs && !system)
            {
                Console.WriteLine("Error: You must specify either --programs or --system.");
                return;
            }

            if (programs)
            {
                Console.WriteLine("Updating programs...");
            }

            if (system)
            {
                Console.WriteLine("Updating system...");
            }
        }

        private static async Task CheckForUpdatesHandle(bool programs, bool system)
        {

            if (!programs && !system)
            {
                Console.WriteLine("Error: You must specify either --programs or --system.");
                return;
            }

            if (programs)
            {
                List<ProgramUpdate> updates = [];
                string updatesFound = await Execution.ExecuteConHost("winget upgrade --include-unknown --disable-interactivity --nowarn");

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

                try
                {
                    await File.WriteAllTextAsync("soft-updates.json", JsonSerializer.Serialize(updates));
                }
                catch (AccessViolationException)
                {
                    await Console.Error.WriteLineAsync("Access Violation while writing results.");
                }
            }

            if (system)
            {
                List<SystemUpdate> systemUpdates = [];
                string updatesFound = await Execution.ExecutePowerShell("(((New-Object -ComObject Microsoft.Update.Session).CreateUpdateSearcher()).Search('IsInstalled=0').Updates) | Select-Object Title, @{Name='KBArticleIDs'; Expression={($_.KBArticleIDs -join ', ')}}, MsrcSeverity, IsMandatory | Out-String -Width 999");

                string[] programUpdates = updatesFound
                    .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Skip(2) // First 2 is just table indexers
                    .ToArray();

                string titles = updatesFound.Split('\n')[1];

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


                try
                {
                    await File.WriteAllTextAsync("sys-updates.json", JsonSerializer.Serialize(systemUpdates));
                }
                catch (AccessViolationException)
                {
                    await Console.Error.WriteLineAsync("Access Violation while writing results.");
                }
            }
        }
    }
}
