using System;
using System.Diagnostics;
using System.Management;
using System.Threading;
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.MiscTools
{
    internal class SystemMonitor : ITool
    {
        public string Name => "System Monitor";
        public string Category => "Misc Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Real-time system monitoring with CPU, memory, disk, and network stats";

        private bool isMonitoring = false;
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;

        public void Main(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();
            
            Logger.Info("System Monitor - Real-time System Information");
            Logger.Info("Monitor your system's performance in real-time.");
            
            try
            {
                InitializeCounters();
                
                while (true)
                {
                    Console.WriteLine();
                    Logger.Info("System Monitor Options:");
                    Logger.WriteBarrierLine("1", "Start Real-time Monitoring");
                    Logger.WriteBarrierLine("2", "System Information");
                    Logger.WriteBarrierLine("3", "Process List");
                    Logger.WriteBarrierLine("4", "Network Statistics");
                    Logger.WriteBarrierLine("0", "Back to Menu");
                    
                    Console.Write("Select option: ");
                    var input = Console.ReadKey();
                    Console.WriteLine();
                    
                    switch (input.Key)
                    {
                        case ConsoleKey.D1:
                            StartRealTimeMonitoring(Logger);
                            break;
                        case ConsoleKey.D2:
                            ShowSystemInformation(Logger);
                            break;
                        case ConsoleKey.D3:
                            ShowProcessList(Logger);
                            break;
                        case ConsoleKey.D4:
                            ShowNetworkStatistics(Logger);
                            break;
                        case ConsoleKey.D0:
                            return;
                        default:
                            Logger.Error("Invalid option selected.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error initializing system monitor: {ex.Message}");
                Logger.Info("Press any key to continue...");
                Console.ReadKey();
            }
        }

        private void InitializeCounters()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        }

        private void StartRealTimeMonitoring(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();
            
            Logger.Info("Real-time System Monitoring");
            Logger.Info("Press 'Q' to stop monitoring");
            Console.WriteLine();
            
            isMonitoring = true;
            
            while (isMonitoring)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Q)
                    {
                        isMonitoring = false;
                        break;
                    }
                }
                
                Console.SetCursorPosition(0, 8);
                
                float cpuUsage = cpuCounter.NextValue();
                Logger.Write($"CPU Usage: {cpuUsage:F1}%");
                Console.WriteLine();
                
                float availableMemory = ramCounter.NextValue();
                float totalMemory = GetTotalMemory();
                float usedMemory = totalMemory - availableMemory;
                float memoryUsage = (usedMemory / totalMemory) * 100;
                
                Logger.Write($"Memory Usage: {memoryUsage:F1}% ({usedMemory:F0}MB / {totalMemory:F0}MB)");
                Console.WriteLine();
                
                ShowDiskUsage(Logger);         
                ShowNetworkInfo(Logger);
                
                Thread.Sleep(1000);
            }
            
            Logger.Info("Monitoring stopped.");
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void ShowSystemInformation(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();
            
            Logger.Info("=== System Information ===");
            Console.WriteLine();
            
            Logger.Write($"Operating System: {Environment.OSVersion}");
            Console.WriteLine();
            Logger.Write($"Platform: {Environment.OSVersion.Platform}");
            Console.WriteLine();
            Logger.Write($"Version: {Environment.OSVersion.Version}");
            Console.WriteLine();
            
            Logger.Write($"Machine Name: {Environment.MachineName}");
            Console.WriteLine();
            Logger.Write($"User Name: {Environment.UserName}");
            Console.WriteLine();
            Logger.Write($"User Domain: {Environment.UserDomainName}");
            Console.WriteLine();
            
            Logger.Write($"Processor Count: {Environment.ProcessorCount}");
            Console.WriteLine();
            Logger.Write($"64-bit Process: {Environment.Is64BitProcess}");
            Console.WriteLine();
            Logger.Write($"64-bit OS: {Environment.Is64BitOperatingSystem}");
            Console.WriteLine();
            
            float totalMemory = GetTotalMemory();
            Logger.Write($"Total Physical Memory: {totalMemory:F0} MB");
            Console.WriteLine();
            
            long workingSet = Environment.WorkingSet / (1024 * 1024);
            Logger.Write($"Current Process Memory: {workingSet} MB");
            Console.WriteLine();
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void ShowProcessList(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();
            
            Logger.Info("=== Top Processes by Memory Usage ===");
            Console.WriteLine();
            
            try
            {
                var processes = Process.GetProcesses()
                    .OrderByDescending(p => p.WorkingSet64)
                    .Take(15);
                
                Logger.Write("Process Name".PadRight(25) + "PID".PadRight(8) + "Memory (MB)".PadRight(12) + "CPU Time");
                Console.WriteLine();
                Logger.Write(new string('-', 60));
                Console.WriteLine();
                
                foreach (var process in processes)
                {
                    try
                    {
                        string name = process.ProcessName.Length > 24 ? process.ProcessName.Substring(0, 24) : process.ProcessName;
                        long memoryMB = process.WorkingSet64 / (1024 * 1024);
                        TimeSpan cpuTime = process.TotalProcessorTime;
                        
                        Logger.Write($"{name.PadRight(25)}{process.Id.ToString().PadRight(8)}{memoryMB.ToString().PadRight(12)}{cpuTime:hh\\:mm\\:ss}");
                        Console.WriteLine();
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting process list: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void ShowNetworkStatistics(Logger Logger)
        {
            Console.Clear();
            Logger.PrintArt();
            
            Logger.Info("=== Network Statistics ===");
            Console.WriteLine();
            
            try
            {
                var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                
                foreach (var netInterface in interfaces)
                {
                    if (netInterface.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                    {
                        Logger.Write($"Interface: {netInterface.Name}");
                        Console.WriteLine();
                        Logger.Write($"Type: {netInterface.NetworkInterfaceType}");
                        Console.WriteLine();
                        Logger.Write($"Speed: {netInterface.Speed / 1000000} Mbps");
                        Console.WriteLine();
                        
                        var stats = netInterface.GetIPv4Statistics();
                        Logger.Write($"Bytes Sent: {stats.BytesSent / 1024 / 1024:F1} MB");
                        Console.WriteLine();
                        Logger.Write($"Bytes Received: {stats.BytesReceived / 1024 / 1024:F1} MB");
                        Console.WriteLine();
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting network statistics: {ex.Message}");
            }
            
            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        private void ShowDiskUsage(Logger Logger)
        {
            try
            {
                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives.Where(d => d.IsReady))
                {
                    double freeSpace = drive.TotalFreeSpace / (1024.0 * 1024 * 1024);
                    double totalSpace = drive.TotalSize / (1024.0 * 1024 * 1024);
                    double usedSpace = totalSpace - freeSpace;
                    double usagePercent = (usedSpace / totalSpace) * 100;
                    
                    Logger.Write($"Drive {drive.Name}: {usagePercent:F1}% ({usedSpace:F1}GB / {totalSpace:F1}GB)");
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Logger.Write($"Disk Info Error: {ex.Message}");
                Console.WriteLine();
            }
        }

        private void ShowNetworkInfo(Logger Logger)
        {
            try
            {
                var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                var activeInterface = interfaces.FirstOrDefault(i => i.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up && 
                                                                   i.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback);
                
                if (activeInterface != null)
                {
                    var stats = activeInterface.GetIPv4Statistics();
                    Logger.Write($"Network: {activeInterface.Name} - Sent: {stats.BytesSent / 1024 / 1024:F1}MB, Recv: {stats.BytesReceived / 1024 / 1024:F1}MB");
                    Console.WriteLine();
                }
            }
            catch
            {
                Logger.Write("Network: Unable to retrieve");
                Console.WriteLine();
            }
        }

        private float GetTotalMemory()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return Convert.ToSingle(obj["TotalPhysicalMemory"]) / (1024 * 1024);
                    }
                }
            }
            catch
            {
                return 8192;
            }
            return 8192;
        }
    }
} 