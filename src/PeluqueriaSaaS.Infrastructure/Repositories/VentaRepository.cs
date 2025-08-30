using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace PeluqueriaSaaS.Infrastructure.Repositories
{
    public class VentaRepository : IVentaRepository
    {
        private readonly PeluqueriaDbContext _context;
        private readonly ILogger<VentaRepository> _logger;
        private const string DEFAULT_TENANT_ID = "default";

        public VentaRepository(PeluqueriaDbContext context, ILogger<VentaRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Venta> CreateAsync(Venta venta)
        {
            try
            {
                _logger.LogInformation($"💾 VentaRepository.CreateAsync - Cliente: {venta.ClienteId}, Empleado: {venta.EmpleadoId}");

                // Establecer TenantId si no está establecido
                if (string.IsNullOrEmpty(venta.TenantId))
                {
                    // Usar reflection para establecer TenantId
                    var tenantProp = venta.GetType().GetProperty("TenantId");
                    if (tenantProp != null && tenantProp.CanWrite)
                    {
                        tenantProp.SetValue(venta, DEFAULT_TENANT_ID);
                    }
                }

                // Generar número de venta usando el nuevo método de la interfaz
                var numeroVenta = await GetNextVentaNumberAsync(venta.TenantId ?? DEFAULT_TENANT_ID);
                
                // Usar reflection para establecer NumeroVenta
                var numeroProp = venta.GetType().GetProperty("NumeroVenta");
                if (numeroProp != null && numeroProp.CanWrite)
                {
                    numeroProp.SetValue(venta, numeroVenta);
                }

                // Agregar la venta al contexto
                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Venta guardada con ID: {venta.VentaId}, Número: {venta.NumeroVenta}");

                // Si hay detalles, guardarlos
                if (venta.VentaDetalles != null && venta.VentaDetalles.Any())
                {
                    _logger.LogInformation($"✅ {venta.VentaDetalles.Count} detalles guardados automáticamente");
                }

                return venta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear venta");
                throw;
            }
        }

        public async Task<Venta?> GetByIdAsync(int id)
        {
            return await GetByIdAsync(id, DEFAULT_TENANT_ID);
        }

        public async Task<Venta> GetByIdAsync(int id, string tenantId)
        {
            try
            {
                _logger.LogInformation($"📊 VentaRepository.GetByIdAsync - ID: {id}, TenantId: {tenantId}");

                var venta = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Empleado)
                    .Where(v => v.VentaId == id && v.TenantId == tenantId)
                    .FirstOrDefaultAsync();

                if (venta == null)
                {
                    throw new InvalidOperationException($"Venta con ID {id} no encontrada para tenant {tenantId}");
                }

                // Cargar detalles manualmente
                var detalles = await _context.Set<VentaDetalle>()
                    .Where(vd => vd.VentaId == venta.VentaId)
                    .ToListAsync();

                foreach (var detalle in detalles)
                {
                    // Cargar servicio si ServicioId es válido (mayor a 0)
                    if (detalle.ServicioId > 0)
                    {
                        detalle.Servicio = await _context.Servicios
                            .FirstOrDefaultAsync(s => s.Id == detalle.ServicioId);
                    }
                }

                return venta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al obtener venta con ID: {id}");
                throw;
            }
        }

        public async Task<IEnumerable<Venta>> GetAllAsync()
        {
            return await GetAllAsync(DEFAULT_TENANT_ID);
        }

        public async Task<IEnumerable<Venta>> GetAllAsync(string tenantId)
        {
            try
            {
                return await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Empleado)
                    .Include(v => v.VentaDetalles)
                        .ThenInclude(vd => vd.Servicio)
                    .Where(v => v.TenantId == tenantId)
                    .OrderByDescending(v => v.FechaVenta)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener todas las ventas");
                throw;
            }
        }

        // NUEVO MÉTODO REQUERIDO POR LA INTERFAZ
        public async Task<IEnumerable<Venta>> GetAllWithoutDetailsAsync(string tenantId)
        {
            try
            {
                return await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Empleado)
                    .Where(v => v.TenantId == tenantId)
                    .OrderByDescending(v => v.FechaVenta)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener ventas sin detalles");
                throw;
            }
        }

        public async Task<Venta> UpdateAsync(Venta venta)
        {
            try
            {
                _context.Ventas.Update(venta);
                await _context.SaveChangesAsync();
                return venta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al actualizar venta con ID: {venta.VentaId}");
                throw;
            }
        }

        // MÉTODO ACTUALIZADO CON tenantId
        public async Task<bool> DeleteAsync(int id, string tenantId)
        {
            try
            {
                var venta = await _context.Ventas
                    .FirstOrDefaultAsync(v => v.VentaId == id && v.TenantId == tenantId);
                    
                if (venta == null)
                    return false;

                _context.Ventas.Remove(venta);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al eliminar venta con ID: {id}");
                throw;
            }
        }

        // Sobrecarga para mantener compatibilidad
        public async Task<bool> DeleteAsync(int id)
        {
            return await DeleteAsync(id, DEFAULT_TENANT_ID);
        }

        // NUEVO MÉTODO REQUERIDO POR LA INTERFAZ
        public async Task<bool> ExistsAsync(int id, string tenantId)
        {
            try
            {
                return await _context.Ventas
                    .AnyAsync(v => v.VentaId == id && v.TenantId == tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al verificar existencia de venta {id}");
                throw;
            }
        }

        // MÉTODO CORREGIDO - Retorna decimal, no IEnumerable<Venta>
        public async Task<decimal> GetVentasDelDiaAsync(string tenantId)
        {
            try
            {
                var hoy = DateTime.Today;
                var mañana = hoy.AddDays(1);

                return await _context.Ventas
                    .Where(v => v.TenantId == tenantId &&
                           v.FechaVenta >= hoy && 
                           v.FechaVenta < mañana &&
                           v.EstadoVenta == "Pagada")
                    .SumAsync(v => v.Total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener ventas del día");
                return 0;
            }
        }

        // Mantener el método antiguo para compatibilidad
        public async Task<IEnumerable<Venta>> GetVentasDelDiaAsync(DateTime fecha, string tenantId)
        {
            try
            {
                var fechaInicio = fecha.Date;
                var fechaFin = fechaInicio.AddDays(1);

                string tenantToUse = tenantId ?? DEFAULT_TENANT_ID;

                return await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Empleado)
                    .Include(v => v.VentaDetalles)
                        .ThenInclude(vd => vd.Servicio)
                    .Where(v => v.TenantId == tenantToUse &&
                           v.FechaVenta >= fechaInicio && 
                           v.FechaVenta < fechaFin)
                    .OrderBy(v => v.FechaVenta)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al obtener ventas del día {fecha:yyyy-MM-dd}");
                throw;
            }
        }

        // NUEVO MÉTODO REQUERIDO POR LA INTERFAZ
        public async Task<decimal> GetVentasDelMesAsync(string tenantId)
        {
            try
            {
                var primerDiaMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var primerDiaSiguienteMes = primerDiaMes.AddMonths(1);

                return await _context.Ventas
                    .Where(v => v.TenantId == tenantId &&
                           v.FechaVenta >= primerDiaMes && 
                           v.FechaVenta < primerDiaSiguienteMes &&
                           v.EstadoVenta == "Pagada")
                    .SumAsync(v => v.Total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener ventas del mes");
                return 0;
            }
        }

        // NUEVO MÉTODO REQUERIDO POR LA INTERFAZ
        public async Task<int> GetClientesDelMesAsync(string tenantId)
        {
            try
            {
                var primerDiaMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var primerDiaSiguienteMes = primerDiaMes.AddMonths(1);

                return await _context.Ventas
                    .Where(v => v.TenantId == tenantId &&
                           v.FechaVenta >= primerDiaMes && 
                           v.FechaVenta < primerDiaSiguienteMes)
                    .Select(v => v.ClienteId)
                    .Distinct()
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener clientes del mes");
                return 0;
            }
        }

        // NUEVO MÉTODO REQUERIDO POR LA INTERFAZ
        public async Task<List<object>> GetTopServiciosAsync(string tenantId, int cantidad = 5)
        {
            try
            {
                var topServicios = await _context.VentaDetalles
                    .Where(vd => vd.TenantId == tenantId)
                    .GroupBy(vd => new { vd.ServicioId, vd.NombreServicio })
                    .Select(g => new
                    {
                        ServicioId = g.Key.ServicioId,
                        Nombre = g.Key.NombreServicio ?? "Servicio",
                        Cantidad = g.Count(),
                        Total = g.Sum(vd => vd.Subtotal)
                    })
                    .OrderByDescending(x => x.Cantidad)
                    .Take(cantidad)
                    .ToListAsync();

                // Convertir a List<object> como espera la interfaz
                return topServicios.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener top servicios");
                return new List<object>();
            }
        }

        // NUEVO MÉTODO REQUERIDO POR LA INTERFAZ
        public async Task<List<Venta>> GetVentasRecientesAsync(string tenantId, int cantidad = 5)
        {
            try
            {
                return await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Empleado)
                    .Where(v => v.TenantId == tenantId)
                    .OrderByDescending(v => v.FechaVenta)
                    .Take(cantidad)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener ventas recientes");
                return new List<Venta>();
            }
        }

        // NUEVO MÉTODO REQUERIDO POR LA INTERFAZ
        public async Task<string> GetNextVentaNumberAsync(string tenantId)
        {
            try
            {
                var ultimaVenta = await _context.Ventas
                    .Where(v => v.TenantId == tenantId)
                    .OrderByDescending(v => v.VentaId)
                    .Select(v => v.NumeroVenta)
                    .FirstOrDefaultAsync();

                int siguienteNumero = 1;
                if (!string.IsNullOrEmpty(ultimaVenta) && ultimaVenta.Contains("-"))
                {
                    var partes = ultimaVenta.Split('-');
                    if (partes.Length == 2 && int.TryParse(partes[1], out int numero))
                    {
                        siguienteNumero = numero + 1;
                    }
                }

                return $"VTA-{siguienteNumero:D6}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener siguiente número de venta");
                return "VTA-000001";
            }
        }

        public async Task<IEnumerable<Venta>> GetVentasPendientesAsync()
        {
            return await GetVentasPendientesAsync(DEFAULT_TENANT_ID);
        }

        public async Task<IEnumerable<Venta>> GetVentasPendientesAsync(string tenantId)
        {
            try
            {
                return await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Empleado)
                    .Include(v => v.VentaDetalles)
                    .Where(v => v.TenantId == tenantId && 
                           (v.EstadoVenta == "EnProceso" || v.EstadoVenta == "Pendiente"))
                    .OrderBy(v => v.FechaVenta)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener ventas pendientes");
                throw;
            }
        }

        public async Task<decimal> GetTotalVentasDelDiaAsync(DateTime fecha, string tenantId)
        {
            try
            {
                var fechaInicio = fecha.Date;
                var fechaFin = fechaInicio.AddDays(1);

                return await _context.Ventas
                    .Where(v => v.TenantId == tenantId &&
                           v.FechaVenta >= fechaInicio && 
                           v.FechaVenta < fechaFin &&
                           v.EstadoVenta == "Pagada")
                    .SumAsync(v => v.Total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al obtener total de ventas del día {fecha:yyyy-MM-dd}");
                throw;
            }
        }

        public async Task ActualizarEstadoVentaAsync(int ventaId, string nuevoEstado)
        {
            try
            {
                var venta = await _context.Ventas.FindAsync(ventaId);
                if (venta != null)
                {
                    // Usar reflection para cambiar EstadoVenta
                    var estadoProp = venta.GetType().GetProperty("EstadoVenta");
                    if (estadoProp != null && estadoProp.CanWrite)
                    {
                        estadoProp.SetValue(venta, nuevoEstado);
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al actualizar estado de venta {ventaId} a {nuevoEstado}");
                throw;
            }
        }

        // Método adicional para agregar detalles a una venta existente
        public async Task AgregarDetalleAsync(int ventaId, VentaDetalle detalle)
        {
            try
            {
                // Usar SQL directo para evitar problemas con ServicioId
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = @"
                    INSERT INTO VentaDetalles (
                        VentaId, ServicioId, NombreServicio, PrecioUnitario, 
                        Cantidad, Subtotal, EmpleadoServicioId, EmpleadoAsignadoId,
                        NotasServicio, EstadoServicioId, TenantId, FechaCreacion
                    ) VALUES (
                        @VentaId, @ServicioId, @NombreServicio, @PrecioUnitario,
                        @Cantidad, @Subtotal, @EmpleadoServicioId, @EmpleadoAsignadoId,
                        @NotasServicio, @EstadoServicioId, @TenantId, @FechaCreacion
                    )";

                command.Parameters.Add(new SqlParameter("@VentaId", ventaId));
                command.Parameters.Add(new SqlParameter("@ServicioId", detalle.ServicioId > 0 ? (object)detalle.ServicioId : DBNull.Value));
                command.Parameters.Add(new SqlParameter("@NombreServicio", detalle.NombreServicio ?? ""));
                command.Parameters.Add(new SqlParameter("@PrecioUnitario", detalle.PrecioUnitario));
                command.Parameters.Add(new SqlParameter("@Cantidad", detalle.Cantidad));
                command.Parameters.Add(new SqlParameter("@Subtotal", detalle.Subtotal));
                command.Parameters.Add(new SqlParameter("@EmpleadoServicioId", detalle.EmpleadoServicioId.HasValue ? (object)detalle.EmpleadoServicioId.Value : DBNull.Value));
                command.Parameters.Add(new SqlParameter("@EmpleadoAsignadoId", detalle.EmpleadoAsignadoId.HasValue ? (object)detalle.EmpleadoAsignadoId.Value : DBNull.Value));
                command.Parameters.Add(new SqlParameter("@NotasServicio", detalle.NotasServicio ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@EstadoServicioId", detalle.EstadoServicioId > 0 ? (object)detalle.EstadoServicioId : DBNull.Value));
                command.Parameters.Add(new SqlParameter("@TenantId", detalle.TenantId ?? DEFAULT_TENANT_ID));
                command.Parameters.Add(new SqlParameter("@FechaCreacion", DateTime.Now));

                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();

                _logger.LogInformation($"✅ Detalle agregado a venta {ventaId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al agregar detalle a venta {ventaId}");
                throw;
            }
        }

        // Métodos estadísticos
        public async Task<Dictionary<string, decimal>> GetEstadisticasVentasAsync(DateTime fechaInicio, DateTime fechaFin, string tenantId)
        {
            try
            {
                var ventas = await _context.Ventas
                    .Where(v => v.TenantId == tenantId &&
                           v.FechaVenta >= fechaInicio &&
                           v.FechaVenta <= fechaFin &&
                           v.EstadoVenta == "Pagada")
                    .ToListAsync();

                return new Dictionary<string, decimal>
                {
                    ["TotalVentas"] = ventas.Sum(v => v.Total),
                    ["CantidadVentas"] = ventas.Count,
                    ["PromedioVenta"] = ventas.Any() ? ventas.Average(v => v.Total) : 0,
                    ["MaximaVenta"] = ventas.Any() ? ventas.Max(v => v.Total) : 0,
                    ["MinimaVenta"] = ventas.Any() ? ventas.Min(v => v.Total) : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener estadísticas de ventas");
                throw;
            }
        }
    }
}