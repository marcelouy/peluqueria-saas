using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Data;

namespace PeluqueriaSaaS.Infrastructure.Repositories
{
    public class ServicioRepository : IServicioRepository
    {
        private readonly PeluqueriaDbContext _context;

        public ServicioRepository(PeluqueriaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Servicio>> GetAllAsync(string tenantId)
        {
            return await _context.Servicios
                .Include(s => s.TipoServicio)
                .Where(s => s.TenantId == tenantId)
                .OrderBy(s => s.Nombre)
                .ToListAsync();
        }

        public async Task<Servicio?> GetByIdAsync(int id, string tenantId)
        {
            return await _context.Servicios
                .Include(s => s.TipoServicio)
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);
        }

        public async Task<IEnumerable<Servicio>> GetByTipoAsync(int tipoServicioId, string tenantId)
        {
            return await _context.Servicios
                .Include(s => s.TipoServicio)
                .Where(s => s.TipoServicioId == tipoServicioId && s.TenantId == tenantId)
                .OrderBy(s => s.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Servicio>> GetByPrecioRangoAsync(decimal precioMin, decimal precioMax, string tenantId)
        {
            return await _context.Servicios
                .Include(s => s.TipoServicio)
                .Where(s => s.Precio.Valor >= precioMin && s.Precio.Valor <= precioMax && s.TenantId == tenantId)
                .OrderBy(s => s.Precio.Valor)
                .ToListAsync();
        }

        public async Task<bool> ExisteNombreAsync(string nombre, string tenantId, int? excluirId = null)
        {
            var query = _context.Servicios.Where(s => s.TenantId == tenantId && s.Nombre.ToLower() == nombre.ToLower());
            
            if (excluirId.HasValue)
                query = query.Where(s => s.Id != excluirId.Value);
                
            return await query.AnyAsync();
        }

        public async Task<decimal> GetPrecioPromedioAsync(string tenantId)
        {
            var servicios = await _context.Servicios
                .Where(s => s.TenantId == tenantId && s.EsActivo)
                .ToListAsync();
                
            return servicios.Any() ? servicios.Average(s => s.Precio.Valor) : 0;
        }

        public async Task<IEnumerable<Servicio>> GetActivosAsync(string tenantId)
        {
            return await _context.Servicios
                .Include(s => s.TipoServicio)
                .Where(s => s.TenantId == tenantId && s.EsActivo)
                .OrderBy(s => s.Nombre)
                .ToListAsync();
        }

        public async Task CreateAsync(Servicio servicio)
        {
            _context.Servicios.Add(servicio);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Servicio servicio)
        {
            _context.Servicios.Update(servicio);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id, string tenantId)
        {
            var servicio = await GetByIdAsync(id, tenantId);
            if (servicio == null)
                return false;

            _context.Servicios.Remove(servicio);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task CreateWithTenantAsync(Servicio servicio, string tenantId)
        {
            var servicioEntity = _context.Servicios.Add(servicio);
            servicioEntity.Property("TenantId").CurrentValue = tenantId;
            servicioEntity.Property("CreadoPor").CurrentValue = "system";
            servicioEntity.Property("ActualizadoPor").CurrentValue = "system";
            servicioEntity.Property("FechaCreacion").CurrentValue = DateTime.UtcNow;
            servicioEntity.Property("FechaActualizacion").CurrentValue = DateTime.UtcNow;
            servicioEntity.Property("Activo").CurrentValue = true;
            await _context.SaveChangesAsync();
        }
    }
}
