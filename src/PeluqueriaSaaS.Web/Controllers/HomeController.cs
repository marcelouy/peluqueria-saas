using Microsoft.AspNetCore.Mvc;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IServicioRepository _servicioRepository;
        private readonly IVentaRepository _ventaRepository;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly ITipoServicioRepository _tipoServicioRepository;
        private readonly string _tenantId = "default";

        public HomeController(
            IServicioRepository servicioRepository,
            IVentaRepository ventaRepository,
            IEmpleadoRepository empleadoRepository,
            ITipoServicioRepository tipoServicioRepository)
        {
            _servicioRepository = servicioRepository;
            _ventaRepository = ventaRepository;
            _empleadoRepository = empleadoRepository;
            _tipoServicioRepository = tipoServicioRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var ventas = await _ventaRepository.GetAllAsync(_tenantId);
                var empleados = await _empleadoRepository.GetAllAsync();

                // Ventas HOY
                var ventasHoy = ventas.Where(v => v.FechaVenta.Date == DateTime.Today).ToList();
                ViewBag.VentasHoy = ventasHoy.Count();
                ViewBag.IngresoHoy = ventasHoy.Sum(v => v.Total);

                // Ventas MES
                var ventasMes = ventas.Where(v => 
                    v.FechaVenta.Month == DateTime.Now.Month && 
                    v.FechaVenta.Year == DateTime.Now.Year).ToList();
                ViewBag.VentasMes = ventasMes.Count();
                ViewBag.IngresoMes = ventasMes.Sum(v => v.Total);

                // Top 5 Empleados del Mes
                var topEmpleadosMes = ventasMes
                    .GroupBy(v => v.EmpleadoId)
                    .Select(g => new {
                        EmpleadoId = g.Key,
                        TotalVentas = g.Sum(v => v.Total),
                        CantidadVentas = g.Count()
                    })
                    .OrderByDescending(x => x.TotalVentas)
                    .Take(5)
                    .ToList();

                var topEmpleadosConNombre = new List<dynamic>();
                foreach(var emp in topEmpleadosMes)
                {
                    var empleado = empleados.FirstOrDefault(e => e.Id == emp.EmpleadoId);
                    if(empleado != null && empleado.Email != "sistema@peluqueria.com")
                    {
                        topEmpleadosConNombre.Add(new {
                            Nombre = $"{empleado.Nombre} {empleado.Apellido}",
                            TotalVentas = emp.TotalVentas,
                            CantidadVentas = emp.CantidadVentas,
                            Promedio = emp.TotalVentas / emp.CantidadVentas
                        });
                    }
                }
                ViewBag.TopEmpleadosMes = topEmpleadosConNombre;
                ViewBag.PromedioVenta = ventas.Any() ? ventas.Average(v => v.Total) : 0;

                return View("Dashboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error Dashboard: {ex.Message}");
                ViewBag.TopEmpleadosMes = new List<dynamic>();
                return View("Dashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var ventas = await _ventaRepository.GetAllAsync(_tenantId);
                var servicios = await _servicioRepository.GetAllAsync(_tenantId);
                var empleados = await _empleadoRepository.GetAllAsync();

                // Ventas últimos 30 días
                var ventasUltimos30Dias = new List<object>();
                for (int i = 29; i >= 0; i--)
                {
                    var fecha = DateTime.Today.AddDays(-i);
                    var ventasDia = ventas.Where(v => v.FechaVenta.Date == fecha);
                    
                    ventasUltimos30Dias.Add(new {
                        fecha = fecha.ToString("dd/MM"),
                        total = ventasDia.Sum(v => v.Total),
                        cantidad = ventasDia.Count()
                    });
                }

                // Ventas por Empleado del Mes
                var ventasMes = ventas.Where(v => 
                    v.FechaVenta.Month == DateTime.Now.Month && 
                    v.FechaVenta.Year == DateTime.Now.Year);

                var ventasPorEmpleado = ventasMes
                    .GroupBy(v => v.EmpleadoId)
                    .Select(g => {
                        var empleado = empleados.FirstOrDefault(e => e.Id == g.Key);
                        if(empleado?.Email == "sistema@peluqueria.com") return null;
                        
                        return new {
                            empleadoNombre = empleado != null ? 
                                $"{empleado.Nombre} {empleado.Apellido}" : "Desconocido",
                            totalVentas = g.Sum(v => v.Total),
                            cantidadVentas = g.Count()
                        };
                    })
                    .Where(x => x != null)
                    .OrderByDescending(x => x.totalVentas)
                    .Take(10)
                    .ToList();

                // Top servicios
                var topServicios = servicios
                    .Where(s => s.EsActivo)
                    .Take(5)
                    .Select(s => new {
                        nombre = s.Nombre,
                        precio = s.Precio.Valor,
                        ventas = Random.Shared.Next(5, 30)
                    })
                    .ToList();

                var tiposServicio = await _tipoServicioRepository.GetAllAsync(_tenantId);
                var serviciosPorTipo = tiposServicio
                    .Select(t => new {
                        tipo = t.Nombre,
                        cantidad = servicios.Count(s => s.TipoServicioId == t.Id && s.EsActivo)
                    })
                    .Where(x => x.cantidad > 0)
                    .ToList();

                return Json(new {
                    ventasUltimos30Dias,
                    topServicios,
                    serviciosPorTipo,
                    ventasPorEmpleado
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                return Json(new { error = "Error" });
            }
        }

        public IActionResult Privacy() => View();
    }
}