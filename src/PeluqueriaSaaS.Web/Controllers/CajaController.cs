using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeluqueriaSaaS.Web.Controllers
{
    public class CajaController : Controller
    {
        private readonly PeluqueriaDbContext _context;

        public CajaController(PeluqueriaDbContext context)
        {
            _context = context;
        }

        // GET: Caja - Lista de ventas pendientes de cobro
        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener ventas con servicios completados pero no cobradas
                var ventasPendientes = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Empleado)
                    .Include(v => v.VentaDetalles)
                        .ThenInclude(vd => vd.Servicio)
                    .Include(v => v.VentaDetalles)
                        .ThenInclude(vd => vd.EmpleadoServicio)
                    .Where(v => v.EstadoVenta == "Completada" || 
                               (v.EstadoVenta == "EnProceso" && 
                                v.VentaDetalles.All(vd => vd.EstadoServicioId == 3))) // 3 = Completado
                    .OrderBy(v => v.FechaVenta)
                    .ToListAsync();

                // Calcular totales y estadísticas
                ViewBag.TotalPendiente = ventasPendientes.Sum(v => v.Total);
                ViewBag.CantidadPendientes = ventasPendientes.Count;
                
                // Ventas cobradas hoy
                var hoy = DateTime.Today;
                var ventasCobradasHoy = await _context.Ventas
                    .Where(v => v.EstadoVenta == "Pagada" && 
                               v.FechaVenta.Date == hoy)
                    .SumAsync(v => v.Total);
                
                ViewBag.TotalCobradoHoy = ventasCobradasHoy;

                return View(ventasPendientes);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar ventas pendientes: {ex.Message}";
                return View();
            }
        }

        // GET: Caja/Cobrar/5 - Vista detallada para cobrar una venta
        public async Task<IActionResult> Cobrar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Empleado)
                .Include(v => v.VentaDetalles)
                    .ThenInclude(vd => vd.Servicio)
                .Include(v => v.VentaDetalles)
                    .ThenInclude(vd => vd.EmpleadoServicio)
                .FirstOrDefaultAsync(v => v.VentaId == id);

            if (venta == null)
            {
                return NotFound();
            }

            // Preparar ViewBag para métodos de pago
            ViewBag.MetodosPago = new[]
            {
                new { Value = "Efectivo", Text = "Efectivo" },
                new { Value = "Tarjeta", Text = "Tarjeta de Crédito/Débito" },
                new { Value = "Transferencia", Text = "Transferencia Bancaria" },
                new { Value = "Mixto", Text = "Pago Mixto" }
            };

            // Calcular tiempos de servicio y guardar en ViewBag
            var tiemposServicio = new Dictionary<int, string>();
            foreach (var detalle in venta.VentaDetalles)
            {
                if (detalle.InicioServicio.HasValue && detalle.FinServicio.HasValue)
                {
                    var duracion = detalle.FinServicio.Value - detalle.InicioServicio.Value;
                    tiemposServicio[detalle.VentaDetalleId] = $"{(int)duracion.TotalMinutes} min";
                }
            }
            ViewBag.TiemposServicio = tiemposServicio;

            return View(venta);
        }

        // POST: Caja/ProcesarCobro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarCobro(int ventaId, string metodoPago, 
            decimal? descuentoAdicional, string observaciones)
        {
            try
            {
                var venta = await _context.Ventas
                    .Include(v => v.VentaDetalles)
                    .FirstOrDefaultAsync(v => v.VentaId == ventaId);

                if (venta == null)
                {
                    return NotFound();
                }

                // Aplicar descuento adicional si existe
                if (descuentoAdicional.HasValue && descuentoAdicional.Value > 0)
                {
                    venta.Descuento = venta.Descuento + descuentoAdicional.Value;
                    // Recalcular total: suma de detalles menos descuento
                    var subtotal = venta.VentaDetalles.Sum(vd => vd.Subtotal);
                    venta.Total = subtotal - venta.Descuento;
                }

                // Actualizar estado y observaciones
                venta.EstadoVenta = "Pagada";
                venta.Observaciones = $"Método de pago: {metodoPago}. {observaciones}";
                // Guardamos la fecha de pago en Observaciones ya que no existe FechaPago

                // Marcar todos los detalles como completados
                foreach (var detalle in venta.VentaDetalles)
                {
                    if (detalle.EstadoServicioId != 3) // Si no está completado
                    {
                        detalle.EstadoServicioId = 3; // Completado
                        detalle.FinServicio = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Venta #{venta.NumeroVenta} cobrada exitosamente. Total: ${venta.Total:N2}";
                
                // TODO: Aquí se generaría el comprobante/factura
                // return RedirectToAction("GenerarComprobante", new { id = ventaId });
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al procesar cobro: {ex.Message}";
                return RedirectToAction(nameof(Cobrar), new { id = ventaId });
            }
        }

        // GET: Caja/ResumenDiario - Cierre de caja del día
        public async Task<IActionResult> ResumenDiario(DateTime? fecha)
        {
            var fechaConsulta = fecha ?? DateTime.Today;
            
            var ventasDelDia = await _context.Ventas
                .Include(v => v.Empleado)
                .Where(v => v.FechaVenta.Date == fechaConsulta.Date && 
                           v.EstadoVenta == "Pagada")
                .ToListAsync();

            // Como no tenemos MetodoPago en Venta, lo extraemos de Observaciones
            var resumen = new
            {
                Fecha = fechaConsulta,
                TotalVentas = ventasDelDia.Count,
                TotalEfectivo = ventasDelDia
                    .Where(v => v.Observaciones != null && v.Observaciones.Contains("Efectivo"))
                    .Sum(v => v.Total),
                TotalTarjeta = ventasDelDia
                    .Where(v => v.Observaciones != null && v.Observaciones.Contains("Tarjeta"))
                    .Sum(v => v.Total),
                TotalTransferencia = ventasDelDia
                    .Where(v => v.Observaciones != null && v.Observaciones.Contains("Transferencia"))
                    .Sum(v => v.Total),
                TotalGeneral = ventasDelDia.Sum(v => v.Total),
                VentasPorEmpleado = ventasDelDia
                    .GroupBy(v => v.Empleado?.Nombre ?? "Sin Empleado")
                    .Select(g => new { 
                        Empleado = g.Key, 
                        Cantidad = g.Count(), 
                        Total = g.Sum(v => v.Total) 
                    })
                    .ToList()
            };

            ViewBag.Resumen = resumen;
            return View(ventasDelDia);
        }
    }
}