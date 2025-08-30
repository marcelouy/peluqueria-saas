using System.Threading.Tasks;

namespace PeluqueriaSaaS.Application.Services
{
    public interface IUserIdentificationService
    {
        Task<string> GetCurrentUserIdentifierAsync();
        Task<string> GetCurrentTenantIdAsync();
        Task<string> GetTenantIdAsync(); // Alias para compatibilidad
    }
}