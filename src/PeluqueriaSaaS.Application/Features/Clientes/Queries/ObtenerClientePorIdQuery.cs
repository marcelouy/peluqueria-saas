// src/PeluqueriaSaaS.Application/Features/Clientes/Queries/ObtenerClientePorIdQuery.cs
using MediatR;
using PeluqueriaSaaS.Application.DTOs;

namespace PeluqueriaSaaS.Application.Features.Clientes.Queries
{
    public class ObtenerClientePorIdQuery : IRequest<ClienteDto?>
    {
        public int Id { get; set; }

        public ObtenerClientePorIdQuery(int id)
        {
            Id = id;
        }
    }
}
