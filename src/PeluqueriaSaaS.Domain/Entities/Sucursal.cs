using PeluqueriaSaaS.Domain.Entities.Base;
using PeluqueriaSaaS.Domain.ValueObjects;

namespace PeluqueriaSaaS.Domain.Entities
{
    public class Sucursal : TenantEntityBase
    {
        public string Nombre { get; private set; } = string.Empty;
        public string? Direccion { get; private set; }
        public Telefono? Telefono { get; private set; }
        public Email? Email { get; private set; }
        public Guid EmpresaId { get; private set; }
        public bool EsActiva { get; private set; } = true;

        // Navigation properties
        public virtual Empresa Empresa { get; private set; } = null!;
        public virtual ICollection<Empleado> Empleados { get; private set; } = new List<Empleado>();
        public virtual ICollection<Estacion> Estaciones { get; private set; } = new List<Estacion>();

        private Sucursal() { }

        public Sucursal(string nombre, Guid empresaId, string? direccion = null, string? telefono = null, string? email = null)
        {
            Nombre = nombre;
            EmpresaId = empresaId;
            Direccion = direccion;
            
            if (!string.IsNullOrEmpty(telefono))
                Telefono = new Telefono(telefono);
            if (!string.IsNullOrEmpty(email))
                Email = new Email(email);
        }

        public void ActualizarInformacion(string nombre, string? direccion = null, string? telefono = null, string? email = null)
        {
            Nombre = nombre;
            Direccion = direccion;
            
            Telefono = !string.IsNullOrEmpty(telefono) ? new Telefono(telefono) : null;
            Email = !string.IsNullOrEmpty(email) ? new Email(email) : null;
        }

        public new void Activar() => EsActiva = true;
        public new void Desactivar() => EsActiva = false;
    }
}
