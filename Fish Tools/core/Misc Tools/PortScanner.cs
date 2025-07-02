using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.MiscTools
{
    internal class PortScanner : ITool
    {
        public string Name => "Port Scanner";
        public string Category => "Misc Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Scan for open ports on a target host";

        public void Main(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();
            
            Logger.Info("Port Scanner - Network Port Detection Tool");
            Logger.Info("This tool scans for open ports on a target host.");
            
            Console.Write("Enter target hostname or IP address: ");
            string target = Console.ReadLine();
            
            if (string.IsNullOrEmpty(target))
            {
                Logger.Error("Invalid target specified.");
                Logger.Info("Press any key to return to menu...");
                Console.ReadKey();
                return;
            }
            
            Console.Write("Enter start port (default 1): ");
            string startPortStr = Console.ReadLine();
            int startPort = string.IsNullOrEmpty(startPortStr) ? 1 : int.Parse(startPortStr);
            
            Console.Write("Enter end port (default 1024): ");
            string endPortStr = Console.ReadLine();
            int endPort = string.IsNullOrEmpty(endPortStr) ? 1024 : int.Parse(endPortStr);
            
            Console.Write("Enter timeout in milliseconds (default 1000): ");
            string timeoutStr = Console.ReadLine();
            int timeout = string.IsNullOrEmpty(timeoutStr) ? 1000 : int.Parse(timeoutStr);
            
            Logger.Info($"Starting port scan on {target} from port {startPort} to {endPort}...");
            Logger.Info("This may take a while depending on the range...");
            
            ScanPorts(target, startPort, endPort, timeout, Logger);
            
            Logger.Info("Press any key to return to menu...");
            Console.ReadKey();
        }

        private async void ScanPorts(string target, int startPort, int endPort, int timeout, Logger Logger)
        {
            var openPorts = new List<int>();
            var tasks = new List<Task>();
            
            for (int port = startPort; port <= endPort; port++)
            {
                int currentPort = port;
                tasks.Add(Task.Run(async () =>
                {
                    if (await IsPortOpen(target, currentPort, timeout))
                    {
                        lock (openPorts)
                        {
                            openPorts.Add(currentPort);
                        }
                    }
                }));
            }
            
            await Task.WhenAll(tasks);
            
            Logger.Success($"Scan completed! Found {openPorts.Count} open ports:");
            
            if (openPorts.Count > 0)
            {
                openPorts.Sort();
                foreach (int port in openPorts)
                {
                    string service = GetServiceName(port);
                    Logger.Write($"Port {port} ({service}) - OPEN");
                    Console.WriteLine();
                }
            }
            else
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