using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeluqueriaSaaS.Infrastructure.Repositories
{
    public class VentaRepository : IVentaRepository
    {
        private readonly PeluqueriaDbContext _context;

        public VentaRepository(PeluqueriaDbContext context)
        {
            _context = context;
        }

        // MÉTODO CRÍTICO - ARREGLADO PARA DASHBOARD (Evita ArticuloId2)
        public async Task<IEnumerable<Venta>> GetAllAsync(string tenantId)
        {
            try
            {
                Console.WriteLine($"📊 VentaRepository.GetAllAsync - TenantId: {tenantId}");
                
                // PASO 1: Cargar ventas principales con Cliente y Empleado
                var ventas = await _context.Ventas
                    .AsNoTracking()  // CRÍTICO: Evita tracking y shadow properties
                    .Where(v => v.TenantId == tenantId)
                    .Include(v => v.Cliente)
                    .Include(v => v.Empleado)
                    .OrderByDescending(v => v.FechaVenta)
                    .ToListAsync();

                // PASO 2: Cargar detalles con PROYECCIÓN EXPLÍCITA para evitar ArticuloId2
                foreach (var venta in ventas)
                {
                    // SOLUCIÓN: Proyección explícita para evitar shadow properties
                    var detallesQuery = await _context.VentaDetalles
                        .AsNoTracking()
                        .Where(vd => vd.VentaId == venta.VentaId)
                        .Select(vd => new VentaDetalle
                        {
                            VentaDetalleId = vd.VentaDetalleId,
                            VentaId = vd.VentaId,
                            ServicioId = vd.ServicioId,
                            NombreServicio = vd.NombreServicio,
                            PrecioUnitario = vd.PrecioUnitario,
                            Cantidad = vd.Cantidad,
                            Subtotal = vd.Subtotal,
                            EmpleadoServicioId = vd.EmpleadoServicioId,
                            NotasServicio = vd.NotasServicio,
                            TenantId = vd.TenantId,
                            FechaCreacion = vd.FechaCreacion
                            // NO incluir navegaciones aquí
                        })
                        .ToListAsync();

                    // PASO 3: Cargar servicios por separado si es necesario
                    foreach (var detalle in detallesQuery)
                    {
                        // ServicioId es int, verificar si tiene un servicio válido (ID > 0)
                        if (detalle.ServicioId > 0)
                        {
                            detalle.Servicio = await _context.Servicios
                                .AsNoTracking()
                                .FirstOrDefaultAsync(s => s.Id == detalle.ServicioId);
                        }
                    }

                    venta.VentaDetalles = detallesQuery;
                }

                Console.WriteLine($"✅ VentaRepository.GetAllAsync - {ventas.Count} ventas cargadas");
                return ventas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetAllAsync: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                throw;
            }
        }

        // MÉTODO ALTERNATIVO - Para queries específicas sin detalles
        public async Task<IEnumerable<Venta>> GetAllWithoutDetailsAsync(string tenantId)
        {
            return await _context.Ventas
                .AsNoTracking()
                .Where(v => v.TenantId == tenantId)
                .Include(v => v.Cliente)
                .Include(v => v.Empleado)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        // GetByIdAsync - También corregido con proyección explícita
        public async Task<Venta?> GetByIdAsync(int id, string tenantId)
        {
            try
            {
                Console.WriteLine($"📊 VentaRepository.GetByIdAsync - ID: {id}, TenantId: {tenantId}");
                
                var venta = await _context.Ventas
                    .AsNoTracking()
                    .Include(v => v.Cliente)
                    .Include(v => v.Empleado)
                    .FirstOrDefaultAsync(v => v.VentaId == id && v.TenantId == tenantId);

                if (venta != null)
                {
                    // SOLUCIÓN: Proyección explícita para evitar ArticuloId2
                    var detalles = await _context.VentaDetalles
                        .AsNoTracking()
                        .Where(vd => vd.VentaId == venta.VentaId)
                        .Select(vd => new VentaDetalle
                        {
                            VentaDetalleId = vd.VentaDetalleId,
                            VentaId = vd.VentaId,
                            ServicioId = vd.ServicioId,
                            NombreServicio = vd.NombreServicio,
                            PrecioUnitario = vd.PrecioUnitario,
                            Cantidad = vd.Cantidad,
                            Subtotal = vd.Subtotal,
                            EmpleadoServicioId = vd.EmpleadoServicioId,
                            NotasServicio = vd.NotasServicio,
                            TenantId = vd.TenantId,
                            FechaCreacion = vd.FechaCreacion
                        })
                        .ToListAsync();

                    // Cargar servicios por separado
                    foreach (var detalle in detalles)
                    {
                        if (detalle.ServicioId > 0)
                        {
                            detalle.Servicio = await _context.Servicios
                                .AsNoTracking()
                                .FirstOrDefaultAsync(s => s.Id == detalle.ServicioId);
                        }
                    }

                    venta.VentaDetalles = detalles;
                }

                return venta;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetByIdAsync: {ex.Message}");
                throw;
            }
        }

        // CreateAsync - CON WORKAROUND SQL DIRECTO (del chat anterior)
        public async Task<Venta> CreateAsync(Venta venta)
        {
            try
            {
                Console.WriteLine($"💾 VentaRepository.CreateAsync - Cliente: {venta.ClienteId}, Empleado: {venta.EmpleadoId}");

                // 1. Generar número de venta
                var ultimaVenta = await _context.Ventas
                    .Where(v => v.TenantId == venta.TenantId)
                    .OrderByDescending(v => v.VentaId)
                    .Select(v => v.NumeroVenta)
                    .FirstOrDefaultAsync();

                int siguienteNumero = 1;
                if (!string.IsNullOrEmpty(ultimaVenta) && ultimaVenta.StartsWith("VTA-"))
                {
                    var numeroStr = ultimaVenta.Substring(4);
                    if (int.TryParse(numeroStr, out int numero))
                    {
                        siguienteNumero = numero + 1;
                    }
                }
                
                venta.NumeroVenta = $"VTA-{siguienteNumero:D6}";
                
                // 2. Guardar detalles temporalmente y limpiar
                var detallesTemp = venta.VentaDetalles?.ToList();
                venta.VentaDetalles = new List<VentaDetalle>();
                
                // 3. Guardar venta principal con EF
                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ Venta guardada con ID: {venta.VentaId}, Número: {venta.NumeroVenta}");

                // 4. Guardar detalles con SQL directo (WORKAROUND para bug ArticuloId2)
                if (detallesTemp != null && detallesTemp.Any())
                {
                    var connectionString = _context.Database.GetConnectionString();
                    
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        
                        foreach (var detalle in detallesTemp)
                        {
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = @"
                                    INSERT INTO VentaDetalles 
                                    (VentaId, ServicioId, Cantidad, PrecioUnitario, Subtotal, 
                                     NombreServicio, NotasServicio, EmpleadoServicioId, 
                                     TenantId, FechaCreacion)
                                    VALUES 
                                    (@VentaId, @ServicioId, @Cantidad, @PrecioUnitario, @Subtotal,
                                     @NombreServicio, @NotasServicio, @EmpleadoServicioId,
                                     @TenantId, @FechaCreacion)";
                                
                                command.Parameters.Add(new SqlParameter("@VentaId", venta.VentaId));
                                
                                // CORREGIDO: ServicioId es int, usar 0 si no tiene valor válido
                                command.Parameters.Add(new SqlParameter("@ServicioId", 
                                    detalle.ServicioId > 0 ? detalle.ServicioId : (object)DBNull.Value));
                                
                                command.Parameters.Add(new SqlParameter("@Cantidad", detalle.Cantidad));
                                command.Parameters.Add(new SqlParameter("@PrecioUnitario", detalle.PrecioUnitario));
                                command.Parameters.Add(new SqlParameter("@Subtotal", detalle.Subtotal));
                                command.Parameters.Add(new SqlParameter("@NombreServicio", detalle.NombreServicio ?? ""));
                                command.Parameters.Add(new SqlParameter("@NotasServicio", detalle.NotasServicio ?? (object)DBNull.Value));
                                command.Parameters.Add(new SqlParameter("@EmpleadoServicioId", detalle.EmpleadoServicioId ?? (object)DBNull.Value));
                                command.Parameters.Add(new SqlParameter("@TenantId", venta.TenantId));
                                command.Parameters.Add(new SqlParameter("@FechaCreacion", DateTime.Now));
                                
                                await command.ExecuteNonQueryAsync();
                                Console.WriteLine($"   ✅ Detalle guardado: {detalle.NombreServicio}");
                            }
                        }
                    }
                    
                    venta.VentaDetalles = detallesTemp;
                }
                
                Console.WriteLine($"✅ VentaRepository.CreateAsync completado - ID: {venta.VentaId}");
                return venta;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en CreateAsync: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                throw;
            }
        }

        // UpdateAsync
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
                Console.WriteLine($"❌ Error en UpdateAsync: {ex.Message}");
                throw;
            }
        }

        // DeleteAsync
        public async Task<bool> DeleteAsync(int id, string tenantId)
        {
            try
            {
                var venta = await _context.Ventas
                    .FirstOrDefaultAsync(v => v.VentaId == id && v.TenantId == tenantId);
                
                if (venta != null)
                {
                    _context.Ventas.Remove(venta);
                    await _context.SaveChangesAsync();
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en DeleteAsync: {ex.Message}");
                throw;
            }
        }

        // MÉTODOS PARA DASHBOARD Y REPORTES

        public async Task<decimal> GetVentasDelDiaAsync(string tenantId)
        {
            var hoy = DateTime.Today;
            return await _context.Ventas
                .Where(v => v.TenantId == tenantId && 
                           v.FechaVenta.Date == hoy && 
                           v.EstadoVenta == "Completada")
                .SumAsync(v => v.Total);
        }

        public async Task<decimal> GetVentasDelMesAsync(string tenantId)
        {
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            return await _context.Ventas
                .Where(v => v.TenantId == tenantId && 
                           v.FechaVenta >= inicioMes && 
                           v.EstadoVenta == "Completada")
                .SumAsync(v => v.Total);
        }

        public async Task<int> GetClientesDelMesAsync(string tenantId)
        {
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            return await _context.Ventas
                .Where(v => v.TenantId == tenantId && v.FechaVenta >= inicioMes)
                .Select(v => v.ClienteId)
                .Distinct()
                .CountAsync();
        }

        public async Task<List<object>> GetTopServiciosAsync(string tenantId, int cantidad = 5)
        {
            var topServicios = await _context.VentaDetalles
                .AsNoTracking()  // IMPORTANTE: AsNoTracking para evitar shadow properties
                .Where(vd => vd.TenantId == tenantId && vd.ServicioId > 0)  // CORREGIDO: ServicioId > 0
                .GroupBy(vd => new { vd.ServicioId, vd.NombreServicio })
                .Select(g => new
                {
                    ServicioId = g.Key.ServicioId,
                    Nombre = g.Key.NombreServicio,
                    Cantidad = g.Sum(vd => vd.Cantidad),
                    Total = g.Sum(vd => vd.Subtotal)
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(cantidad)
                .ToListAsync();

            return topServicios.Cast<object>().ToList();
        }

        public async Task<List<Venta>> GetVentasRecientesAsync(string tenantId, int cantidad = 5)
        {
            // Sin Include de VentaDetalles para evitar ArticuloId2
            return await _context.Ventas
                .AsNoTracking()
                .Where(v => v.TenantId == tenantId)
                .Include(v => v.Cliente)
                .Include(v => v.Empleado)
                .OrderByDescending(v => v.FechaVenta)
                .Take(cantidad)
                .ToListAsync();
        }

        // Método auxiliar para verificar existencia
        public async Task<bool> ExistsAsync(int id, string tenantId)
        {
            return await _context.Ventas
                .AnyAsync(v => v.VentaId == id && v.TenantId == tenantId);
        }

        // Método para obtener el siguiente número de venta
        public async Task<string> GetNextVentaNumberAsync(string tenantId)
        {
            var ultimaVenta = await _context.Ventas
                .Where(v => v.TenantId == tenantId)
                .OrderByDescending(v => v.VentaId)
                .Select(v => v.NumeroVenta)
                .FirstOrDefaultAsync();

            int siguienteNumero = 1;
            if (!string.IsNullOrEmpty(ultimaVenta) && ultimaVenta.StartsWith("VTA-"))
            {
                var numeroStr = ultimaVenta.Substring(4);
                if (int.TryParse(numeroStr, out int numero))
                {
                    siguienteNumero = numero + 1;
                }
            }

            return $"VTA-{siguienteNumero:D6}";
        }
    }
}