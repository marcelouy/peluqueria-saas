using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Infrastructure.Services
{
    /// <summary>
    /// ✅ IMPLEMENTACIÓN SIMPLIFICADA: Sin dependencias AspNetCore
    /// Para resolver problemas de compilación temporalmente
    /// </summary>
    public class UserIdentificationService : IUserIdentificationService
    {
        private readonly IEmpleadoRepository _empleadoRepository;
        private const string DEFAULT_TENANT_ID = "1";
        private const string SYSTEM_USER = "Sistema";
        private const string DEFAULT_EMPLEADO_EMAIL = "admin@peluqueria.com";

        public UserIdentificationService(IEmpleadoRepository empleadoRepository)
        {
            _empleadoRepository = empleadoRepository;
        }

        // ✅ MÉTODO PRINCIPAL: Obtener identificador usuario actual (versión simplificada)
        public async Task<string> GetCurrentUserIdentifierAsync()
        {
            try
            {
                // TEMPORALMENTE: Usar empleado por defecto
                // TODO: Implementar autenticación real más adelante
                var empleadoDefault = await GetEmpleadoDefaultAsync();
                if (empleadoDefault != null)
                {
                    var identifier = $"{empleadoDefault.Nombre} {empleadoDefault.Apellido}".Trim();
                    Console.WriteLine($"🔐 Usuario identificado via Empleado Default: {identifier}");
                    return identifier;
                }

                Console.WriteLine($"⚠️ No se pudo identificar usuario, usando: {SYSTEM_USER}");
                return SYSTEM_USER;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error identificando usuario: {ex.Message}");
                return SYSTEM_USER;
            }
        }

        // ✅ MÉTODO: Obtener nombre completo usuario
        public async Task<string> GetCurrentUserNameAsync()
        {
            try
            {
                var empleado = await GetCurrentEmpleadoAsync();
                if (empleado != null)
                {
                    return $"{empleado.Nombre} {empleado.Apellido}".Trim();
                }

                var identifier = await GetCurrentUserIdentifierAsync();
                return identifier;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error obteniendo nombre usuario: {ex.Message}");
                return SYSTEM_USER;
            }
        }

        // ✅ MÉTODO: Obtener empleado actual
        public async Task<Empleado?> GetCurrentEmpleadoAsync()
        {
            try
            {
                var tenantId = await GetTenantIdAsync();
                
                // TEMPORALMENTE: Usar empleado administrador por defecto
                // TODO: Implementar lógica de autenticación real
                var empleadoDefault = await GetEmpleadoDefaultAsync();
                if (empleadoDefault != null)
                {
                    Console.WriteLine($"🔐 Usando empleado default: {empleadoDefault.Nombre}");
                    return empleadoDefault;
                }

                Console.WriteLine($"⚠️ No se encontró empleado actual");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error obteniendo empleado actual: {ex.Message}");
                return null;
            }
        }

        // ✅ MÉTODO: Obtener TenantId actual
        public async Task<string> GetTenantIdAsync()
        {
            try
            {
                // TODO: Implementar lógica multi-tenant real
                await Task.CompletedTask; // Para hacer método async real
                return DEFAULT_TENANT_ID;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error obteniendo TenantId: {ex.Message}");
                return DEFAULT_TENANT_ID;
            }
        }

        // ✅ MÉTODO: Log de auditoría
        public void LogAuditoriaAction(string action, string entityName, object entityId, string details = "")
        {
            try
            {
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                var logMessage = $"[{timestamp}] 🔍 AUDITORIA: {action} {entityName} ID:{entityId}";
                
                if (!string.IsNullOrEmpty(details))
                {
                    logMessage += $" - {details}";
                }

                Console.WriteLine(logMessage);
                
                // TODO: Implementar log persistente en BD o archivo
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error logging auditoria: {ex.Message}");
            }
        }

        // ✅ MÉTODO: Verificar si usuario actual es administrador
        public async Task<bool> IsCurrentUserAdminAsync()
        {
            try
            {
                var empleado = await GetCurrentEmpleadoAsync();
                if (empleado == null) return false;

                // Verificar por cargo
                var cargo = empleado.Cargo?.ToLower() ?? "";
                return cargo.Contains("admin") || 
                       cargo.Contains("gerente") || 
                       cargo.Contains("supervisor");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error verificando admin: {ex.Message}");
                return false;
            }
        }

        // ✅ MÉTODO: Obtener contexto completo de auditoría (versión simplificada)
        public async Task<AuditoriaContext> GetAuditoriaContextAsync()
        {
            try
            {
                var context = new AuditoriaContext();
                
                // Información usuario
                context.UsuarioId = await GetCurrentUserIdentifierAsync();
                context.UsuarioNombre = await GetCurrentUserNameAsync();
                context.TenantId = await GetTenantIdAsync();
                
                // Información empleado
                var empleado = await GetCurrentEmpleadoAsync();
                if (empleado != null)
                {
                    context.EmpleadoId = empleado.Id.ToString();
                    context.EmpleadoNombre = $"{empleado.Nombre} {empleado.Apellido}".Trim();
                }
                
                // Información técnica (simplificada)
                context.IpAddress = "Local";
                context.UserAgent = "PeluqueriaSaaS";
                
                return context;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error obteniendo contexto auditoría: {ex.Message}");
                return new AuditoriaContext
                {
                    UsuarioId = SYSTEM_USER,
                    UsuarioNombre = SYSTEM_USER,
                    TenantId = DEFAULT_TENANT_ID
                };
            }
        }

        // ✅ MÉTODO PRIVADO: Obtener empleado administrador por defecto (CORREGIDO)
        private async Task<Empleado?> GetEmpleadoDefaultAsync()
        {
            try
            {
                // ✅ CORREGIDO: Llamar al método que realmente existe y funciona
                var empleados = await _empleadoRepository.GetActivosAsync();
                
                // Buscar administrador por email conocido
                var admin = empleados.FirstOrDefault(e => 
                    e.Email?.Equals(DEFAULT_EMPLEADO_EMAIL, StringComparison.OrdinalIgnoreCase) == true);
                
                if (admin != null)
                {
                    return admin;
                }

                // Buscar por cargo "Administrador" o "Gerente"
                admin = empleados.FirstOrDefault(e => 
                    e.Cargo?.Contains("Admin", StringComparison.OrdinalIgnoreCase) == true ||
                    e.Cargo?.Contains("Gerente", StringComparison.OrdinalIgnoreCase) == true);
                
                if (admin != null)
                {
                    return admin;
                }

                // Fallback: Primer empleado activo
                return empleados.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error obteniendo empleado default: {ex.Message}");
                return null;
            }
        }
    }
}