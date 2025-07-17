using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Domain.Interfaces
{
    public interface IEmpleadoRepository
    {
        // Métodos básicos CRUD
        Task<IEnumerable<Empleado>> GetAllAsync();
        Task<Empleado?> GetByIdAsync(int id);
        Task<Empleado> AddAsync(Empleado empleado);
        Task<Empleado> UpdateAsync(Empleado empleado);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        
        // Métodos específicos que usan los handlers
        Task<Empleado> CreateAsync(Empleado empleado); // Alias para AddAsync
        Task<IEnumerable<Empleado>> GetActivosAsync(); // Solo empleados activos
    }
}
