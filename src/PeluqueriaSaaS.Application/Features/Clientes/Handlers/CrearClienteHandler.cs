using MediatR;
using PeluqueriaSaaS.Application.Features.Clientes.Commands;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Features.Clientes.Handlers;

public class CrearClienteHandler : IRequestHandler<CrearClienteCommand, ClienteDto>
{
    private readonly IRepositoryManagerTemp _repository;

    public CrearClienteHandler(IRepositoryManagerTemp repository)
    {
        _repository = repository;
    }

    public async Task<ClienteDto> Handle(CrearClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = new ClienteBasico { Nombre = request.Nombre };
        await _repository.Cliente.AddAsync(cliente);
        await _repository.SaveChangesAsync();

        return new ClienteDto
        {
            Id = new Guid(cliente.Id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
            Nombre = cliente.Nombre,
            Email = "temp@email.com",
            Telefono = "123456789"
        };
    }
}
