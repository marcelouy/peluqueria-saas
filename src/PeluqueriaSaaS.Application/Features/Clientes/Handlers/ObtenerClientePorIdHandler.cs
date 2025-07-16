// src/PeluqueriaSaaS.Application/Features/Clientes/Handlers/ObtenerClientePorIdHandler.cs
using MediatR;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Application.Features.Clientes.Queries;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Features.Clientes.Handlers
{
    public class ObtenerClientePorIdHandler : IRequestHandler<ObtenerClientePorIdQuery, ClienteDto?>
    {
        private readonly IRepositoryManagerTemp _repositoryManager;

        public ObtenerClientePorIdHandler(IRepositoryManagerTemp repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<ClienteDto?> Handle(ObtenerClientePorIdQuery request, CancellationToken cancellationToken)
        {
            var cliente = await _repositoryManager.GetClienteByIdAsync(request.Id);
            if (cliente == null) return null;

            return new ClienteDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                FechaNacimiento = cliente.FechaNacimiento,
                FechaRegistro = cliente.FechaRegistro,
                Direccion = cliente.Direccion,
                Ciudad = cliente.Ciudad,
                CodigoPostal = cliente.CodigoPostal,
                Notas = cliente.Notas,
                EsActivo = cliente.EsActivo,
                UltimaVisita = cliente.UltimaVisita
            };
        }
    }
}
