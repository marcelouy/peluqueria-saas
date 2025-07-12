using PeluqueriaSaaS.Domain.Entities.Base;
using PeluqueriaSaaS.Domain.Entities.Configuration;
using PeluqueriaSaaS.Domain.ValueObjects;

namespace PeluqueriaSaaS.Domain.Entities
{
    public class Empleado : TenantEntityBase
    {
        public string Nombre { get; private set; } = string.Empty;
        public string Apellido { get; private set; } = string.Empty;
        public Email? Email { get; private set; }
        public Telefono? Telefono { get; private set; }
        public Guid EstadoEmpleadoId { get; private set; }
        public Guid SucursalId { get; private set; }
        public bool EsActivo { get; private set; } = true;

        // Navigation properties
        public virtual EstadoEmpleado EstadoEmpleado { get; private set; } = null!;
        public virtual Sucursal Sucursal { get; private set; } = null!;

        private Empleado() { }

        public Empleado(string nombre, string apellido, Guid estadoEmpleadoId, Guid sucursalId, string? email = null, string? telefono = null)
        {
            Nombre = nombre;
            Apellido = apellido;
            EstadoEmpleadoId = estadoEmpleadoId;
            SucursalId = sucursalId;
            
            if (!string.IsNullOrEmpty(email))
                Email = new Email(email);
            if (!string.IsNullOrEmpty(telefono))
                Telefono = new Telefono(telefono);
        }

        public string NombreCompleto => $"{Nombre} {Apellido}";

        public void ActualizarInformacion(string nombre, string apellido, string? email = null, string? telefono = null)
        {
            Nombre = nombre;
            Apellido = apellido;
            
            Email = !string.IsNullOrEmpty(email) ? new Email(email) : null;
            Telefono = !string.IsNullOrEmpty(telefono) ? new Telefono(telefono) : null;
        }

        public void CambiarEstado(Guid estadoEmpleadoId) => EstadoEmpleadoId = estadoEmpleadoId;
        public new void Activar() => EsActivo = true;
        public new void Desactivar() => EsActivo = false;
    }
}
