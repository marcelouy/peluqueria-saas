using PeluqueriaSaaS.Domain.Entities.Base;
using PeluqueriaSaaS.Domain.Entities.Configuration;
using PeluqueriaSaaS.Domain.ValueObjects;

namespace PeluqueriaSaaS.Domain.Entities
{
    // =================================================================
    // EMPRESA (TENANT PRINCIPAL)
    // =================================================================
    public class Empresa : EntityBase
    {
        public string Nombre { get; private set; } = string.Empty;
        public string RUT { get; private set; } = string.Empty;
        public string Direccion { get; private set; } = string.Empty;
        public Telefono Telefono { get; private set; } = null!;
        public Email Email { get; private set; } = null!;
        public string? SitioWeb { get; private set; }
        public string? Logo { get; private set; }
        public bool EsActiva { get; private set; } = true;
        public DateTime FechaRegistro { get; private set; }
        public string PlanSuscripcion { get; private set; } = "BASICO";
        public DateTime? FechaVencimientoSuscripcion { get; private set; }

        // Configuraciones de la empresa
        public TimeSpan HorarioAperturaDefecto { get; private set; } = new(9, 0, 0);
        public TimeSpan HorarioCierreDefecto { get; private set; } = new(18, 0, 0);
        public int TiempoSlotMinutos { get; private set; } = 15;
        public int DiasAnticipacionMaxima { get; private set; } = 30;
        public bool PermiteReservaOnline { get; private set; } = true;

        // Navegación
        private readonly List<Sucursal> _sucursales = new();
        public IReadOnlyList<Sucursal> Sucursales => _sucursales.AsReadOnly();

        private Empresa() { }

        public Empresa(string nombre, string rut, string direccion, Telefono telefono, Email email)
        {
            Nombre = nombre;
            RUT = rut;
            Direccion = direccion;
            Telefono = telefono;
            Email = email;
            FechaRegistro = DateTime.UtcNow;
        }

        public void ActualizarInformacion(string nombre, string direccion, Telefono telefono, Email email, string? sitioWeb = null)
        {
            Nombre = nombre;
            Direccion = direccion;
            Telefono = telefono;
            Email = email;
            SitioWeb = sitioWeb;
        }

        public void ConfigurarHorarios(TimeSpan horarioApertura, TimeSpan horarioCierre, int tiempoSlotMinutos = 15)
        {
            if (horarioApertura >= horarioCierre)
                throw new ArgumentException("Horario de apertura debe ser menor al de cierre");

            HorarioAperturaDefecto = horarioApertura;
            HorarioCierreDefecto = horarioCierre;
            TiempoSlotMinutos = tiempoSlotMinutos;
        }

        public void AgregarSucursal(Sucursal sucursal)
        {
            _sucursales.Add(sucursal);
        }
    }
}