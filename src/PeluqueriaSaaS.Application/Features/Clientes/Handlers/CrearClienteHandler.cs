// ====================================================================
// CrearClienteHandler.cs - FIXED
// ====================================================================
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
            var cliente = new Cliente(
                request.Nombre,
                request.Apellido,
                request.Email,
                request.Telefono,
                request.FechaNacimiento
            );

            if (!string.IsNullOrEmpty(request.Notas))
            {
                cliente.ActualizarNotas(request.Notas);
            }

            if (request.EsActivo)
            {
                cliente.Activar();
            }
            else
            {
                cliente.Desactivar();
            }

            var clienteCreado = await _repositoryManager.AddClienteAsync(cliente);

            return new ClienteDto
            {
                Id = clienteCreado.Id,
                Nombre = clienteCreado.Nombre,
                Apellido = clienteCreado.Apellido,
                Email = clienteCreado.Email ?? "",           // ✅ FIXED: Direct string
                Telefono = clienteCreado.Telefono ?? "",     // ✅ FIXED: Direct string
                FechaNacimiento = clienteCreado.FechaNacimiento,
                FechaRegistro = clienteCreado.FechaCreacion,
                Notas = clienteCreado.Notas ?? "",
                EsActivo = clienteCreado.EsActivo
            };
        }
    }
}
