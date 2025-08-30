using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Domain.Interfaces
{
    public interface IEmpleadoRepository
    {
        // ✅ MÉTODOS EXISTENTES EN LA IMPLEMENTACIÓN ACTUAL
        Task<IEnumerable<Empleado>> GetAllAsync();
        Task<Empleado?> GetByIdAsync(int id);
        Task<Empleado> AddAsync(Empleado empleado);
        Task<Empleado> UpdateAsync(Empleado empleado);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        
        // ✅ MÉTODOS ADICIONALES EXISTENTES
        Task<Empleado> CreateAsync(Empleado empleado);
        Task<IEnumerable<Empleado>> GetActivosAsync();
        
        // ✅ NUEVOS MÉTODOS PARA EMPLEADO SISTEMA
        Task<Empleado?> GetByEmailAsync(string email);  // Sin tenantId para búsqueda simple
        Task<Empleado> CreateSistemaAsync();  // Crear empleado sistema automáticamente
        
        // ✅ MÉTODOS PARA USERIDENTIFICATIONSERVICE (ya implementados)
        Task<Empleado?> GetByEmailAsync(string email, string tenantId);
        Task<Empleado?> GetByDocumentoAsync(string documento, string tenantId);
        Task<IEnumerable<Empleado>> GetAdministradoresAsync(string tenantId);
        Task<IEnumerable<string>> GetCargosAsync(string tenantId);
        Task<IEnumerable<Empleado>> GetEmpleadosActivosSimpleAsync(string tenantId);
        Task<bool> IsEmpleadoAdminAsync(int empleadoId, string tenantId);
        Task<Empleado?> GetByUsernameAsync(string username, string tenantId);
        Task<IEnumerable<Empleado>> SearchEmpleadosAsync(string searchTerm, string tenantId, int limit = 10);
        
        // ✅ MÉTODO ADICIONAL PARA USERIDENTIFICATIONSERVICE CON TENANTID
        Task<IEnumerable<Empleado>> GetActivosAsync(string tenantId);
    }
}