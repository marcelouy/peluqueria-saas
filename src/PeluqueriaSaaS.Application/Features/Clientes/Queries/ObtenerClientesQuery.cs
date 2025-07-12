using MediatR;
using PeluqueriaSaaS.Application.DTOs;

namespace PeluqueriaSaaS.Application.Features.Clientes.Queries
{
    public class ObtenerClientesQuery : IRequest<IEnumerable<ClienteDto>>
    {
        public bool SoloActivos { get; set; } = true;
    }
}
