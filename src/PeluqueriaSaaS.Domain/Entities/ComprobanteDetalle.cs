using PeluqueriaSaaS.Domain.Entities.Base;

namespace PeluqueriaSaaS.Domain.Entities
{
    /// <summary>
    /// Representa una línea de detalle en un comprobante
    /// </summary>
    public class ComprobanteDetalle : EntityBase
    {
        // Constructor vacío para EF Core - CAMBIADO A PUBLIC
        public ComprobanteDetalle() 
        {
            TipoItem = string.Empty;
            Descripcion = string.Empty;
            TenantId = string.Empty;
            CreadoPor = string.Empty;
        }

        // 🆕 CONSTRUCTOR SIMPLIFICADO AGREGADO para uso desde ComprobanteService
        public ComprobanteDetalle(
            int comprobanteId,
            string tipoItem,
            string descripcion,
            decimal cantidad,
            decimal precioUnitario,
            decimal total) : this()
        {
            ComprobanteId = comprobanteId;
            TipoItem = tipoItem;
            Descripcion = descripcion;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
            Total = total;
            
            // Valores por defecto
            ItemId = null;
            Descuento = 0;
            Impuestos = 0;
            EmpleadoNombre = null;
            TenantId = "default";
            CreadoPor = "Sistema";
            FechaCreacion = DateTime.UtcNow;
            FechaActualizacion = DateTime.UtcNow;
            ActualizadoPor = "Sistema";
            Activo = true;
        }

        // Constructor principal EXISTENTE - NO SE MODIFICA
        public ComprobanteDetalle(
            int comprobanteId,
            string tipoItem,
            int? itemId,
            string descripcion,
            decimal cantidad,
            decimal precioUnitario,
            decimal descuento,
            decimal impuestos,
            decimal total,
            string? empleadoNombre,
            string tenantId,
            string creadoPor) : this()
        {
            ComprobanteId = comprobanteId;
            TipoItem = tipoItem;
            ItemId = itemId;
            Descripcion = descripcion;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
            Descuento = descuento;
            Impuestos = impuestos;
            Total = total;
            EmpleadoNombre = empleadoNombre;
            TenantId = tenantId;
            CreadoPor = creadoPor;
            FechaCreacion = DateTime.UtcNow;
            FechaActualizacion = DateTime.UtcNow;
            ActualizadoPor = creadoPor;
            Activo = true;
        }

        // REMOVIDO DetalleId - usamos Id de EntityBase
        // La propiedad Id viene de EntityBase y se mapeará a DetalleId en la BD
        
        // 🔧 PROPIEDADES CAMBIADAS A PUBLIC SET para permitir asignación
        public int ComprobanteId { get; set; } // Ya estaba con set
        public string TipoItem { get; set; } // CAMBIADO: private set -> set
        public int? ItemId { get; set; } // CAMBIADO: private set -> set
        public string Descripcion { get; set; } // CAMBIADO: private set -> set
        public decimal Cantidad { get; set; } // CAMBIADO: private set -> set
        public decimal PrecioUnitario { get; set; } // CAMBIADO: private set -> set
        public decimal Descuento { get; set; } // CAMBIADO: private set -> set
        public decimal Impuestos { get; set; } // CAMBIADO: private set -> set
        public decimal Total { get; set; } // CAMBIADO: private set -> set
        public string? EmpleadoNombre { get; set; } // CAMBIADO: private set -> set
        
        // Campos adicionales para auditoría y multi-tenant
        public string TenantId { get; set; } // CAMBIADO: private set -> set
        // FechaCreacion y CreadoPor vienen de EntityBase
        public int? EmpleadoId { get; set; }

        // Navegación
        public virtual Comprobante? Comprobante { get; set; } // CAMBIADO: private set -> set

        // Métodos de negocio
        public decimal CalcularSubtotal() => Cantidad * PrecioUnitario;
        
        public decimal CalcularTotalConDescuento() => CalcularSubtotal() - Descuento;
        
        public decimal CalcularTotalFinal() => CalcularTotalConDescuento() + Impuestos;

        public bool EsServicio() => TipoItem == "SERVICIO";
        
        public bool EsProducto() => TipoItem == "PRODUCTO";
    }
}