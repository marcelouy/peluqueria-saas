using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Domain.Interfaces
{
    public interface IEstadoServicioRepository
    {
        Task<List<EstadoServicio>> GetAllAsync();
        Task<EstadoServicio> GetByIdAsync(int id);
        Task<bool> UpdateVentaDetalleEstadoAsync(int ventaDetalleId, int nuevoEstadoId, int? empleadoId);
        Task<List<VentaDetalle>> GetServiciosPorEmpleadoAsync(int empleadoId);
        Task<List<VentaDetalle>> GetServiciosPorVentaAsync(int ventaId);
    }
}