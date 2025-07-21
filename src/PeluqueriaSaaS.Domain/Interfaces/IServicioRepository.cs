using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Domain.Interfaces
{
    public interface IServicioRepository
    {
        Task<IEnumerable<Servicio>> GetAllAsync(string tenantId);
        Task<Servicio?> GetByIdAsync(int id, string tenantId);
        Task<IEnumerable<Servicio>> GetByTipoAsync(int tipoServicioId, string tenantId);
        Task<IEnumerable<Servicio>> GetByPrecioRangoAsync(decimal precioMin, decimal precioMax, string tenantId);
        Task<bool> ExisteNombreAsync(string nombre, string tenantId, int? excluirId = null);
        Task<decimal> GetPrecioPromedioAsync(string tenantId);
        Task<IEnumerable<Servicio>> GetActivosAsync(string tenantId);
        Task CreateAsync(Servicio servicio);
        Task UpdateAsync(Servicio servicio);
        Task<bool> DeleteAsync(int id, string tenantId);
        Task CreateWithTenantAsync(Servicio servicio, string tenantId);
    }
}
