using PeluqueriaSaaS.Domain.Entities.Base;

namespace PeluqueriaSaaS.Domain.Entities
{
    public class HistorialCliente : TenantEntityBase
    {
        public Guid ClienteId { get; private set; }
        public Guid CitaId { get; private set; }
        public DateTime Fecha { get; private set; }
        public string Evento { get; private set; } = string.Empty;
        public string? Descripcion { get; private set; }
        public string? Observaciones { get; private set; }

        // Navigation properties
        public virtual Cliente Cliente { get; private set; } = null!;
        public virtual Cita Cita { get; private set; } = null!;

        private HistorialCliente() { }

        public HistorialCliente(Guid clienteId, Guid citaId, string evento, string? descripcion = null, string? observaciones = null)
        {
            ClienteId = clienteId;
            CitaId = citaId;
            Fecha = DateTime.UtcNow;
            Evento = evento;
            Descripcion = descripcion;
            Observaciones = observaciones;
        }

        public void ActualizarObservaciones(string observaciones) => Observaciones = observaciones;
    }
}
