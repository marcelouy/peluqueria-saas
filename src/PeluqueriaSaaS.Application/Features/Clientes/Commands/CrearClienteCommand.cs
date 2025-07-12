using MediatR;
using PeluqueriaSaaS.Application.DTOs;

namespace PeluqueriaSaaS.Application.Features.Clientes.Commands
{
    public class CrearClienteCommand : IRequest<ClienteDto>
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
    }
}
