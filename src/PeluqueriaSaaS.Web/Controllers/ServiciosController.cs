using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Domain.ValueObjects;
using PeluqueriaSaaS.Domain.Entities;
using System.Reflection;

namespace PeluqueriaSaaS.Web.Controllers
{
    public class ServiciosController : Controller
    {
        private readonly IServicioRepository _servicioRepository;
        private readonly ITipoServicioRepository _tipoServicioRepository;
        // 🔥 FIX CRÍTICO: Cambiar de "tenant-demo" a "default"
        private readonly string _tenantId = "default"; // ✅ CORREGIDO

        public ServiciosController(IServicioRepository servicioRepository, ITipoServicioRepository tipoServicioRepository)
        {
            _servicioRepository = servicioRepository;
            _tipoServicioRepository = tipoServicioRepository;
        }

        // GET: Servicios
        public async Task<IActionResult> Index()
        {
            var servicios = await _servicioRepository.GetAllAsync(_tenantId);
            var serviciosDto = servicios.Select(ServicioListDto.FromEntity).ToList();
            await CargarTiposServicio();
            return View(serviciosDto);
        }

        // GET: Servicios/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var servicio = await _servicioRepository.GetByIdAsync(id, _tenantId);
            if (servicio == null)
                return NotFound();

            var servicioDto = ServicioDto.FromEntity(servicio);
            return View(servicioDto);
        }

        // GET: Servicios/Create
        public async Task<IActionResult> Create()
        {
            await CargarTiposServicio();
            return View(new ServicioCreateDto());
        }

        // POST: Servicios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServicioCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                await CargarTiposServicio();
                return View(dto);
            }

            try
            {
                if (await _servicioRepository.ExisteNombreAsync(dto.Nombre, _tenantId))
                {
                    ModelState.AddModelError("Nombre", "Ya existe un servicio con este nombre.");
                    await CargarTiposServicio();
                    return View(dto);
                }

                var dinero = new Dinero(dto.Precio, dto.MonedaCodigo);
                var servicio = new Servicio(
                    dto.Nombre,
                    dinero,
                    dto.DuracionMinutos,
                    dto.TipoServicioId,
                    dto.Descripcion
                );

                // ✅ SOLUCION: Usar método específico del repository
                await _servicioRepository.CreateWithTenantAsync(servicio, _tenantId);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al crear el servicio: {ex.Message}");
                await CargarTiposServicio();
                return View(dto);
            }
        }

        // GET: Servicios/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var servicio = await _servicioRepository.GetByIdAsync(id, _tenantId);
            if (servicio == null)
                return NotFound();

            var dto = ServicioUpdateDto.FromEntity(servicio);
            await CargarTiposServicio();
            return View(dto);
        }

        // POST: Servicios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServicioUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await CargarTiposServicio();
                return View(dto);
            }

            try
            {
                if (await _servicioRepository.ExisteNombreAsync(dto.Nombre, _tenantId, dto.Id))
                {
                    ModelState.AddModelError("Nombre", "Ya existe un servicio con este nombre.");
                    await CargarTiposServicio();
                    return View(dto);
                }

                var servicio = await _servicioRepository.GetByIdAsync(id, _tenantId);
                if (servicio == null)
                    return NotFound();

                var nuevoPrecio = dto.GetDinero();
                servicio.ActualizarInformacion(dto.Nombre, nuevoPrecio, dto.DuracionMinutos, dto.Descripcion);
                
                if (servicio.TipoServicioId != dto.TipoServicioId)
                {
                    servicio.ActualizarTipoServicio(dto.TipoServicioId);
                }

                // Actualizar estado activo/inactivo
                if (dto.EsActivo)
                    servicio.Activar();
                else
                    servicio.Desactivar();
                
                await _servicioRepository.UpdateAsync(servicio);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar el servicio: {ex.Message}");
                await CargarTiposServicio();
                return View(dto);
            }
        }

        // GET: Servicios/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var servicio = await _servicioRepository.GetByIdAsync(id, _tenantId);
            if (servicio == null)
                return NotFound();

            var dto = ServicioDto.FromEntity(servicio);
            return View(dto);
        }

        // POST: Servicios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _servicioRepository.DeleteAsync(id, _tenantId);
                if (!result)
                    return NotFound();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar el servicio: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // AJAX: Cambiar estado activo
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            try
            {
                var servicio = await _servicioRepository.GetByIdAsync(id, _tenantId);
                if (servicio == null)
                    return NotFound();

                if (servicio.EsActivo)
                    servicio.Desactivar();
                else
                    servicio.Activar();

                await _servicioRepository.UpdateAsync(servicio);
                return Json(new { success = true, nuevoEstado = servicio.EsActivo });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // AJAX: Obtener estadísticas
        [HttpGet]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            try
            {
                var precioPromedio = await _servicioRepository.GetPrecioPromedioAsync(_tenantId);
                var servicios = await _servicioRepository.GetAllAsync(_tenantId);
                
                var stats = new
                {
                    Total = servicios.Count(),
                    Activos = servicios.Count(s => s.EsActivo),
                    Inactivos = servicios.Count(s => !s.EsActivo),
                    PrecioPromedio = precioPromedio,
                    DuracionPromedio = servicios.Where(s => s.EsActivo).Any() 
                        ? servicios.Where(s => s.EsActivo).Average(s => s.DuracionMinutos) 
                        : 0
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // AJAX: Buscar servicios
        [HttpGet]
        public async Task<IActionResult> Buscar(string termino, int? tipoServicioId, decimal? precioMin, decimal? precioMax)
        {
            try
            {
                IEnumerable<Servicio> servicios;

                if (!string.IsNullOrEmpty(termino))
                {
                    servicios = await _servicioRepository.GetAllAsync(_tenantId);
                    servicios = servicios.Where(s => s.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                                                   (s.Descripcion?.Contains(termino, StringComparison.OrdinalIgnoreCase) ?? false));
                }
                else if (tipoServicioId.HasValue)
                {
                    servicios = await _servicioRepository.GetByTipoAsync(tipoServicioId.Value, _tenantId);
                }
                else if (precioMin.HasValue && precioMax.HasValue)
                {
                    servicios = await _servicioRepository.GetByPrecioRangoAsync(precioMin.Value, precioMax.Value, _tenantId);
                }
                else
                {
                    servicios = await _servicioRepository.GetAllAsync(_tenantId);
                }

                var resultado = servicios.Select(s => new
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Precio = s.Precio.Valor,
                    Moneda = s.Precio.Moneda,
                    Duracion = s.DuracionMinutos,
                    TipoServicio = s.TipoServicio?.Nombre ?? "Sin tipo",
                    EsActivo = s.EsActivo
                });

                return Json(resultado);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private async Task CargarTiposServicio()
        {
            var tipos = await _tipoServicioRepository.GetAllAsync(_tenantId);
            ViewBag.TipoServicioId = new SelectList(tipos, "Id", "Nombre");
            ViewBag.TiposServicio = new SelectList(tipos, "Id", "Nombre");
        }
    }
}