/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Fish_Tools.core.Utils;
using System.Linq;

namespace Fish_Tools.core.MiscTools
{
    internal class PortScanner : ITool
    {
        public string Name => "Port Scanner";
        public string Category => "Network & System Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Scan for open ports on a target host";

        public void Main(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();
            
            Logger.Info("Port Scanner - Network Port Detection Tool");
            Logger.Info("This tool scans for open ports on your local machine.");
            
            string target = GetLocalIPv4();
            if (string.IsNullOrEmpty(target))
            {
                Logger.Error("Could not determine local IP address.");
                Logger.Info("Press any key to return to menu...");
                Console.ReadKey();
                return;
            }
            Logger.Info($"Automatically detected local IP: {target}");
            int startPort = 1;
            int endPort = 1024;
            int timeout = 60;
            Logger.Info($"Starting port scan on {target} from port {startPort} to {endPort}...");
            Logger.Info("This may take a while depending on the range...");
            try
            {
                ScanPorts(target, startPort, endPort, timeout, Logger).GetAwaiter().GetResult();
                Logger.Debug("ScanPorts completed and returned to Main.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Exception during scan: {ex.Message}");
            }
            Logger.Info("Press any key to return to menu...");
            Console.ReadKey();
        }

        private string GetLocalIPv4()
        {
            try
            {
                foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                        ni.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                    {
                        var ipProps = ni.GetIPProperties();
                        foreach (var addr in ipProps.UnicastAddresses)
                        {
                            if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                return addr.Address.ToString();
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        private async Task ScanPorts(string target, int startPort, int endPort, int timeout, Logger Logger)
        {
            var openPorts = new List<int>();
            var closedPorts = new List<int>();
            var portResults = new List<(int port, string result)>();
            var tasks = new List<Task>();
            int totalPorts = endPort - startPort + 1;
            int scannedCount = 0;
            object progressLock = new object();
            for (int port = startPort; port <= endPort; port++)
            {
                int currentPort = port;
                tasks.Add(Task.Run(async () =>
                {
                    bool isOpen = await IsPortOpen(target, currentPort, timeout);
                    string service = GetServiceName(currentPort);
                    lock (progressLock)
                    {
                        if (isOpen)
                        {
                            openPorts.Add(currentPort);
                            portResults.Add((currentPort, $"Port {currentPort} ({service}) - OPEN"));
                            Logger.Write($"Port {currentPort} ({service}) - OPEN");
                        }
                        else
                        {
                            closedPorts.Add(currentPort);
                            portResults.Add((currentPort, $"Port {currentPort} ({service}) - CLOSED"));
                            Logger.Write($"Port {currentPort} ({service}) - CLOSED");
                        }
                        scannedCount++;
                    }
                }));
            }
            await Task.WhenAll(tasks);
            Logger.Debug("All port scan tasks completed.");
            Logger.Write("");
            foreach (var result in portResults.OrderBy(r => r.port))
            {
                Logger.Write(result.result);
            }
            Logger.Success($"Scan completed! Found {openPorts.Count} open ports, {closedPorts.Count} closed ports.");
            if (openPorts.Count == 0)
            {
                Logger.Warn("No open ports found in the specified range.");
            }
        }

        private async Task<bool> IsPortOpen(string target, int port, int timeout)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var connectTask = client.ConnectAsync(target, port);
                    var timeoutTask = Task.Delay(timeout);
                    
                    var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                    
                    if (completedTask == connectTask && client.Connected)
                    {
                        client.Close();
                        return true;
                    }
                }
            }
            catch
            {
            }
            
            return false;
        }

        private string GetServiceName(int port)
        {
            return port switch
            {
                21 => "FTP",
                22 => "SSH",
                23 => "Telnet",
                25 => "SMTP",
                53 => "DNS",
                80 => "HTTP",
                110 => "POP3",
                143 => "IMAP",
                443 => "HTTPS",
                993 => "IMAP SSL",
                995 => "POP3 SSL",
                1433 => "SQL Server",
                3306 => "MySQL",
                3389 => "RDP",
                5432 => "PostgreSQL",
                5900 => "VNC",
                8080 => "HTTP Proxy",
                _ => "Unknown"
            };
        }
    }
} 