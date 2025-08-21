using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Data;
using PeluqueriaSaaS.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PeluqueriaSaaS.Web.Controllers
{
    public class EstacionesController : Controller
    {
        private readonly PeluqueriaDbContext _context;
        private readonly IEstadoServicioRepository _estadoRepository;
        private readonly IEmpleadoRepository _empleadoRepository;

        public EstacionesController(
            PeluqueriaDbContext context,
            IEstadoServicioRepository estadoRepository,
            IEmpleadoRepository empleadoRepository)
        {
            _context = context;
            _estadoRepository = estadoRepository;
            _empleadoRepository = empleadoRepository;
        }
        
        public IActionResult Test()
        {
            return Content("El controller funciona!");
        }

        // Vista principal de estaciones de trabajo
        public async Task<IActionResult> Index()
        {
            // Cargar VentaDetalles pendientes con TODAS las relaciones necesarias
            var serviciosPendientes = await _context.VentaDetalles
                .Include(vd => vd.Servicio)
                .Include(vd => vd.Venta)
                    .ThenInclude(v => v.Cliente)
                .Include(vd => vd.EmpleadoServicio)  // Por si acaso tiene valor
                .Where(vd => vd.EstadoServicioId != 3 && vd.EstadoServicioId != 4)
                .OrderBy(vd => vd.VentaDetalleId)
                .ToListAsync();

            // Cargar ESTADOS disponibles para el dropdown
            var estadosDisponibles = await _estadoRepository.GetAllAsync();
            ViewBag.EstadosDiccionario = estadosDisponibles.ToDictionary(e => e.Id, e => e);

            // Cargar TODOS los empleados para poder buscar por EmpleadoAsignadoId
            var empleados = await _context.Empleados.Where(e => e.EsActivo).ToListAsync();
            var empleadosDict = empleados.ToDictionary(e => e.Id, e => e);
            ViewBag.EmpleadosDict = empleadosDict;

            return View(serviciosPendientes);
        }

        // Actualizar estado de un servicio
        [HttpPost]
        public async Task<IActionResult> ActualizarEstado(int ventaDetalleId, int nuevoEstadoId, int? empleadoId)
        {
            try
            {
                var ventaDetalle = await _context.VentaDetalles
                    .FirstOrDefaultAsync(vd => vd.VentaDetalleId == ventaDetalleId);

                if (ventaDetalle == null)
                {
                    return Json(new { success = false, message = "Servicio no encontrado" });
                }

                var estadoAnterior = ventaDetalle.EstadoServicioId;
                ventaDetalle.EstadoServicioId = nuevoEstadoId;

                // Si se proporciona empleado, actualizar AMBOS campos
                if (empleadoId.HasValue && empleadoId.Value > 0)
                {
                    ventaDetalle.EmpleadoServicioId = empleadoId.Value;
                    ventaDetalle.EmpleadoAsignadoId = empleadoId.Value;
                }
                // Si no se proporciona pero ya tiene EmpleadoAsignadoId, sincronizar
                else if (ventaDetalle.EmpleadoAsignadoId.HasValue && !ventaDetalle.EmpleadoServicioId.HasValue)
                {
                    ventaDetalle.EmpleadoServicioId = ventaDetalle.EmpleadoAsignadoId.Value;
                }

                // Actualizar timestamps según el nuevo estado
                var ahora = DateTime.Now;
                switch (nuevoEstadoId)
                {
                    case 2: // En Proceso
                        if (!ventaDetalle.InicioServicio.HasValue)
                        {
                            ventaDetalle.InicioServicio = ahora;
                        }
                        break;
                    case 3: // Completado
                        if (!ventaDetalle.FinServicio.HasValue)
                        {
                            ventaDetalle.FinServicio = ahora;
                        }
                        break;
                    case 4: // Cancelado
                        ventaDetalle.FinServicio = ahora;
                        break;
                }

                await _context.SaveChangesAsync();

                // Log del cambio
                var estado = await _estadoRepository.GetByIdAsync(nuevoEstadoId);
                
                return Json(new { 
                    success = true, 
                    message = $"Estado actualizado a: {estado?.Nombre ?? "Desconocido"}",
                    nuevoEstado = estado?.Nombre
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Error al actualizar: {ex.Message}" 
                });
            }
        }

        // Agregar este método en EstacionesController.cs
        [HttpPost]
        public async Task<IActionResult> CambiarEmpleado(int ventaDetalleId, int nuevoEmpleadoId)
        {
            try
            {
                var ventaDetalle = await _context.VentaDetalles
                    .FirstOrDefaultAsync(vd => vd.VentaDetalleId == ventaDetalleId);

                if (ventaDetalle == null)
                {
                    return Json(new { success = false, message = "Servicio no encontrado" });
                }

                // Actualizar AMBOS campos de empleado
                ventaDetalle.EmpleadoAsignadoId = nuevoEmpleadoId;
                ventaDetalle.EmpleadoServicioId = nuevoEmpleadoId;

                await _context.SaveChangesAsync();

                // Obtener nombre del nuevo empleado para el mensaje
                var empleado = await _context.Empleados
                    .FirstOrDefaultAsync(e => e.Id == nuevoEmpleadoId);

                return Json(new
                {
                    success = true,
                    message = $"Servicio reasignado a {empleado?.Nombre} {empleado?.Apellido}"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error al cambiar empleado: {ex.Message}"
                });
            }
        }


        // Vista monitor para cajero - ve todos los servicios con precios
        public async Task<IActionResult> Monitor()
        {
            try
            {
                // Cargar TODAS las ventas en proceso con sus detalles
                var ventasEnProceso = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Empleado)
                    .Include(v => v.VentaDetalles)
                        .ThenInclude(vd => vd.Servicio)
                    .Include(v => v.VentaDetalles)
                        .ThenInclude(vd => vd.EmpleadoServicio)
                    .Where(v => v.EstadoVenta != "Completada" && v.EstadoVenta != "Cancelada")
                    .OrderByDescending(v => v.FechaVenta)
                    .ToListAsync();

                // También cargar VentaDetalles directamente para servicios activos
                var serviciosActivos = await _context.VentaDetalles
                    .Include(vd => vd.Servicio)
                    .Include(vd => vd.Venta)
                        .ThenInclude(v => v.Cliente)
                    .Include(vd => vd.EmpleadoServicio)
                    .Where(vd => vd.EstadoServicioId <= 2) // Solo Esperando (1) y EnProceso (2)
                    .OrderBy(vd => vd.EstadoServicioId)
                    .ThenBy(vd => vd.FechaCreacion)
                    .ToListAsync();

                // Cargar TODOS los empleados activos
                var todosEmpleados = await _context.Empleados
                    .Where(e => e.EsActivo)
                    .OrderBy(e => e.Nombre)
                    .ToListAsync();

                // Identificar empleados ocupados (tienen servicios en estado 1 o 2)
                var empleadosOcupados = serviciosActivos
                    .Where(s => s.EmpleadoAsignadoId.HasValue)
                    .Select(s => s.EmpleadoAsignadoId.Value)
                    .Distinct()
                    .ToList();

                // Empleados disponibles = Todos - Ocupados
                var empleadosDisponibles = todosEmpleados
                    .Where(e => !empleadosOcupados.Contains(e.Id))
                    .ToList();

                // Calcular carga de trabajo por empleado
                var cargaPorEmpleado = serviciosActivos
                    .Where(s => s.EmpleadoAsignadoId.HasValue)
                    .GroupBy(s => s.EmpleadoAsignadoId.Value)
                    .Select(g => new
                    {
                        EmpleadoId = g.Key,
                        EnProceso = g.Count(s => s.EstadoServicioId == 2),
                        Esperando = g.Count(s => s.EstadoServicioId == 1),
                        Total = g.Count()
                    })
                    .ToList();

                // Cargar estados para mostrar colores y nombres
                ViewBag.Estados = await _estadoRepository.GetAllAsync();
                ViewBag.ServiciosActivos = serviciosActivos;
                ViewBag.EmpleadosDisponibles = empleadosDisponibles;
                ViewBag.TodosEmpleados = todosEmpleados;
                ViewBag.CargaPorEmpleado = cargaPorEmpleado;

                // Estadísticas para el dashboard
                var hoy = DateTime.Today;
                ViewBag.CompletadosHoy = await _context.VentaDetalles
                    .CountAsync(vd => vd.EstadoServicioId == 3 && vd.FinServicio.HasValue && vd.FinServicio.Value.Date == hoy);

                ViewBag.FacturadoHoy = await _context.Ventas
                    .Where(v => v.FechaVenta.Date == hoy && v.EstadoVenta == "Completada")
                    .SumAsync(v => v.Total);

                return View(ventasEnProceso);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Monitor: {ex.Message}");
                ViewBag.Error = ex.Message;
                return View(new List<Venta>());
            }
        }

        // API para obtener estado de un servicio (para AJAX)
        [HttpGet]
        public async Task<IActionResult> ObtenerEstadoServicio(int ventaDetalleId)
        {
            var detalle = await _context.VentaDetalles
                .Include(vd => vd.Servicio)
                .Include(vd => vd.EmpleadoServicio)
                .FirstOrDefaultAsync(vd => vd.VentaDetalleId == ventaDetalleId);

            if (detalle == null)
                return NotFound();

            var estado = await _estadoRepository.GetByIdAsync(detalle.EstadoServicioId);
            
            // Calcular tiempo transcurrido si está en proceso
            string? tiempoTranscurrido = null;
            if (detalle.InicioServicio.HasValue)
            {
                var duracion = DateTime.Now - detalle.InicioServicio.Value;
                tiempoTranscurrido = duracion.TotalHours >= 1 
                    ? $"{(int)duracion.TotalHours}h {duracion.Minutes}m"
                    : $"{(int)duracion.TotalMinutes}m";
            }

            // Obtener nombre del empleado ASIGNADO AL SERVICIO
            string? empleadoNombre = null;
            if (detalle.EmpleadoServicio != null)
            {
                empleadoNombre = $"{detalle.EmpleadoServicio.Nombre} {detalle.EmpleadoServicio.Apellido}";
            }

            return Json(new
            {
                estadoId = estado?.Id ?? 0,
                nombre = estado?.Nombre ?? "Desconocido",
                color = estado?.Color ?? "#000000",
                inicioServicio = detalle.InicioServicio,
                finServicio = detalle.FinServicio,
                tiempoTranscurrido = tiempoTranscurrido,
                servicioNombre = detalle.Servicio?.Nombre,
                empleadoNombre = empleadoNombre,
                empleadoAsignadoId = detalle.EmpleadoAsignadoId
            });
        }

        // Debug mejorado con más información
        public IActionResult Debug()
        {
            var result = "=== DEBUG INFORMACIÓN DEL MODELO ===\n";
            result += "=====================================\n\n";
            
            try
            {
                // Mostrar asignaciones actuales
                var asignaciones = _context.VentaDetalles
                    .Include(vd => vd.EmpleadoServicio)
                    .Include(vd => vd.Servicio)
                    .Where(vd => vd.EstadoServicioId != 3 && vd.EstadoServicioId != 4)
                    .Select(vd => new 
                    {
                        vd.VentaDetalleId,
                        ServicioNombre = vd.Servicio.Nombre,
                        vd.EmpleadoAsignadoId,
                        vd.EmpleadoServicioId,
                        EmpleadoNombre = vd.EmpleadoServicio != null 
                            ? vd.EmpleadoServicio.Nombre + " " + vd.EmpleadoServicio.Apellido 
                            : "Sin Asignar",
                        vd.EstadoServicioId
                    })
                    .ToList();

                result += "=== SERVICIOS ACTIVOS Y SUS EMPLEADOS ===\n";
                result += "-----------------------------------------\n";
                foreach (var asig in asignaciones)
                {
                    result += $"ID: {asig.VentaDetalleId}\n";
                    result += $"  Servicio: {asig.ServicioNombre}\n";
                    result += $"  EmpleadoAsignadoId: {asig.EmpleadoAsignadoId?.ToString() ?? "NULL"}\n";
                    result += $"  EmpleadoServicioId: {asig.EmpleadoServicioId?.ToString() ?? "NULL"}\n";
                    result += $"  Empleado: {asig.EmpleadoNombre}\n";
                    result += $"  Estado: {asig.EstadoServicioId}\n";
                    result += "---\n";
                }

                if (!asignaciones.Any())
                {
                    result += "(No hay servicios activos)\n";
                }
            }
            catch (Exception ex)
            {
                result += $"\nERROR: {ex.Message}\n";
            }
            
            return Content(result, "text/plain");
        }
    }
}