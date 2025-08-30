// Infrastructure/Repositories/ArticuloRepository.cs
using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Data;
using PeluqueriaSaaS.Infrastructure.Services;
using System.Reflection;
using PeluqueriaSaaS.Application.Services;

namespace PeluqueriaSaaS.Infrastructure.Repositories
{
    public class ArticuloRepository : IArticuloRepository
    {
        private readonly PeluqueriaDbContext _context;
        private readonly IUserIdentificationService _userService;

        public ArticuloRepository(PeluqueriaDbContext context, IUserIdentificationService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<Articulo> CreateAsync(Articulo articulo)
        {
            // ✅ Obtener datos usuario (esto funciona perfectamente)
            var currentUser = await _userService.GetCurrentUserIdentifierAsync();
            var tenantId = await _userService.GetTenantIdAsync();
            
            Console.WriteLine($"🔐 Usuario identificado: {currentUser}");
            Console.WriteLine($"🏢 TenantId obtenido: {tenantId}");

            // ✅ SOLUCIÓN: TenantId assignment con reflection robusta
            SetTenantIdRobust(articulo, tenantId);
            
            // ✅ Campos auditoria (estos ya funcionan)
            SetAuditFieldsSafe(articulo, currentUser, true);
            
            // Lógica de negocio
            articulo.CalcularMargen();
            
            _context.Articulos.Add(articulo);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"✅ Artículo creado exitosamente - ID: {articulo.Id}");
            return articulo;
        }

        public async Task<Articulo> UpdateAsync(Articulo articulo)
        {
            Console.WriteLine($"🔧 UpdateAsync iniciado - ID: {articulo.Id}");

            // VALIDACIÓN: ID debe ser > 0
            if (articulo.Id <= 0)
            {
                var errorMsg = $"ID inválido para UPDATE: {articulo.Id}";
                Console.WriteLine($"❌ {errorMsg}");
                throw new ArgumentException(errorMsg, nameof(articulo));
            }

            try
            {
                // CLEAR tracking conflicts
                Console.WriteLine($"🔧 Clearing change tracker...");
                _context.ChangeTracker.Clear();

                // Obtener entidad existente para UPDATE manual
                var existingEntity = await _context.Articulos
                    .FirstOrDefaultAsync(a => a.Id == articulo.Id && a.TenantId == "1");

                if (existingEntity == null)
                {
                    var errorMsg = $"Artículo no encontrado para UPDATE - ID: {articulo.Id}";
                    Console.WriteLine($"❌ {errorMsg}");
                    throw new InvalidOperationException(errorMsg);
                }

                Console.WriteLine($"✅ Entidad existente encontrada - preservando audit fields");

                // Actualizar propiedades manualmente (preservar campos críticos)
                existingEntity.Nombre = articulo.Nombre;
                existingEntity.Descripcion = articulo.Descripcion;
                existingEntity.Precio = articulo.Precio;
                existingEntity.PrecioCosto = articulo.PrecioCosto;
                existingEntity.Categoria = articulo.Categoria;
                existingEntity.Marca = articulo.Marca;
                existingEntity.Proveedor = articulo.Proveedor;
                existingEntity.Stock = articulo.Stock;
                existingEntity.StockMinimo = articulo.StockMinimo;
                existingEntity.Oferta = articulo.Oferta;
                existingEntity.PrecioOferta = articulo.PrecioOferta;
                existingEntity.RequiereStock = articulo.RequiereStock;
                existingEntity.Peso = articulo.Peso;
                existingEntity.Contenido = articulo.Contenido;

                // Actualizar campos auditoria (preservar CreadoPor)
                var currentUser = await _userService.GetCurrentUserIdentifierAsync();
                SetAuditFieldsSafe(existingEntity, currentUser, false);

                // Calcular margen
                existingEntity.CalcularMargen();

                Console.WriteLine($"✅ Propiedades actualizadas - Usuario: {currentUser}");

                // EF Core detecta cambios automáticamente
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ UPDATE ejecutado exitoso - ID: {articulo.Id}");
                return existingEntity;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en UpdateAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 🔍 MÉTODO ULTRA-ROBUSTA: Debugging profundo para TenantId assignment
        /// </summary>
        private void SetTenantIdRobust(object entity, string tenantId)
        {
            if (entity == null || string.IsNullOrEmpty(tenantId))
            {
                Console.WriteLine("❌ Entity o TenantId null/empty");
                return;
            }

            try
            {
                var entityType = entity.GetType();
                Console.WriteLine($"🔍 DEBUGGING: Entidad tipo {entityType.Name}");
                Console.WriteLine($"🔍 DEBUGGING: TenantId a asignar: '{tenantId}'");

                // ✅ TÉCNICA 1: Backing field
                var backingFieldNames = new[]
                {
                    "<TenantId>k__BackingField",
                    "_tenantId",
                    "_TenantId",
                    "tenantId",
                    "TenantId"
                };

                foreach (var fieldName in backingFieldNames)
                {
                    var backingField = entityType.GetField(fieldName, 
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    
                    if (backingField != null && backingField.FieldType == typeof(string))
                    {
                        backingField.SetValue(entity, tenantId);
                        Console.WriteLine($"✅ SUCCESS: TenantId asignado via backing field '{fieldName}': {tenantId}");
                        return;
                    }
                }

                // ✅ TÉCNICA 2: Property setter
                var tenantProperty = entityType.GetProperty("TenantId", 
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (tenantProperty != null)
                {
                    var setter = tenantProperty.GetSetMethod(true);
                    if (setter != null && tenantProperty.PropertyType == typeof(string))
                    {
                        setter.Invoke(entity, new object[] { tenantId });
                        Console.WriteLine($"✅ SUCCESS: TenantId asignado via property setter: {tenantId}");
                        return;
                    }
                }

                // ✅ TÉCNICA 3: Jerarquía de herencia
                var currentType = entityType;
                while (currentType != null && currentType != typeof(object))
                {
                    var prop = currentType.GetProperty("TenantId", 
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    
                    if (prop != null)
                    {
                        var setMethod = prop.GetSetMethod(true);
                        if (setMethod != null)
                        {
                            setMethod.Invoke(entity, new object[] { tenantId });
                            Console.WriteLine($"✅ SUCCESS: TenantId asignado via herencia en {currentType.Name}: {tenantId}");
                            return;
                        }
                    }
                    
                    currentType = currentType.BaseType;
                }

                Console.WriteLine("❌ No se pudo asignar TenantId");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en SetTenantIdRobust: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ Método audit fields para CREATE + UPDATE con reflection robusta
        /// </summary>
        private void SetAuditFieldsSafe(object entity, string userName, bool isCreating)
        {
            try
            {
                Console.WriteLine($"🔧 SetAuditFieldsSafe - isCreating: {isCreating}, Usuario: {userName}");

                if (isCreating)
                {
                    // CREATE mode: Setear campos creación
                    SetPropertySafe(entity, "CreadoPor", userName);
                    SetPropertySafe(entity, "FechaCreacion", DateTime.UtcNow);
                }

                // ALWAYS: Setear campos actualización
                SetPropertySafe(entity, "ActualizadoPor", userName);
                SetPropertySafe(entity, "FechaActualizacion", DateTime.UtcNow);

                Console.WriteLine($"✅ Campos auditoria asignados - Usuario: {userName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error en campos auditoria: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ Reflection robusta para protected setters
        /// </summary>
        private void SetPropertySafe(object entity, string propertyName, object? value)
        {
            try
            {
                var entityType = entity.GetType();
                
                // Buscar property en toda la jerarquía
                var property = entityType.GetProperty(propertyName, 
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (property != null)
                {
                    // Intentar setter público primero
                    if (property.CanWrite && property.GetSetMethod() != null)
                    {
                        property.SetValue(entity, value);
                        Console.WriteLine($"✅ {propertyName} asignado via public setter: {value}");
                        return;
                    }
                    
                    // Si setter público no disponible, usar reflection para protected setter
                    var setter = property.GetSetMethod(true);
                    if (setter != null)
                    {
                        setter.Invoke(entity, new object?[] { value });
                        Console.WriteLine($"✅ {propertyName} asignado via protected setter: {value}");
                        return;
                    }
                }
                
                Console.WriteLine($"⚠️ No se pudo asignar {propertyName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error asignando {propertyName}: {ex.Message}");
            }
        }

        // ===== RESTO DE MÉTODOS CRUD =====

        public async Task<IEnumerable<Articulo>> GetAllAsync(string tenantId)
        {
            return await _context.Articulos
                .Where(a => a.TenantId == tenantId && a.Activo)
                .OrderBy(a => a.Nombre)
                .ToListAsync();
        }

        public async Task<Articulo?> GetByIdAsync(int id, string tenantId)
        {
            return await _context.Articulos
                .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);
        }

        public async Task<bool> DeleteAsync(int id, string tenantId)
        {
            var articulo = await GetByIdAsync(id, tenantId);
            if (articulo == null) return false;

            var currentUser = await _userService.GetCurrentUserIdentifierAsync();
            
            SetPropertySafe(articulo, "Activo", false);
            SetAuditFieldsSafe(articulo, currentUser, false);
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Articulo>> GetActivosAsync(string tenantId)
        {
            return await _context.Articulos
                .Where(a => a.TenantId == tenantId && a.Activo)
                .OrderBy(a => a.Nombre)
                .ToListAsync();
        }

        public async Task<bool> ExisteCodigo(string codigo, string tenantId, int? excludeId = null)
        {
            if (string.IsNullOrEmpty(codigo)) return false;

            var query = _context.Articulos
                .Where(a => a.TenantId == tenantId && a.Codigo == codigo && a.Activo);

            if (excludeId.HasValue)
                query = query.Where(a => a.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Articulo>> GetBajoStockAsync(string tenantId)
        {
            return await _context.Articulos
                .Where(a => a.TenantId == tenantId && a.Activo && 
                           a.RequiereStock && a.Stock <= a.StockMinimo)
                .OrderBy(a => a.Stock)
                .ToListAsync();
        }

        public async Task<bool> DescontarStock(int articuloId, int cantidad, string tenantId)
        {
            var articulo = await GetByIdAsync(articuloId, tenantId);
            if (articulo == null || !articulo.RequiereStock) return false;

            if (articulo.Stock < cantidad) return false;

            articulo.Stock -= cantidad;
            
            var currentUser = await _userService.GetCurrentUserIdentifierAsync();
            SetAuditFieldsSafe(articulo, currentUser, false);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<string>> GetCategoriasAsync(string tenantId)
        {
            return await _context.Articulos
                .Where(a => a.TenantId == tenantId && a.Activo && !string.IsNullOrEmpty(a.Categoria))
                .Select(a => a.Categoria!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetMarcasAsync(string tenantId)
        {
            return await _context.Articulos
                .Where(a => a.TenantId == tenantId && a.Activo && !string.IsNullOrEmpty(a.Marca))
                .Select(a => a.Marca!)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetProveedoresAsync(string tenantId)
        {
            return await _context.Articulos
                .Where(a => a.TenantId == tenantId && a.Activo && !string.IsNullOrEmpty(a.Proveedor))
                .Select(a => a.Proveedor!)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
        }
    }
}