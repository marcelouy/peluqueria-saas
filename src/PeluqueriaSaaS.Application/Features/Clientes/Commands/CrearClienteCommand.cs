// src/PeluqueriaSaaS.Application/Features/Clientes/Commands/CrearClienteCommand.cs
using MediatR;
using PeluqueriaSaaS.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace PeluqueriaSaaS.Application.Features.Clientes.Commands
{
    public class CrearClienteCommand : IRequest<ClienteDto>
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50)]
        public string Apellido { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Phone]
        [StringLength(20)]
        public string? Telefono { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }
        
        // Campos adicionales
        [StringLength(200)]
        public string? Direccion { get; set; }
        
        [StringLength(100)]
        public string? Ciudad { get; set; }
        
        [StringLength(10)]
        public string? CodigoPostal { get; set; }
        
        [StringLength(500)]
        public string? Notas { get; set; }
        
        public bool EsActivo { get; set; } = true;
    }
}
