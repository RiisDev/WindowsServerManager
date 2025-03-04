namespace WindowsServerManager.Libraries.Win32.Firewall;
public record FirewallRule(
    string RuleName,
    string Enabled,
    string Direction,
    string Profiles,
    string LocalIp,
    string RemoteIp,
    string Protocol,
    string LocalPort,
    string RemotePort,
    string EdgeTraversal,
    string Action
);