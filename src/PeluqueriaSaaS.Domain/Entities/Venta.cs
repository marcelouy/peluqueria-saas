using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PeluqueriaSaaS.Domain.Entities
{
    /// <summary>
    /// Entity principal para registro de ventas POS
    /// FASE A: Resolver dolor #1 usuario beta (caja manual → digital)
    /// </summary>
    public class Venta
    {
        [Key]
        public int VentaId { get; set; }
        
        /// <summary>
        /// Fecha y hora de la venta
        /// </summary>
        [Required]
        public DateTime FechaVenta { get; set; }
        
        /// <summary>
        /// Número correlativo de venta (V-001, V-002, etc.)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string NumeroVenta { get; set; }
        
        /// <summary>
        /// FK al empleado que realizó la venta
        /// </summary>
        [Required]
        public int EmpleadoId { get; set; }
        
        /// <summary>
        /// FK al cliente que recibió el servicio
        /// </summary>
        [Required]
        public int ClienteId { get; set; }
        
        /// <summary>
        /// Subtotal antes de descuentos
        /// </summary>
        [Required]
        [Range(0, 999999.99)]
        public decimal SubTotal { get; set; }
        
        /// <summary>
        /// Descuento aplicado (opcional)
        /// </summary>
        [Range(0, 999999.99)]
        public decimal Descuento { get; set; } = 0;
        
        /// <summary>
        /// Total final de la venta (SubTotal - Descuento)
        /// </summary>
        [Required]
        [Range(0, 999999.99)]
        public decimal Total { get; set; }
        
        /// <summary>
        /// Estado de la venta: Completada, Anulada
        /// </summary>
        [Required]
        [StringLength(20)]
        public string EstadoVenta { get; set; } = "Completada";
        
        /// <summary>
        /// Observaciones adicionales de la venta
        /// </summary>
        [StringLength(500)]
        public string Observaciones { get; set; }
        
        /// <summary>
        /// CRÍTICO: Multi-tenant identifier
        /// MANTENER "default" para consistencia sistema
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TenantId { get; set; } = "default";
        
        /// <summary>
        /// Soft delete - mantener consistencia con entities existentes
        /// </summary>
        public bool EsActivo { get; set; } = true;
        
        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        // ==========================================
        // NAVIGATION PROPERTIES
        // ==========================================
        
        /// <summary>
        /// Navegación al empleado que realizó la venta
        /// </summary>
        public virtual Empleado Empleado { get; set; }
        
        /// <summary>
        /// Navegación al cliente que recibió el servicio
        /// </summary>
        public virtual Cliente Cliente { get; set; }
        
        /// <summary>
        /// Colección de detalles de la venta (servicios vendidos)
        /// </summary>
        public virtual ICollection<VentaDetalle> VentaDetalles { get; set; } = new List<VentaDetalle>();
    }
}