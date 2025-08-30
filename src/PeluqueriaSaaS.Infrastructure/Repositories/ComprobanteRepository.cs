using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Data;

namespace PeluqueriaSaaS.Infrastructure.Repositories
{
    public class ComprobanteRepository : IComprobanteRepository
    {
        private readonly PeluqueriaDbContext _context;

        public ComprobanteRepository(PeluqueriaDbContext context)
        {
            _context = context;
        }

        public async Task<Comprobante?> GetByIdAsync(int id)
        {
            return await _context.Comprobantes
                .Include(c => c.Venta)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
        }

        public async Task<Comprobante?> GetByVentaIdAsync(int ventaId)
        {
            return await _context.Comprobantes
                .Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.VentaId == ventaId && c.Activo);
        }

        public async Task<Comprobante?> GetWithDetallesAsync(int id)
        {
            return await _context.Comprobantes
                .Include(c => c.Detalles)
                .Include(c => c.Venta)
                    .ThenInclude(v => v!.Cliente)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
        }

        public async Task<IEnumerable<Comprobante>> GetByFechaAsync(DateTime fecha)
        {
            var fechaInicio = fecha.Date;
            var fechaFin = fecha.Date.AddDays(1);

            return await _context.Comprobantes
                .Where(c => c.FechaEmision >= fechaInicio &&
                           c.FechaEmision < fechaFin &&
                           c.Activo)
                .OrderByDescending(c => c.Numero)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comprobante>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.Comprobantes
                .Include(c => c.Detalles)
                .Include(c => c.Venta)
                .Where(c => c.Venta != null &&
                           c.Venta.ClienteId == clienteId &&
                           c.Activo)
                .OrderByDescending(c => c.FechaEmision)
                .ToListAsync();
        }

        public async Task<int> GetNextNumberAsync(string serie, string tenantId)
        {
            var ultimoComprobante = await _context.Comprobantes
                .Where(c => c.Serie == serie && c.TenantId == tenantId)
                .OrderByDescending(c => c.Numero)
                .FirstOrDefaultAsync();

            var ultimoNumero = ultimoComprobante?.Numero ?? 0;
            return ultimoNumero + 1;
        }

        public async Task<Comprobante> CreateAsync(Comprobante comprobante)
        {
            // Agregar el comprobante con sus detalles
            _context.Comprobantes.Add(comprobante);

            // SaveChanges asignará el ComprobanteId automáticamente
            await _context.SaveChangesAsync();

            return comprobante;
        }

        public async Task UpdateAsync(Comprobante comprobante)
        {
            // Los campos de auditoría se actualizan automáticamente por EF Core
            // mediante el override de SaveChangesAsync en el DbContext

            _context.Comprobantes.Update(comprobante);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int ventaId)
        {
            return await _context.Comprobantes
                .AnyAsync(c => c.VentaId == ventaId &&
                              c.Estado == "EMITIDO" &&
                              c.Activo);
        }

        public async Task<IEnumerable<Comprobante>> GetRecentAsync(int count = 10)
        {
            return await _context.Comprobantes
                .Include(c => c.Venta)
                    .ThenInclude(v => v!.Cliente)
                .Where(c => c.Activo)
                .OrderByDescending(c => c.FechaEmision)
                .Take(count)
                .ToListAsync();
        }

        public async Task GuardarDetalleAsync(ComprobanteDetalle detalle)
        {
            _context.ComprobanteDetalles.Add(detalle);
            await _context.SaveChangesAsync();
        }

    }
}