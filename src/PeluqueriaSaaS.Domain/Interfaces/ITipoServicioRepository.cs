using PeluqueriaSaaS.Domain.Entities.Configuration;

namespace PeluqueriaSaaS.Domain.Interfaces
{
    public interface ITipoServicioRepository
    {
        Task<IEnumerable<TipoServicio>> GetAllAsync(string tenantId);
        Task<IEnumerable<TipoServicio>> GetActivosAsync(string tenantId);
        Task<TipoServicio?> GetByIdAsync(int id, string tenantId);
        Task<TipoServicio> CreateAsync(TipoServicio tipoServicio);
        Task<TipoServicio> UpdateAsync(TipoServicio tipoServicio);
        Task<bool> DeleteAsync(int id, string tenantId);
        Task<bool> ExisteNombreAsync(string nombre, string tenantId, int? idExcluir = null);
    }
}