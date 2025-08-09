// Infrastructure/Repositories/ArticuloRepository.cs
using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Data;
using PeluqueriaSaaS.Infrastructure.Services;
using System.Reflection;

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
                Console.WriteLine($"🔍 DEBUGGING: Base type {entityType.BaseType?.Name}");
                Console.WriteLine($"🔍 DEBUGGING: TenantId a asignar: '{tenantId}'");

                // DEBUGGING: Listar TODOS los fields disponibles
                Console.WriteLine("🔍 DEBUGGING: Todos los fields disponibles:");
                var allFields = entityType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in allFields)
                {
                    Console.WriteLine($"   - Field: {field.Name}, Type: {field.FieldType.Name}, DeclaringType: {field.DeclaringType?.Name}");
                }

                // DEBUGGING: Listar TODAS las properties disponibles
                Console.WriteLine("🔍 DEBUGGING: Todas las properties disponibles:");
                var allProps = entityType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var prop in allProps)
                {
                    var getter = prop.GetGetMethod(true);
                    var setter = prop.GetSetMethod(true);
                    Console.WriteLine($"   - Property: {prop.Name}, Type: {prop.PropertyType.Name}, Getter: {getter != null}, Setter: {setter != null}, DeclaringType: {prop.DeclaringType?.Name}");
                }

                // ✅ TÉCNICA 1: Backing field con debugging detallado
                var backingFieldNames = new[]
                {
                    "<TenantId>k__BackingField",  // Auto-property backing field
                    "_tenantId",                   // Manual backing field lowercase
                    "_TenantId",                   // Manual backing field Pascal
                    "tenantId",                    // Field lowercase
                    "TenantId"                     // Field Pascal (unlikely pero posible)
                };

                Console.WriteLine("🔍 DEBUGGING: Intentando backing fields...");
                foreach (var fieldName in backingFieldNames)
                {
                    var backingField = entityType.GetField(fieldName, 
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    
                    Console.WriteLine($"   - Buscando field '{fieldName}': {(backingField != null ? "ENCONTRADO" : "NO ENCONTRADO")}");
                    
                    if (backingField != null)
                    {
                        Console.WriteLine($"     Type: {backingField.FieldType.Name}, CanWrite: true");
                        if (backingField.FieldType == typeof(string))
                        {
                            backingField.SetValue(entity, tenantId);
                            Console.WriteLine($"✅ SUCCESS: TenantId asignado via backing field '{fieldName}': {tenantId}");
                            
                            // Verificar que se asignó correctamente
                            var verificacion = backingField.GetValue(entity);
                            Console.WriteLine($"✅ VERIFICACIÓN: Valor después de asignar: '{verificacion}'");
                            return;
                        }
                    }
                }

                // ✅ TÉCNICA 2: Property setter con debugging
                Console.WriteLine("🔍 DEBUGGING: Intentando property setters...");
                var tenantProperty = entityType.GetProperty("TenantId", 
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (tenantProperty != null)
                {
                    Console.WriteLine($"   - Property TenantId encontrada: Type={tenantProperty.PropertyType.Name}");
                    var getter = tenantProperty.GetGetMethod(true);
                    var setter = tenantProperty.GetSetMethod(true);
                    Console.WriteLine($"   - Getter: {(getter != null ? "EXISTE" : "NO EXISTE")}");
                    Console.WriteLine($"   - Setter: {(setter != null ? "EXISTE" : "NO EXISTE")}");
                    
                    if (setter != null && tenantProperty.PropertyType == typeof(string))
                    {
                        Console.WriteLine($"   - Intentando invocar setter...");
                        setter.Invoke(entity, new object[] { tenantId });
                        Console.WriteLine($"✅ SUCCESS: TenantId asignado via property setter: {tenantId}");
                        
                        // Verificar asignación
                        var verificacion = tenantProperty.GetValue(entity);
                        Console.WriteLine($"✅ VERIFICACIÓN: Valor después de asignar: '{verificacion}'");
                        return;
                    }
                }

                // ✅ TÉCNICA 3: Jerarquía con debugging detallado
                Console.WriteLine("🔍 DEBUGGING: Intentando jerarquía de herencia...");
                var currentType = entityType;
                while (currentType != null && currentType != typeof(object))
                {
                    Console.WriteLine($"   - Explorando tipo: {currentType.Name}");
                    
                    var prop = currentType.GetProperty("TenantId", 
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    
                    if (prop != null)
                    {
                        Console.WriteLine($"     TenantId encontrado en {currentType.Name}");
                        var setMethod = prop.GetSetMethod(true);
                        if (setMethod != null)
                        {
                            Console.WriteLine($"     Setter encontrado, invocando...");
                            setMethod.Invoke(entity, new object[] { tenantId });
                            Console.WriteLine($"✅ SUCCESS: TenantId asignado via herencia en {currentType.Name}: {tenantId}");
                            
                            // Verificar asignación
                            var verificacion = prop.GetValue(entity);
                            Console.WriteLine($"✅ VERIFICACIÓN: Valor después de asignar: '{verificacion}'");
                            return;
                        }
                        else
                        {
                            Console.WriteLine($"     Setter NO encontrado en {currentType.Name}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"     TenantId NO encontrado en {currentType.Name}");
                    }
                    
                    currentType = currentType.BaseType;
                }

                Console.WriteLine("❌ FRACASO TOTAL: Ninguna técnica funcionó para asignar TenantId");
                
                // DEBUGGING FINAL: Mostrar valor actual de TenantId
                try
                {
                    var currentValue = entityType.GetProperty("TenantId")?.GetValue(entity);
                    Console.WriteLine($"🔍 VALOR ACTUAL TenantId: '{currentValue}'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ No se pudo leer valor actual TenantId: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EXCEPCIÓN en SetTenantIdRobust: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// ✅ Método que YA FUNCIONA para campos auditoria - mantenido igual
        /// </summary>
        private void SetAuditFieldsSafe(object entity, string userName, bool isCreating)
        {
            try
            {
                var entityType = entity.GetType();

                if (isCreating)
                {
                    // CreatedBy / CreadoPor
                    SetPropertySafe(entity, "CreadoPor", userName);
                    SetPropertySafe(entity, "CreatedBy", userName);
                }

                // UpdatedBy / ActualizadoPor
                SetPropertySafe(entity, "ActualizadoPor", userName);
                SetPropertySafe(entity, "UpdatedBy", userName);

                Console.WriteLine($"✅ Campos auditoria asignados - Usuario: {userName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error en campos auditoria: {ex.Message}");
            }
        }

        private void SetPropertySafe(object entity, string propertyName, object value)
        {
            try
            {
                var property = entity.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(entity, value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ No se pudo setear {propertyName}: {ex.Message}");
            }
        }

        // ===== RESTO DE MÉTODOS CRUD SIN CAMBIOS =====

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

        public async Task<Articulo> UpdateAsync(Articulo articulo)
        {
            var currentUser = await _userService.GetCurrentUserIdentifierAsync();
            
            SetAuditFieldsSafe(articulo, currentUser, false); // Solo update fields
            articulo.CalcularMargen();
            
            _context.Articulos.Update(articulo);
            await _context.SaveChangesAsync();
            return articulo;
        }

        public async Task<bool> DeleteAsync(int id, string tenantId)
        {
            var articulo = await GetByIdAsync(id, tenantId);
            if (articulo == null) return false;

            var currentUser = await _userService.GetCurrentUserIdentifierAsync();
            
            // Soft delete - usar reflection para Activo (setter privado)
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

            if (articulo.Stock < cantidad) return false; // Stock insuficiente

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