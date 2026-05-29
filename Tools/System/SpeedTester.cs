using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace FishTools.App;

internal sealed class SpeedTester : ITool
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

    public string Id => "speed-tester";
    public string Name => "Network Speed Test";
    public string Category => ToolCategories.SystemHardware;
    public string Description => "Estimate download speed, upload speed, and round-trip latency.";

    public async Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        ConsoleUi.Info("Benchmarking network (Download, Upload, Latency)...");

        var downloadMbps = await MeasureDownloadMbpsAsync();
        var uploadMbps = await MeasureUploadMbpsAsync();
        var latencyMs = await MeasureLatencyAsync();

        ConsoleUi.ResetScreen(Name);
        ConsoleUi.Success($"Download: {downloadMbps:0.00} Mbps");
        ConsoleUi.Success($"Upload: {uploadMbps:0.00} Mbps");
        ConsoleUi.Success($"Latency: {latencyMs:0.0} ms");
        ConsoleUi.Info($"Network Rating: {Rate(downloadMbps, uploadMbps)}");
        ConsoleUi.Pause();
    }

    private static async Task<double> MeasureDownloadMbpsAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        var bytes = await HttpClient.GetByteArrayAsync("https://speed.cloudflare.com/__down?bytes=10000000");
        stopwatch.Stop();
        return (bytes.Length * 8d) / 1_000_000d / stopwatch.Elapsed.TotalSeconds;
    }

    private static async Task<double> MeasureUploadMbpsAsync()
    {
        var payload = RandomNumberGenerator.GetBytes(1_000_000);
        var stopwatch = Stopwatch.StartNew();
        using var response = await HttpClient.PostAsync("https://httpbin.org/post", new ByteArrayContent(payload));
        response.EnsureSuccessStatusCode();
        stopwatch.Stop();
        return (payload.Length * 8d) / 1_000_000d / stopwatch.Elapsed.TotalSeconds;
    }

    private static async Task<double> MeasureLatencyAsync()
    {
        var hosts = new[] { "1.1.1.1", "8.8.8.8", "9.9.9.9" };
        var results = new List<long>();
        foreach (var host in hosts)
        {
            using var p = new Ping();
            var reply = await p.SendPingAsync(host, 3000);
            if (reply.Status == IPStatus.Success)
                results.Add(reply.RoundtripTime);
        }
        return results.Count == 0 ? 0 : results.Average();
    }

    private static string Rate(double dl, double ul)
    {
        var avg = (dl + ul) / 2;
        if (avg >= 100)
            return "Gigabit / High Speed";
        if (avg >= 50)
            return "Excellent Broadband";
        if (avg >= 25)
            return "Standard Broadband";
        if (avg >= 10)
            return "Fair";
        return "Legacy / Slow";
    }
}
