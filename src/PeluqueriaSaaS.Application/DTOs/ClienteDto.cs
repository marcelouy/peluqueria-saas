// src/PeluqueriaSaaS.Application/DTOs/ClienteDto.cs
namespace PeluqueriaSaaS.Application.DTOs
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public DateTime FechaRegistro { get; set; }
        
        // Campos adicionales
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Notas { get; set; }
        public bool EsActivo { get; set; } = true;
        public DateTime? UltimaVisita { get; set; }
    }
}
