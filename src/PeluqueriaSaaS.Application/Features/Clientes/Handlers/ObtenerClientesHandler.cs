using MediatR;
using PeluqueriaSaaS.Application.Features.Clientes.Queries;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Features.Clientes.Handlers;

public class ObtenerClientesHandler : IRequestHandler<ObtenerClientesQuery, IEnumerable<ClienteDto>>
{
    private readonly IRepositoryManagerTemp _repository;

    public ObtenerClientesHandler(IRepositoryManagerTemp repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ClienteDto>> Handle(ObtenerClientesQuery request, CancellationToken cancellationToken)
    {
        var clientes = await _repository.Cliente.GetAllAsync();
        
        return clientes.Select(c => new ClienteDto
        {
            Id = new Guid(c.Id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
            Nombre = c.Nombre,
            Email = "temp@email.com",
            Telefono = "123456789"
        });
    }
}
