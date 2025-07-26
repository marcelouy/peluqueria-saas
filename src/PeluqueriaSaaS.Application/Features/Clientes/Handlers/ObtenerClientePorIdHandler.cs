// ====================================================================
// ObtenerClientesQueryHandler.cs - FIXED
// ====================================================================
using MediatR;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Application.Features.Clientes.Queries;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Features.Clientes.Queries
{
    public class ObtenerClientesQueryHandler : IRequestHandler<ObtenerClientesQuery, IEnumerable<ClienteDto>>
    {
        private readonly IRepositoryManagerTemp _repositoryManager;

        public ObtenerClientesQueryHandler(IRepositoryManagerTemp repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<IEnumerable<ClienteDto>> Handle(ObtenerClientesQuery request, CancellationToken cancellationToken)
        {
            var clientes = await _repositoryManager.GetAllClientesAsync();
            
            return clientes.Select(cliente => new ClienteDto
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
            });
        }
    }
}
