using Microsoft.AspNetCore.Mvc;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Web.Controllers
{
    public class ArticulosController : Controller
    {
        private readonly IArticuloRepository _articuloRepository;
        private const string TENANT_ID = "1";

        public ArticulosController(IArticuloRepository articuloRepository)
        {
            _articuloRepository = articuloRepository;
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
            return View();
        }

        // POST: Articulos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Articulo articulo)
        {
            Console.WriteLine($"🔧 POST Create recibido - Nombre: {articulo.Nombre}, Precio: {articulo.Precio}");
            
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
                        return View(articulo);
                    }
                }

                Console.WriteLine("🔧 Llamando Repository.CreateAsync...");
                var result = await _articuloRepository.CreateAsync(articulo);
                Console.WriteLine($"✅ Repository retornó artículo ID: {result.Id}");
                
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
            return View(articulo);
        }

        // POST: Articulos/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Articulo articulo)
        {
            Console.WriteLine($"🔧 POST Edit recibido - Route ID: {id}, Model ID: {articulo.Id}");
            Console.WriteLine($"🔧 Nombre: {articulo.Nombre}, Precio: {articulo.Precio}");

            if (id <= 0)
            {
                Console.WriteLine("❌ Route ID inválido para UPDATE");
                TempData["Error"] = "ID de artículo inválido";
                return RedirectToAction(nameof(Index));
            }

            // ✅ ASEGURAR que el articulo tenga el ID correcto
            SetIdViaReflection(articulo, id);

            ModelState.Remove("TenantId");

            if (ModelState.IsValid)
            {
                // Validar código único
                if (!string.IsNullOrEmpty(articulo.Codigo))
                {
                    if (await _articuloRepository.ExisteCodigo(articulo.Codigo, TENANT_ID, id))
                    {
                        Console.WriteLine($"❌ Código duplicado: {articulo.Codigo}");
                        ModelState.AddModelError("Codigo", "Ya existe otro artículo con este código");
                        ViewBag.ArticuloId = id;
                        await PrepararDropdownData();
                        return View(articulo);
                    }
                }

                try
                {
                    Console.WriteLine($"🔧 Llamando Repository.UpdateAsync para ID: {id}");
                    await _articuloRepository.UpdateAsync(articulo);
                    
                    Console.WriteLine($"✅ Repository UPDATE exitoso - ID: {id}");
                    TempData["Success"] = "Artículo actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error en UpdateAsync: {ex.Message}");
                    TempData["Error"] = $"Error al actualizar el artículo: {ex.Message}";
                    ViewBag.ArticuloId = id;
                    await PrepararDropdownData();
                    return View(articulo);
                }
            }
            else
            {
                Console.WriteLine("❌ ModelState inválido:");
                foreach (var error in ModelState.Where(x => x.Value.Errors.Count > 0))
                {
                    Console.WriteLine($"   - {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            ViewBag.ArticuloId = id;
            await PrepararDropdownData();
            return View(articulo);
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
    }
}