using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Entities.Configuration;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Infrastructure.Extensions;

public static class RepositoryExtensions
{
    public static async Task<IEnumerable<Cita>> GetByClienteAsync(
        this IRepository<Cita> repository, Guid clienteId)
    {
        return await repository.FindAsync(c => c.ClienteId == clienteId);
    }
    
    public static async Task<Cliente?> GetByEmailAsync(
        this IRepository<Cliente> repository, string email)
    {
        var clientes = await repository.FindAsync(c => c.Email.Valor == email);
        return clientes?.FirstOrDefault();
    }
}
