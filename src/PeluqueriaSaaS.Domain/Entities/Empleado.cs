namespace PeluqueriaSaaS.Domain.Entities
{
    public class Empleado
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        
        // Campos específicos de Empleado
        public string Cargo { get; set; } = string.Empty;
        public decimal? Salario { get; set; }
        public DateTime FechaContratacion { get; set; } = DateTime.Now;
        public string? Horario { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Notas { get; set; }
        public bool EsActivo { get; set; } = true;
        
        // Propiedades calculadas
        public string NombreCompleto => $"{Nombre} {Apellido}";
        public int? Edad => FechaNacimiento.HasValue ? 
            DateTime.Now.Year - FechaNacimiento.Value.Year : null;
    }
}
