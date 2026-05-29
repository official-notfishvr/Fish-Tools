using System.Diagnostics;

namespace FishTools.App;

internal sealed class SystemSpoofer : ITool
{
    public string Id => "system-spoofer";
    public string Name => "System Spoofer";
    public string Category => ToolCategories.Security;
    public string Description => "Network interface fixing, DNS flushing, and basic HWID registry spoofing.";

    public Task RunAsync(ToolContext context)
    {
        while (true)
        {
            ConsoleUi.ResetScreen(Name);
            var choice = ConsoleUi.ShowMenu("Choose an action", ["Enable Network Interfaces", "Flush DNS Cache", "List Current HWIDs", "Generate Random HWIDs", "Clean Software Traces", "Back"]);
            if (choice == 5)
            {
                return Task.CompletedTask;
            }

            try
            {
                switch (choice)
                {
                    case 0:
                        FixNetwork();
                        break;
                    case 1:
                        FlushDns();
                        break;
                    case 2:
                        ListHwids();
                        break;
                    case 3:
                        SpoofHwids();
                        break;
                    case 4:
                        CleanTraces();
                        break;
                }
            }
            catch (Exception ex)
            {
                ConsoleUi.Error(ex.Message);
            }

            ConsoleUi.Pause();
        }
    }

    private static void FixNetwork()
    {
        ConsoleUi.Info("Enabling all network adapters...");
        var adapters = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}");
        if (adapters == null)
            return;

        foreach (var adapter in adapters.GetSubKeyNames())
        {
            if (adapter == "Properties")
                continue;
            using var subKey = adapters.OpenSubKey(adapter);
            var instanceId = subKey?.GetValue("NetCfgInstanceId")?.ToString();
            if (string.IsNullOrEmpty(instanceId))
                continue;

            var interfaceName = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(i => i.Id == instanceId)?.Name;
            if (!string.IsNullOrEmpty(interfaceName))
            {
                Process.Start(new ProcessStartInfo("netsh", $"interface set interface \"{interfaceName}\" enable") { CreateNoWindow = true, UseShellExecute = false })?.WaitForExit();
                ConsoleUi.Success($"Checked {interfaceName}");
            }
        }
    }

    private static void FlushDns()
    {
        ConsoleUi.Info("Flushing DNS and refreshing IP...");
        var commands = new[] { "ipconfig /release", "ipconfig /flushdns", "ipconfig /renew", "ipconfig /flushdns" };
        foreach (var cmd in commands)
        {
            var parts = cmd.Split(' ');
            Process.Start(new ProcessStartInfo(parts[0], cmd[parts[0].Length..].Trim()) { CreateNoWindow = true, UseShellExecute = false })?.WaitForExit();
            ConsoleUi.Info(cmd);
        }
        ConsoleUi.Success("DNS cache cleared.");
    }

    private static void ListHwids()
    {
        var keys = GetHwidRegistryKeys();
        foreach (var entry in keys)
        {
            var value = Microsoft.Win32.Registry.GetValue(entry.Path, entry.Key, null)?.ToString()?.Replace("{", "").Replace("}", "");
            if (value != null)
            {
                ConsoleUi.Info($"{entry.Key}: {value}");
            }
        }
    }

    private static void SpoofHwids()
    {
        ConsoleUi.Warning("This operation modifies the Windows Registry. Ensure you have administrator rights.");
        if (!ConsoleUi.Confirm("Apply random HWIDs to the registry?"))
            return;

        var keys = GetHwidRegistryKeys();
        foreach (var entry in keys)
        {
            var newId = GenerateRandomGuid();
            try
            {
                Microsoft.Win32.Registry.SetValue(entry.Path, entry.Key, newId);
                ConsoleUi.Success($"{entry.Key} -> {newId}");
            }
            catch (Exception ex)
            {
                ConsoleUi.Warning($"Failed to update {entry.Key}: {ex.Message}");
            }
        }
    }

    private static void CleanTraces()
    {
        ConsoleUi.Info("Cleaning standard software traces...");
        var paths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Battle.net"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Blizzard Entertainment"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Battle.net"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Battle.net"),
        };

        foreach (var path in paths)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                    ConsoleUi.Success($"Cleaned {path}");
                }
                catch { }
            }
        }
        ConsoleUi.Success("Trace cleanup finished.");
    }

    private static (string Path, string Key)[] GetHwidRegistryKeys() =>
        [
            (@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\IDConfigDB\Hardware Profiles\0001", "HwProfileGuid"),
            (@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography", "MachineGuid"),
            (@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\SQMClient", "MachineId"),
            (@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductID"),
        ];

    private static string GenerateRandomGuid() => $"{{{Guid.NewGuid().ToString().ToUpper()}}}";
}
