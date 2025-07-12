using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Entities.Configuration;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Data;

namespace PeluqueriaSaaS.Infrastructure.Repositories;

public class RepositoryManager : IRepositoryManager
{
    private readonly PeluqueriaDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public RepositoryManager(PeluqueriaDbContext context)
    {
        _context = context;
    }

    public IRepository<Cliente> Cliente => GetRepository<Cliente>();
    public IRepository<Empleado> Empleado => GetRepository<Empleado>();
    public IRepository<Cita> Cita => GetRepository<Cita>();
    public IRepository<Empresa> Empresa => GetRepository<Empresa>();
    public IRepository<Sucursal> Sucursal => GetRepository<Sucursal>();
    public IRepository<Servicio> Servicio => GetRepository<Servicio>();
    public IRepository<Estacion> Estacion => GetRepository<Estacion>();
    public IRepository<TipoServicio> TipoServicio => GetRepository<TipoServicio>();
    public IRepository<EstadoCita> EstadoCita => GetRepository<EstadoCita>();
    public IRepository<EstadoEmpleado> EstadoEmpleado => GetRepository<EstadoEmpleado>();
    public IRepository<TipoEstacion> TipoEstacion => GetRepository<TipoEstacion>();
    public IRepository<CitaServicio> CitaServicio => GetRepository<CitaServicio>();
    public IRepository<CitaEstacion> CitaEstacion => GetRepository<CitaEstacion>();
    public IRepository<EmpleadoEstacion> EmpleadoEstacion => GetRepository<EmpleadoEstacion>();
    public IRepository<HistorialCliente> HistorialCliente => GetRepository<HistorialCliente>();

    private IRepository<T> GetRepository<T>() where T : class
    {
        if (_repositories.ContainsKey(typeof(T)))
            return (IRepository<T>)_repositories[typeof(T)];

        var repository = new Repository<T>(_context);
        _repositories.Add(typeof(T), repository);
        return repository;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
