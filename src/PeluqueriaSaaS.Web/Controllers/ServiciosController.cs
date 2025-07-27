using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Domain.ValueObjects;
using PeluqueriaSaaS.Domain.Entities;
using ClosedXML.Excel;
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

        [HttpGet]
        public async Task<IActionResult> Index(
            string? nombre = null,
            int? tipoServicioId = null, 
            decimal? precioMin = null,
            decimal? precioMax = null,
            bool soloActivos = false)
        {
            try
            {
                Console.WriteLine($"🔍 ServiciosController.Index - Filtros aplicados:");
                Console.WriteLine($"   - Nombre: {nombre ?? "null"}");
                Console.WriteLine($"   - TipoServicioId: {tipoServicioId?.ToString() ?? "null"}");
                Console.WriteLine($"   - PrecioMin: {precioMin?.ToString() ?? "null"}");
                Console.WriteLine($"   - PrecioMax: {precioMax?.ToString() ?? "null"}");
                Console.WriteLine($"   - SoloActivos: {soloActivos}");

                // ✅ CORREGIDO: Usar _servicioRepository directamente (no _repositoryManager)
                var servicios = await _servicioRepository.GetAllAsync(_tenantId);
                
                Console.WriteLine($"📊 Total servicios antes de filtros: {servicios.Count()}");

                // Aplicar filtros usando LINQ
                var serviciosFiltrados = servicios.AsQueryable();

                // Filtro por nombre (busca en nombre y descripción)
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    var nombreLower = nombre.ToLower().Trim();
                    serviciosFiltrados = serviciosFiltrados.Where(s => 
                        s.Nombre.ToLower().Contains(nombreLower) ||
                        (!string.IsNullOrEmpty(s.Descripcion) && s.Descripcion.ToLower().Contains(nombreLower))
                    );
                    Console.WriteLine($"🔍 Filtro nombre '{nombre}' aplicado");
                }

                // Filtro por tipo de servicio
                if (tipoServicioId.HasValue)
                {
                    serviciosFiltrados = serviciosFiltrados.Where(s => s.TipoServicioId == tipoServicioId.Value);
                    Console.WriteLine($"🏷️ Filtro tipo servicio ID {tipoServicioId} aplicado");
                }

                // Filtro por precio mínimo
                if (precioMin.HasValue)
                {
                    serviciosFiltrados = serviciosFiltrados.Where(s => s.Precio.Valor >= precioMin.Value);
                    Console.WriteLine($"💰 Filtro precio mínimo ${precioMin} aplicado");
                }

                // Filtro por precio máximo
                if (precioMax.HasValue)
                {
                    serviciosFiltrados = serviciosFiltrados.Where(s => s.Precio.Valor <= precioMax.Value);
                    Console.WriteLine($"💰 Filtro precio máximo ${precioMax} aplicado");
                }

                // Filtro solo activos
                if (soloActivos)
                {
                    serviciosFiltrados = serviciosFiltrados.Where(s => s.EsActivo);
                    Console.WriteLine($"✅ Filtro solo activos aplicado");
                }

                var serviciosFinales = serviciosFiltrados.ToList();
                Console.WriteLine($"📊 Total servicios después de filtros: {serviciosFinales.Count}");

                // Convertir a DTOs para la vista
                var serviciosDto = serviciosFinales.Select(ServicioDto.FromEntity).ToList();

                // ✅ CORREGIDO: Usar _tipoServicioRepository directamente
                var tiposServicio = await _tipoServicioRepository.GetAllAsync(_tenantId);
                var tiposServicioSelectList = tiposServicio.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Nombre,
                    Selected = t.Id == tipoServicioId
                }).ToList();

                // Agregar opción "Todos" al inicio
                tiposServicioSelectList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "Todos los tipos",
                    Selected = !tipoServicioId.HasValue
                });

                // Mantener valores de filtros en ViewBag para el formulario
                ViewBag.FiltroNombre = nombre;
                ViewBag.FiltroTipoServicioId = tipoServicioId;
                ViewBag.FiltroPrecioMin = precioMin;
                ViewBag.FiltroPrecioMax = precioMax;
                ViewBag.FiltroSoloActivos = soloActivos;
                ViewBag.TiposServicio = tiposServicioSelectList;

                // Información adicional para la vista
                ViewBag.TotalServicios = serviciosDto.Count;
                ViewBag.ServiciosActivos = serviciosDto.Count(s => s.EsActivo);
                ViewBag.ServiciosInactivos = serviciosDto.Count(s => !s.EsActivo);
                ViewBag.TieneFiltros = !string.IsNullOrWhiteSpace(nombre) || 
                                     tipoServicioId.HasValue || 
                                     precioMin.HasValue || 
                                     precioMax.HasValue || 
                                     soloActivos;

                Console.WriteLine($"✅ ServiciosController.Index completado - {serviciosDto.Count} servicios a mostrar");

                return View(serviciosDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en ServiciosController.Index: {ex.Message}");
                TempData["Error"] = "Error al cargar los servicios. Por favor, intenta nuevamente.";
                return View(new List<ServicioDto>());
            }
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

        // ✅ AJAX: Cambiar estado CORREGIDO - Compatible con JavaScript
        [HttpPost]
        public async Task<IActionResult> CambiarEstado([FromBody] CambiarEstadoRequest request)
        {
            try
            {
                Console.WriteLine($"🔄 CambiarEstado - ServicioId: {request.ServicioId}, NuevoEstado: {request.NuevoEstado}");

                // Obtener el servicio
                var servicio = await _servicioRepository.GetByIdAsync(request.ServicioId, _tenantId);
                if (servicio == null)
                {
                    Console.WriteLine($"❌ Servicio {request.ServicioId} no encontrado");
                    return Json(new { success = false, message = "Servicio no encontrado" });
                }

                // Cambiar estado
                if (request.NuevoEstado)
                {
                    servicio.Activar();
                    Console.WriteLine($"✅ Servicio {request.ServicioId} activado");
                }
                else
                {
                    servicio.Desactivar();
                    Console.WriteLine($"🚫 Servicio {request.ServicioId} desactivado");
                }

                // Guardar cambios
                await _servicioRepository.UpdateAsync(servicio);

                Console.WriteLine($"💾 Cambio de estado guardado en BD");

                return Json(new { 
                    success = true, 
                    message = $"Servicio {(request.NuevoEstado ? "activado" : "desactivado")} correctamente",
                    nuevoEstado = request.NuevoEstado
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en CambiarEstado: {ex.Message}");
                return Json(new { success = false, message = "Error al cambiar el estado del servicio" });
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

        [HttpGet]
        public async Task<IActionResult> ExportarServiciosExcel(
            string? nombre = null,
            int? tipoServicioId = null, 
            decimal? precioMin = null,
            decimal? precioMax = null,
            bool soloActivos = false)
        {
            try
            {
                Console.WriteLine($"📊 ExportarServiciosExcel - Iniciando export con filtros");
                Console.WriteLine($"   - Filtros: nombre={nombre}, tipo={tipoServicioId}, precioMin={precioMin}, precioMax={precioMax}, soloActivos={soloActivos}");

                // Obtener servicios con misma lógica que Index (reutilizar código)
                var servicios = await _servicioRepository.GetAllAsync(_tenantId);
                
                // Aplicar mismos filtros que Index
                var serviciosFiltrados = servicios.AsQueryable();

                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    var nombreLower = nombre.ToLower().Trim();
                    serviciosFiltrados = serviciosFiltrados.Where(s => 
                        s.Nombre.ToLower().Contains(nombreLower) ||
                        (!string.IsNullOrEmpty(s.Descripcion) && s.Descripcion.ToLower().Contains(nombreLower))
                    );
                }

                if (tipoServicioId.HasValue)
                {
                    serviciosFiltrados = serviciosFiltrados.Where(s => s.TipoServicioId == tipoServicioId.Value);
                }

                if (precioMin.HasValue)
                {
                    serviciosFiltrados = serviciosFiltrados.Where(s => s.Precio.Valor >= precioMin.Value);
                }

                if (precioMax.HasValue)
                {
                    serviciosFiltrados = serviciosFiltrados.Where(s => s.Precio.Valor <= precioMax.Value);
                }

                if (soloActivos)
                {
                    serviciosFiltrados = serviciosFiltrados.Where(s => s.EsActivo);
                }

                var serviciosParaExport = serviciosFiltrados.ToList();
                Console.WriteLine($"📊 Total servicios para export: {serviciosParaExport.Count}");

                // Crear Excel usando ClosedXML
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Servicios");

                // Configurar encabezados
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Nombre";
                worksheet.Cell(1, 3).Value = "Descripción";
                worksheet.Cell(1, 4).Value = "Precio";
                worksheet.Cell(1, 5).Value = "Moneda";
                worksheet.Cell(1, 6).Value = "Duración (min)";
                worksheet.Cell(1, 7).Value = "Tipo Servicio";
                worksheet.Cell(1, 8).Value = "Estado";
                worksheet.Cell(1, 9).Value = "Fecha Creación";
                worksheet.Cell(1, 10).Value = "Fecha Actualización";

                // Estilo encabezados
                var headerRange = worksheet.Range(1, 1, 1, 10);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Llenar datos
                int row = 2;
                foreach (var servicio in serviciosParaExport)
                {
                    worksheet.Cell(row, 1).Value = servicio.Id;
                    worksheet.Cell(row, 2).Value = servicio.Nombre;
                    worksheet.Cell(row, 3).Value = servicio.Descripcion ?? "";
                    worksheet.Cell(row, 4).Value = servicio.Precio.Valor;
                    worksheet.Cell(row, 5).Value = servicio.Precio.Moneda;
                    worksheet.Cell(row, 6).Value = servicio.DuracionMinutos;
                    worksheet.Cell(row, 7).Value = servicio.TipoServicio?.Nombre ?? "Sin categoría";
                    worksheet.Cell(row, 8).Value = servicio.EsActivo ? "Activo" : "Inactivo";
                    worksheet.Cell(row, 9).Value = servicio.FechaCreacion;
                    worksheet.Cell(row, 10).Value = servicio.FechaActualizacion?.ToString() ?? "";
                    row++;
                }

                // Ajustar ancho columnas automáticamente
                worksheet.Columns().AdjustToContents();

                // Agregar filtros automáticos
                worksheet.RangeUsed().SetAutoFilter();

                // Crear nombre archivo con timestamp
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var filtrosTexto = "";
                
                if (!string.IsNullOrWhiteSpace(nombre) || tipoServicioId.HasValue || precioMin.HasValue || precioMax.HasValue || soloActivos)
                {
                    filtrosTexto = "_filtrado";
                }

                var fileName = $"servicios_export_{timestamp}{filtrosTexto}.xlsx";

                // Crear stream y devolver archivo
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                Console.WriteLine($"✅ Excel generado exitosamente: {fileName}");

                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generando Excel: {ex.Message}");
                TempData["Error"] = "Error al generar el archivo Excel. Intenta nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        // NOTA: Agregar using para ClosedXML al inicio del archivo:
        // 


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

    // ✅ CLASE REQUEST PARA AJAX
    public class CambiarEstadoRequest
    {
        public int ServicioId { get; set; }
        public bool NuevoEstado { get; set; }
    }
}