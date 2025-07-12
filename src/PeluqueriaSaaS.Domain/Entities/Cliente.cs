using PeluqueriaSaaS.Domain.Entities.Base;
using PeluqueriaSaaS.Domain.ValueObjects;

namespace PeluqueriaSaaS.Domain.Entities
{
    public class Cliente : TenantEntityBase
    {
        public string Nombre { get; private set; } = string.Empty;
        public string Apellido { get; private set; } = string.Empty;
        public Email? Email { get; private set; }
        public Telefono? Telefono { get; private set; }
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
            
            if (!string.IsNullOrEmpty(email))
                Email = new Email(email);
            if (!string.IsNullOrEmpty(telefono))
                Telefono = new Telefono(telefono);
        }

        public string NombreCompleto => $"{Nombre} {Apellido}";

        public void ActualizarInformacion(string nombre, string apellido, string? email = null, string? telefono = null, DateTime? fechaNacimiento = null)
        {
            Nombre = nombre;
            Apellido = apellido;
            FechaNacimiento = fechaNacimiento;
            
            Email = !string.IsNullOrEmpty(email) ? new Email(email) : null;
            Telefono = !string.IsNullOrEmpty(telefono) ? new Telefono(telefono) : null;
        }

        public void ActualizarNotas(string notas) => Notas = notas;
        public new void Activar() => EsActivo = true;
        public new void Desactivar() => EsActivo = false;
    }
}
