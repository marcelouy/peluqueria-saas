using System.ComponentModel.DataAnnotations;

namespace PeluqueriaSaaS.Application.DTOs
{
    public class EmpleadoCreateDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string? Telefono { get; set; }

        [StringLength(100, ErrorMessage = "El cargo no puede exceder 100 caracteres")]
        public string? Cargo { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El salario debe ser mayor o igual a 0")]
        public decimal? Salario { get; set; }

        public bool EsActivo { get; set; } = true;
    }
}