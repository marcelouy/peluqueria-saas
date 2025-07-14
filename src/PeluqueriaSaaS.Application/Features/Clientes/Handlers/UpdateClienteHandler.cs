// src/PeluqueriaSaaS.Application/Features/Clientes/Handlers/UpdateClienteHandler.cs
using MediatR;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Application.Features.Clientes.Commands;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Features.Clientes.Handlers
{
    public class UpdateClienteHandler : IRequestHandler<UpdateClienteCommand, ClienteDto?>
    {
        private readonly IRepositoryManagerTemp _repositoryManager;

        public UpdateClienteHandler(IRepositoryManagerTemp repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<ClienteDto?> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
        {
            var cliente = await _repositoryManager.GetClienteByIdAsync(request.Id);
            
            if (cliente == null)
                return null;

            // Actualizar propiedades
            cliente.Nombre = request.Nombre;
            cliente.Apellido = request.Apellido;
            cliente.Email = request.Email;
            cliente.Telefono = request.Telefono;
            cliente.FechaNacimiento = request.FechaNacimiento;

            await _repositoryManager.UpdateClienteAsync(cliente);

            return new ClienteDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                FechaNacimiento = cliente.FechaNacimiento,
                FechaRegistro = cliente.FechaRegistro
            };
        }
    }
}
