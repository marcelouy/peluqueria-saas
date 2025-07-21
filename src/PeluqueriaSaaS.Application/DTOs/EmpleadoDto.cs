using System.ComponentModel.DataAnnotations;

namespace PeluqueriaSaaS.Application.DTOs
{
    public class EmpleadoListDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string Cargo { get; set; } = string.Empty;
        public decimal? Salario { get; set; }
        public DateTime FechaContratacion { get; set; }
        public bool EsActivo { get; set; }
    }

    public class EmpleadoDetailDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Cargo { get; set; } = string.Empty;
        public decimal? Salario { get; set; }
        public DateTime FechaContratacion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool EsActivo { get; set; }
    }

    public class CreateEmpleadoDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, ErrorMessage = "El apellido no puede tener más de 100 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "El teléfono no tiene un formato válido")]
        public string? Telefono { get; set; }

        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El cargo es requerido")]
        [StringLength(100, ErrorMessage = "El cargo no puede tener más de 100 caracteres")]
        public string Cargo { get; set; } = string.Empty;

        [Range(0, 999999999, ErrorMessage = "El salario debe ser un valor positivo")]
        public decimal? Salario { get; set; }

        [Required(ErrorMessage = "La fecha de contratación es requerida")]
        [Display(Name = "Fecha de Contratación")]
        public DateTime FechaContratacion { get; set; } = DateTime.Now;
    }

    public class UpdateEmpleadoDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, ErrorMessage = "El apellido no puede tener más de 100 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "El teléfono no tiene un formato válido")]
        public string? Telefono { get; set; }

        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El cargo es requerido")]
        [StringLength(100, ErrorMessage = "El cargo no puede tener más de 100 caracteres")]
        public string Cargo { get; set; } = string.Empty;

        [Range(0, 999999999, ErrorMessage = "El salario debe ser un valor positivo")]
        public decimal? Salario { get; set; }

        [Required(ErrorMessage = "La fecha de contratación es requerida")]
        [Display(Name = "Fecha de Contratación")]
        public DateTime FechaContratacion { get; set; }

        public bool EsActivo { get; set; }
    }
}