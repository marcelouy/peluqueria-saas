using MediatR;
using PeluqueriaSaaS.Application.DTOs;

namespace PeluqueriaSaaS.Application.Features.Clientes.Queries;

// 📋 QUERY: Solo para LEER datos
// No modifica nada, solo consulta
public class ObtenerClientePorIdQuery : IRequest<ClienteDto?>
{
    public Guid Id { get; set; }
}
