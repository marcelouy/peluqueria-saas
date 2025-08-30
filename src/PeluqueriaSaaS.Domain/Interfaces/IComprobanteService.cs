using PeluqueriaSaaS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeluqueriaSaaS.Domain.Interfaces
{
    public interface IComprobanteService
    {
        Task<Comprobante> GenerarComprobanteAsync(int ventaId);
        Task<Comprobante?> ObtenerComprobanteAsync(int comprobanteId);
        Task<Comprobante?> ObtenerComprobantePorVentaAsync(int ventaId);
        Task<IEnumerable<Comprobante>> ObtenerComprobantesAsync(DateTime? fechaInicio, DateTime? fechaFin);
        Task<IEnumerable<Comprobante>> ObtenerComprobantesRecientesAsync(int cantidad);
        Task<IEnumerable<Comprobante>> ObtenerComprobantesPorFechaAsync(DateTime fecha);
        Task AnularComprobanteAsync(int comprobanteId, string motivo, string usuario);
        Task<int> ObtenerSiguienteNumeroAsync(string serie);
    }
}