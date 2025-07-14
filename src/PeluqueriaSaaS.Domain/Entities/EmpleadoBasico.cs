// src/PeluqueriaSaaS.Domain/Entities/EmpleadoBasico.cs
namespace PeluqueriaSaaS.Domain.Entities
{
    public class EmpleadoBasico
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public DateTime FechaContratacion { get; set; } = DateTime.Now;
        public string? Cargo { get; set; }
        public decimal? Salario { get; set; }
    }
}
