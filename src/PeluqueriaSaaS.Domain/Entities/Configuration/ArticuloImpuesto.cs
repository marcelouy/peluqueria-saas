using System;
using System.ComponentModel.DataAnnotations;
using PeluqueriaSaaS.Domain.Entities.Base;

namespace PeluqueriaSaaS.Domain.Entities.Configuration
{
    /// <summary>
    /// Relación entre Artículos y sus impuestos aplicables
    /// SÍ tiene TenantId porque cada empresa decide qué impuestos aplica
    /// </summary>
    public class ArticuloImpuesto : TenantEntityBase
    {
        [Required]
        public int ArticuloId { get; set; }
        
        [Required]
        public int TasaImpuestoId { get; set; }
        
        [Required]
        public DateTime FechaInicioAplicacion { get; set; }
        
        public DateTime? FechaFinAplicacion { get; set; }
        
        // Para casos especiales donde hay un porcentaje negociado
        public decimal? PorcentajeEspecial { get; set; }
        
        [StringLength(500)]
        public string Notas { get; set; }
        
        // Navegación
        public virtual Articulo Articulo { get; set; }
        public virtual TasaImpuesto TasaImpuesto { get; set; }
    }
}
