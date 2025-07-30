using PuppeteerSharp;

namespace PeluqueriaSaaS.Infrastructure.Services
{
    public interface IBrowserPool
    {
        Task<PooledBrowser> GetBrowserAsync();
        void ReturnBrowser(IBrowser browser);
        Task<bool> IsHealthyAsync();
        void Dispose();
    }
}