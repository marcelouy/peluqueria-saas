using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Entities.Configuration;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Data;
using System.Linq;

namespace PeluqueriaSaaS.Web.Controllers
{
    public class ArticulosController : Controller
    {
        private readonly IArticuloRepository _articuloRepository;
        private readonly PeluqueriaDbContext _context;
        private const string TENANT_ID = "1";

        public ArticulosController(IArticuloRepository articuloRepository, PeluqueriaDbContext context)
        {
            _articuloRepository = articuloRepository;
            _context = context;
        }

        // GET: Articulos
        public async Task<IActionResult> Index()
        {
            var articulos = await _articuloRepository.GetAllAsync(TENANT_ID);
            return View(articulos);
        }

        // GET: Articulos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var articulo = await _articuloRepository.GetByIdAsync(id, TENANT_ID);
            if (articulo == null)
            {
                TempData["Error"] = "Artículo no encontrado";
                return RedirectToAction(nameof(Index));
            }
            return View(articulo);
        }

        // GET: Articulos/Create
        public async Task<IActionResult> Create()
        {
            await PrepararDropdownData();
            await CargarImpuestos();
            return View();
        }

        // POST: Articulos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Articulo articulo, int[] impuestosSeleccionados)
        {
            Console.WriteLine($"🔧 POST Create recibido - Nombre: {articulo.Nombre}, Precio: {articulo.Precio}");
            Console.WriteLine($"📋 Impuestos seleccionados: {string.Join(", ", impuestosSeleccionados ?? new int[0])}");
            
            ModelState.Remove("TenantId");
            
            if (ModelState.IsValid)
            {
                Console.WriteLine("✅ ModelState válido");
                
                // Validar código único
                if (!string.IsNullOrEmpty(articulo.Codigo))
                {
                    if (await _articuloRepository.ExisteCodigo(articulo.Codigo, TENANT_ID))
                    {
                        Console.WriteLine($"❌ Código duplicado: {articulo.Codigo}");
                        ModelState.AddModelError("Codigo", "Ya existe un artículo con este código");
                        await PrepararDropdownData();
                        await CargarImpuestos();
                        return View(articulo);
                    }
                }

                Console.WriteLine("🔧 Llamando Repository.CreateAsync...");
                var result = await _articuloRepository.CreateAsync(articulo);
                Console.WriteLine($"✅ Repository retornó artículo ID: {result.Id}");
                
                // Guardar impuestos asociados
                if (impuestosSeleccionados != null && impuestosSeleccionados.Length > 0)
                {
                    await GuardarImpuestosArticulo(result.Id, impuestosSeleccionados);
                }
                
                TempData["Success"] = "Artículo creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                Console.WriteLine("❌ ModelState inválido:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"   {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            await PrepararDropdownData();
            await CargarImpuestos();
            return View(articulo);
        }

        // GET: Articulos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            Console.WriteLine($"🔧 GET Edit - ID recibido: {id}");
            
            if (id <= 0)
            {
                Console.WriteLine("❌ ID inválido para Edit");
                return NotFound();
            }

            var articulo = await _articuloRepository.GetByIdAsync(id, TENANT_ID);
            if (articulo == null)
            {
                Console.WriteLine($"❌ Artículo no encontrado - ID: {id}");
                TempData["Error"] = "Artículo no encontrado";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ArticuloId = id;
            Console.WriteLine($"✅ Artículo encontrado - ID: {articulo.Id}, Nombre: {articulo.Nombre}");

            await PrepararDropdownData();
            await CargarImpuestos();
            await CargarImpuestosArticulo(id);
            
            return View(articulo);
        }

        // POST: Articulos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Articulo articulo, int[] impuestosSeleccionados)
        {
            Console.WriteLine($"🔧 POST Edit recibido - Route ID: {id}, Model ID: {articulo.Id}");
            Console.WriteLine($"🔧 Nombre: {articulo.Nombre}, Precio: {articulo.Precio}");
            Console.WriteLine($"📋 Impuestos seleccionados: {string.Join(", ", impuestosSeleccionados ?? new int[0])}");

            if (id <= 0)
            {
                Console.WriteLine("❌ Route ID inválido para UPDATE");
                TempData["Error"] = "ID de artículo inválido";
                return RedirectToAction(nameof(Index));
            }

            // ✅ ASEGURAR que el articulo tenga el ID correcto
            SetIdViaReflection(articulo, id);

            ModelState.Remove("TenantId");
            ModelState.Remove("ArticulosImpuestos"); // Ignorar la colección de navegación

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState inválido:");
                foreach (var error in ModelState.Where(x => x.Value.Errors.Count > 0))
                {
                    Console.WriteLine($"   - {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                
                ViewBag.ArticuloId = id;
                await PrepararDropdownData();
                await CargarImpuestos();
                await CargarImpuestosArticulo(id);
                return View(articulo);
            }

            // Validar código único
            if (!string.IsNullOrEmpty(articulo.Codigo))
            {
                if (await _articuloRepository.ExisteCodigo(articulo.Codigo, TENANT_ID, id))
                {
                    Console.WriteLine($"❌ Código duplicado: {articulo.Codigo}");
                    ModelState.AddModelError("Codigo", "Ya existe otro artículo con este código");
                    ViewBag.ArticuloId = id;
                    await PrepararDropdownData();
                    await CargarImpuestos();
                    await CargarImpuestosArticulo(id);
                    return View(articulo);
                }
            }

            try
            {
                Console.WriteLine($"🔧 Llamando Repository.UpdateAsync para ID: {id}");
                await _articuloRepository.UpdateAsync(articulo);
                
                // Actualizar impuestos asociados
                await ActualizarImpuestosArticulo(id, impuestosSeleccionados);
                
                Console.WriteLine($"✅ Repository UPDATE exitoso - ID: {id}");
                TempData["Success"] = "Artículo actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en UpdateAsync: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                TempData["Error"] = $"Error al actualizar el artículo: {ex.Message}";
                ViewBag.ArticuloId = id;
                await PrepararDropdownData();
                await CargarImpuestos();
                await CargarImpuestosArticulo(id);
                return View(articulo);
            }
        }

        /// <summary>
        /// Método auxiliar para asignar Id via reflection (protected setter)
        /// </summary>
        private void SetIdViaReflection(Articulo articulo, int id)
        {
            try
            {
                var entityType = typeof(Articulo);
                
                // Buscar property Id en jerarquía
                var idProperty = entityType.GetProperty("Id", 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                
                if (idProperty != null)
                {
                    // Intentar setter protegido
                    var setter = idProperty.GetSetMethod(true);
                    if (setter != null)
                    {
                        setter.Invoke(articulo, new object[] { id });
                        Console.WriteLine($"✅ Id asignado via reflection: {id}");
                        return;
                    }
                }
                
                // Fallback: backing field
                var backingField = entityType.GetField("<Id>k__BackingField", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                
                if (backingField != null)
                {
                    backingField.SetValue(articulo, id);
                    Console.WriteLine($"✅ Id asignado via backing field: {id}");
                    return;
                }
                
                Console.WriteLine($"⚠️ No se pudo asignar Id via reflection");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error asignando Id via reflection: {ex.Message}");
            }
        }

        // POST: Articulos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _articuloRepository.DeleteAsync(id, TENANT_ID);
            
            if (result)
            {
                TempData["Success"] = "Artículo eliminado exitosamente";
            }
            else
            {
                TempData["Error"] = "No se pudo eliminar el artículo";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Verificar disponibilidad código
        [HttpGet]
        public async Task<JsonResult> VerificarCodigo(string codigo, int? id)
        {
            if (string.IsNullOrEmpty(codigo))
                return Json(true);

            var existe = await _articuloRepository.ExisteCodigo(codigo, TENANT_ID, id);
            return Json(!existe);
        }

        // GET: Articulos/BajoStock
        public async Task<IActionResult> BajoStock()
        {
            var articulos = await _articuloRepository.GetBajoStockAsync(TENANT_ID);
            ViewBag.ModoFiltro = "BajoStock";
            return View("Index", articulos);
        }

        // Método auxiliar para dropdowns
        private async Task PrepararDropdownData()
        {
            try
            {
                ViewBag.Categorias = await _articuloRepository.GetCategoriasAsync(TENANT_ID);
                ViewBag.Marcas = await _articuloRepository.GetMarcasAsync(TENANT_ID);
                ViewBag.Proveedores = await _articuloRepository.GetProveedoresAsync(TENANT_ID);
                
                Console.WriteLine($"✅ Dropdowns preparados");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error preparando dropdowns: {ex.Message}");
                ViewBag.Categorias = new List<string>();
                ViewBag.Marcas = new List<string>();
                ViewBag.Proveedores = new List<string>();
            }
        }

        // 🆕 MÉTODOS PARA GESTIÓN DE IMPUESTOS
        
        private async Task CargarImpuestos()
        {
            try
            {
                // Cargar tipos de impuestos con sus tasas vigentes
                var tiposImpuestos = await _context.TiposImpuestos
                    .Include(t => t.Tasas)
                    .Where(t => t.Activo)
                    .OrderBy(t => t.OrdenAplicacion)
                    .ToListAsync();

                Console.WriteLine($"📊 Tipos de impuestos encontrados: {tiposImpuestos.Count}");

                var impuestosDisponibles = new List<dynamic>();
                var hoy = DateTime.Today;

                foreach (var tipo in tiposImpuestos)
                {
                    Console.WriteLine($"   - {tipo.Nombre} ({tipo.Codigo}): {tipo.Tasas.Count} tasas totales");
                    
                    var tasasVigentes = tipo.Tasas
                        .Where(t => t.Activo && 
                                   t.FechaInicio <= hoy && 
                                   (t.FechaFin == null || t.FechaFin >= hoy))
                        .OrderBy(t => t.Porcentaje)
                        .ToList();

                    Console.WriteLine($"     Tasas vigentes: {tasasVigentes.Count}");

                    if (tasasVigentes.Any())
                    {
                        var tasasParaVista = tasasVigentes.Select(t => new
                        {
                            TasaId = t.Id,
                            Nombre = t.Nombre,
                            Porcentaje = t.Porcentaje,
                            EsDefault = t.EsTasaPorDefecto
                        }).ToList(); // Asegurar que se materialice la lista
                        
                        impuestosDisponibles.Add(new
                        {
                            TipoId = tipo.Id,
                            TipoCodigo = tipo.Codigo,
                            TipoNombre = tipo.Nombre,
                            Tasas = tasasParaVista
                        });
                        
                        foreach (var tasa in tasasVigentes)
                        {
                            Console.WriteLine($"       • {tasa.Nombre}: {tasa.Porcentaje}%");
                        }
                    }
                }

                ViewBag.ImpuestosDisponibles = impuestosDisponibles;
                Console.WriteLine($"✅ Impuestos cargados: {impuestosDisponibles.Count} tipos con tasas vigentes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cargando impuestos: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                ViewBag.ImpuestosDisponibles = new List<dynamic>();
            }
        }

        private async Task CargarImpuestosArticulo(int articuloId)
        {
            try
            {
                var impuestosActuales = await _context.ArticulosImpuestos
                    .Where(ai => ai.ArticuloId == articuloId && 
                                ai.TenantId == TENANT_ID && 
                                ai.Activo)
                    .Select(ai => ai.TasaImpuestoId)
                    .ToListAsync();

                ViewBag.ImpuestosActuales = impuestosActuales;
                Console.WriteLine($"✅ Impuestos del artículo cargados: {impuestosActuales.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cargando impuestos del artículo: {ex.Message}");
                ViewBag.ImpuestosActuales = new List<int>();
            }
        }

        private async Task GuardarImpuestosArticulo(int articuloId, int[] tasasIds)
        {
            try
            {
                var hoy = DateTime.Now;
                
                foreach (var tasaId in tasasIds)
                {
                    var articuloImpuesto = new ArticuloImpuesto
                    {
                        ArticuloId = articuloId,
                        TasaImpuestoId = tasaId,
                        FechaInicioAplicacion = hoy
                    };
                    
                    // Usar reflection para propiedades protegidas
                    SetProtectedProperty(articuloImpuesto, "TenantId", TENANT_ID);
                    SetProtectedProperty(articuloImpuesto, "Activo", true);
                    SetProtectedProperty(articuloImpuesto, "FechaCreacion", hoy);
                    SetProtectedProperty(articuloImpuesto, "CreadoPor", "Sistema");

                    _context.ArticulosImpuestos.Add(articuloImpuesto);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ {tasasIds.Length} impuestos guardados para artículo {articuloId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error guardando impuestos: {ex.Message}");
            }
        }

        private async Task ActualizarImpuestosArticulo(int articuloId, int[]? tasasIds)
        {
            try
            {
                // Desactivar impuestos actuales
                var impuestosActuales = await _context.ArticulosImpuestos
                    .Where(ai => ai.ArticuloId == articuloId && 
                                ai.TenantId == TENANT_ID && 
                                ai.Activo)
                    .ToListAsync();

                foreach (var impuesto in impuestosActuales)
                {
                    // Usar reflection para propiedades protegidas
                    SetProtectedProperty(impuesto, "Activo", false);
                    SetProtectedProperty(impuesto, "FechaActualizacion", DateTime.Now);
                    SetProtectedProperty(impuesto, "ActualizadoPor", "Sistema");
                    
                    impuesto.FechaFinAplicacion = DateTime.Now;
                }

                // Agregar nuevos impuestos
                if (tasasIds != null && tasasIds.Length > 0)
                {
                    await GuardarImpuestosArticulo(articuloId, tasasIds);
                }
                else
                {
                    await _context.SaveChangesAsync();
                }

                Console.WriteLine($"✅ Impuestos actualizados para artículo {articuloId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error actualizando impuestos: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Método helper para establecer propiedades protegidas usando reflection
        /// </summary>
        private void SetProtectedProperty(object obj, string propertyName, object value)
        {
            var type = obj.GetType();
            var property = type.GetProperty(propertyName, 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (property != null)
            {
                var setter = property.GetSetMethod(true);
                if (setter != null)
                {
                    setter.Invoke(obj, new[] { value });
                }
                else
                {
                    // Intentar con backing field
                    var field = type.GetField($"<{propertyName}>k__BackingField", 
                        System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Instance);
                    field?.SetValue(obj, value);
                }
            }
        }

        // AJAX: Obtener tasas de un tipo de impuesto
        [HttpGet]
        public async Task<JsonResult> ObtenerTasasImpuesto(int tipoId)
        {
            var hoy = DateTime.Today;
            var tasas = await _context.TasasImpuestos
                .Where(t => t.TipoImpuestoId == tipoId && 
                           t.Activo &&
                           t.FechaInicio <= hoy && 
                           (t.FechaFin == null || t.FechaFin >= hoy))
                .Select(t => new { 
                    t.Id, 
                    t.Nombre, 
                    t.Porcentaje,
                    t.EsTasaPorDefecto
                })
                .OrderBy(t => t.Porcentaje)
                .ToListAsync();

            return Json(tasas);
        }
    }
}