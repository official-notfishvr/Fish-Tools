using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using FishConsole = Fish.Console.FishConsole;

namespace FishTools.App;

internal sealed class SystemMonitor : ITool
{
    public string Id => "system-monitor";
    public string Name => "System Monitor";
    public string Category => ToolCategories.SystemHardware;
    public string Description => "Display live CPU, RAM, and disk utilization snapshots.";

    public Task RunAsync(ToolContext context)
    {
        while (true)
        {
            ConsoleUi.ResetScreen(Name);
            var choice = ConsoleUi.ShowMenu("Choose an action", ["Live Monitor Dashboard", "Static System Snapshot", "View Top Processes", "Network Interface Data", "Back"]);
            switch (choice)
            {
                case 0:
                    LiveDashboard();
                    break;
                case 1:
                    ShowSnapshot();
                    break;
                case 2:
                    ShowTopProcesses();
                    break;
                case 3:
                    ShowNetworkInterfaces();
                    break;
                default:
                    return Task.CompletedTask;
            }
        }
    }

    private static void LiveDashboard()
    {
        ConsoleUi.ResetScreen("CPU & RAM Monitor");
        ConsoleUi.Info("Press 'Q' to quit.");
        var previous = CpuSnapshot.Capture();

        while (true)
        {
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
            {
                break;
            }

            Thread.Sleep(1000);
            var current = CpuSnapshot.Capture();
            var cpu = CpuSnapshot.GetUsage(previous, current);
            previous = current;

            var memory = MemorySnapshot.Capture();
            var usedPercent = 100 - ((double)memory.AvailablePhysical / memory.TotalPhysical * 100);

            ConsoleUi.ResetScreen("Live Monitor Dashboard");
            ConsoleUi.Info($"CPU Utilization: {cpu:0.0}%");
            ConsoleUi.Info($"RAM Utilization: {usedPercent:0.0}% ({Helpers.FormatBytes((long)(memory.TotalPhysical - memory.AvailablePhysical))} / {Helpers.FormatBytes((long)memory.TotalPhysical)})");

            var systemDrive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.Name.StartsWith(Path.GetPathRoot(Environment.SystemDirectory)!, StringComparison.OrdinalIgnoreCase));
            if (systemDrive is not null)
            {
                var used = systemDrive.TotalSize - systemDrive.AvailableFreeSpace;
                ConsoleUi.Info($"Primary Drive: {Helpers.FormatBytes(used)} used / {Helpers.FormatBytes(systemDrive.TotalSize)} total");
            }

            FishConsole.WriteLine();
            ConsoleUi.Info("Press 'Q' to exit monitoring.");
        }
    }

    private static void ShowSnapshot()
    {
        ConsoleUi.ResetScreen("System Hardware Profile");
        var memory = MemorySnapshot.Capture();
        ConsoleUi.Info($"Hostname: {Environment.MachineName}");
        ConsoleUi.Info($"User: {Environment.UserName}");
        ConsoleUi.Info($"OS: {RuntimeInformation.OSDescription}");
        ConsoleUi.Info($"Architecture: {RuntimeInformation.OSArchitecture}");
        ConsoleUi.Info($"Logic Cores: {Environment.ProcessorCount}");
        ConsoleUi.Info($"Ram Size: {Helpers.FormatBytes((long)memory.TotalPhysical)}");
        FishConsole.WriteLine();

        ConsoleUi.Section("Logical Disk Drives");
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
        {
            var used = drive.TotalSize - drive.AvailableFreeSpace;
            ConsoleUi.Info($"{drive.Name} [{drive.DriveFormat}] {Helpers.FormatBytes(used)} / {Helpers.FormatBytes(drive.TotalSize)}");
        }

        ConsoleUi.Pause();
    }

    private static void ShowTopProcesses()
    {
        ConsoleUi.ResetScreen("Process Explorer (Top 20 RAM)");
        foreach (var process in Process.GetProcesses().OrderByDescending(p => p.WorkingSet64).Take(20))
        {
            try
            {
                ConsoleUi.Info($"{process.ProcessName, -28} #{process.Id, -7} {Helpers.FormatBytes(process.WorkingSet64), 10}");
            }
            catch { }
        }

        ConsoleUi.Pause();
    }

    private static void ShowNetworkInterfaces()
    {
        ConsoleUi.ResetScreen("Active Network Adapters");
        foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up))
        {
            var stats = adapter.GetIPStatistics();
            ConsoleUi.Info($"{adapter.Name} ({adapter.NetworkInterfaceType})");
            ConsoleUi.Info($"  Link Speed: {adapter.Speed / 1_000_000} Mbps");
            ConsoleUi.Info($"  Traffic: TX {Helpers.FormatBytes(stats.BytesSent)} / RX {Helpers.FormatBytes(stats.BytesReceived)}");
            FishConsole.WriteLine();
        }

        ConsoleUi.Pause();
    }

    private readonly record struct CpuSnapshot(ulong Idle, ulong Kernel, ulong User)
    {
        public static CpuSnapshot Capture()
        {
            GetSystemTimes(out var idle, out var kernel, out var user);
            return new CpuSnapshot(ToUInt64(idle), ToUInt64(kernel), ToUInt64(user));
        }

        public static double GetUsage(CpuSnapshot before, CpuSnapshot after)
        {
            var idleCount = after.Idle - before.Idle;
            var kernelCount = after.Kernel - before.Kernel;
            var userCount = after.User - before.User;
            var totalCount = kernelCount + userCount;
            if (totalCount == 0)
                return 0;
            return Math.Clamp((totalCount - idleCount) * 100d / totalCount, 0, 100);
        }
    }

    private readonly record struct MemorySnapshot(ulong TotalPhysical, ulong AvailablePhysical)
    {
        public static MemorySnapshot Capture()
        {
            var status = new MEMORYSTATUSEX();
            GlobalMemoryStatusEx(status);
            return new MemorySnapshot(status.ullTotalPhys, status.ullAvailPhys);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FILETIME
    {
        public uint DwLowDateTime;
        public uint DwHighDateTime;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private sealed class MEMORYSTATUSEX
    {
        public uint dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll")]
    private static extern bool GetSystemTimes(out FILETIME idleTime, out FILETIME kernelTime, out FILETIME userTime);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    private static ulong ToUInt64(FILETIME time) => ((ulong)time.DwHighDateTime << 32) | time.DwLowDateTime;
}
