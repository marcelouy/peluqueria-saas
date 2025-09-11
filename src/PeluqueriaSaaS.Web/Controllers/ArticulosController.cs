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

        [HttpGet]
        public async Task<IActionResult> GetArticulosParaVenta()
        {
            try
            {
                var articulos = await _context.Articulos
                    .Where(a => a.Activo && a.TenantId == "default")
                    .Select(a => new
                    {
                        id = a.Id,
                        codigo = a.Codigo,
                        nombre = a.Nombre,
                        precio = a.Precio,
                        stock = a.Stock,
                        categoria = a.Categoria
                    })
                    .OrderBy(a => a.nombre)
                    .ToListAsync();

                return Json(articulos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetArticulosParaVenta: {ex.Message}");
                return Json(new { error = ex.Message });
            }
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
        // POST: Articulos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Articulo articulo, int[] impuestosSeleccionados)
        {
            Console.WriteLine($"=====================================");
            Console.WriteLine($"🔧 POST Edit recibido - Route ID: {id}, Model ID: {articulo.Id}");
            Console.WriteLine($"📋 Datos del artículo:");
            Console.WriteLine($"   - Nombre: {articulo.Nombre}");
            Console.WriteLine($"   - Precio: {articulo.Precio}");
            Console.WriteLine($"   - Categoría: {articulo.Categoria}");
            Console.WriteLine($"📋 Impuestos seleccionados: {(impuestosSeleccionados == null || impuestosSeleccionados.Length == 0 ? "NINGUNO" : string.Join(", ", impuestosSeleccionados))}");
            Console.WriteLine($"=====================================");

            // Validación inicial del ID
            if (id <= 0)
            {
                Console.WriteLine("❌ Route ID inválido para UPDATE");
                TempData["Error"] = "ID de artículo inválido";
                return RedirectToAction(nameof(Index));
            }

            // Asegurar que el artículo tenga el ID correcto
            SetIdViaReflection(articulo, id);
            Console.WriteLine($"✅ ID asignado via reflection: {id}");

            // Remover validaciones que no aplican
            ModelState.Remove("TenantId");
            ModelState.Remove("ArticulosImpuestos");
            ModelState.Remove("CreadoPor");
            ModelState.Remove("ActualizadoPor");
            ModelState.Remove("FechaCreacion");
            ModelState.Remove("FechaActualizacion");

            // Validar ModelState
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

            // Validar código único (si aplica)
            if (!string.IsNullOrEmpty(articulo.Codigo))
            {
                var codigoExiste = await _articuloRepository.ExisteCodigo(articulo.Codigo, TENANT_ID, id);
                if (codigoExiste)
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
                // PASO 1: Verificar estado ANTES de actualizar
                Console.WriteLine($"🔄 PASO 1: Verificando estado actual en BD...");
                var articuloAntes = await _context.Articulos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (articuloAntes == null)
                {
                    Console.WriteLine($"❌ Artículo ID {id} no existe en BD");
                    TempData["Error"] = "El artículo no existe";
                    return RedirectToAction(nameof(Index));
                }

                Console.WriteLine($"   BD ANTES: Nombre='{articuloAntes.Nombre}', Precio={articuloAntes.Precio}");

                var impuestosAntes = await _context.ArticulosImpuestos
                    .Where(ai => ai.ArticuloId == id && ai.Activo)
                    .Select(ai => ai.TasaImpuestoId)
                    .ToListAsync();
                Console.WriteLine($"   Impuestos ANTES: {(impuestosAntes.Any() ? string.Join(",", impuestosAntes) : "NINGUNO")}");

                // PASO 2: Actualizar el artículo
                Console.WriteLine($"🔄 PASO 2: Actualizando artículo...");
                await _articuloRepository.UpdateAsync(articulo);
                Console.WriteLine($"✅ Artículo actualizado en repository");

                // PASO 3: Verificar que el artículo se actualizó
                var articuloDespues = await _context.Articulos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == id);
                Console.WriteLine($"   BD DESPUÉS: Nombre='{articuloDespues?.Nombre}', Precio={articuloDespues?.Precio}");

                // PASO 4: Actualizar impuestos
                Console.WriteLine($"🔄 PASO 3: Actualizando impuestos...");
                Console.WriteLine($"   Impuestos a guardar: {(impuestosSeleccionados == null || impuestosSeleccionados.Length == 0 ? "NINGUNO" : string.Join(",", impuestosSeleccionados))}");

                await ActualizarImpuestosArticulo(id, impuestosSeleccionados);

                // PASO 5: Verificar impuestos finales
                Console.WriteLine($"🔄 PASO 4: Verificando impuestos finales...");
                var impuestosDespues = await _context.ArticulosImpuestos
                    .Where(ai => ai.ArticuloId == id && ai.Activo)
                    .Select(ai => new { ai.TasaImpuestoId, ai.FechaInicioAplicacion })
                    .ToListAsync();

                Console.WriteLine($"   Impuestos DESPUÉS: {impuestosDespues.Count} activos");
                foreach (var imp in impuestosDespues)
                {
                    Console.WriteLine($"      - TasaId: {imp.TasaImpuestoId}, Fecha: {imp.FechaInicioAplicacion}");
                }

                Console.WriteLine($"✅ PROCESO COMPLETADO EXITOSAMENTE");
                TempData["Success"] = "Artículo actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR EN UPDATE:");
                Console.WriteLine($"   Tipo: {ex.GetType().Name}");
                Console.WriteLine($"   Mensaje: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
                }

                TempData["Error"] = $"Error al actualizar: {ex.Message}";
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
                    // Verificar que no exista ya
                    var existe = await _context.ArticulosImpuestos
                        .AnyAsync(ai => ai.ArticuloId == articuloId &&
                                       ai.TasaImpuestoId == tasaId &&
                                       ai.TenantId == TENANT_ID &&
                                       ai.Activo);

                    if (!existe)
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
                        SetProtectedProperty(articuloImpuesto, "FechaActualizacion", hoy);
                        SetProtectedProperty(articuloImpuesto, "ActualizadoPor", "Sistema");

                        _context.ArticulosImpuestos.Add(articuloImpuesto);
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ {tasasIds.Length} impuestos guardados para artículo {articuloId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error guardando impuestos: {ex.Message}");
                throw; // Propagar para manejar en el método que llama
            }
        }


        // AGREGAR este método TEMPORAL en ArticulosController.cs para diagnosticar
        // NO reemplazar nada, solo agregar para debug

        private async Task DiagnosticarTablaImpuestos(int articuloId)
        {
            try
            {
                Console.WriteLine($"🔍 === DIAGNÓSTICO DE TABLA ArticulosImpuestos ===");

                // 1. Verificar estructura de la tabla
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = @"
            SELECT COLUMN_NAME 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'ArticulosImpuestos'
            ORDER BY ORDINAL_POSITION";

                Console.WriteLine("📊 Columnas en la tabla:");
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Console.WriteLine($"   - {reader.GetString(0)}");
                }
                reader.Close();

                // 2. Ver qué datos hay actualmente
                command.CommandText = @"
            SELECT TOP 5 * 
            FROM ArticulosImpuestos 
            WHERE ArticuloId = @articuloId";
                command.Parameters.Clear();
                command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@articuloId", articuloId));

                Console.WriteLine($"📋 Registros actuales para artículo {articuloId}:");
                using var reader2 = await command.ExecuteReaderAsync();
                if (!reader2.HasRows)
                {
                    Console.WriteLine("   (No hay registros)");
                }
                else
                {
                    while (await reader2.ReadAsync())
                    {
                        Console.WriteLine($"   ID: {reader2["Id"]}, TasaId: {reader2["TasaImpuestoId"]}, Activo: {reader2["Activo"]}");
                    }
                }

                await connection.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en diagnóstico: {ex.Message}");
            }
        }

        private async Task ActualizarImpuestosArticulo(int articuloId, int[]? tasasIds)
        {
            Console.WriteLine($"   === ActualizarImpuestosArticulo START (OPTIMIZADO v3) ===");
            Console.WriteLine($"   ArticuloId: {articuloId}");
            Console.WriteLine($"   Tasas recibidas: {(tasasIds == null || tasasIds.Length == 0 ? "NULL/EMPTY" : string.Join(",", tasasIds))}");

            try
            {
                // Crear una conexión independiente para no afectar el DbContext
                var connectionString = _context.Database.GetConnectionString();

                using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // PASO 1: OBTENER IMPUESTOS ACTUALES ACTIVOS
                    var impuestosActuales = new List<int>();
                    using (var queryCommand = connection.CreateCommand())
                    {
                        queryCommand.CommandText = @"
                    SELECT TasaImpuestoId 
                    FROM ArticulosImpuestos 
                    WHERE ArticuloId = @articuloId 
                        AND TenantId = @tenantId 
                        AND Activo = 1
                    ORDER BY TasaImpuestoId";

                        queryCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@articuloId", articuloId));
                        queryCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@tenantId", TENANT_ID));

                        using (var reader = await queryCommand.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                impuestosActuales.Add(reader.GetInt32(0));
                            }
                        }
                    }

                    Console.WriteLine($"   Impuestos ACTUALES en BD: {(impuestosActuales.Any() ? string.Join(",", impuestosActuales) : "NINGUNO")}");

                    // PASO 2: PREPARAR Y COMPARAR CON NUEVOS
                    var nuevosImpuestos = tasasIds?.OrderBy(x => x).ToList() ?? new List<int>();
                    Console.WriteLine($"   Impuestos NUEVOS solicitados: {(nuevosImpuestos.Any() ? string.Join(",", nuevosImpuestos) : "NINGUNO")}");

                    // VERIFICAR SI SON IGUALES
                    bool sonIguales = impuestosActuales.Count == nuevosImpuestos.Count &&
                                     impuestosActuales.SequenceEqual(nuevosImpuestos);

                    if (sonIguales)
                    {
                        Console.WriteLine($"   ✅ Los impuestos NO han cambiado. No se requiere actualización.");
                        Console.WriteLine($"   === ActualizarImpuestosArticulo END (SIN CAMBIOS) ===");
                        return; // SALIR SIN HACER NADA - OPTIMIZACIÓN CLAVE
                    }

                    // PASO 3: HAY CAMBIOS - PROCEDER CON ACTUALIZACIÓN
                    Console.WriteLine($"   ⚠️ Detectados cambios en impuestos. Actualizando...");

                    // Calcular diferencias
                    var paraDesactivar = impuestosActuales.Except(nuevosImpuestos).ToList();
                    var paraAgregar = nuevosImpuestos.Except(impuestosActuales).ToList();

                    Console.WriteLine($"   📊 Análisis de cambios:");
                    Console.WriteLine($"      - Para DESACTIVAR: {(paraDesactivar.Any() ? string.Join(",", paraDesactivar) : "ninguno")}");
                    Console.WriteLine($"      - Para AGREGAR: {(paraAgregar.Any() ? string.Join(",", paraAgregar) : "ninguno")}");
                    Console.WriteLine($"      - Se MANTIENEN: {string.Join(",", impuestosActuales.Intersect(nuevosImpuestos))}");

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // DESACTIVAR solo los que se quitaron
                            if (paraDesactivar.Any())
                            {
                                foreach (var tasaId in paraDesactivar)
                                {
                                    using (var updateCommand = connection.CreateCommand())
                                    {
                                        updateCommand.Transaction = transaction;
                                        updateCommand.CommandText = @"
                                    UPDATE ArticulosImpuestos 
                                    SET Activo = 0, 
                                        FechaFinAplicacion = GETDATE(),
                                        FechaActualizacion = GETDATE(),
                                        ActualizadoPor = 'Sistema'
                                    WHERE ArticuloId = @articuloId 
                                        AND TasaImpuestoId = @tasaId
                                        AND TenantId = @tenantId 
                                        AND Activo = 1";

                                        updateCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@articuloId", articuloId));
                                        updateCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@tasaId", tasaId));
                                        updateCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@tenantId", TENANT_ID));

                                        var rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                                        Console.WriteLine($"      ✅ TasaId {tasaId} desactivado (rows: {rowsAffected})");
                                    }
                                }
                            }

                            // AGREGAR solo los nuevos
                            if (paraAgregar.Any())
                            {
                                foreach (var tasaId in paraAgregar)
                                {
                                    using (var insertCommand = connection.CreateCommand())
                                    {
                                        insertCommand.Transaction = transaction;
                                        insertCommand.CommandText = @"
                                    INSERT INTO ArticulosImpuestos 
                                    (ArticuloId, TasaImpuestoId, TenantId, FechaInicioAplicacion, 
                                     Activo, FechaCreacion, CreadoPor, FechaActualizacion, ActualizadoPor)
                                    VALUES 
                                    (@articuloId, @tasaId, @tenantId, GETDATE(), 
                                     1, GETDATE(), 'Sistema', GETDATE(), 'Sistema')";

                                        insertCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@articuloId", articuloId));
                                        insertCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@tasaId", tasaId));
                                        insertCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@tenantId", TENANT_ID));

                                        await insertCommand.ExecuteNonQueryAsync();
                                        Console.WriteLine($"      ✅ TasaId {tasaId} agregado");
                                    }
                                }
                            }

                            // Confirmar transacción
                            await transaction.CommitAsync();
                            Console.WriteLine($"   ✅ Transacción confirmada - Cambios aplicados exitosamente");

                            // Verificación final
                            using (var verifyCommand = connection.CreateCommand())
                            {
                                verifyCommand.CommandText = @"
                            SELECT COUNT(*) 
                            FROM ArticulosImpuestos 
                            WHERE ArticuloId = @articuloId 
                                AND TenantId = @tenantId 
                                AND Activo = 1";

                                verifyCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@articuloId", articuloId));
                                verifyCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@tenantId", TENANT_ID));

                                var totalActivos = await verifyCommand.ExecuteScalarAsync();
                                Console.WriteLine($"   📊 Total impuestos activos finales: {totalActivos}");
                            }
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            Console.WriteLine($"   ❌ ERROR en transacción - Rollback ejecutado");
                            Console.WriteLine($"      Mensaje: {ex.Message}");
                            throw;
                        }
                    }
                }

                Console.WriteLine($"   === ActualizarImpuestosArticulo END (CON CAMBIOS APLICADOS) ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ ERROR en ActualizarImpuestosArticulo:");
                Console.WriteLine($"      Tipo: {ex.GetType().Name}");
                Console.WriteLine($"      Mensaje: {ex.Message}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"      Inner: {ex.InnerException.Message}");
                }

                // NO lanzar excepción para que el artículo se guarde aunque fallen los impuestos
                // Pero registrar el error para debugging
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