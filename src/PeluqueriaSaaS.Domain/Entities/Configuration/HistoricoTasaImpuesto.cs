using System;
using System.ComponentModel.DataAnnotations;
using PeluqueriaSaaS.Domain.Entities.Base;

namespace PeluqueriaSaaS.Domain.Entities.Configuration
{
    /// <summary>
    /// Histórico de cambios en las tasas de impuestos para auditoría
    /// NO tiene TenantId porque es histórico nacional
    /// </summary>
    public class HistoricoTasaImpuesto : EntityBase
    {
        [Required]
        public int TasaImpuestoId { get; set; }
        
        [Required]
        public decimal PorcentajeAnterior { get; set; }
        
        [Required]
        public decimal PorcentajeNuevo { get; set; }
        
        [Required]
        public DateTime FechaCambio { get; set; }
        
        [StringLength(500)]
        public string Motivo { get; set; }
        
        [StringLength(200)]
        public string DecretoLey { get; set; }
        
        [StringLength(100)]
        public string ModificadoPor { get; set; }
        
        // Navegación
        public virtual TasaImpuesto TasaImpuesto { get; set; }
    }
}
