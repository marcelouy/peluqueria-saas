using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PeluqueriaSaaS.Domain.Entities.Base;

namespace PeluqueriaSaaS.Domain.Entities.Configuration
{
    /// <summary>
    /// Representa una tasa específica de un impuesto con su período de vigencia
    /// NO tiene TenantId porque las tasas son NACIONALES
    /// </summary>
    public class TasaImpuesto : EntityBase  // NO TenantEntityBase
    {
        [Required]
        public int TipoImpuestoId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } // "IVA Tasa Básica", "IVA Tasa Mínima"
        
        [Required]
        [Range(0, 100)]
        public decimal Porcentaje { get; set; }
        
        [Required]
        public DateTime FechaInicio { get; set; }
        
        public DateTime? FechaFin { get; set; }
        
        [StringLength(50)]
        public string CodigoTasa { get; set; } // IVA_0, IVA_10, IVA_22
        
        public bool EsTasaPorDefecto { get; set; } = false;
        
        [StringLength(200)]
        public string DecretoLey { get; set; }
        
        // Navegación
        public virtual TipoImpuesto TipoImpuesto { get; set; }
        public virtual ICollection<ArticuloImpuesto> ArticulosImpuestos { get; set; } = new List<ArticuloImpuesto>();
        public virtual ICollection<ServicioImpuesto> ServiciosImpuestos { get; set; } = new List<ServicioImpuesto>();
    }
}
