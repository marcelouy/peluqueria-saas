using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Entities.Configuration;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Data;

namespace PeluqueriaSaaS.Infrastructure.Repositories
{
    public class TipoServicioRepository : ITipoServicioRepository
    {
        private readonly PeluqueriaDbContext _context;

        public TipoServicioRepository(PeluqueriaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TipoServicio>> GetAllAsync(string tenantId)
        {
            return await _context.TiposServicio
                .Where(ts => ts.TenantId == tenantId)
                .OrderBy(ts => ts.Orden)
                .ThenBy(ts => ts.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<TipoServicio>> GetActivosAsync(string tenantId)
        {
            return await _context.TiposServicio
                .Where(ts => ts.TenantId == tenantId)
                .OrderBy(ts => ts.Orden)
                .ThenBy(ts => ts.Nombre)
                .ToListAsync();
        }

        public async Task<TipoServicio?> GetByIdAsync(int id, string tenantId)
        {
            return await _context.TiposServicio
                .FirstOrDefaultAsync(ts => ts.Id == id && ts.TenantId == tenantId);
        }

        public async Task<TipoServicio> CreateAsync(TipoServicio tipoServicio)
        {
            _context.TiposServicio.Add(tipoServicio);
            await _context.SaveChangesAsync();
            return tipoServicio;
        }

        public async Task<TipoServicio> UpdateAsync(TipoServicio tipoServicio)
        {
            _context.TiposServicio.Update(tipoServicio);
            await _context.SaveChangesAsync();
            return tipoServicio;
        }

        public async Task<bool> DeleteAsync(int id, string tenantId)
        {
            var tipoServicio = await GetByIdAsync(id, tenantId);
            if (tipoServicio == null) return false;

            _context.TiposServicio.Remove(tipoServicio);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteNombreAsync(string nombre, string tenantId, int? idExcluir = null)
        {
            var query = _context.TiposServicio
                .Where(ts => ts.TenantId == tenantId && 
                           ts.Nombre.ToLower() == nombre.ToLower());

            if (idExcluir.HasValue)
            {
                query = query.Where(ts => ts.Id != idExcluir.Value);
            }

            return await query.AnyAsync();
        }
    }
}
