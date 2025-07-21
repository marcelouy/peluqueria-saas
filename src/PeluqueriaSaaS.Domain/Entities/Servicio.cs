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
        public int TipoServicioId { get; private set; } // ✅ CORREGIDO: Guid → int
        public bool EsActivo { get; private set; } = true;

        // Navigation properties
        public virtual TipoServicio TipoServicio { get; private set; } = null!;

        private Servicio() { }

        public Servicio(string nombre, Dinero precio, int duracionMinutos, int tipoServicioId, string? descripcion = null) // ✅ CORREGIDO: int tipoServicioId
        {
            Nombre = nombre;
            Precio = precio;
            DuracionMinutos = duracionMinutos;
            TipoServicioId = tipoServicioId; // ✅ CORREGIDO
            Descripcion = descripcion;
        }

        public void ActualizarInformacion(string nombre, Dinero precio, int duracionMinutos, string? descripcion = null)
        {
            Nombre = nombre;
            Precio = precio;
            DuracionMinutos = duracionMinutos;
            Descripcion = descripcion;
        }

        // ✅ NUEVO MÉTODO: Para actualizar TipoServicioId
        public void ActualizarTipoServicio(int tipoServicioId)
        {
            TipoServicioId = tipoServicioId;
        }

        public new void Activar() => EsActivo = true;
        public new void Desactivar() => EsActivo = false;
    }
}