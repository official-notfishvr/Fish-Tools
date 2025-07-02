/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.Utils;
using System.Net.Http;
using System.Diagnostics;

namespace Fish_Tools.core.NetworkTools
{
    internal class NetworkSpeedTest : ITool
    {
        public string Name => "Network Speed Test";
        public string Category => "Network & System Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Test your internet download and upload speeds";

        public void Main(Logger logger)
        {
            logger.Info("Starting Network Speed Test...");
            logger.Info("This tool will test your internet connection speed");

            try
            {
                logger.Info("Testing download speed...");
                var downloadSpeed = TestDownloadSpeed(logger);
                
                logger.Info("Testing upload speed...");
                var uploadSpeed = TestUploadSpeed(logger);

                logger.Success($"Download Speed: {downloadSpeed:F2} Mbps");
                logger.Success($"Upload Speed: {uploadSpeed:F2} Mbps");

                var latency = TestLatency(logger);
                logger.Success($"Latency: {latency:F0} ms");

                string rating = GetSpeedRating(downloadSpeed, uploadSpeed);
                logger.Info($"Speed Rating: {rating}");
            }
            catch (Exception ex)
            {
                logger.Error($"Error during speed test: {ex.Message}");
            }

            logger.Info("Press any key to return to menu...");
            Console.ReadKey();
        }

        private double TestDownloadSpeed(Logger logger)
        {
            var testUrls = new[]
            {
                "https://speed.cloudflare.com/__down?bytes=25000000", // 25MB
                "https://httpbin.org/bytes/10000000", // 10MB
                "https://www.google.com/generate_204"
            };

            double totalSpeed = 0;
            int successfulTests = 0;

            foreach (var url in testUrls)
            {
                try
                {
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(30);

                    var stopwatch = Stopwatch.StartNew();
                    var response = client.GetAsync(url).Result;
                    var content = response.Content.ReadAsByteArrayAsync().Result;
                    stopwatch.Stop();

                    if (response.IsSuccessStatusCode)
                    {
                        double speedMbps = (content.Length * 8.0) / (1024 * 1024) / (stopwatch.ElapsedMilliseconds / 1000.0);
                        totalSpeed += speedMbps;
                        successfulTests++;
                        logger.Debug($"Test {successfulTests}: {speedMbps:F2} Mbps");
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug($"Failed to test {url}: {ex.Message}");
                }
            }

            return successfulTests > 0 ? totalSpeed / successfulTests : 0;
        }

        private double TestUploadSpeed(Logger logger)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                var testData = new byte[1024 * 1024];
                new Random().NextBytes(testData);
                var content = new ByteArrayContent(testData);

                var stopwatch = Stopwatch.StartNew();
                var response = client.PostAsync("https://httpbin.org/post", content).Result;
                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    double speedMbps = (testData.Length * 8.0) / (1024 * 1024) / (stopwatch.ElapsedMilliseconds / 1000.0);
                    return speedMbps;
                }
            }
            catch (Exception ex)
            {
                logger.Debug($"Upload test failed: {ex.Message}");
            }

            return 0;
        }

        private double TestLatency(Logger logger)
        {
            var hosts = new[] { "8.8.8.8", "1.1.1.1", "208.67.222.222" };
            var totalLatency = 0.0;
            var successfulPings = 0;

            foreach (var host in hosts)
            {
                try
                {
                    using var ping = new System.Net.NetworkInformation.Ping();
                    var reply = ping.Send(host, 3000);
                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        totalLatency += reply.RoundtripTime;
                        successfulPings++;
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug($"Ping to {host} failed: {ex.Message}");
                }
            }

            return successfulPings > 0 ? totalLatency / successfulPings : 0;
        }

        private string GetSpeedRating(double downloadSpeed, double uploadSpeed)
        {
            var avgSpeed = (downloadSpeed + uploadSpeed) / 2;

            if (avgSpeed >= 100) return "Excellent (Fiber)";
            if (avgSpeed >= 50) return "Very Good (Cable)";
            if (avgSpeed >= 25) return "Good (DSL)";
            if (avgSpeed >= 10) return "Fair (Basic Broadband)";
            if (avgSpeed >= 5) return "Poor (Slow Connection)";
            return "Very Poor (Dial-up level)";
        }
    }
} 