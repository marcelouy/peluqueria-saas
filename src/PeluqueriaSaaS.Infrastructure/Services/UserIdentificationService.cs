using System.Threading.Tasks;

namespace PeluqueriaSaaS.Application.Services
{
    public class UserIdentificationService : IUserIdentificationService
    {
        private const string DEFAULT_USER = "María González";
        private const string DEFAULT_TENANT = "TENANT_001";

        public async Task<string> GetCurrentUserIdentifierAsync()
        {
            await Task.CompletedTask;
            return DEFAULT_USER;
        }

        public async Task<string> GetCurrentTenantIdAsync()
        {
            await Task.CompletedTask;
            return DEFAULT_TENANT;
        }

        // ESTE MÉTODO DEBE ESTAR AQUÍ
        public async Task<string> GetTenantIdAsync()
        {
            await Task.CompletedTask;
            return DEFAULT_TENANT;
        }
    }
}