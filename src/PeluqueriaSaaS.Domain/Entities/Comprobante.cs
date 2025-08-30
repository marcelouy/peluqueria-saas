using PeluqueriaSaaS.Domain.Entities.Base;

namespace PeluqueriaSaaS.Domain.Entities
{
    /// <summary>
    /// Representa un comprobante fiscal emitido para una venta
    /// </summary>
    public class Comprobante : TenantEntityBase
    {
        private readonly List<ComprobanteDetalle> _detalles = new();

        // Constructor vacío para EF Core
        protected Comprobante() 
        {
            Serie = string.Empty;
            ClienteNombre = string.Empty;
            MetodoPago = string.Empty;
            Estado = "EMITIDO";
            UsuarioEmision = string.Empty;
        }

        // 🆕 CONSTRUCTOR SIMPLIFICADO AGREGADO - NO ROMPE NADA
        // Este constructor acepta los 7 parámetros que ComprobanteService está enviando
        public Comprobante(
            int ventaId,
            string clienteNombre,
            decimal subtotal,
            decimal total,
            string metodoPago,
            string usuarioEmision,
            string tenantId) : this() // Llama al constructor sin parámetros
        {
            VentaId = ventaId;
            ClienteNombre = clienteNombre;
            Subtotal = subtotal;
            Total = total;
            MetodoPago = metodoPago;
            UsuarioEmision = usuarioEmision;
            
            // VALORES POR DEFECTO para los campos faltantes
            Serie = "A001";
            Numero = 1; // Se actualizará después con SetNumero
            ClienteDocumento = null;
            Descuento = 0;
            Impuestos = 0;
            FechaEmision = DateTime.UtcNow;
            Estado = "EMITIDO";
            
            // Establecer campos de auditoría
            FechaCreacion = DateTime.UtcNow;
            FechaActualizacion = DateTime.UtcNow;
            CreadoPor = usuarioEmision;
            ActualizadoPor = usuarioEmision;
            Activo = true;
            
            // Usar el método SetTenant heredado de TenantEntityBase
            SetTenant(tenantId);
        }

        // Constructor principal EXISTENTE - NO SE MODIFICA
        public Comprobante(
            int ventaId,
            string serie,
            int numero,
            string clienteNombre,
            string? clienteDocumento,
            decimal subtotal,
            decimal descuento,
            decimal impuestos,
            decimal total,
            string metodoPago,
            string usuarioEmision,
            string tenantId) : this() // Llama al constructor sin parámetros
        {
            VentaId = ventaId;
            Serie = serie;
            Numero = numero;
            ClienteNombre = clienteNombre;
            ClienteDocumento = clienteDocumento;
            Subtotal = subtotal;
            Descuento = descuento;
            Impuestos = impuestos;
            Total = total;
            MetodoPago = metodoPago;
            FechaEmision = DateTime.UtcNow;
            Estado = "EMITIDO";
            UsuarioEmision = usuarioEmision;
            
            // Establecer campos de auditoría
            FechaCreacion = DateTime.UtcNow;
            FechaActualizacion = DateTime.UtcNow;
            CreadoPor = usuarioEmision;
            ActualizadoPor = usuarioEmision;
            Activo = true;
            
            // Usar el método SetTenant heredado de TenantEntityBase
            SetTenant(tenantId);
        }

        // REMOVIDO ComprobanteId - usamos Id de EntityBase
        // La propiedad Id viene de EntityBase y se mapeará a ComprobanteId en la BD
        
        // Propiedades
        public int VentaId { get; private set; }
        public string Serie { get; private set; }
        public int Numero { get; private set; }
        public string NumeroCompleto => $"{Serie}-{Numero:D8}";
        public DateTime FechaEmision { get; private set; }
        public string ClienteNombre { get; private set; }
        public string? ClienteDocumento { get; private set; }
        public decimal Subtotal { get; private set; }
        public decimal Descuento { get; private set; }
        public decimal Impuestos { get; private set; }
        public decimal Total { get; private set; }
        public string MetodoPago { get; private set; }
        public string Estado { get; private set; }
        public DateTime? FechaAnulacion { get; private set; }
        public string? MotivoAnulacion { get; private set; }
        public string UsuarioEmision { get; private set; }
        public string? UsuarioAnulacion { get; private set; }
        public int? ClienteId { get; private set; }
        public int EmpleadoId { get; private set; }

        // Navegación
        public virtual Venta? Venta { get; private set; }
        public virtual IReadOnlyCollection<ComprobanteDetalle> Detalles => _detalles.AsReadOnly();

        // 🆕 MÉTODO AUXILIAR AGREGADO para establecer el número después de creación
        public void SetNumero(int numero)
        {
            Numero = numero;
        }

        // 🆕 MÉTODO AUXILIAR AGREGADO para establecer la serie si es diferente
        public void SetSerie(string serie)
        {
            if (!string.IsNullOrWhiteSpace(serie))
                Serie = serie;
        }

        // Métodos de negocio
        public void AgregarDetalle(ComprobanteDetalle detalle)
        {
            if (detalle == null)
                throw new ArgumentNullException(nameof(detalle));

            _detalles.Add(detalle);
        }

        public void AgregarDetalles(IEnumerable<ComprobanteDetalle> detalles)
        {
            if (detalles == null)
                throw new ArgumentNullException(nameof(detalles));

            _detalles.AddRange(detalles);
        }

        public void Anular(string motivo, string usuarioAnulacion)
        {
            if (Estado == "ANULADO")
                throw new InvalidOperationException("El comprobante ya está anulado");

            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("El motivo de anulación es requerido", nameof(motivo));

            if (string.IsNullOrWhiteSpace(usuarioAnulacion))
                throw new ArgumentException("El usuario de anulación es requerido", nameof(usuarioAnulacion));

            Estado = "ANULADO";
            FechaAnulacion = DateTime.UtcNow;
            MotivoAnulacion = motivo;
            UsuarioAnulacion = usuarioAnulacion;
        }

        public bool EstaAnulado() => Estado == "ANULADO";

        public bool PuedeAnularse() => Estado == "EMITIDO" && !EstaAnulado();
    }
}