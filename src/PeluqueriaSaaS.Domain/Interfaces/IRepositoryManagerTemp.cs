using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Domain.Interfaces;

public interface IRepositoryManagerTemp
{
    // Métodos genéricos
    IRepository<T> GetRepository<T>() where T : class;
    Task SaveAsync();
    
    // Métodos específicos para ClienteBasico que usan los handlers
    Task<IEnumerable<ClienteBasico>> GetAllClientesAsync();
    Task<ClienteBasico?> GetClienteByIdAsync(int id);
    Task<ClienteBasico> AddClienteAsync(ClienteBasico cliente);
    Task<ClienteBasico> UpdateClienteAsync(ClienteBasico cliente);
    Task<bool> DeleteClienteAsync(int id);
    
    // Métodos específicos para EmpleadoBasico
    Task<IEnumerable<EmpleadoBasico>> GetAllEmpleadosAsync();
    Task<EmpleadoBasico?> GetEmpleadoByIdAsync(int id);
    Task<EmpleadoBasico> AddEmpleadoAsync(EmpleadoBasico empleado);
    Task<EmpleadoBasico> UpdateEmpleadoAsync(EmpleadoBasico empleado);
    Task<bool> DeleteEmpleadoAsync(int id);
}
