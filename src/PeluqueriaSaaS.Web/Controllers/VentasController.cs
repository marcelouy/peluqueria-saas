using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private const string TENANT_ID = "default";

        public VentasController(
            IVentaRepository ventaRepository,
            IEmpleadoRepository empleadoRepository,
            IMediator mediator,
            IServicioRepository servicioRepository)
        {
            _ventaRepository = ventaRepository;
            _empleadoRepository = empleadoRepository;
            _mediator = mediator;
            _servicioRepository = servicioRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var ventas = await _ventaRepository.GetAllAsync(TENANT_ID);
                return View(ventas);
            }
            catch (Exception ex)
            {
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

                // Cargar clientes usando MediatR
                var clientesQuery = new ObtenerClientesQuery();
                var clientesData = await _mediator.Send(clientesQuery);
                model.Clientes = clientesData.Where(c => c.EsActivo).Select(c => new ClienteBasicoDto
                {
                    ClienteId = c.Id,
                    Nombre = c.Nombre
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
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> POS(PosViewModel model)
        {
            try
            {
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

                await _ventaRepository.CreateAsync(venta);
                TempData["Success"] = "Venta creada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(POS));
            }
        }

        public async Task<IActionResult> Reportes(DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? empleadoId = null)
        {
            try
            {
                // Valores por defecto
                fechaDesde ??= DateTime.Today.AddMonths(-1);
                fechaHasta ??= DateTime.Today;

                var ventas = await _ventaRepository.GetAllAsync(TENANT_ID);
                
                // Cargar empleados para dropdown filtros
                var empleados = await _empleadoRepository.GetAllAsync();
                ViewBag.Empleados = new SelectList(empleados.Where(e => e.EsActivo), "Id", "NombreCompleto");
                
                // Filtrar ventas
                var ventasFiltradas = ventas.Where(v => 
                    v.FechaVenta.Date >= fechaDesde.Value.Date && 
                    v.FechaVenta.Date <= fechaHasta.Value.Date);
                
                if (empleadoId.HasValue)
                    ventasFiltradas = ventasFiltradas.Where(v => v.EmpleadoId == empleadoId.Value);

                var ventasLista = ventasFiltradas.ToList();
                
                var model = new ReporteVentasViewModel
                {
                    FechaDesde = fechaDesde.Value,
                    FechaHasta = fechaHasta.Value,
                    CantidadVentas = ventasLista.Count,
                    TotalVentas = ventasLista.Sum(v => v.Total),
                    PromedioVenta = ventasLista.Any() ? ventasLista.Average(v => v.Total) : 0,
                    Ventas = ventasLista.Select(v => new VentaDto
                    {
                        VentaId = v.VentaId,
                        NumeroVenta = v.NumeroVenta ?? $"V-{v.VentaId:000}",
                        FechaVenta = v.FechaVenta,
                        Total = v.Total,
                        ClienteNombre = "Cliente", // Temporal
                        EmpleadoNombre = "Empleado", // Temporal
                        EstadoVenta = "Completada"
                    }).ToList()
                };
                
                return View(model);
            }
            catch (Exception ex)
            {
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