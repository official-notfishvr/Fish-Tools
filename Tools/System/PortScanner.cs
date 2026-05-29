using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace FishTools.App;

internal sealed class PortScanner : ITool
{
    public string Id => "port-scanner";
    public string Name => "Port Scanner";
    public string Category => ToolCategories.SystemHardware;
    public string Description => "Probe a target IP for open network ports (TCP).";

    public async Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        var detectedIp = GetLocalIPv4();
        var target = ConsoleUi.Prompt("Target IP (detected local)", detectedIp ?? "127.0.0.1");
        var startPort = ConsoleUi.PromptInt("Starting port", 1, 1, 65535);
        var endPort = ConsoleUi.PromptInt("Ending port", 1024, 1, 65535);
        var timeoutMs = ConsoleUi.PromptInt("Timeout (ms)", 100, 1, 5000);
        ConsoleUi.ResetScreen(Name);

        ConsoleUi.Info($"Scanning host target: {target} [{startPort} - {endPort}]");

        var openPorts = new List<int>();
        var tasks = new List<Task>();
        var total = endPort - startPort + 1;
        var scannedCount = 0;

        for (var port = startPort; port <= endPort; port++)
        {
            var p = port;
            tasks.Add(
                Task.Run(async () =>
                {
                    if (await IsPortOpen(target, p, timeoutMs))
                    {
                        lock (openPorts)
                        {
                            openPorts.Add(p);
                            ConsoleUi.Success($"Port {p} ({GetServiceName(p)}) is OPEN");
                        }
                    }
                    Interlocked.Increment(ref scannedCount);
                    if (scannedCount % 50 == 0)
                        Console.Title = $"Fish Tools | Port Scan: {scannedCount}/{total}";
                })
            );
        }

        await Task.WhenAll(tasks);
        ConsoleUi.ResetScreen(Name);

        if (openPorts.Count == 0)
        {
            ConsoleUi.Warning($"No open ports found on {target} in the searched range.");
        }
        else
        {
            ConsoleUi.Success($"Scan complete. Found {openPorts.Count} open TCP ports:");
            foreach (var port in openPorts.OrderBy(x => x))
                ConsoleUi.Info($"- {port} ({GetServiceName(port)})");
        }

        ConsoleUi.Pause();
    }

    private static string GetLocalIPv4()
    {
        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces().Where(n => n.OperationalStatus == OperationalStatus.Up))
            {
                foreach (var addr in ni.GetIPProperties().UnicastAddresses.Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork))
                    return addr.Address.ToString();
            }
        }
        catch { }
        return null;
    }

    private static async Task<bool> IsPortOpen(string target, int port, int timeout)
    {
        try
        {
            using var client = new TcpClient();
            var connect = client.ConnectAsync(target, port);
            if (await Task.WhenAny(connect, Task.Delay(timeout)) == connect)
                return client.Connected;
        }
        catch { }
        return false;
    }

    private static string GetServiceName(int p) =>
        p switch
        {
            21 => "FTP",
            22 => "SSH",
            23 => "Telnet",
            25 => "SMTP",
            53 => "DNS",
            80 => "HTTP",
            443 => "HTTPS",
            1433 => "MSSQL",
            3306 => "MySQL",
            3389 => "RDP",
            8080 => "Proxy",
            _ => "Unknown",
        };
}
