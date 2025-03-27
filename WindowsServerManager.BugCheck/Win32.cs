using System.Diagnostics;

namespace WindowsServerManager.BugCheck
{
    public class Win32
    {
        public record BugCheck(DateTime Timestamp, string ProcessName, string ImageName, string Module, string BsodType, string Description);

        public static async Task<BugCheck?> AnalyzeKernelDump(string dumpFilePath, DateTime creationDate)
        {
            ProcessStartInfo psi = new()
            {
                FileName = $@"{AppDomain.CurrentDomain.BaseDirectory}..\windbg\kd.exe",
                WorkingDirectory = $@"{AppDomain.CurrentDomain.BaseDirectory}..\windbg",
                Arguments = $"-z \"{dumpFilePath}\" -c \"!analyze -v; q\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = Process.Start(psi)!;

            string shellReturn = await process.StandardOutput.ReadToEndAsync();
            string shellError = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            Program.LogService?.LogInformation($"Process Return: {shellReturn}");

            if (!string.IsNullOrEmpty(shellError))
                Program.LogService?.LogError($"Process Error Return: {shellError}");

            return ParseDebugOutput(shellReturn, creationDate);
        }

        private static BugCheck? ParseDebugOutput(string? output, DateTime creationDate)
        {
            if (string.IsNullOrEmpty(output)) return default;

            string processName = "";
            string imageName = "";
            string module = "";
            string bsodType = "";
            string description = "";

            string[] lines = output.Split('\n');

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string lineText = lines[lineIndex];

                if (lineText.Contains("Bugcheck Analysis"))
                {
                    bsodType = lines[lineIndex + 4];

                    for (int descriptionIndex = lineIndex + 5; descriptionIndex < lines.Length; descriptionIndex++)
                    {
                        string descriptionText = lines[descriptionIndex];

                        if (descriptionText == "Arguments:") break;

                        description += descriptionText;
                    }
                }

                if (lineText.Contains("PROCESS_NAME"))
                    processName = lineText[15..];

                if (lineText.Contains("IMAGE_NAME"))
                    imageName = lineText[13..];

                if (lineText.Contains("MODULE_NAME"))
                    module = lineText[13..];
            }


            return new BugCheck(creationDate, processName.Trim(), imageName.Trim(), module.Trim(), bsodType.Trim(), description.Trim());
        }
    }
}
