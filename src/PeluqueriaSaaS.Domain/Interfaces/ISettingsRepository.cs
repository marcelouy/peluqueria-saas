// src/PeluqueriaSaaS.Domain/Interfaces/ISettingsRepository.cs
using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Domain.Interfaces
{
    /// <summary>
    /// ✅ ISETTINGS REPOSITORY INTERFACE - Domain layer
    /// Propósito: Contract para Settings data access operations
    /// Ubicación: Domain/Interfaces (siguiendo clean architecture)
    /// Pattern: Consistente con IEmpleadoRepository, IServicioRepository, etc.
    /// </summary>
    public interface ISettingsRepository
    {
        /// <summary>
        /// Obtener settings principal (primer registro activo)
        /// </summary>
        Task<Settings?> GetSettingsAsync();

        /// <summary>
        /// Obtener settings por código peluquería (multi-tenant futuro)
        /// </summary>
        Task<Settings?> GetSettingsByCodigoAsync(string codigo);

        /// <summary>
        /// Crear nuevos settings
        /// </summary>
        Task<Settings> CreateSettingsAsync(Settings settings);

        /// <summary>
        /// Actualizar settings existente
        /// </summary>
        Task<Settings> UpdateSettingsAsync(Settings settings);

        /// <summary>
        /// Eliminar settings (soft delete)
        /// </summary>
        Task<bool> DeleteSettingsAsync(int id);

        /// <summary>
        /// Verificar si existen settings
        /// </summary>
        Task<bool> ExistenSettingsAsync();

        /// <summary>
        /// Obtener o crear settings default
        /// Útil para inicialización automática
        /// </summary>
        Task<Settings> GetOrCreateDefaultSettingsAsync();
    }
}