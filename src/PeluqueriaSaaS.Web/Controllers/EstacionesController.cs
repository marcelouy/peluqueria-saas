using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Data;
using PeluqueriaSaaS.Domain.Entities;

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

        // Vista para empleados - muestra sus servicios asignados
        public async Task<IActionResult> Index()
        {
            // Cargar VentaDetalles pendientes
            var serviciosPendientes = await _context.VentaDetalles
                .AsNoTracking()
                .Where(vd => vd.EstadoServicioId != 3 && vd.EstadoServicioId != 4)
                .ToListAsync();

            // Cargar Ventas con Clientes
            if (serviciosPendientes.Any())
            {
                var ventaIds = serviciosPendientes.Select(vd => vd.VentaId).Distinct().ToList();
                
                // NO USAR ToDictionaryAsync - tiene bug con WITH
                var ventas = await _context.Ventas
                    .AsNoTracking()
                    .Include(v => v.Cliente)
                    .Where(v => ventaIds.Contains(v.VentaId))
                    .ToListAsync(); // <-- CAMBIO AQUÍ

                // Asignar manualmente
                foreach (var detalle in serviciosPendientes)
                {
                    detalle.Venta = ventas.FirstOrDefault(v => v.VentaId == detalle.VentaId);
                }
            }

            ViewBag.Estados = await _estadoRepository.GetAllAsync();
            ViewBag.Empleados = await _context.Empleados.Where(e => e.EsActivo).ToListAsync();

            return View(serviciosPendientes);
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

        // Actualizar estado de un servicio
        [HttpPost]
        public async Task<IActionResult> ActualizarEstado(int ventaDetalleId, int nuevoEstadoId, int? empleadoId)
        {
            var resultado = await _estadoRepository.UpdateVentaDetalleEstadoAsync(
                ventaDetalleId, 
                nuevoEstadoId, 
                empleadoId ?? 1); // Por defecto empleado 1 si no se especifica

            if (resultado)
            {
                TempData["Success"] = "Estado actualizado correctamente";
            }
            else
            {
                TempData["Error"] = "No se pudo actualizar el estado";
            }

            return RedirectToAction(nameof(Index));
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

        // API para obtener estado de un servicio (para futuro AJAX)
        [HttpGet]
        public async Task<IActionResult> ObtenerEstadoServicio(int ventaDetalleId)
        {
            var detalle = await _context.VentaDetalles
                .FirstOrDefaultAsync(vd => vd.VentaDetalleId == ventaDetalleId);

            if (detalle == null)
                return NotFound();

            var estado = await _estadoRepository.GetByIdAsync(detalle.EstadoServicioId);

            return Json(new
            {
                estadoId = estado.Id,
                nombre = estado.Nombre,
                color = estado.Color,
                inicioServicio = detalle.InicioServicio,
                finServicio = detalle.FinServicio
            });
        }
    }
}