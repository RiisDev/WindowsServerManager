using System.Diagnostics;
using System.Text.RegularExpressions;
using WindowsServerManager.Libraries.Win32.Shell;
using static System.Text.RegularExpressions.Regex;

namespace WindowsServerManager.Libraries.Win32.Firewall
{
    public static class FirewallHandler
    {
        public static async Task<List<FirewallRule>?> GetFirewallRules()
        {
            string output = await Execution.ExecuteConHost($"netsh advfirewall firewall show rule name=all");
            return ParseFirewallRules(output);
        }

        public static async Task<string> CreateFirewallRule(string command) => await Execution.ExecuteConHost(command);

        private static List<FirewallRule>? ParseFirewallRules(string output)
        {
            List<FirewallRule>? rules = [];

            string[] ruleBlocks = output.Split(["----------------------------------------------------------------------"], StringSplitOptions.RemoveEmptyEntries).
                Skip(1).
                ToArray(); // I don't like the skip, but for some reason netsh returns an invalid rule at first invoke

            rules.AddRange(ruleBlocks.Select(ParseFirewallRule).OfType<FirewallRule>());

            return rules;
        }

        private static FirewallRule? ParseFirewallRule(string block)
        {
            Match ruleNameMatch = Match(input: block, pattern: @"^Rule Name:\s*(.*)", options: RegexOptions.Multiline);
            Match enabledMatch = Match(input: block, pattern: @"^Enabled:\s*(.*)", options: RegexOptions.Multiline);
            Match directionMatch = Match(input: block, pattern: @"^Direction:\s*(.*)", options: RegexOptions.Multiline);
            Match profilesMatch = Match(input: block, pattern: @"^Profiles:\s*(.*)", options: RegexOptions.Multiline);
            Match localIpMatch = Match(input: block, pattern: @"^LocalIP:\s*(.*)", options: RegexOptions.Multiline);
            Match remoteIpMatch = Match(input: block, pattern: @"^RemoteIP:\s*(.*)", options: RegexOptions.Multiline);
            Match protocolMatch = Match(input: block, pattern: @"^Protocol:\s*(.*)", options: RegexOptions.Multiline);
            Match localPortMatch = Match(input: block, pattern: @"^LocalPort:\s*(.*)", options: RegexOptions.Multiline);
            Match remotePortMatch = Match(input: block, pattern: @"^RemotePort:\s*(.*)", options: RegexOptions.Multiline);
            Match edgeTraversalMatch = Match(input: block, pattern: @"^Edge traversal:\s*(.*)", options: RegexOptions.Multiline);
            Match actionMatch = Match(input: block, pattern: @"^Action:\s*(.*)", options: RegexOptions.Multiline);

            if (ruleNameMatch.Success)
            {
                return new FirewallRule
                (
                    RuleName: ruleNameMatch.Groups[groupnum: 1].Value.Trim(),
                    Enabled: enabledMatch.Groups[groupnum: 1].Value.Trim(),
                    Direction: directionMatch.Groups[groupnum: 1].Value.Trim(),
                    Profiles: profilesMatch.Groups[groupnum: 1].Value.Trim(),
                    LocalIp: localIpMatch.Groups[groupnum: 1].Value.Trim(),
                    RemoteIp: remoteIpMatch.Groups[groupnum: 1].Value.Trim(),
                    Protocol: protocolMatch.Groups[groupnum: 1].Value.Trim(),
                    LocalPort: localPortMatch.Groups[groupnum: 1].Value.Trim(),
                    RemotePort: remotePortMatch.Groups[groupnum: 1].Value.Trim(),
                    EdgeTraversal: edgeTraversalMatch.Groups[groupnum: 1].Value.Trim(),
                    Action: actionMatch.Groups[groupnum: 1].Value.Trim()
                );
            }

            return null;
        }
    }
}
