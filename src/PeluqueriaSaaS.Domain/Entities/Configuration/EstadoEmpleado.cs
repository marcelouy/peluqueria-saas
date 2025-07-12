namespace PeluqueriaSaaS.Domain.Entities.Configuration
{
    public class EstadoEmpleado : ConfiguracionBase
    {
        public bool PuedeAtenderClientes { get; private set; } = true;
        public bool PuedeRecibirCitas { get; private set; } = true;
        public bool EsVisible { get; private set; } = true;
        public bool RequiereJustificacion { get; private set; } = false;
        public int? TiempoMaximoMinutos { get; private set; }

        private EstadoEmpleado() { }

        public EstadoEmpleado(string codigo, string nombre, string? descripcion = null, 
            bool puedeAtenderClientes = true, bool puedeRecibirCitas = true, 
            bool esVisible = true, bool requiereJustificacion = false, 
            int? tiempoMaximoMinutos = null, string? color = null, 
            string? icono = null, int orden = 0, bool esPorDefecto = false, 
            bool esSistema = false)
            : base(codigo, nombre, descripcion, color, icono, orden, esPorDefecto, esSistema)
        {
            PuedeAtenderClientes = puedeAtenderClientes;
            PuedeRecibirCitas = puedeRecibirCitas;
            EsVisible = esVisible;
            RequiereJustificacion = requiereJustificacion;
            TiempoMaximoMinutos = tiempoMaximoMinutos;
        }

        public void ActualizarDisponibilidad(bool puedeAtenderClientes, bool puedeRecibirCitas, 
            bool esVisible, bool requiereJustificacion = false, int? tiempoMaximoMinutos = null)
        {
            PuedeAtenderClientes = puedeAtenderClientes;
            PuedeRecibirCitas = puedeRecibirCitas;
            EsVisible = esVisible;
            RequiereJustificacion = requiereJustificacion;
            TiempoMaximoMinutos = tiempoMaximoMinutos;
        }
    }
}
