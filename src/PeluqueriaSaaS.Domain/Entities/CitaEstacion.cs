using PeluqueriaSaaS.Domain.Entities.Base;

namespace PeluqueriaSaaS.Domain.Entities
{
    public class CitaEstacion : TenantEntityBase
    {
        public Guid CitaId { get; private set; }
        public Guid EstacionId { get; private set; }
        public DateTime HoraInicio { get; private set; }
        public DateTime HoraFin { get; private set; }
        public int Orden { get; private set; }
        public bool EsActiva { get; private set; } = true;

        // Navigation properties
        public virtual Cita Cita { get; private set; } = null!;
        public virtual Estacion Estacion { get; private set; } = null!;

        private CitaEstacion() { }

        public CitaEstacion(Guid citaId, Guid estacionId, DateTime horaInicio, DateTime horaFin, int orden)
        {
            CitaId = citaId;
            EstacionId = estacionId;
            HoraInicio = horaInicio;
            HoraFin = horaFin;
            Orden = orden;
        }

        public new void Activar() => EsActiva = true;
        public new void Desactivar() => EsActiva = false;
        public void ActualizarHorarios(DateTime inicio, DateTime fin)
        {
            HoraInicio = inicio;
            HoraFin = fin;
        }
    }
}
