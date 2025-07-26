using PeluqueriaSaaS.Domain.Entities.Base;
using PeluqueriaSaaS.Domain.ValueObjects;

namespace PeluqueriaSaaS.Domain.Entities
{
    public class Cliente : TenantEntityBase
    {
        public string Nombre { get; private set; } = string.Empty;
        public string Apellido { get; private set; } = string.Empty;
        public string? Email { get; private set; }
        public string? Telefono { get; private set; }
        public DateTime? FechaNacimiento { get; private set; }
        public string? Notas { get; private set; }
        public bool EsActivo { get; private set; } = true;

        // Navigation properties
        public virtual ICollection<Cita> Citas { get; private set; } = new List<Cita>();
        public virtual ICollection<HistorialCliente> Historial { get; private set; } = new List<HistorialCliente>();

        private Cliente() { }

        public Cliente(string nombre, string apellido, string? email = null, string? telefono = null, DateTime? fechaNacimiento = null)
        {
            Nombre = nombre;
            Apellido = apellido;
            FechaNacimiento = fechaNacimiento;
            
            // ✅ FIXED: Direct string assignment (NO ValueObjects)
            Email = email;
            Telefono = telefono;
            
            // ✅ FIXED: Use base class methods to set ALL inherited properties
            SetTenant("default-tenant");      // TenantEntityBase method
            MarcarCreacion("SYSTEM");         // EntityBase method - sets CreadoPor
            MarcarActualizacion("SYSTEM");    // EntityBase method - sets ActualizadoPor + FechaActualizacion
            
            // ✅ Set Client-specific defaults
            EsActivo = true;
            // Note: Activo, FechaCreacion already set by EntityBase constructor
        }

        public string NombreCompleto => $"{Nombre} {Apellido}";

        public void ActualizarInformacion(string nombre, string apellido, string? email = null, string? telefono = null, DateTime? fechaNacimiento = null)
        {
            Nombre = nombre;
            Apellido = apellido;
            FechaNacimiento = fechaNacimiento;
            
            // ✅ FIXED: Direct string assignment (NO ValueObjects)
            Email = email;
            Telefono = telefono;
        }

        public void ActualizarNotas(string notas) => Notas = notas;
        public new void Activar() => EsActivo = true;
        public new void Desactivar() => EsActivo = false;
    }
}