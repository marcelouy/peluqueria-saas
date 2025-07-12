using PeluqueriaSaaS.Domain.Entities.Base;
using PeluqueriaSaaS.Domain.Entities.Configuration;
using PeluqueriaSaaS.Domain.ValueObjects;

namespace PeluqueriaSaaS.Domain.Entities
{
    public class Cita : TenantEntityBase
    {
        public DateTime FechaHora { get; private set; }
        public Dinero MontoTotal { get; private set; } = Dinero.Cero;
        public Dinero MontoPagado { get; private set; } = Dinero.Cero;
        public string? NotasCliente { get; private set; }
        public string? NotasInternas { get; private set; }
        public DateTime? FechaPago { get; private set; }

        // Foreign Keys
        public Guid ClienteId { get; private set; }
        public Guid EstadoCitaId { get; private set; }
        public Guid? EmpleadoId { get; private set; }
        public Guid SucursalId { get; private set; }

        // Navigation Properties
        public virtual Cliente Cliente { get; private set; } = null!;
        public virtual EstadoCita EstadoCita { get; private set; } = null!;
        public virtual Empleado? Empleado { get; private set; }
        public virtual Sucursal Sucursal { get; private set; } = null!;

        // Collections
        public virtual ICollection<CitaServicio> Servicios { get; private set; } = new List<CitaServicio>();
        public virtual ICollection<CitaEstacion> Estaciones { get; private set; } = new List<CitaEstacion>();

        private Cita() { }

        public Cita(DateTime fechaHora, Guid clienteId, Guid estadoCitaId, Guid sucursalId, Guid? empleadoId = null)
        {
            FechaHora = fechaHora;
            ClienteId = clienteId;
            EstadoCitaId = estadoCitaId;
            SucursalId = sucursalId;
            EmpleadoId = empleadoId;
        }

        public void ActualizarFechaHora(DateTime nuevaFechaHora) => FechaHora = nuevaFechaHora;
        public void CambiarEstado(Guid nuevoEstadoId) => EstadoCitaId = nuevoEstadoId;
        public void AsignarEmpleado(Guid empleadoId) => EmpleadoId = empleadoId;
        public void ActualizarNotasCliente(string notas) => NotasCliente = notas;
        public void ActualizarNotasInternas(string notas) => NotasInternas = notas;

        public void AgregarServicio(CitaServicio servicio)
        {
            Servicios.Add(servicio);
            RecalcularMontoTotal();
        }

        public void RemoverServicio(CitaServicio servicio)
        {
            Servicios.Remove(servicio);
            RecalcularMontoTotal();
        }

        private void RecalcularMontoTotal()
        {
            var total = Servicios.Sum(s => s.PrecioFinal.Valor);
            MontoTotal = new Dinero(total);
        }

        public void RegistrarPago(Dinero monto)
        {
            MontoPagado = MontoPagado + monto;
            if (MontoPagado.Valor >= MontoTotal.Valor)
            {
                FechaPago = DateTime.UtcNow;
            }
        }

        public bool EstaCompletamentePagada => MontoPagado.Valor >= MontoTotal.Valor;
        public Dinero MontoRestante => MontoTotal - MontoPagado;
    }
}
