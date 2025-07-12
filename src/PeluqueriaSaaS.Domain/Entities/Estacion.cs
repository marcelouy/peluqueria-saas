using PeluqueriaSaaS.Domain.Entities.Base;
using PeluqueriaSaaS.Domain.Entities.Configuration;

namespace PeluqueriaSaaS.Domain.Entities
{
    public class Estacion : TenantEntityBase
    {
        public string Nombre { get; private set; } = string.Empty;
        public string? Descripcion { get; private set; }
        public Guid TipoEstacionId { get; private set; }
        public Guid SucursalId { get; private set; }
        public bool EsActiva { get; private set; } = true;
        public int Capacidad { get; private set; } = 1;

        // Navigation properties
        public virtual TipoEstacion TipoEstacion { get; private set; } = null!;
        public virtual Sucursal Sucursal { get; private set; } = null!;

        private Estacion() { }

        public Estacion(string nombre, Guid tipoEstacionId, Guid sucursalId, int capacidad = 1, string? descripcion = null)
        {
            Nombre = nombre;
            TipoEstacionId = tipoEstacionId;
            SucursalId = sucursalId;
            Capacidad = capacidad;
            Descripcion = descripcion;
        }

        public void ActualizarInformacion(string nombre, int capacidad, string? descripcion = null)
        {
            Nombre = nombre;
            Capacidad = capacidad;
            Descripcion = descripcion;
        }

        public new void Activar() => EsActiva = true;
        public new void Desactivar() => EsActiva = false;
    }
}
