using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeluqueriaSaaS.Infrastructure.Repositories
{
    /// <summary>
    /// Repository para gestión de ventas POS
    /// FASE A: CRUD básico + queries específicas reportes
    /// </summary>
    public interface IVentaRepository
    {
        // CRUD básico
        Task<IEnumerable<Venta>> GetAllAsync(string tenantId = "default");
        Task<Venta?> GetByIdAsync(int ventaId, string tenantId = "default");
        Task<Venta> CreateAsync(Venta venta);
        Task<Venta> UpdateAsync(Venta venta);
        Task<bool> DeleteAsync(int ventaId, string tenantId = "default");
        
        // Queries específicas POS
        Task<IEnumerable<Venta>> GetVentasByFechaAsync(DateTime fecha, string tenantId = "default");
        Task<IEnumerable<Venta>> GetVentasByRangoFechaAsync(DateTime fechaInicio, DateTime fechaFin, string tenantId = "default");
        Task<IEnumerable<Venta>> GetVentasByEmpleadoAsync(int empleadoId, string tenantId = "default");
        Task<IEnumerable<Venta>> GetVentasByClienteAsync(int clienteId, string tenantId = "default");
        
        // Reportes básicos FASE A
        Task<decimal> GetTotalVentasDiaAsync(DateTime fecha, string tenantId = "default");
        Task<decimal> GetTotalVentasMesAsync(int año, int mes, string tenantId = "default");
        Task<string> GetNextNumeroVentaAsync(string tenantId = "default");
        
        // Con detalles (includes)
        Task<Venta?> GetVentaWithDetallesAsync(int ventaId, string tenantId = "default");
        Task<IEnumerable<Venta>> GetVentasWithDetallesByFechaAsync(DateTime fecha, string tenantId = "default");
    }

    public class VentaRepository : IVentaRepository
    {
        private readonly PeluqueriaDbContext _context;

        public VentaRepository(PeluqueriaDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // CRUD BÁSICO
        // ==========================================

        public async Task<IEnumerable<Venta>> GetAllAsync(string tenantId = "default")
        {
            return await _context.Ventas
                .Where(v => v.TenantId == tenantId && v.EsActivo)
                .AsNoTracking()
                .Select(v => new Venta
                {
                    VentaId = v.VentaId,
                    NumeroVenta = v.NumeroVenta ?? "V-000",
                    FechaVenta = v.FechaVenta,
                    Total = v.Total,
                    SubTotal = v.SubTotal,
                    Descuento = v.Descuento,
                    EstadoVenta = v.EstadoVenta ?? "Completada",
                    EmpleadoId = v.EmpleadoId,
                    ClienteId = v.ClienteId,
                    TenantId = v.TenantId,
                    EsActivo = v.EsActivo,
                    FechaCreacion = v.FechaCreacion,
                    Observaciones = v.Observaciones ?? ""
                })
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }


        public async Task<Venta?> GetByIdAsync(int ventaId, string tenantId = "default")
        {
            return await _context.Ventas
                .Where(v => v.VentaId == ventaId && v.TenantId == tenantId && v.EsActivo)
                .Include(v => v.Empleado)
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync();
        }

        public async Task<Venta> CreateAsync(Venta venta)
        {
            // Asegurar TenantId correcto
            venta.TenantId = "default";
            venta.EsActivo = true;
            venta.FechaCreacion = DateTime.UtcNow;
            
            // Auto-generar número de venta si no existe
            if (string.IsNullOrEmpty(venta.NumeroVenta))
            {
                venta.NumeroVenta = await GetNextNumeroVentaAsync(venta.TenantId);
            }

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();
            
            return venta;
        }

        public async Task<Venta> UpdateAsync(Venta venta)
        {
            // Preservar TenantId
            venta.TenantId = "default";
            
            _context.Entry(venta).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            return venta;
        }

        public async Task<bool> DeleteAsync(int ventaId, string tenantId = "default")
        {
            var venta = await _context.Ventas
                .FirstOrDefaultAsync(v => v.VentaId == ventaId && v.TenantId == tenantId);

            if (venta == null) return false;

            // Soft delete
            venta.EsActivo = false;
            await _context.SaveChangesAsync();
            
            return true;
        }

        // ==========================================
        // QUERIES ESPECÍFICAS POS
        // ==========================================

        public async Task<IEnumerable<Venta>> GetVentasByFechaAsync(DateTime fecha, string tenantId = "default")
        {
            var fechaInicio = fecha.Date;
            var fechaFin = fechaInicio.AddDays(1);

            return await _context.Ventas
                .Where(v => v.TenantId == tenantId && v.EsActivo 
                         && v.FechaVenta >= fechaInicio 
                         && v.FechaVenta < fechaFin)
                .Include(v => v.Empleado)
                .Include(v => v.Cliente)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetVentasByRangoFechaAsync(DateTime fechaInicio, DateTime fechaFin, string tenantId = "default")
        {
            return await _context.Ventas
                .Where(v => v.TenantId == tenantId && v.EsActivo 
                         && v.FechaVenta >= fechaInicio.Date 
                         && v.FechaVenta <= fechaFin.Date.AddDays(1))
                .Include(v => v.Empleado)
                .Include(v => v.Cliente)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetVentasByEmpleadoAsync(int empleadoId, string tenantId = "default")
        {
            return await _context.Ventas
                .Where(v => v.TenantId == tenantId && v.EsActivo && v.EmpleadoId == empleadoId)
                .Include(v => v.Empleado)
                .Include(v => v.Cliente)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetVentasByClienteAsync(int clienteId, string tenantId = "default")
        {
            return await _context.Ventas
                .Where(v => v.TenantId == tenantId && v.EsActivo && v.ClienteId == clienteId)
                .Include(v => v.Empleado)
                .Include(v => v.Cliente)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        // ==========================================
        // REPORTES BÁSICOS FASE A
        // ==========================================

        public async Task<decimal> GetTotalVentasDiaAsync(DateTime fecha, string tenantId = "default")
        {
            var fechaInicio = fecha.Date;
            var fechaFin = fechaInicio.AddDays(1);

            return await _context.Ventas
                .Where(v => v.TenantId == tenantId && v.EsActivo 
                         && v.EstadoVenta == "Completada"
                         && v.FechaVenta >= fechaInicio 
                         && v.FechaVenta < fechaFin)
                .SumAsync(v => v.Total);
        }

        public async Task<decimal> GetTotalVentasMesAsync(int año, int mes, string tenantId = "default")
        {
            var fechaInicio = new DateTime(año, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1);

            return await _context.Ventas
                .Where(v => v.TenantId == tenantId && v.EsActivo 
                         && v.EstadoVenta == "Completada"
                         && v.FechaVenta >= fechaInicio 
                         && v.FechaVenta < fechaFin)
                .SumAsync(v => v.Total);
        }

        public async Task<string> GetNextNumeroVentaAsync(string tenantId = "default")
    {
        try
        {
            // ✅ Query específico solo para NumeroVenta evitando materialización objeto completo
            var ultimoNumero = await _context.Ventas
                .Where(v => v.TenantId == tenantId)
                .OrderByDescending(v => v.VentaId)
                .Select(v => v.NumeroVenta) // ← Solo NumeroVenta, no objeto completo
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(ultimoNumero))
            {
                return "V-001";
            }

            // Extraer número del formato V-XXX
            if (ultimoNumero.StartsWith("V-") && int.TryParse(ultimoNumero.Substring(2), out int numero))
            {
                return $"V-{(numero + 1):D3}";
            }

            // Fallback si formato no reconocido
            var totalVentas = await _context.Ventas.CountAsync(v => v.TenantId == tenantId);
            return $"V-{(totalVentas + 1):D3}";
        }
        catch (Exception ex)
        {
            // Fallback seguro si query falla
            Console.WriteLine($"Error en GetNextNumeroVentaAsync: {ex.Message}");
            var totalVentas = await _context.Ventas.CountAsync(v => v.TenantId == tenantId);
            return $"V-{(totalVentas + 1):D3}";
        }
    }

        // ==========================================
        // CON DETALLES (INCLUDES)
        // ==========================================

        public async Task<Venta?> GetVentaWithDetallesAsync(int ventaId, string tenantId = "default")
        {
            return await _context.Ventas
                .Where(v => v.VentaId == ventaId && v.TenantId == tenantId && v.EsActivo)
                .Include(v => v.Empleado)
                .Include(v => v.Cliente)
                .Include(v => v.VentaDetalles)
                    .ThenInclude(vd => vd.Servicio)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Venta>> GetVentasWithDetallesByFechaAsync(DateTime fecha, string tenantId = "default")
        {
            var fechaInicio = fecha.Date;
            var fechaFin = fechaInicio.AddDays(1);

            return await _context.Ventas
                .Where(v => v.TenantId == tenantId && v.EsActivo 
                         && v.FechaVenta >= fechaInicio 
                         && v.FechaVenta < fechaFin)
                .Include(v => v.Empleado)
                .Include(v => v.Cliente)
                .Include(v => v.VentaDetalles)
                    .ThenInclude(vd => vd.Servicio)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }
    }
}