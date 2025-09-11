using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MediatR;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Application.Features.Clientes.Queries;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;
using System.Linq;
using System.Text.Json;

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
                    .OrderBy(c => c.Nombre == "CLIENTE" && c.Apellido == "OCASIONAL" ? 0 : 1) // Cliente ocasional primero
                    .ThenBy(c => c.Nombre)
                    .ThenBy(c => c.Apellido)
                    .Select(c => new { c.Id, c.Nombre, c.Apellido })
                    .ToListAsync();

                model.Clientes = clientes.Select(c => new ClienteBasicoDto
                {
                    ClienteId = c.Id,
                    Nombre = $"{c.Nombre} {c.Apellido}".Trim()
                }).ToList();

                // ❌ ELIMINADO: Ya no agregamos "Cliente Ocasional (Walk-in)"
                // El cliente ocasional ya existe en la BD con ID=8

                // Cargar servicios
                model.ServiciosAgrupados = await CargarServiciosAgrupados();

                // ✅ MENSAJE INFORMATIVO INICIAL
                if (TempData["Info"] == null && TempData["Warning"] == null && TempData["Success"] == null)
                {
                    TempData["Info"] = "💡 Selecciona empleado (obligatorio), cliente (opcional), agrega servicios y procesa la venta";
                }

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
                // 🔍 DEBUG NIVEL 1: VERIFICACIÓN BÁSICA MODEL
                Console.WriteLine($"=== DEBUG POST VENTA NIVEL 1 ===");
                Console.WriteLine($"POST method called at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"model es null: {model == null}");

                if (model == null)
                {
                    Console.WriteLine($"❌ Model is null - returning to POS");
                    TempData["Error"] = "Error: No se recibieron datos del formulario";
                    return RedirectToAction(nameof(POS));
                }

                Console.WriteLine($"VentaActual es null: {model.VentaActual == null}");

                if (model.VentaActual == null)
                {
                    Console.WriteLine($"❌ VentaActual is null - returning to POS");
                    TempData["Error"] = "Error: Datos de venta no válidos";
                    return RedirectToAction(nameof(POS));
                }

                // 🔍 DEBUG NIVEL 2: VERIFICACIÓN DATOS VENTA
                Console.WriteLine($"=== DEBUG POST VENTA NIVEL 2 ===");
                Console.WriteLine($"VentaActual.EmpleadoId: {model.VentaActual.EmpleadoId}");
                Console.WriteLine($"VentaActual.ClienteId: {model.VentaActual.ClienteId}");
                Console.WriteLine($"VentaActual.Descuento: {model.VentaActual.Descuento}");
                Console.WriteLine($"VentaActual.FechaVenta: {model.VentaActual.FechaVenta}");
                Console.WriteLine($"VentaActual.Observaciones: '{model.VentaActual.Observaciones}'");

                // Verificar Detalles
                Console.WriteLine($"Detalles es null: {model.VentaActual.Detalles == null}");
                Console.WriteLine($"Detalles count: {model.VentaActual.Detalles?.Count ?? 0}");

                // 🔍 DEBUG NIVEL 3: VERIFICACIÓN MODELSTATE
                Console.WriteLine($"=== DEBUG POST VENTA NIVEL 3 ===");
                Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
                Console.WriteLine($"ModelState.ErrorCount: {ModelState.ErrorCount}");

                // ✅ UX IMPROVEMENT: VALIDACIÓN MODELSTATE MEJORADA
                if (!ModelState.IsValid)
                {
                    Console.WriteLine($"❌ VALIDATION FAILED - ModelState invalid");

                    // ✅ MENSAJES ESPECÍFICOS POR ERROR
                    var erroresEspecificos = new List<string>();

                    foreach (var modelError in ModelState)
                    {
                        foreach (var error in modelError.Value.Errors)
                        {
                            erroresEspecificos.Add($"• {error.ErrorMessage}");
                        }
                    }

                    if (erroresEspecificos.Any())
                    {
                        TempData["Warning"] = $"Por favor corrige los siguientes campos:\n{string.Join("\n", erroresEspecificos)}";
                    }
                    else
                    {
                        TempData["Warning"] = "Por favor revisa los datos del formulario";
                    }

                    // ✅ MANTENER DATOS - Recargar vista con modelo actual
                    await CargarDatosPOS(model);
                    return View(model);
                }

                // 🔍 DEBUG NIVEL 4: VERIFICACIÓN DETALLES ESPECÍFICOS
                if (model.VentaActual.Detalles != null)
                {
                    Console.WriteLine($"=== DEBUG POST VENTA NIVEL 4 - DETALLES ===");
                    Console.WriteLine($"Total detalles para procesar: {model.VentaActual.Detalles.Count}");

                    for (int i = 0; i < model.VentaActual.Detalles.Count; i++)
                    {
                        var detalle = model.VentaActual.Detalles[i];
                        Console.WriteLine($"--- Detalle {i + 1} ---");
                        Console.WriteLine($"  ServicioId: {detalle.ServicioId}");
                        Console.WriteLine($"  NombreServicio: '{detalle.NombreServicio ?? "NULL"}'");
                        Console.WriteLine($"  Cantidad: {detalle.Cantidad}");
                        Console.WriteLine($"  PrecioUnitario: {detalle.PrecioUnitario}");
                        Console.WriteLine($"  Subtotal: {detalle.Subtotal}");
                        Console.WriteLine($"  EmpleadoServicioId: {detalle.EmpleadoServicioId?.ToString() ?? "NULL"}");
                        Console.WriteLine($"  NotasServicio: '{detalle.NotasServicio ?? "NULL"}'");
                    }
                    Console.WriteLine($"===========================================");
                }

                // 🔍 DEBUG NIVEL 5: VERIFICACIÓN CÁLCULOS
                Console.WriteLine($"=== DEBUG POST VENTA NIVEL 5 - CÁLCULOS ===");
                Console.WriteLine($"SubTotalCalculado: {model.SubTotalCalculado}");
                Console.WriteLine($"TotalCalculado: {model.TotalCalculado}");
                Console.WriteLine($"Manual SubTotal check: {model.VentaActual.Detalles?.Sum(d => d.Subtotal) ?? 0}");
                Console.WriteLine($"Manual Total check: {(model.VentaActual.Detalles?.Sum(d => d.Subtotal) ?? 0) - model.VentaActual.Descuento}");
                Console.WriteLine($"==========================================");

                // ✅ UX IMPROVEMENT: VALIDACIÓN SERVICIOS MEJORADA
                if (model.VentaActual.Detalles?.Any() != true)
                {
                    Console.WriteLine($"❌ VALIDATION FAILED - No services selected");

                    TempData["Warning"] = "⚠️ Debe agregar al menos un servicio a la venta para poder procesarla. Use el panel de servicios para seleccionar los servicios que el cliente desea.";

                    // ✅ MANTENER DATOS - Recargar vista con modelo actual
                    await CargarDatosPOS(model);
                    return View(model);
                }

                // ✅ UX IMPROVEMENT: VALIDACIÓN EMPLEADO MEJORADA
                if (model.VentaActual.EmpleadoId <= 0)
                {
                    Console.WriteLine($"❌ VALIDATION FAILED - No employee selected");

                    TempData["Warning"] = "⚠️ Debe seleccionar un empleado. El empleado es obligatorio para tracking de comisiones y responsabilidad de la venta.";

                    // ✅ MANTENER DATOS - Recargar vista con modelo actual
                    await CargarDatosPOS(model);
                    return View(model);
                }

                // ✅ CLIENTE OPCIONAL CON MENSAJE INFORMATIVO
                int clienteIdFinal = model.VentaActual.ClienteId;
                if (clienteIdFinal <= 0)
                {
                    clienteIdFinal = await ObtenerClientePorDefectoId();
                    Console.WriteLine($"🏪 Cliente no seleccionado - usando cliente por defecto: {clienteIdFinal}");

                    // ✅ MENSAJE INFORMATIVO (NO ERROR)
                    TempData["Info"] = "ℹ️ No se seleccionó cliente específico. La venta se asignará a 'Cliente Ocasional' automáticamente.";
                }

                // 🔍 DEBUG NIVEL 6: CREACIÓN ENTITY VENTA
                Console.WriteLine($"=== DEBUG POST VENTA NIVEL 6 - ENTITY CREATION ===");

                var venta = new Venta
                {
                    FechaVenta = DateTime.Now,
                    EmpleadoId = model.VentaActual.EmpleadoId,
                    ClienteId = clienteIdFinal,  // ✅ FIXED: Usar clienteIdFinal en lugar de model.VentaActual.ClienteId
                    SubTotal = model.SubTotalCalculado,
                    Descuento = model.VentaActual.Descuento,
                    Total = model.TotalCalculado,
                    EstadoVenta = "Completada",
                    Observaciones = model.VentaActual.Observaciones,
                    TenantId = TENANT_ID,
                    EsActivo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                Console.WriteLine($"Venta entity created:");
                Console.WriteLine($"  FechaVenta: {venta.FechaVenta}");
                Console.WriteLine($"  EmpleadoId: {venta.EmpleadoId}");
                Console.WriteLine($"  ClienteId: {venta.ClienteId} (USING clienteIdFinal)");
                Console.WriteLine($"  SubTotal: {venta.SubTotal}");
                Console.WriteLine($"  Descuento: {venta.Descuento}");
                Console.WriteLine($"  Total: {venta.Total}");
                Console.WriteLine($"  TenantId: '{venta.TenantId}'");
                Console.WriteLine($"  EsActivo: {venta.EsActivo}");

                // 🔍 DEBUG NIVEL 7: CREACIÓN VENTADETALLES
                Console.WriteLine($"=== DEBUG POST VENTA NIVEL 7 - VENTADETALLES ===");

                venta.VentaDetalles = new List<VentaDetalle>();

                foreach (var detalleDto in model.VentaActual.Detalles)
                {
                    Console.WriteLine($"Processing detalle: TipoItem={detalleDto.TipoItem}, ServicioId={detalleDto.ServicioId}, ArticuloId={detalleDto.ArticuloId}");

                    var ventaDetalle = new VentaDetalle
                    {
                        ServicioId = detalleDto.ServicioId > 0 ? detalleDto.ServicioId : 1,
                        TipoItem = detalleDto.TipoItem,
                        ArticuloId = detalleDto.ArticuloId,
                        NombreServicio = !string.IsNullOrEmpty(detalleDto.NombreServicio)
                            ? detalleDto.NombreServicio
                            : $"Item-{detalleDto.ServicioId}",
                        PrecioUnitario = detalleDto.PrecioUnitario,
                        Cantidad = detalleDto.Cantidad,
                        Subtotal = detalleDto.Subtotal,
                        EmpleadoAsignadoId = model.VentaActual.EmpleadoId,
                        EmpleadoServicioId = model.VentaActual.EmpleadoId,
                        EstadoServicioId = 1,
                        NotasServicio = detalleDto.NotasServicio,
                        TenantId = TENANT_ID,
                        FechaCreacion = DateTime.Now
                    };

                    // Si es artículo, verificar y descontar stock
                    if (detalleDto.TipoItem == "ARTICULO" && detalleDto.ArticuloId.HasValue)
                    {
                        var articulo = await _dbContext.Articulos.FindAsync(detalleDto.ArticuloId.Value);
                        if (articulo != null)
                        {
                            if (articulo.Stock < detalleDto.Cantidad)
                            {
                                TempData["Warning"] = $"Stock insuficiente para {articulo.Nombre}. Disponible: {articulo.Stock}";
                                await CargarDatosPOS(model);
                                return View(model);
                            }

                            articulo.Stock -= detalleDto.Cantidad;
                            _dbContext.Articulos.Update(articulo);
                            Console.WriteLine($"Stock descontado: {articulo.Nombre} - Nuevo stock: {articulo.Stock}");
                        }
                    }

                    venta.VentaDetalles.Add(ventaDetalle);
                    Console.WriteLine($"VentaDetalle created - TipoItem: {ventaDetalle.TipoItem}");
                }

                // 🔍 DEBUG NIVEL 8: REPOSITORY SAVE
                Console.WriteLine($"=== DEBUG POST VENTA NIVEL 8 - REPOSITORY SAVE ===");
                Console.WriteLine($"About to call _ventaRepository.CreateAsync...");
                Console.WriteLine($"Repository type: {_ventaRepository.GetType().Name}");

                try
                {
                    Console.WriteLine($"Calling CreateAsync at: {DateTime.Now:HH:mm:ss.fff}");
                    await _ventaRepository.CreateAsync(venta);
                    Console.WriteLine($"CreateAsync completed at: {DateTime.Now:HH:mm:ss.fff}");
                    Console.WriteLine($"✅ VENTA GUARDADA EXITOSAMENTE");
                }
                catch (Exception repositoryEx)
                {
                    Console.WriteLine($"❌ REPOSITORY ERROR:");
                    Console.WriteLine($"  Message: {repositoryEx.Message}");
                    Console.WriteLine($"  StackTrace: {repositoryEx.StackTrace}");

                    if (repositoryEx.InnerException != null)
                    {
                        Console.WriteLine($"  InnerException: {repositoryEx.InnerException.Message}");
                        Console.WriteLine($"  InnerStackTrace: {repositoryEx.InnerException.StackTrace}");
                    }

                    throw; // Re-throw para capturar en outer catch
                }

                Console.WriteLine($"=== VENTA CREATION COMPLETED SUCCESSFULLY ===");
                Console.WriteLine($"Venta ID assigned: {venta.VentaId}");
                Console.WriteLine($"NumeroVenta: {venta.NumeroVenta}");
                Console.WriteLine($"==============================================");

                // ✅ UX IMPROVEMENT: SUCCESS MESSAGE MEJORADO
                var empleadoNombre = "Empleado";
                var clienteNombre = "Cliente";

                try
                {
                    var empleado = await _dbContext.Empleados.FindAsync(venta.EmpleadoId);
                    var cliente = await _dbContext.Clientes.FindAsync(venta.ClienteId);

                    empleadoNombre = empleado?.Nombre ?? "Empleado";
                    clienteNombre = cliente != null ? $"{cliente.Nombre} {cliente.Apellido}".Trim() : "Cliente Ocasional";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error obteniendo nombres para mensaje: {ex.Message}");
                }

                TempData["Success"] = $"✅ Venta #{venta.VentaId} creada exitosamente\n" +
                                     $"💰 Total: ${venta.Total:N0}\n" +
                                     $"👨‍💼 Empleado: {empleadoNombre}\n" +
                                     $"👤 Cliente: {clienteNombre}\n" +
                                     $"📋 Servicios: {venta.VentaDetalles.Count}";

                return RedirectToAction(nameof(POS));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== OUTER EXCEPTION CAUGHT ===");
                Console.WriteLine($"Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"Exception Message: {ex.Message}");
                Console.WriteLine($"Exception StackTrace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner StackTrace: {ex.InnerException.StackTrace}");
                }
                Console.WriteLine($"===============================");

                TempData["Error"] = $"Error al crear la venta: {ex.Message}";
                return RedirectToAction(nameof(POS));
            }
        }

        /// <summary>
        /// Recarga datos necesarios para vista POS manteniendo información del usuario
        /// Usado cuando hay errores de validación para no perder trabajo del usuario
        /// </summary>
        private async Task CargarDatosPOS(PosViewModel model)
        {
            try
            {
                Console.WriteLine($"🔄 Recargando datos POS manteniendo información del usuario...");

                // Verificar que model y VentaActual existan
                if (model == null)
                {
                    Console.WriteLine($"❌ Model es null en CargarDatosPOS");
                    return;
                }

                if (model.VentaActual == null)
                {
                    model.VentaActual = new CreateVentaDto();
                }

                // Recargar empleados para dropdown
                var empleados = await _empleadoRepository.GetAllAsync();
                model.Empleados = empleados.Where(e => e.EsActivo).Select(e => new EmpleadoBasicoDto
                {
                    EmpleadoId = e.Id,
                    Nombre = e.Nombre
                }).ToList();

                Console.WriteLine($"✅ Empleados cargados: {model.Empleados.Count}");

                // Recargar clientes para dropdown - CLIENTE OCASIONAL primero
                var clientes = await _dbContext.Clientes
                    .Where(c => c.EsActivo)
                    .OrderBy(c => c.Nombre == "CLIENTE" && c.Apellido == "OCASIONAL" ? 0 : 1) // Cliente ocasional primero
                    .ThenBy(c => c.Nombre)
                    .ThenBy(c => c.Apellido)
                    .Select(c => new { c.Id, c.Nombre, c.Apellido })
                    .ToListAsync();

                model.Clientes = clientes.Select(c => new ClienteBasicoDto
                {
                    ClienteId = c.Id,
                    Nombre = $"{c.Nombre} {c.Apellido}".Trim()
                }).ToList();

                // ❌ ELIMINADO: Ya no agregamos "Cliente Ocasional (Walk-in)"
                // El cliente ocasional ya existe en la BD con ID=8

                Console.WriteLine($"✅ Clientes cargados: {model.Clientes.Count}");

                // Recargar servicios agrupados
                model.ServiciosAgrupados = await CargarServiciosAgrupados();

                Console.WriteLine($"✅ Servicios agrupados cargados: {model.ServiciosAgrupados.Count} categorías");
                Console.WriteLine($"✅ Servicios seleccionados mantenidos: {model.VentaActual?.Detalles?.Count ?? 0}");

                // Inicializar listas si son null
                model.VentaActual.Detalles ??= new List<CreateVentaDetalleDto>();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error recargando datos POS: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // En caso de error, mantener listas vacías para evitar errores de vista
                model.Empleados ??= new List<EmpleadoBasicoDto>();
                model.Clientes ??= new List<ClienteBasicoDto>();
                model.ServiciosAgrupados ??= new Dictionary<string, List<ServicioBasicoDto>>();

                if (model.VentaActual == null)
                {
                    model.VentaActual = new CreateVentaDto();
                }
                model.VentaActual.Detalles ??= new List<CreateVentaDetalleDto>();
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
                    return Json(new
                    {
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
                    return Json(new
                    {
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
                var ventaData = new
                {
                    Id = venta.VentaId,
                    FechaVenta = venta.FechaVenta,
                    Total = venta.Total,
                    ClienteNombre = cliente != null ? $"{cliente.Nombre} {cliente.Apellido}".Trim() : "Cliente"
                };

                // Generar HTML del resumen
                var resumenHtml = GenerarResumenHTML(settings, ventaData, ventaDetalles, empleado);

                Console.WriteLine($"✅ Resumen generado exitosamente para venta {ventaId}");

                return Json(new
                {
                    success = true,
                    html = resumenHtml,
                    ventaId = ventaId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generando resumen: {ex.Message}");
                return Json(new
                {
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

                var ventaData = new
                {
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

        
        private async Task<int> ObtenerClientePorDefectoId()
        {
            try
            {
                Console.WriteLine($"🔍 Buscando cliente por defecto...");

                // Buscar primer cliente activo del tenant
                var primerCliente = await _dbContext.Clientes
                    .Where(c => c.TenantId == TENANT_ID && c.EsActivo)
                    .OrderBy(c => c.Id)  // Tomar el primero por ID
                    .FirstOrDefaultAsync();

                if (primerCliente != null)
                {
                    Console.WriteLine($"✅ Cliente por defecto encontrado: ID {primerCliente.Id} - {primerCliente.Nombre} {primerCliente.Apellido}");
                    return primerCliente.Id;
                }

                Console.WriteLine($"⚠️ No se encontraron clientes activos - usando fallback ID 1");
                return 1; // Fallback básico
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error obteniendo cliente por defecto: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return 1; // Fallback en caso de error
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

        // Buscar empleados para Select2
        [HttpGet]
        public async Task<IActionResult> BuscarEmpleados(string q = "", int page = 1)
        {
            const int pageSize = 20;

            try
            {
                // Query base
                var query = _dbContext.Empleados
                    .Where(e => e.EsActivo);

                // Aplicar búsqueda si hay término
                if (!string.IsNullOrWhiteSpace(q))
                {
                    q = q.ToLower();
                    query = query.Where(e =>
                        e.Nombre.ToLower().Contains(q) ||
                        e.Apellido.ToLower().Contains(q) ||
                        e.Cargo.ToLower().Contains(q) ||
                        (e.Telefono != null && e.Telefono.Contains(q))
                    );
                }

                // Contar total
                var total = await query.CountAsync();

                // Paginar y obtener resultados
                var empleados = await query
                    .OrderBy(e => e.Nombre)
                    .ThenBy(e => e.Apellido)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => new
                    {
                        empleadoId = e.Id,
                        nombre = e.Nombre,
                        apellido = e.Apellido,
                        cargo = e.Cargo,
                        telefono = e.Telefono,
                        esActivo = e.EsActivo
                    })
                    .ToListAsync();

                return Json(new
                {
                    items = empleados,
                    hasMore = (page * pageSize) < total
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en BuscarEmpleados: {ex.Message}");
                return Json(new { items = new List<object>(), hasMore = false });
            }
        }

        // Buscar clientes para Select2
        [HttpGet]
        public async Task<IActionResult> BuscarClientes(string q = "", int page = 1)
        {
            const int pageSize = 20;

            try
            {
                // Query base - CORREGIDO: _dbContext con C mayúscula
                var query = _dbContext.Clientes
                    .Where(c => c.EsActivo);

                // Aplicar búsqueda si hay término
                if (!string.IsNullOrWhiteSpace(q))
                {
                    q = q.ToLower();
                    query = query.Where(c =>
                        c.Nombre.ToLower().Contains(q) ||
                        c.Apellido.ToLower().Contains(q) ||
                        (c.Telefono != null && c.Telefono.Contains(q)) ||
                        (c.Email != null && c.Email.ToLower().Contains(q))
                    );
                }

                // Contar total
                var total = await query.CountAsync();

                // Obtener clientes con información de visitas - CORREGIDO: _dbContext
                var clientes = await query
                    .OrderBy(c => c.Nombre)
                    .ThenBy(c => c.Apellido)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new
                    {
                        clienteId = c.Id,
                        nombre = c.Nombre,
                        apellido = c.Apellido,
                        telefono = c.Telefono,
                        email = c.Email,
                        totalVisitas = _dbContext.Ventas.Count(v => v.ClienteId == c.Id),
                        ultimaVisita = _dbContext.Ventas
                            .Where(v => v.ClienteId == c.Id)
                            .OrderByDescending(v => v.FechaVenta)
                            .Select(v => v.FechaVenta)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return Json(new
                {
                    items = clientes,
                    hasMore = (page * pageSize) < total
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en BuscarClientes: {ex.Message}");
                return Json(new { items = new List<object>(), hasMore = false });
            }
        }

        // Obtener clientes frecuentes
        [HttpGet]
        public async Task<IActionResult> ClientesFrecuentes()
        {
            try
            {
                var fechaInicio = DateTime.Now.AddMonths(-3); // Últimos 3 meses

                // CORREGIDO: _dbContext con C mayúscula
                var clientesFrecuentes = await _dbContext.Ventas
                    .Where(v => v.FechaVenta >= fechaInicio)
                    .GroupBy(v => new { v.ClienteId, v.Cliente.Nombre, v.Cliente.Apellido })
                    .Select(g => new
                    {
                        clienteId = g.Key.ClienteId,
                        nombre = g.Key.Nombre,
                        apellido = g.Key.Apellido,
                        visitas = g.Count()
                    })
                    .OrderByDescending(c => c.visitas)
                    .Take(10)
                    .ToListAsync();

                return Json(clientesFrecuentes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ClientesFrecuentes: {ex.Message}");
                return Json(new List<object>());
            }
        }

        // Crear cliente rápido desde POS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearClienteRapido(string nombre, string apellido, string telefono, string email)
        {
            try
            {
                // Validación básica
                if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido) || string.IsNullOrWhiteSpace(telefono))
                {
                    return Json(new { success = false, message = "Faltan datos obligatorios" });
                }

                // Verificar si ya existe por teléfono - CORREGIDO: _dbContext
                var existente = await _dbContext.Clientes
                    .FirstOrDefaultAsync(c => c.Telefono == telefono);

                if (existente != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Ya existe un cliente con ese teléfono: {existente.Nombre} {existente.Apellido}"
                    });
                }

                // CORREGIDO: Usar el constructor correcto de Cliente
                var nuevoCliente = new Cliente(
                    nombre: nombre,
                    apellido: apellido,
                    email: string.IsNullOrWhiteSpace(email) ? null : email,
                    telefono: telefono,
                    fechaNacimiento: null
                );

                // SetTenant si es necesario (ya se llama en el constructor con "default-tenant")
                nuevoCliente.SetTenant(TENANT_ID);

                // CORREGIDO: _dbContext con C mayúscula
                _dbContext.Clientes.Add(nuevoCliente);
                await _dbContext.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    clienteId = nuevoCliente.Id,
                    message = "Cliente creado exitosamente"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en CrearClienteRapido: {ex.Message}");
                return Json(new { success = false, message = "Error al crear cliente" });
            }
        }

        // GET: /Ventas/AsignacionRapida
        [HttpGet]
        public async Task<IActionResult> AsignacionRapida()
        {
            try
            {
                // Cargar datos para dropdowns
                ViewBag.Clientes = await _dbContext.Clientes  // _dbContext, no _context
                    .Where(c => c.EsActivo)
                    .OrderBy(c => c.Nombre)
                    .Select(c => new { c.Id, NombreCompleto = c.Nombre + " " + c.Apellido })
                    .ToListAsync();

                ViewBag.Servicios = await _servicioRepository.GetAllAsync(TENANT_ID); // TENANT_ID confirmado

                ViewBag.Empleados = await _dbContext.Empleados
                    .Where(e => e.EsActivo)
                    .OrderBy(e => e.Nombre)
                    .ToListAsync();

                // Detectar empleado actual
                var empleados = ViewBag.Empleados as List<Empleado>;
                ViewBag.EmpleadoActualId = (empleados != null && empleados.Count > 0) ? empleados[0].Id : 0;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en AsignacionRapida: {ex.Message}"); // Console, no _logger
                TempData["Error"] = "Error al cargar la vista de asignación";
                return RedirectToAction("Index");
            }
        }


        // POST: /Ventas/AsignacionRapida
        [HttpPost]
        public async Task<IActionResult> AsignacionRapida(AsignacionRapidaDto model)
        {
            try
            {
                // Crear venta con estado "EnProceso"
                var venta = new Venta
                {
                    ClienteId = model.ClienteId,
                    EmpleadoId = model.Asignaciones.FirstOrDefault()?.EmpleadoAsignadoId ?? 1,
                    FechaVenta = DateTime.Now,
                    EstadoVenta = "EnProceso",
                    TenantId = TENANT_ID,  // TENANT_ID correcto
                    SubTotal = 0,
                    Total = 0,
                    Descuento = 0,
                    VentaDetalles = new List<VentaDetalle>()
                };

                // Agregar cada servicio
                decimal subtotal = 0;
                foreach (var asignacion in model.Asignaciones.Where(a => a.ServicioId > 0))
                {
                    var servicio = await _servicioRepository.GetByIdAsync(asignacion.ServicioId, TENANT_ID);
                    if (servicio != null)
                    {
                        var detalle = new VentaDetalle
                        {
                            ServicioId = servicio.Id,
                            NombreServicio = servicio.Nombre,
                            PrecioUnitario = servicio.Precio.Valor,
                            Cantidad = 1,
                            Subtotal = servicio.Precio.Valor,
                            EmpleadoAsignadoId = asignacion.EmpleadoAsignadoId,
                            EmpleadoServicioId = asignacion.EmpleadoAsignadoId, 
                            EstadoServicioId = 1, // Pendiente
                            TenantId = TENANT_ID,
                            FechaCreacion = DateTime.Now
                        };

                        venta.VentaDetalles.Add(detalle);
                        subtotal += detalle.Subtotal;
                    }
                }

                venta.SubTotal = subtotal;
                venta.Total = subtotal;

                // Guardar
                await _ventaRepository.CreateAsync(venta);

                TempData["Success"] = $"Orden de trabajo creada - {model.Asignaciones.Count} servicios asignados";
                return RedirectToAction("Index", "Estaciones");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creando asignación: {ex.Message}");
                TempData["Error"] = "Error al crear la asignación";
                return View(model);
            }
        }
    }
}