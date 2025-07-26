using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Domain.Interfaces;

public interface IRepositoryManagerTemp
{
    // Métodos genéricos
    IRepository<T> GetRepository<T>() where T : class;
    Task SaveAsync();
    
    // ✅ FIX: Métodos específicos para Cliente (entidad actual con datos)
    Task<IEnumerable<Cliente>> GetAllClientesAsync();
    Task<Cliente?> GetClienteByIdAsync(int id);
    Task<Cliente> AddClienteAsync(Cliente cliente);
    Task<Cliente> UpdateClienteAsync(Cliente cliente);
    Task<bool> DeleteClienteAsync(int id);
    
    // ✅ FIX: Métodos específicos para Empleado (entidad actual con datos)
    Task<IEnumerable<Empleado>> GetAllEmpleadosAsync();
    Task<Empleado?> GetEmpleadoByIdAsync(int id);
    Task<Empleado> AddEmpleadoAsync(Empleado empleado);
    Task<Empleado> UpdateEmpleadoAsync(Empleado empleado);
    Task<bool> DeleteEmpleadoAsync(int id);
}