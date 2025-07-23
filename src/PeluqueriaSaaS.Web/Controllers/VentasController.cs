using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MediatR;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Application.Features.Clientes.Queries;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Repositories;

namespace PeluqueriaSaaS.Web.Controllers
{
    public class VentasController : Controller
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly IMediator _mediator;
        private readonly IServicioRepository _servicioRepository;
        private readonly PeluqueriaSaaS.Infrastructure.Data.PeluqueriaDbContext _dbContext;
        private const string TENANT_ID = "default";

        public VentasController(
            IVentaRepository ventaRepository,
            IEmpleadoRepository empleadoRepository,
            IMediator mediator,
            IServicioRepository servicioRepository,
            PeluqueriaSaaS.Infrastructure.Data.PeluqueriaDbContext dbContext)
        {
            _ventaRepository = ventaRepository;
            _empleadoRepository = empleadoRepository;
            _mediator = mediator;
            _servicioRepository = servicioRepository;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index(DateTime? fecha = null)
        {
            try
            {
                fecha ??= DateTime.Today;  // Default hoy si no especifica
                
                // ⚡ QUERY CON FILTRO FECHA
                var ventas = await _dbContext.Ventas
                    .Where(v => v.TenantId == TENANT_ID && v.EsActivo 
                             && v.FechaVenta.Date == fecha.Value.Date)  // ← FILTRO FECHA
                    .OrderByDescending(v => v.FechaVenta)
                    .Select(v => new VentaDto
                    {
                        VentaId = v.VentaId,
                        NumeroVenta = v.NumeroVenta ?? $"V-{v.VentaId:000}",
                        FechaVenta = v.FechaVenta,
                        Total = v.Total,
                        EmpleadoId = v.EmpleadoId,
                        ClienteId = v.ClienteId,
                        EstadoVenta = v.EstadoVenta ?? "Completada"
                    })
                    .ToListAsync();

                // ⚡ OBTENER NOMBRES DE TABLA CLIENTES - QUERY ESPECÍFICO COLUMNAS EXISTENTES
                foreach (var venta in ventas)
                {
                    var empleado = await _dbContext.Empleados.FindAsync(venta.EmpleadoId);
                    
                    // ✅ FIX: Query específico solo columnas existentes
                    var cliente = await _dbContext.Clientes
                        .Where(c => c.Id == venta.ClienteId)
                        .Select(c => new { c.Nombre, c.Apellido })
                        .FirstOrDefaultAsync();
                    
                    venta.EmpleadoNombre = empleado?.Nombre ?? "Empleado";
                    venta.ClienteNombre = cliente != null ? $"{cliente.Nombre} {cliente.Apellido}".Trim() : "Cliente";
                }

                // ⚡ CALCULAR RESUMEN DEL DÍA
                var ventasHoy = ventas; // Ya están filtradas por fecha!
                
                ViewBag.CantidadVentas = ventasHoy.Count;
                ViewBag.TotalVentas = ventasHoy.Sum(v => v.Total);
                ViewBag.FechaFiltro = fecha.Value;  // Para mostrar en calendar

                Console.WriteLine($"=== RESUMEN DÍA DEBUG ===");
                Console.WriteLine($"Fecha filtrada: {fecha.Value:yyyy-MM-dd}");
                Console.WriteLine($"Ventas hoy count: {ventasHoy.Count}");
                Console.WriteLine($"Total hoy: ${ventasHoy.Sum(v => v.Total)}");
                Console.WriteLine($"========================");

                return View(ventas);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Index: {ex.Message}");
                TempData["Error"] = $"Error: {ex.Message}";
                return View(new List<VentaDto>());
            }
        }

        public async Task<IActionResult> POS()
        {
            try
            {
                var model = new PosViewModel();
                
                // Cargar empleados
                var empleados = await _empleadoRepository.GetAllAsync();
                model.Empleados = empleados.Where(e => e.EsActivo).Select(e => new EmpleadoBasicoDto
                {
                    EmpleadoId = e.Id,
                    Nombre = e.Nombre
                }).ToList();

                // ⚡ CARGAR CLIENTES DESDE TABLA UNIFICADA CLIENTES
                var clientes = await _dbContext.Clientes
                    .Where(c => c.EsActivo)
                    .Select(c => new { c.Id, c.Nombre, c.Apellido })
                    .ToListAsync();
                    
                model.Clientes = clientes.Select(c => new ClienteBasicoDto
                {
                    ClienteId = c.Id,
                    Nombre = $"{c.Nombre} {c.Apellido}".Trim()
                }).ToList();

                // Cargar servicios
                var servicios = await _servicioRepository.GetAllAsync(TENANT_ID);
                model.Servicios = servicios.Where(s => s.EsActivo).Select(s => new ServicioBasicoDto
                {
                    ServicioId = s.Id,
                    Nombre = s.Nombre,
                    Precio = s.Precio.Valor
                }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en POS GET: {ex.Message}");
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> POS(PosViewModel model)
        {
            try
            {
                // 🔍 DEBUG ESPECÍFICO - Mantener temporalmente
                Console.WriteLine($"=== DEBUG POST VENTA ===");
                Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
                Console.WriteLine($"VentaActual es null: {model.VentaActual == null}");
                Console.WriteLine($"Detalles count: {model.VentaActual?.Detalles?.Count ?? 0}");
                Console.WriteLine($"EmpleadoId: {model.VentaActual?.EmpleadoId}");
                Console.WriteLine($"ClienteId: {model.VentaActual?.ClienteId}");
                
                // Debug ModelState errors
                if (!ModelState.IsValid)
                {
                    foreach (var error in ModelState)
                    {
                        Console.WriteLine($"ModelState Error - Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
                
                // Debug detalles específicos
                if (model.VentaActual?.Detalles != null)
                {
                    for (int i = 0; i < model.VentaActual.Detalles.Count; i++)
                    {
                        var detalle = model.VentaActual.Detalles[i];
                        Console.WriteLine($"Detalle {i}: ServicioId={detalle.ServicioId}, Cantidad={detalle.Cantidad}, Precio={detalle.PrecioUnitario}");
                    }
                }
                
                Console.WriteLine($"========================");

                if (!ModelState.IsValid || !model.VentaActual.Detalles.Any())
                {
                    TempData["Error"] = "Debe agregar servicios a la venta";
                    return RedirectToAction(nameof(POS));
                }

                var venta = new Venta
                {
                    FechaVenta = DateTime.Now,
                    EmpleadoId = model.VentaActual.EmpleadoId,
                    ClienteId = model.VentaActual.ClienteId,
                    SubTotal = model.SubTotalCalculado,
                    Descuento = model.VentaActual.Descuento,
                    Total = model.TotalCalculado,
                    TenantId = TENANT_ID,
                    VentaDetalles = model.VentaActual.Detalles.Select(d => new VentaDetalle
                    {
                        ServicioId = d.ServicioId,
                        NombreServicio = d.NombreServicio,
                        PrecioUnitario = d.PrecioUnitario,
                        Cantidad = d.Cantidad,
                        Subtotal = d.Subtotal,
                        TenantId = TENANT_ID
                    }).ToList()
                };

                // 🔍 DEBUG antes de guardar
                Console.WriteLine($"=== ANTES GUARDAR VENTA ===");
                Console.WriteLine($"Venta Total: {venta.Total}");
                Console.WriteLine($"Detalles count: {venta.VentaDetalles.Count}");
                
                await _ventaRepository.CreateAsync(venta);
                
                Console.WriteLine($"=== VENTA GUARDADA EXITOSAMENTE ===");
                
                TempData["Success"] = "Venta creada exitosamente";
               return RedirectToAction(nameof(POS));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR EN POST VENTA ===");
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(POS));
            }
        }
        
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // ✅ SELECT ESPECÍFICO NULL-SAFE (pattern validado)
                var venta = await _dbContext.Ventas
                    .Where(v => v.VentaId == id && v.TenantId == TENANT_ID && v.EsActivo)
                    .Select(v => new VentaDto
                    {
                        VentaId = v.VentaId,
                        NumeroVenta = v.NumeroVenta ?? $"V-{v.VentaId:000}",
                        FechaVenta = v.FechaVenta,
                        Total = v.Total,
                        SubTotal = v.SubTotal,
                        Descuento = v.Descuento,
                        EmpleadoId = v.EmpleadoId,
                        ClienteId = v.ClienteId,
                        EstadoVenta = v.EstadoVenta ?? "Completada",
                        Observaciones = v.Observaciones ?? ""
                    })
                    .FirstOrDefaultAsync();

                if (venta == null)
                {
                    TempData["Error"] = "Venta no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                // ✅ CARGAR DETALLES SEPARADAMENTE NULL-SAFE
                var ventaDetalles = await _dbContext.VentaDetalles
                    .Where(vd => vd.VentaId == id)
                    .Select(vd => new VentaDetalleDto
                    {
                        VentaDetalleId = vd.VentaDetalleId,
                        ServicioId = vd.ServicioId,
                        NombreServicio = vd.NombreServicio ?? "Servicio",        // ✅ NULL-safe
                        PrecioUnitario = vd.PrecioUnitario,
                        Cantidad = vd.Cantidad,
                        Subtotal = vd.Subtotal,
                        EmpleadoServicioId = vd.EmpleadoServicioId,
                        NotasServicio = vd.NotasServicio ?? ""                   // ✅ NULL-safe
                    })
                    .ToListAsync();

                // ✅ ASIGNAR DETALLES CARGADOS
                venta.Detalles = ventaDetalles;

                // ✅ CARGAR NOMBRES COMO PATTERN VALIDADO
                var empleado = await _dbContext.Empleados.FindAsync(venta.EmpleadoId);
                var cliente = await _dbContext.Clientes
                    .Where(c => c.Id == venta.ClienteId)
                    .Select(c => new { c.Nombre, c.Apellido })
                    .FirstOrDefaultAsync();
                
                venta.EmpleadoNombre = empleado?.Nombre ?? "Empleado";
                venta.ClienteNombre = cliente != null ? $"{cliente.Nombre} {cliente.Apellido}".Trim() : "Cliente";

                return View(venta);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Details: {ex.Message}");
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Reportes(DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? empleadoId = null)
        {
            try
            {
                // Valores por defecto
                fechaDesde ??= new DateTime(2025, 7, 22);     // ✅ Fecha exacta ventas BD
                fechaHasta ??= DateTime.Today;                // Hasta hoy 23/07/2025

                // ⚡ QUERY DIRECTA EVITANDO JOIN PROBLEMÁTICO
                var ventas = await _dbContext.Ventas
                .Where(v => v.TenantId == TENANT_ID &&
                        v.FechaVenta.Date >= fechaDesde.Value.Date &&
                        v.FechaVenta.Date <= fechaHasta.Value.Date)
                .Select(v => new
                {
                    v.VentaId,
                    v.FechaVenta,
                    v.Total,
                    v.EmpleadoId,
                    v.ClienteId,
                    NumeroVenta = v.NumeroVenta ?? $"V-{v.VentaId:000}"
                })
                .ToListAsync();

                // Cargar empleados para dropdown filtros
                var empleados = await _empleadoRepository.GetAllAsync();
                ViewBag.Empleados = new SelectList(empleados.Where(e => e.EsActivo), "Id", "Nombre");

                // Filtrar por empleado si especificado
                if (empleadoId.HasValue)
                    ventas = ventas.Where(v => v.EmpleadoId == empleadoId.Value).ToList();

                var model = new ReporteVentasViewModel
                {
                    FechaDesde = fechaDesde.Value,
                    FechaHasta = fechaHasta.Value,
                    CantidadVentas = ventas.Count,
                    TotalVentas = ventas.Sum(v => v.Total),
                    PromedioVenta = ventas.Any() ? ventas.Average(v => v.Total) : 0,
                    Ventas = ventas.Select(v => new VentaDto
                    {
                        VentaId = v.VentaId,
                        NumeroVenta = v.NumeroVenta ?? $"V-{v.VentaId:000}",
                        FechaVenta = v.FechaVenta,
                        Total = v.Total,
                        EmpleadoId = v.EmpleadoId,        // ← AGREGAR ESTA LÍNEA
                        ClienteId = v.ClienteId,
                        ClienteNombre = "Cliente", // Cargar después si necesario
                        EmpleadoNombre = "Empleado", // Cargar después si necesario
                        EstadoVenta = "Completada"
                    }).ToList()
                };

                foreach (var venta in model.Ventas)
                {
                    var empleado = await _dbContext.Empleados.FindAsync(venta.EmpleadoId);
                    var cliente = await _dbContext.Clientes
                        .Where(c => c.Id == venta.ClienteId)
                        .Select(c => new { c.Nombre, c.Apellido })
                        .FirstOrDefaultAsync();

                    venta.EmpleadoNombre = empleado?.Nombre ?? "Empleado";
                    venta.ClienteNombre = cliente != null ? $"{cliente.Nombre} {cliente.Apellido}".Trim() : "Cliente";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Reportes: {ex.Message}");
                TempData["Error"] = $"Error: {ex.Message}";
                ViewBag.Empleados = new SelectList(new List<object>(), "Id", "Nombre");
                return View(new ReporteVentasViewModel
                {
                    FechaDesde = DateTime.Today.AddMonths(-1),
                    FechaHasta = DateTime.Today,
                    Ventas = new List<VentaDto>()
                });
            }
        }
    }
}