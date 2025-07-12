namespace PeluqueriaSaaS.Domain.Entities.Configuration
{
    public class EstadoCita : ConfiguracionBase
    {
        public bool EsEstadoInicial { get; private set; } = false;
        public bool EsEstadoFinal { get; private set; } = false;
        public bool PermiteCancelacion { get; private set; } = true;
        public bool PermiteModificacion { get; private set; } = true;
        public bool RequiereConfirmacion { get; private set; } = false;
        public bool NotificarCliente { get; private set; } = true;
        public bool NotificarEmpleado { get; private set; } = true;

        private EstadoCita() { }

        public EstadoCita(string codigo, string nombre, string? descripcion = null, 
            bool esEstadoInicial = false, bool esEstadoFinal = false, 
            bool permiteCancelacion = true, bool permiteModificacion = true, 
            bool requiereConfirmacion = false, bool notificarCliente = true, 
            bool notificarEmpleado = true, string? color = null, 
            string? icono = null, int orden = 0, bool esPorDefecto = false, 
            bool esSistema = false)
            : base(codigo, nombre, descripcion, color, icono, orden, esPorDefecto, esSistema)
        {
            EsEstadoInicial = esEstadoInicial;
            EsEstadoFinal = esEstadoFinal;
            PermiteCancelacion = permiteCancelacion;
            PermiteModificacion = permiteModificacion;
            RequiereConfirmacion = requiereConfirmacion;
            NotificarCliente = notificarCliente;
            NotificarEmpleado = notificarEmpleado;
        }

        public void ActualizarComportamiento(bool permiteCancelacion, bool permiteModificacion,
            bool requiereConfirmacion, bool notificarCliente, bool notificarEmpleado)
        {
            PermiteCancelacion = permiteCancelacion;
            PermiteModificacion = permiteModificacion;
            RequiereConfirmacion = requiereConfirmacion;
            NotificarCliente = notificarCliente;
            NotificarEmpleado = notificarEmpleado;
        }
    }
}
