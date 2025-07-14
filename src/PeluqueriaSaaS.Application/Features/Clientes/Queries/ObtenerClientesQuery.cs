// src/PeluqueriaSaaS.Application/Features/Clientes/Queries/ObtenerClientesQuery.cs
using MediatR;
using PeluqueriaSaaS.Application.DTOs;

namespace PeluqueriaSaaS.Application.Features.Clientes.Queries
{
    public class ObtenerClientesQuery : IRequest<IEnumerable<ClienteDto>>
    {
    }
}
