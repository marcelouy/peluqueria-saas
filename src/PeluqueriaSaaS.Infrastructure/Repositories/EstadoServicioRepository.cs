using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Data;

namespace PeluqueriaSaaS.Infrastructure.Repositories
{
    public class EstadoServicioRepository : IEstadoServicioRepository
    {
        private readonly PeluqueriaDbContext _context;

        public EstadoServicioRepository(PeluqueriaDbContext context)
        {
            _context = context;
        }

        public async Task<List<EstadoServicio>> GetAllAsync()
        {
            return await _context.EstadosServicio
                .Where(e => e.Activo)
                .OrderBy(e => e.Orden)
                .ToListAsync();
        }

        public async Task<EstadoServicio> GetByIdAsync(int id)
        {
            return await _context.EstadosServicio
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<bool> UpdateVentaDetalleEstadoAsync(int ventaDetalleId, int nuevoEstadoId, int? empleadoId)
        {
            var detalle = await _context.VentaDetalles
                .FirstOrDefaultAsync(vd => vd.VentaDetalleId == ventaDetalleId);

            if (detalle == null) return false;

            var estadoAnterior = detalle.EstadoServicioId;
            detalle.EstadoServicioId = nuevoEstadoId;

            // Si pasa a En Proceso, registrar inicio
            if (estadoAnterior == 1 && nuevoEstadoId == 2)
            {
                detalle.InicioServicio = DateTime.Now;
                detalle.EmpleadoAsignadoId = empleadoId;
            }

            // Si pasa a Completado, registrar fin
            if (nuevoEstadoId == 3)
            {
                detalle.FinServicio = DateTime.Now;
                detalle.EmpleadoServicioId = empleadoId ?? detalle.EmpleadoAsignadoId;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<VentaDetalle>> GetServiciosPorEmpleadoAsync(int empleadoId)
        {
            return await _context.VentaDetalles
                .Include(vd => vd.Venta)
                .Include(vd => vd.Venta.Cliente)
                .Where(vd => vd.EmpleadoAsignadoId == empleadoId && 
                            vd.EstadoServicioId != 3 && // No completados
                            vd.EstadoServicioId != 4)   // No cancelados
                .OrderBy(vd => vd.VentaId)
                .ToListAsync();
        }

        public async Task<List<VentaDetalle>> GetServiciosPorVentaAsync(int ventaId)
        {
            return await _context.VentaDetalles
                .Where(vd => vd.VentaId == ventaId)
                .OrderBy(vd => vd.VentaDetalleId)
                .ToListAsync();
        }
    }
}