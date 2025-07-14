// src/PeluqueriaSaaS.Application/Features/Clientes/Handlers/CrearClienteHandler.cs
using MediatR;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Application.Features.Clientes.Commands;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Application.Features.Clientes.Handlers
{
    public class CrearClienteHandler : IRequestHandler<CrearClienteCommand, ClienteDto>
    {
        private readonly IRepositoryManagerTemp _repositoryManager;

        public CrearClienteHandler(IRepositoryManagerTemp repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<ClienteDto> Handle(CrearClienteCommand request, CancellationToken cancellationToken)
        {
            var cliente = new ClienteBasico
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                Telefono = request.Telefono,
                FechaNacimiento = request.FechaNacimiento,
                FechaRegistro = DateTime.Now
            };

            var clienteCreado = await _repositoryManager.AddClienteAsync(cliente);

            return new ClienteDto
            {
                Id = clienteCreado.Id,
                Nombre = clienteCreado.Nombre,
                Apellido = clienteCreado.Apellido,
                Email = clienteCreado.Email,
                Telefono = clienteCreado.Telefono,
                FechaNacimiento = clienteCreado.FechaNacimiento,
                FechaRegistro = clienteCreado.FechaRegistro
            };
        }
    }
}
