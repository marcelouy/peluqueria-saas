using Microsoft.AspNetCore.Mvc;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Web.Controllers
{
    public class ArticulosController : Controller
    {
        private readonly IArticuloRepository _articuloRepository;
        private const string TENANT_ID = "1"; // Para filtros y validaciones

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

        // POST: Articulos/Create - ✅ VERSIÓN CON DEBUGGING + FIX TENANTID
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Articulo articulo)
        {
            Console.WriteLine($"🔧 POST Create recibido - Nombre: {articulo.Nombre}, Precio: {articulo.Precio}");
            
            // ✅ FIX: Remover error TenantId del ModelState (Repository lo asignará)
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
                // ✅ Repository maneja TenantId automáticamente
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
            var articulo = await _articuloRepository.GetByIdAsync(id, TENANT_ID);
            if (articulo == null)
            {
                TempData["Error"] = "Artículo no encontrado";
                return RedirectToAction(nameof(Index));
            }

            await PrepararDropdownData();
            return View(articulo);
        }

        // POST: Articulos/Edit/5 - ✅ VERSIÓN SIMPLIFICADA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Articulo articulo)
        {
            Console.WriteLine($"🔧 POST Edit recibido - Artículo ID: {articulo.Id}, Nombre: {articulo.Nombre}");
            
            // ✅ FIX: Remover error TenantId del ModelState (igual que CREATE)
            ModelState.Remove("TenantId");
            Console.WriteLine("✅ TenantId removido del ModelState");

            if (ModelState.IsValid)
            {
                Console.WriteLine("✅ ModelState válido para UPDATE");
                
                // ✅ FIX: Validación código solo si CAMBIÓ desde original
                if (!string.IsNullOrEmpty(articulo.Codigo))
                {
                    // Obtener artículo original para comparar código
                    var articuloOriginal = await _articuloRepository.GetByIdAsync(articulo.Id, TENANT_ID);
                    
                    // Solo validar si código cambió
                    if (articuloOriginal != null && articulo.Codigo != articuloOriginal.Codigo)
                    {
                        Console.WriteLine($"🔍 Código cambió de '{articuloOriginal.Codigo}' a '{articulo.Codigo}' - validando unicidad...");
                        
                        if (await _articuloRepository.ExisteCodigo(articulo.Codigo, TENANT_ID))
                        {
                            Console.WriteLine($"❌ Código duplicado en UPDATE: {articulo.Codigo}");
                            ModelState.AddModelError("Codigo", "Ya existe un artículo con este código");
                            await PrepararDropdownData();
                            return View(articulo);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"✅ Código no cambió ('{articulo.Codigo}') - omitiendo validación");
                    }
                }

                Console.WriteLine("🔧 Llamando Repository.UpdateAsync...");
                // ✅ Repository maneja TenantId automáticamente
                var result = await _articuloRepository.UpdateAsync(articulo);
                Console.WriteLine($"✅ Repository UPDATE exitoso - ID: {result.Id}");
                
                TempData["Success"] = "Artículo actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                Console.WriteLine("❌ ModelState inválido en UPDATE:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"   {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            await PrepararDropdownData();
            return View(articulo);
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
            }
            catch (Exception)
            {
                // Si falla, inicializar con listas vacías
                ViewBag.Categorias = new List<string>();
                ViewBag.Marcas = new List<string>();
                ViewBag.Proveedores = new List<string>();
            }
        }
    }
}