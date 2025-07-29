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
        private readonly ITipoServicioRepository _tipoServicioRepository;
        private readonly ISettingsRepository _settingsRepository;

        public VentasController
         (
              IVentaRepository ventaRepository,
              IEmpleadoRepository empleadoRepository,
              IMediator mediator,
              IServicioRepository servicioRepository,
              ITipoServicioRepository tipoServicioRepository,
              PeluqueriaSaaS.Infrastructure.Data.PeluqueriaDbContext dbContext,
              ISettingsRepository settingsRepository)
        {
            _ventaRepository = ventaRepository;
            _empleadoRepository = empleadoRepository;
            _mediator = mediator;
            _servicioRepository = servicioRepository;
            _tipoServicioRepository = tipoServicioRepository;
            _settingsRepository = settingsRepository;
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

                // ✅ AGREGAR RESUMEN HABILITADO PARA INDEX
                ViewBag.ResumenHabilitado = await IsResumenHabilitado();

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
                model.ServiciosAgrupados = await CargarServiciosAgrupados();

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

                // ✅ AGREGAR ESTA LÍNEA PARA RESUMEN:
                ViewBag.ResumenHabilitado = await IsResumenHabilitado();

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

        private async Task<Dictionary<string, List<ServicioBasicoDto>>> CargarServiciosAgrupados()
        {
            var servicios = await _servicioRepository.GetAllAsync(TENANT_ID);
            var tipos = await _tipoServicioRepository.GetAllAsync(TENANT_ID);

            return servicios
                .Where(s => s.EsActivo)
                .GroupBy(s => s.TipoServicio?.Nombre ?? "Sin Categoría")
                .ToDictionary(g => g.Key, g => g.Select(s => new ServicioBasicoDto
                {
                    ServicioId = s.Id,
                    Nombre = s.Nombre,
                    Precio = s.Precio.Valor,
                    TipoServicioNombre = g.Key,
                    DuracionMinutos = s.DuracionMinutos
                }).ToList());
        }

        // Generar resumen de servicio para una venta específica
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GenerarResumen(int ventaId)
        {
            try
            {
                Console.WriteLine($"🧾 Generando resumen para venta: {ventaId}");
                
                // Verificar si resumen está habilitado
                var settings = await _settingsRepository.GetSettingsAsync();
                if (settings == null || !settings.ResumenServicioHabilitado)
                {
                    return Json(new { 
                        success = false, 
                        message = "El resumen de servicio no está habilitado. Configure en Settings." 
                    });
                }
                
                // Obtener datos de la venta
                var venta = await _dbContext.Ventas
                    .Where(v => v.VentaId == ventaId && v.TenantId == TENANT_ID)
                    .FirstOrDefaultAsync();
                    
                if (venta == null)
                {
                    return Json(new { 
                        success = false, 
                        message = "Venta no encontrada" 
                    });
                }
                
                // Obtener detalles de la venta
                var ventaDetalles = await _dbContext.VentaDetalles
                    .Where(vd => vd.VentaId == ventaId)
                    .ToListAsync();
                
                // ✅ FIX: EmpleadoId nullable correction
                var empleado = venta.EmpleadoId > 0 ?
                    await _dbContext.Empleados.FindAsync(venta.EmpleadoId) : null;
                
                // Obtener datos cliente
                var cliente = await _dbContext.Clientes
                    .Where(c => c.Id == venta.ClienteId)
                    .Select(c => new { c.Nombre, c.Apellido })
                    .FirstOrDefaultAsync();
                
                // Crear objeto venta para template
                var ventaData = new {
                    Id = venta.VentaId,
                    FechaVenta = venta.FechaVenta,
                    Total = venta.Total,
                    ClienteNombre = cliente != null ? $"{cliente.Nombre} {cliente.Apellido}".Trim() : "Cliente"
                };
                
                // Generar HTML del resumen
                var resumenHtml = GenerarResumenHTML(settings, ventaData, ventaDetalles, empleado);
                
                Console.WriteLine($"✅ Resumen generado exitosamente para venta {ventaId}");
                
                return Json(new { 
                    success = true, 
                    html = resumenHtml,
                    ventaId = ventaId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generando resumen: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = "Error al generar el resumen de servicio" 
                });
            }
        }

        
        /// <summary>
        /// Descargar resumen como PDF
        /// UBICACIÓN: Agregar al final de VentasController.cs antes del último }
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DescargarResumenPDF(int ventaId)
        {
            try
            {
                Console.WriteLine($"📥 Descargando PDF resumen venta: {ventaId}");
                
                // Verificar settings habilitado
                var settings = await _settingsRepository.GetSettingsAsync();
                if (settings == null || !settings.ResumenServicioHabilitado)
                {
                    return BadRequest("Resumen de servicio no habilitado");
                }
                
                // Obtener datos venta
                var venta = await _dbContext.Ventas
                    .Where(v => v.VentaId == ventaId && v.TenantId == TENANT_ID)
                    .FirstOrDefaultAsync();
                    
                if (venta == null)
                {
                    return NotFound("Venta no encontrada");
                }
                
                var ventaDetalles = await _dbContext.VentaDetalles
                    .Where(vd => vd.VentaId == ventaId)
                    .ToListAsync();
                    
                // ✅ FIX: EmpleadoId int type handling (not nullable)  
                var empleado = venta.EmpleadoId > 0 ? 
                    await _dbContext.Empleados.FindAsync(venta.EmpleadoId) : null;
                
                var cliente = await _dbContext.Clientes
                    .Where(c => c.Id == venta.ClienteId)
                    .Select(c => new { c.Nombre, c.Apellido })
                    .FirstOrDefaultAsync();
                
                var ventaData = new {
                    Id = venta.VentaId,
                    FechaVenta = venta.FechaVenta,
                    Total = venta.Total,
                    ClienteNombre = cliente != null ? $"{cliente.Nombre} {cliente.Apellido}".Trim() : "Cliente"
                };
                
                // Generar HTML
                var resumenHtml = GenerarResumenHTML(settings, ventaData, ventaDetalles, empleado);
                
                // Convertir a PDF (implementación básica)
                var pdfBytes = GenerarPDFFromHTML(resumenHtml, $"Resumen_Venta_{ventaId}");
                
                var fileName = $"resumen_venta_{ventaId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                
                Console.WriteLine($"✅ PDF generado: {fileName}");
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generando PDF: {ex.Message}");
                return BadRequest("Error al generar PDF");
            }
        }



        /// <summary>
        /// Verificar si resumen está habilitado (para mostrar/ocultar botón)
        /// </summary>
        public async Task<bool> IsResumenHabilitado()
        {
            try
            {
                var settings = await _settingsRepository.GetSettingsAsync();
                return settings?.ResumenServicioHabilitado ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error verificando resumen habilitado: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generar HTML del resumen con datos reales de la venta
        /// </summary>
        private string GenerarResumenHTML(Settings settings, dynamic venta, dynamic ventaDetalles, dynamic empleado)
        {
            var clienteNombre = venta.ClienteNombre ?? "Cliente";
            var empleadoNombre = empleado?.Nombre ?? "Empleado";
            var fechaVenta = venta.FechaVenta ?? DateTime.Now;
            var totalVenta = venta.Total ?? 0;
            
            // Construir servicios HTML
            var serviciosHtml = "";
            if (settings.MostrarDetalleServicios && ventaDetalles != null)
            {
                var serviciosDetalle = "";
                decimal subtotal = 0;
                
                foreach (var detalle in ventaDetalles)
                {
                    var servicioNombre = detalle.NombreServicio ?? "Servicio";
                    var precio = detalle.PrecioUnitario ?? 0;
                    subtotal += precio;
                    
                    serviciosDetalle += $@"
                        <div style='border-bottom: 1px dotted #ccc; padding: 5px 0; display: flex; justify-content: space-between;'>
                            <span>{servicioNombre}</span>
                            <span>{settings.SimboloMoneda} {precio:N0}</span>
                        </div>";
                }
                
                serviciosHtml = $@"
                    <div style='margin: 15px 0;'>
                        <strong>Servicios Realizados:</strong>
                        <div style='margin-left: 15px; margin-top: 8px;'>
                            {serviciosDetalle}
                        </div>
                    </div>";
            }
            
            // Template principal
            if (settings.UsarTemplateCustom && !string.IsNullOrEmpty(settings.TemplateCustomHTML))
            {
                // Template personalizado con reemplazos
                var customHtml = settings.TemplateCustomHTML
                    .Replace("{{CLIENTE}}", clienteNombre)
                    .Replace("{{EMPLEADO}}", empleadoNombre)
                    .Replace("{{FECHA}}", fechaVenta.ToString("dd/MM/yyyy HH:mm"))
                    .Replace("{{TOTAL}}", $"{settings.SimboloMoneda} {totalVenta:N0}")
                    .Replace("{{SERVICIOS}}", serviciosHtml);
                
                return $@"
                    <div class='resumen-container'>
                        <div class='resumen-custom'>
                            {customHtml}
                        </div>
                    </div>";
            }
            
            // Template estándar con datos reales
            var logoHtml = "";
            if (settings.MostrarLogoEnResumen && !string.IsNullOrEmpty(settings.RutaLogo))
            {
                logoHtml = $@"<img src='{settings.RutaLogo}' alt='Logo' style='max-height: 60px; margin-bottom: 10px;' onerror='this.style.display=""none""'>";
            }
            
            var encabezadoHtml = "";
            if (!string.IsNullOrEmpty(settings.ResumenEncabezado))
            {
                encabezadoHtml = $@"
                    <div class='resumen-mensaje mb-3' style='
                        background-color: #f8f9fa;
                        padding: 10px;
                        border-radius: 5px;
                        border-left: 4px solid {settings.ColorPrimario};
                        font-style: italic;
                        margin-bottom: 15px;
                    '>
                        {settings.ResumenEncabezado}
                    </div>";
            }
            
            var datosClienteHtml = settings.MostrarDatosCliente ? $@"
                <div style='margin-bottom: 10px;'>
                    <strong>Cliente:</strong> <span style='color: {settings.ColorSecundario};'>{clienteNombre}</span>
                </div>" : "";
            
            // ✅ FIX: empleadoHtml variable declared correctly
            var empleadoHtml = settings.MostrarEmpleadoServicio ? $@"
                <div style='margin-bottom: 10px;'>
                    <strong>Atendido por:</strong> <span style='color: {settings.ColorSecundario};'>{empleadoNombre}</span>
                </div>" : "";
            
            var fechaHtml = settings.MostrarFechaHora ? $@"
                <div style='margin-bottom: 10px;'>
                    <strong>Fecha y Hora:</strong> {fechaVenta:dd/MM/yyyy HH:mm}
                </div>" : "";
            
            var totalHtml = settings.MostrarTotalServicio ? $@"
                <div style='
                    margin-top: 15px;
                    padding-top: 10px;
                    border-top: 2px solid {settings.ColorPrimario};
                    text-align: right;
                '>
                    <strong style='font-size: 1.2em; color: {settings.ColorPrimario};'>
                        TOTAL: {settings.SimboloMoneda} {totalVenta:N0}
                    </strong>
                </div>" : "";
            
            var piePaginaHtml = !string.IsNullOrEmpty(settings.ResumenPiePagina) ? $@"
                <div style='
                    margin-top: 20px;
                    padding-top: 15px;
                    border-top: 1px solid #dee2e6;
                    text-align: center;
                    font-size: 0.9em;
                    color: {settings.ColorSecundario};
                '>
                    {settings.ResumenPiePagina}
                </div>" : "";
            
            return $@"
                <div class='resumen-container'>
                    <div class='resumen-print' style='
                        font-family: Arial, sans-serif;
                        font-size: {settings.TamanoFuente};
                        color: #333;
                        max-width: 400px;
                        margin: 0 auto;
                        padding: 20px;
                        border: 2px solid {settings.ColorPrimario};
                        border-radius: 8px;
                        background-color: #fff;
                    '>
                        <!-- ENCABEZADO -->
                        <div style='
                            text-align: center;
                            border-bottom: 2px solid {settings.ColorSecundario};
                            padding-bottom: 15px;
                            margin-bottom: 15px;
                        '>
                            {logoHtml}
                            <h3 style='color: {settings.ColorPrimario}; margin: 0;'>
                                {settings.NombrePeluqueria}
                            </h3>
                            <p style='margin: 5px 0; font-size: 0.9em; color: #666;'>
                                {settings.DireccionPeluqueria}
                            </p>
                            <p style='margin: 5px 0; font-size: 0.9em; color: #666;'>
                                Tel: {settings.TelefonoPeluqueria} | {settings.EmailPeluqueria}
                            </p>
                        </div>

                        {encabezadoHtml}

                        <!-- INFORMACIÓN VENTA -->
                        <div>
                            <h4 style='color: {settings.ColorPrimario}; margin-bottom: 15px; text-align: center;'>
                                RESUMEN DE SERVICIO
                            </h4>
                            <div style='margin-bottom: 10px;'>
                                <strong>Venta #:</strong> {venta.Id}
                            </div>
                            {fechaHtml}
                            {datosClienteHtml}
                            {empleadoHtml}
                            {serviciosHtml}
                            {totalHtml}
                        </div>

                        {piePaginaHtml}

                        <!-- FOOTER -->
                        <div style='
                            margin-top: 15px;
                            padding-top: 10px;
                            border-top: 1px solid #dee2e6;
                            text-align: center;
                            font-size: 0.75em;
                            color: #999;
                        '>
                            <div>Comprobante interno - Sin valor fiscal</div>
                            <div>Generado el {DateTime.Now:dd/MM/yyyy HH:mm}</div>
                        </div>
                    </div>
                </div>";
        }


/// <summary>
        /// Generar PDF desde HTML (implementación básica)
        /// </summary>
        private byte[] GenerarPDFFromHTML(string html, string titulo)
        {
            var htmlCompleto = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>{titulo}</title>
                    <style>
                        body {{ 
                            margin: 0; 
                            padding: 20px; 
                            font-family: Arial, sans-serif; 
                            background: white;
                        }}
                        .resumen-container {{ 
                            background: white; 
                            padding: 0; 
                        }}
                        @media print {{
                            body {{ margin: 0; padding: 10px; }}
                        }}
                    </style>
                </head>
                <body>
                    {html}
                </body>
                </html>";
            
            // Por ahora HTML como PDF (en producción usar iTextSharp o similar)
            return System.Text.Encoding.UTF8.GetBytes(htmlCompleto);
        }
    }
}