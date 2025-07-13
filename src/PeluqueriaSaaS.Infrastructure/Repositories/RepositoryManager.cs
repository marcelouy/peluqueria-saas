using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Data;

namespace PeluqueriaSaaS.Infrastructure.Repositories;

public class RepositoryManager : IRepositoryManagerTemp
{
    private readonly PeluqueriaDbContext _context;

    public RepositoryManager(PeluqueriaDbContext context)
    {
        _context = context;
    }

    // Usar entidades básicas temporalmente
    public IRepository<ClienteBasico> Cliente => GetRepository<ClienteBasico>();
    public IRepository<EmpleadoBasico> Empleado => GetRepository<EmpleadoBasico>();
    public IRepository<ServicioBasico> Servicio => GetRepository<ServicioBasico>();
    public IRepository<CitaBasica> Cita => GetRepository<CitaBasica>();

    private IRepository<T> GetRepository<T>() where T : class
    {
        return new Repository<T>(_context);
    }

    public async Task<bool> SaveChangesAsync()
    {
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
