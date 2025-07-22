using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Infrastructure.Repositories;

namespace PeluqueriaSaaS.Web.Controllers
{
    public class VentasController : Controller
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IServicioRepository _servicioRepository;
        private const string TENANT_ID = "default";

        public VentasController(
            IVentaRepository ventaRepository,
            IEmpleadoRepository empleadoRepository,
            IClienteRepository clienteRepository,
            IServicioRepository servicioRepository)
        {
            _ventaRepository = ventaRepository;
            _empleadoRepository = empleadoRepository;
            _clienteRepository = clienteRepository;
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
                var empleados = await _empleadoRepository.GetAllAsync(TENANT_ID);
                model.Empleados = empleados.Where(e => e.EsActivo).Select(e => new EmpleadoBasicoDto
                {
                    EmpleadoId = e.EmpleadoId,
                    Nombre = e.Nombre
                }).ToList();

                // Cargar clientes  
                var clientes = await _clienteRepository.GetAllAsync(TENANT_ID);
                model.Clientes = clientes.Where(c => c.EsActivo).Select(c => new ClienteBasicoDto
                {
                    ClienteId = c.ClienteId,
                    Nombre = c.Nombre
                }).ToList();

                // Cargar servicios
                var servicios = await _servicioRepository.GetAllAsync(TENANT_ID);
                model.Servicios = servicios.Where(s => s.EsActivo).Select(s => new ServicioBasicoDto
                {
                    ServicioId = s.ServicioId,
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

        public async Task<IActionResult> Reportes()
        {
            try
            {
                var ventas = await _ventaRepository.GetAllAsync(TENANT_ID);
                var model = new ReporteVentasViewModel
                {
                    Ventas = ventas.Select(v => new VentaDto
                    {
                        VentaId = v.VentaId,
                        NumeroVenta = v.NumeroVenta,
                        FechaVenta = v.FechaVenta,
                        Total = v.Total
                    }).ToList()
                };
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return View(new ReporteVentasViewModel());
            }
        }
    }
}