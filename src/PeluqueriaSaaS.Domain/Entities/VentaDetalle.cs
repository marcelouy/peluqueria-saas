using System;
using System.ComponentModel.DataAnnotations;

namespace PeluqueriaSaaS.Domain.Entities
{
    /// <summary>
    /// Entity para detalles de cada venta (servicios vendidos)
    /// FASE A: Líneas de servicios por venta POS
    /// </summary>
    public class VentaDetalle
    {
        [Key]
        public int VentaDetalleId { get; set; }
        
        /// <summary>
        /// FK a la venta principal
        /// </summary>
        [Required]
        public int VentaId { get; set; }
        
        /// <summary>
        /// FK al servicio vendido
        /// </summary>
        [Required]
        public int ServicioId { get; set; }
        
        /// <summary>
        /// Snapshot del nombre del servicio al momento de la venta
        /// Preserva información aunque se modifique el servicio después
        /// </summary>
        [Required]
        [StringLength(100)]
        public string NombreServicio { get; set; }
        
        /// <summary>
        /// Precio unitario del servicio al momento de la venta
        /// Snapshot para preservar precio histórico
        /// </summary>
        [Required]
        [Range(0, 999999.99)]
        public decimal PrecioUnitario { get; set; }
        
        /// <summary>
        /// Cantidad de servicios (normalmente 1, pero permite flexibilidad)
        /// </summary>
        [Required]
        [Range(1, 999)]
        public int Cantidad { get; set; } = 1;
        
        /// <summary>
        /// Subtotal de esta línea (PrecioUnitario * Cantidad)
        /// </summary>
        [Required]
        [Range(0, 999999.99)]
        public decimal Subtotal { get; set; }
        
        /// <summary>
        /// Empleado específico que realizó este servicio
        /// Permite múltiples empleados en una venta (futuro)
        /// </summary>
        public int? EmpleadoServicioId { get; set; }
        
        /// <summary>
        /// Notas específicas de este servicio en la venta
        /// ✅ FIX CRÍTICO: Nullable para manejar NULL values en BD
        /// </summary>
        [StringLength(200)]
        public string? NotasServicio { get; set; }
        
        /// <summary>
        /// CRÍTICO: Multi-tenant identifier
        /// MANTENER "default" para consistencia sistema
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TenantId { get; set; } = "default";
        
        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public int EstadoServicioId { get; set; } = 1;
        public int? EmpleadoAsignadoId { get; set; }
        public DateTime? InicioServicio { get; set; }
        public DateTime? FinServicio { get; set; }
        
        // ==========================================
        // NAVIGATION PROPERTIES
        // ==========================================

        /// <summary>
        /// Navegación a la venta principal
        /// </summary>
        public virtual Venta Venta { get; set; }
        
        /// <summary>
        /// Navegación al servicio vendido
        /// </summary>
        public virtual Servicio Servicio { get; set; }
        
        /// <summary>
        /// Navegación al empleado que realizó específicamente este servicio
        /// (opcional - para ventas multi-empleado futuras)
        /// </summary>
        public virtual Empleado? EmpleadoServicio { get; set; }
    }
}