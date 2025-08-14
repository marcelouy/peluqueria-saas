using PeluqueriaSaaS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeluqueriaSaaS.Domain.Interfaces
{
    public interface IVentaRepository
    {
        // CRUD básico
        Task<IEnumerable<Venta>> GetAllAsync(string tenantId);
        Task<IEnumerable<Venta>> GetAllWithoutDetailsAsync(string tenantId);
        Task<Venta> GetByIdAsync(int id, string tenantId);
        Task<Venta> CreateAsync(Venta venta);
        Task<Venta> UpdateAsync(Venta venta);
        Task<bool> DeleteAsync(int id, string tenantId);
        Task<bool> ExistsAsync(int id, string tenantId);

        // Métodos para Dashboard
        Task<decimal> GetVentasDelDiaAsync(string tenantId);
        Task<decimal> GetVentasDelMesAsync(string tenantId);
        Task<int> GetClientesDelMesAsync(string tenantId);
        Task<List<object>> GetTopServiciosAsync(string tenantId, int cantidad = 5);
        Task<List<Venta>> GetVentasRecientesAsync(string tenantId, int cantidad = 5);

        // Utilidades
        Task<string> GetNextVentaNumberAsync(string tenantId);
    }
}