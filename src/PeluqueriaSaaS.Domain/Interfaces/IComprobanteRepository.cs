using PeluqueriaSaaS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeluqueriaSaaS.Domain.Interfaces
{
    public interface IComprobanteRepository
    {
        Task<Comprobante?> GetByIdAsync(int id);
        Task<Comprobante?> GetByVentaIdAsync(int ventaId);
        Task<Comprobante?> GetWithDetallesAsync(int id);
        Task<int> GetNextNumberAsync(string serie, string tenantId);
        Task<Comprobante> CreateAsync(Comprobante comprobante);
        Task UpdateAsync(Comprobante comprobante);
        Task<bool> ExistsAsync(int ventaId);
        
        // AGREGAR ESTOS MÉTODOS:
        Task GuardarDetalleAsync(ComprobanteDetalle detalle);
        Task<IEnumerable<Comprobante>> GetRecentAsync(int count = 10);
        Task<IEnumerable<Comprobante>> GetByFechaAsync(DateTime fecha);
    }
}