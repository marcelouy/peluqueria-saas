using MediatR;
using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Application.Commands.Empleados
{
    public class CreateEmpleadoCommand : IRequest<Empleado>
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Cargo { get; set; } = string.Empty;
        public decimal? Salario { get; set; }
        public DateTime FechaContratacion { get; set; } = DateTime.Now;
        public string? Horario { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Notas { get; set; }
        public bool EsActivo { get; set; } = true;
    }
}
