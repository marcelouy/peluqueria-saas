// ====================================================================
// ObtenerClientePorIdHandler.cs - FIXED
// ====================================================================
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