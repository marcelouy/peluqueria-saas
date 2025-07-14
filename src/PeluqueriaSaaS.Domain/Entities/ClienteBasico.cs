// src/PeluqueriaSaaS.Domain/Entities/ClienteBasico.cs
namespace PeluqueriaSaaS.Domain.Entities
{
    public class ClienteBasico
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
