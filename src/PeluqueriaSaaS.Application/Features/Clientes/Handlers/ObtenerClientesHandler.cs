// src/PeluqueriaSaaS.Application/Features/Clientes/Handlers/ObtenerClientesHandler.cs
using MediatR;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Application.Features.Clientes.Queries;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Features.Clientes.Handlers
{
    public class ObtenerClientesHandler : IRequestHandler<ObtenerClientesQuery, IEnumerable<ClienteDto>>
    {
        private readonly IRepositoryManagerTemp _repositoryManager;

        public ObtenerClientesHandler(IRepositoryManagerTemp repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<IEnumerable<ClienteDto>> Handle(ObtenerClientesQuery request, CancellationToken cancellationToken)
        {
            var clientes = await _repositoryManager.GetAllClientesAsync();
            return clientes.Select(c => new ClienteDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Apellido = c.Apellido,
                Email = c.Email,
                Telefono = c.Telefono,
                FechaNacimiento = c.FechaNacimiento,
                FechaRegistro = c.FechaRegistro,
                Direccion = c.Direccion,
                Ciudad = c.Ciudad,
                CodigoPostal = c.CodigoPostal,
                Notas = c.Notas,
                EsActivo = c.EsActivo,
                UltimaVisita = c.UltimaVisita
            });
        }
    }
}
