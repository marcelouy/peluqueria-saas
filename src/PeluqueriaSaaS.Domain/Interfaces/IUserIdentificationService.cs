using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Domain.Interfaces
{
    /// <summary>
    /// ✅ INTERFACE DOMAIN LAYER: Identificación y auditoría de usuarios
    /// Ubicación correcta según Clean Architecture
    /// </summary>
    public interface IUserIdentificationService
    {
        /// <summary>
        /// Obtiene el identificador único del usuario actual (email, username, etc.)
        /// </summary>
        /// <returns>Identificador del usuario o "Sistema" como fallback</returns>
        Task<string> GetCurrentUserIdentifierAsync();

        /// <summary>
        /// Obtiene el nombre completo del usuario actual para mostrar en UI
        /// </summary>
        /// <returns>Nombre completo del usuario</returns>
        Task<string> GetCurrentUserNameAsync();

        /// <summary>
        /// Obtiene la entidad Empleado correspondiente al usuario actual
        /// </summary>
        /// <returns>Empleado actual o null si no se encuentra</returns>
        Task<Empleado?> GetCurrentEmpleadoAsync();

        /// <summary>
        /// Obtiene el TenantId del contexto actual
        /// </summary>
        /// <returns>TenantId actual</returns>
        Task<string> GetTenantIdAsync();

        /// <summary>
        /// Registra una acción de auditoría para trazabilidad
        /// </summary>
        /// <param name="action">Acción realizada (CREATE, UPDATE, DELETE, etc.)</param>
        /// <param name="entityName">Nombre de la entidad afectada</param>
        /// <param name="entityId">ID de la entidad</param>
        /// <param name="details">Detalles adicionales de la operación</param>
        void LogAuditoriaAction(string action, string entityName, object entityId, string details = "");

        /// <summary>
        /// Verifica si el usuario actual tiene permisos de administrador
        /// </summary>
        /// <returns>True si es administrador</returns>
        Task<bool> IsCurrentUserAdminAsync();

        /// <summary>
        /// Obtiene información detallada del contexto actual para auditoría
        /// </summary>
        /// <returns>Información del contexto (IP, User Agent, etc.)</returns>
        Task<AuditoriaContext> GetAuditoriaContextAsync();
    }

    /// <summary>
    /// ✅ VALUE OBJECT: Contexto de auditoría
    /// </summary>
    public class AuditoriaContext
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? EmpleadoId { get; set; }
        public string? EmpleadoNombre { get; set; }
    }
}