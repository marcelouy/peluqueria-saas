using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PeluqueriaSaaS.Domain.Entities.Base;

namespace PeluqueriaSaaS.Domain.Entities.Configuration
{
    /// <summary>
    /// Representa un tipo de impuesto en el sistema (IVA, IMESI, etc.)
    /// NO tiene TenantId porque los impuestos son NACIONALES
    /// </summary>
    public class TipoImpuesto : EntityBase  // NO TenantEntityBase porque es nacional
    {
        [Required]
        [StringLength(50)]
        public string Codigo { get; set; } // IVA, IMESI, TRIBUTO_PROF
        
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }
        
        [StringLength(500)]
        public string Descripcion { get; set; }
        
        [Required]
        [StringLength(20)]
        public string TipoCalculo { get; set; } = "PORCENTAJE";
        
        [Required]
        [StringLength(20)]
        public string AplicaA { get; set; } = "AMBOS"; // PRODUCTOS, SERVICIOS, AMBOS
        
        public int OrdenAplicacion { get; set; } = 1;
        
        public bool IncluidoEnPrecio { get; set; } = false;
        
        // Navegación
        public virtual ICollection<TasaImpuesto> Tasas { get; set; } = new List<TasaImpuesto>();
    }
}