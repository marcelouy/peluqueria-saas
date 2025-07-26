// ====================================================================
// UpdateClienteHandler.cs - FIXED
// ====================================================================
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
            if (cliente == null) return null;

            cliente.ActualizarInformacion(
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

            await _repositoryManager.UpdateClienteAsync(cliente);

            return new ClienteDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Email = cliente.Email ?? "",           // ✅ FIXED: Direct string
                Telefono = cliente.Telefono ?? "",     // ✅ FIXED: Direct string  
                FechaNacimiento = cliente.FechaNacimiento,
                FechaRegistro = cliente.FechaCreacion,
                Notas = cliente.Notas ?? "",
                EsActivo = cliente.EsActivo
            };
        }
    }
}