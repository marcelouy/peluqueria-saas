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
            // Cargar VentaDetalles pendientes con todas las relaciones necesarias
            var serviciosPendientes = await _context.VentaDetalles
                .Include(vd => vd.Servicio)
                .Include(vd => vd.Venta)
                    .ThenInclude(v => v.Cliente)
                .Where(vd => vd.EstadoServicioId != 3 && vd.EstadoServicioId != 4) // No mostrar Completados ni Cancelados
                .OrderBy(vd => vd.VentaDetalleId)
                .ToListAsync();

            // ✅ CORRECTO: Cargar ESTADOS disponibles, NO empleados
            var estadosDisponibles = await _estadoRepository.GetAllAsync();
            
            // Crear SelectList para los estados con formato amigable
            ViewBag.EstadosDisponibles = estadosDisponibles.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = GetEstadoDisplayText(e.Nombre)
            }).ToList();

            // Cargar empleados SOLO para mostrar quién tiene asignado cada servicio
            var empleados = await _context.Empleados.Where(e => e.EsActivo).ToListAsync();
            ViewBag.EmpleadosInfo = empleados; // Solo para información, no para dropdown

            // Pasar los estados como diccionario para consulta rápida
            ViewBag.EstadosDiccionario = estadosDisponibles.ToDictionary(e => e.Id, e => e);

            return View(serviciosPendientes);
        }

        // Método helper para formatear nombres de estados
        private string GetEstadoDisplayText(string estadoNombre)
        {
            return estadoNombre switch
            {
                "Esperando" => "⏳ Esperando",
                "EnProceso" => "🔄 En Proceso",
                "Completado" => "✅ Completado",
                "Cancelado" => "❌ Cancelado",
                _ => estadoNombre
            };
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

                // Si se proporciona empleado, actualizarlo (si existe la propiedad)
                // NOTA: VentaDetalle podría no tener EmpleadoId directamente
                // Verificar el modelo real y ajustar según corresponda

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
                            // Calcular duración si hay inicio
                            if (ventaDetalle.InicioServicio.HasValue)
                            {
                                var duracion = (ahora - ventaDetalle.InicioServicio.Value).TotalMinutes;
                                // Podrías guardar la duración en algún campo si lo tienes
                            }
                        }
                        break;
                    case 4: // Cancelado
                        ventaDetalle.FinServicio = ahora;
                        break;
                }

                await _context.SaveChangesAsync();

                // Log del cambio
                var estado = await _estadoRepository.GetByIdAsync(nuevoEstadoId);
                TempData["Success"] = $"Estado actualizado a: {estado.Nombre}";

                return Json(new { 
                    success = true, 
                    message = "Estado actualizado correctamente",
                    nuevoEstado = estado.Nombre
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

        // Vista monitor para cajero - ve todos los servicios con precios
        public async Task<IActionResult> Monitor()
        {
            var ventasEnProceso = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Empleado)
                .Include(v => v.VentaDetalles)
                    .ThenInclude(vd => vd.Servicio)
                .Where(v => v.EstadoVenta != "Completada" && v.EstadoVenta != "Cancelada")
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();

            // Cargar estados para mostrar colores y nombres
            ViewBag.Estados = await _estadoRepository.GetAllAsync();

            return View(ventasEnProceso);
        }

        // API para obtener estado de un servicio (para AJAX)
        [HttpGet]
        public async Task<IActionResult> ObtenerEstadoServicio(int ventaDetalleId)
        {
            var detalle = await _context.VentaDetalles
                .Include(vd => vd.Servicio)
                .FirstOrDefaultAsync(vd => vd.VentaDetalleId == ventaDetalleId);

            if (detalle == null)
                return NotFound();

            var estado = await _estadoRepository.GetByIdAsync(detalle.EstadoServicioId);
            
            // Calcular tiempo transcurrido si está en proceso
            string tiempoTranscurrido = null;
            if (detalle.InicioServicio.HasValue)
            {
                var duracion = DateTime.Now - detalle.InicioServicio.Value;
                tiempoTranscurrido = duracion.TotalHours >= 1 
                    ? $"{(int)duracion.TotalHours}h {duracion.Minutes}m"
                    : $"{(int)duracion.TotalMinutes}m";
            }

            return Json(new
            {
                estadoId = estado.Id,
                nombre = estado.Nombre,
                color = estado.Color,
                inicioServicio = detalle.InicioServicio,
                finServicio = detalle.FinServicio,
                tiempoTranscurrido = tiempoTranscurrido,
                servicioNombre = detalle.Servicio?.Nombre
            });
        }

        public IActionResult Debug()
        {
            var entityType = _context.Model.FindEntityType(typeof(VentaDetalle));
            var properties = entityType.GetProperties();
            var shadowProps = properties.Where(p => p.IsShadowProperty()).ToList();
            
            var result = "PROPIEDADES SHADOW EN VENTADETALLE:\n";
            foreach (var prop in shadowProps)
            {
                result += $"- {prop.Name} (Type: {prop.ClrType.Name})\n";
            }
            
            return Content(result);
        }
    }
}