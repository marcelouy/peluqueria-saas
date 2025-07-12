using PeluqueriaSaaS.Domain.Entities.Base;
using PeluqueriaSaaS.Domain.Entities.Configuration;
using PeluqueriaSaaS.Domain.ValueObjects;

namespace PeluqueriaSaaS.Domain.Entities
{
    public class Servicio : TenantEntityBase
    {
        public string Nombre { get; private set; } = string.Empty;
        public string? Descripcion { get; private set; }
        public Dinero Precio { get; private set; } = Dinero.Cero;
        public int DuracionMinutos { get; private set; }
        public Guid TipoServicioId { get; private set; }
        public bool EsActivo { get; private set; } = true;

        // Navigation properties
        public virtual TipoServicio TipoServicio { get; private set; } = null!;

        private Servicio() { }

        public Servicio(string nombre, Dinero precio, int duracionMinutos, Guid tipoServicioId, string? descripcion = null)
        {
            Nombre = nombre;
            Precio = precio;
            DuracionMinutos = duracionMinutos;
            TipoServicioId = tipoServicioId;
            Descripcion = descripcion;
        }

        public void ActualizarInformacion(string nombre, Dinero precio, int duracionMinutos, string? descripcion = null)
        {
            Nombre = nombre;
            Precio = precio;
            DuracionMinutos = duracionMinutos;
            Descripcion = descripcion;
        }

        public new void Activar() => EsActivo = true;
        public new void Desactivar() => EsActivo = false;
    }
}
