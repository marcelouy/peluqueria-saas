using PuppeteerSharp;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace PeluqueriaSaaS.Infrastructure.Services
{
    public class BrowserPool : IBrowserPool, IDisposable
    {
        private readonly ConcurrentQueue<IBrowser> _browsers = new();
        private readonly SemaphoreSlim _semaphore;
        private readonly ILogger<BrowserPool> _logger;
        private readonly int _maxBrowsers;
        private bool _disposed = false;
        private static bool _chromiumDownloaded = false;
        private static string? _cachedExecutablePath = null;
        private static readonly SemaphoreSlim _downloadSemaphore = new(1, 1);

        public BrowserPool(ILogger<BrowserPool> logger, int maxBrowsers = 3)
        {
            _logger = logger;
            _maxBrowsers = maxBrowsers;
            _semaphore = new SemaphoreSlim(maxBrowsers, maxBrowsers);
        }

        public async Task<PooledBrowser> GetBrowserAsync()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(BrowserPool));
            
            await _semaphore.WaitAsync();
            
            try
            {
                // Try to get existing browser
                if (_browsers.TryDequeue(out var browser))
                {
                    if (browser.IsConnected)
                    {
                        _logger.LogDebug("Reutilizando browser existente del pool");
                        return new PooledBrowser(browser, this);
                    }
                    else
                    {
                        _logger.LogWarning("Browser desconectado encontrado en pool, creando nuevo");
                        await browser.DisposeAsync();
                    }
                }

                // Create new browser
                _logger.LogDebug("Creando nuevo browser");
                var newBrowser = await CreateNewBrowserAsync();
                return new PooledBrowser(newBrowser, this);
            }
            catch (Exception ex)
            {
                _semaphore.Release();
                _logger.LogError(ex, "Error al obtener browser del pool");
                throw;
            }
        }

        public void ReturnBrowser(IBrowser browser)
        {
            if (_disposed || browser == null) 
            {
                _semaphore.Release();
                return;
            }

            try
            {
                if (browser.IsConnected)
                {
                    _browsers.Enqueue(browser);
                    _logger.LogDebug("Browser devuelto al pool");
                }
                else
                {
                    _logger.LogWarning("Browser desconectado no devuelto al pool");
                    _ = Task.Run(async () => await browser.DisposeAsync());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al devolver browser al pool");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                using var testBrowser = await GetBrowserAsync();
                using var page = await testBrowser.NewPageAsync();
                await page.SetContentAsync("<html><body>Health Check</body></html>");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check falló");
                return false;
            }
        }

        private async Task<IBrowser> CreateNewBrowserAsync()
        {
            // ✅ CHROMIUM AUTO-DOWNLOAD - Solo primera vez
            var executablePath = await EnsureChromiumDownloadedAsync();

            var launchOptions = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = executablePath, // ✅ USAR PATH CORRECTO
                Args = new[] 
                { 
                    "--no-sandbox", 
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-gpu",
                    "--no-first-run",
                    "--disable-background-timer-throttling",
                    "--disable-backgrounding-occluded-windows",
                    "--disable-renderer-backgrounding"
                },
                DefaultViewport = new ViewPortOptions
                {
                    Width = 1200,
                    Height = 800
                }
            };

            return await Puppeteer.LaunchAsync(launchOptions);
        }

        private async Task<string> EnsureChromiumDownloadedAsync()
        {
            if (_chromiumDownloaded && !string.IsNullOrEmpty(_cachedExecutablePath)) 
                return _cachedExecutablePath;

            await _downloadSemaphore.WaitAsync();
            try
            {
                if (_chromiumDownloaded && !string.IsNullOrEmpty(_cachedExecutablePath)) 
                    return _cachedExecutablePath;

                _logger.LogInformation("🔽 Verificando Chromium disponibilidad...");
                
                var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
                {
                    Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                                       "PuppeteerSharp")
                });
                
                // ✅ API MODERNA: GetInstalledBrowsers() reemplaza LocalRevisions()
                var installedBrowsers = browserFetcher.GetInstalledBrowsers();
                
                string executablePath;
                
                if (!installedBrowsers.Any())
                {
                    _logger.LogInformation("📥 Descargando Chromium (~100MB) - Primera ejecución...");
                    
                    // ✅ API MODERNA: DownloadAsync() sin parámetros descarga versión recomendada
                    var installedBrowser = await browserFetcher.DownloadAsync();
                    executablePath = installedBrowser.GetExecutablePath();
                    _logger.LogInformation("✅ Chromium descargado: {ExecutablePath}", executablePath);
                }
                else
                {
                    var browser = installedBrowsers.First();
                    executablePath = browser.GetExecutablePath();
                    _logger.LogInformation("✅ Chromium ya disponible: {BuildId} -> {ExecutablePath}", browser.BuildId, executablePath);
                }

                _cachedExecutablePath = executablePath;
                _chromiumDownloaded = true;
                return executablePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error descargando Chromium");
                throw;
            }
            finally
            {
                _downloadSemaphore.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;
            
            // Close all browsers
            while (_browsers.TryDequeue(out var browser))
            {
                try
                {
                    _ = Task.Run(async () => await browser.DisposeAsync());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al cerrar browser durante dispose");
                }
            }
            
            _semaphore?.Dispose();
            _logger.LogInformation("BrowserPool disposed");
        }
    }

    // Simplified wrapper - NO IBrowser inheritance
    public class PooledBrowser : IDisposable
    {
        private readonly IBrowser _browser;
        private readonly IBrowserPool _pool;
        private bool _disposed = false;

        public PooledBrowser(IBrowser browser, IBrowserPool pool)
        {
            _browser = browser ?? throw new ArgumentNullException(nameof(browser));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        }

        // Only expose essential methods needed for PDF generation
        public Task<IPage> NewPageAsync() => _browser.NewPageAsync();
        public Task<IPage[]> PagesAsync() => _browser.PagesAsync();
        public bool IsConnected => _browser.IsConnected;
        public string WebSocketEndpoint => _browser.WebSocketEndpoint;
        public Task CloseAsync() => _browser.CloseAsync();

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _pool.ReturnBrowser(_browser);
        }
    }
}