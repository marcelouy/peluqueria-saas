using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Infrastructure.Data;

namespace PeluqueriaSaaS.Infrastructure.Repositories;

public class RepositoryManager : IRepositoryManagerTemp
{
    private readonly PeluqueriaDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public RepositoryManager(PeluqueriaDbContext context)
    {
        _context = context;
    }

    // Métodos genéricos
    public IRepository<T> GetRepository<T>() where T : class
    {
        var type = typeof(T);
        
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new Repository<T>(_context);
        }
        
        return (IRepository<T>)_repositories[type];
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    // ✅ FIX: Métodos para Cliente (tabla actual con datos)
    public async Task<IEnumerable<Cliente>> GetAllClientesAsync()
    {
        return await _context.Clientes.Where(c => c.EsActivo).ToListAsync(); // ✅ Tabla correcta + filtro activos
    }

    public async Task<Cliente?> GetClienteByIdAsync(int id)
    {
        return await _context.Clientes.FindAsync(id); // ✅ Tabla correcta
    }

    public async Task<Cliente> AddClienteAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente); // ✅ Tabla correcta
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task<Cliente> UpdateClienteAsync(Cliente cliente)
    {
        _context.Entry(cliente).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task<bool> DeleteClienteAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id); // ✅ Tabla correcta
        if (cliente != null)
        {
            _context.Clientes.Remove(cliente); // ✅ Tabla correcta
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    // ✅ FIX: Métodos para Empleado (entidad actual con datos)
    public async Task<IEnumerable<Empleado>> GetAllEmpleadosAsync()
    {
        return await _context.Empleados.Where(e => e.EsActivo).ToListAsync(); // ✅ Tabla correcta + filtro activos
    }

    public async Task<Empleado?> GetEmpleadoByIdAsync(int id)
    {
        return await _context.Empleados.FindAsync(id); // ✅ Tabla correcta
    }

    public async Task<Empleado> AddEmpleadoAsync(Empleado empleado)
    {
        _context.Empleados.Add(empleado); // ✅ Tabla correcta
        await _context.SaveChangesAsync();
        return empleado;
    }

    public async Task<Empleado> UpdateEmpleadoAsync(Empleado empleado)
    {
        _context.Entry(empleado).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return empleado;
    }

    public async Task<bool> DeleteEmpleadoAsync(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id); // ✅ Tabla correcta
        if (empleado != null)
        {
            _context.Empleados.Remove(empleado); // ✅ Tabla correcta
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }
}