using PeluqueriaSaaS.Domain.Entities.Base;
using PeluqueriaSaaS.Domain.ValueObjects;

namespace PeluqueriaSaaS.Domain.Entities
{
    public class CitaServicio : TenantEntityBase
    {
        public Guid CitaId { get; private set; }
        public Guid ServicioId { get; private set; }
        public Guid? EmpleadoId { get; private set; }
        public int Orden { get; private set; }
        public Dinero PrecioFinal { get; private set; } = Dinero.Cero;
        public int DuracionMinutos { get; private set; }
        public DateTime? HoraInicio { get; private set; }
        public DateTime? HoraFin { get; private set; }
        public string? Notas { get; private set; }

        // Navigation properties
        public virtual Cita Cita { get; private set; } = null!;
        public virtual Servicio Servicio { get; private set; } = null!;
        public virtual Empleado? Empleado { get; private set; }

        private CitaServicio() { }

        public CitaServicio(Guid citaId, Guid servicioId, int orden, Dinero precioFinal, int duracionMinutos, Guid? empleadoId = null)
        {
            CitaId = citaId;
            ServicioId = servicioId;
            EmpleadoId = empleadoId;
            Orden = orden;
            PrecioFinal = precioFinal;
            DuracionMinutos = duracionMinutos;
        }

        public void AsignarEmpleado(Guid empleadoId) => EmpleadoId = empleadoId;
        public void IniciarServicio() => HoraInicio = DateTime.UtcNow;
        public void FinalizarServicio() => HoraFin = DateTime.UtcNow;
        public void ActualizarNotas(string notas) => Notas = notas;
    }
}
