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
        public DateTime FechaRegistro { get; set; }    // ✅ Mapped from Entity.FechaCreacion
        public string? Notas { get; set; }
        public bool EsActivo { get; set; } = true;

        // ✅ ELIMINADO: Direccion, Ciudad, CodigoPostal, UltimaVisita (NO existen en Entity Cliente)
        // ✅ MAPPING CONFIRMADO:
        //    - Id, Nombre, Apellido: direct mapping from Entity
        //    - Email: mapped from Entity.Email?.Valor ?? ""
        //    - Telefono: mapped from Entity.Telefono?.Numero ?? ""
        //    - FechaNacimiento: direct mapping from Entity
        //    - FechaRegistro: mapped from Entity.FechaCreacion (NOT FechaRegistro)
        //    - Notas: direct mapping with NULL-safe
        //    - EsActivo: direct mapping from Entity
    }
}