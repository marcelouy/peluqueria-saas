using PeluqueriaSaaS.Domain.Entities.Base;

namespace PeluqueriaSaaS.Domain.Entities
{
    public class EmpleadoEstacion : TenantEntityBase
    {
        public Guid EmpleadoId { get; private set; }
        public Guid EstacionId { get; private set; }
        public DateTime FechaAsignacion { get; private set; }
        public DateTime? FechaDesasignacion { get; private set; }
        public bool EsActiva { get; private set; } = true;
        public bool EsPrincipal { get; private set; } = false;

        // Navigation properties
        public virtual Empleado Empleado { get; private set; } = null!;
        public virtual Estacion Estacion { get; private set; } = null!;

        private EmpleadoEstacion() { }

        public EmpleadoEstacion(Guid empleadoId, Guid estacionId, bool esPrincipal = false)
        {
            EmpleadoId = empleadoId;
            EstacionId = estacionId;
            FechaAsignacion = DateTime.UtcNow;
            EsPrincipal = esPrincipal;
        }

        public void Desasignar()
        {
            EsActiva = false;
            FechaDesasignacion = DateTime.UtcNow;
        }

        public void MarcarComoPrincipal() => EsPrincipal = true;
        public void QuitarPrincipal() => EsPrincipal = false;
    }
}
