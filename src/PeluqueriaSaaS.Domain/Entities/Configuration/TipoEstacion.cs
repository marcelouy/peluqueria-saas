namespace PeluqueriaSaaS.Domain.Entities.Configuration
{
    public class TipoEstacion : ConfiguracionBase
    {
        public int CapacidadMaxima { get; private set; } = 1;
        public bool RequiereEquipoEspecial { get; private set; } = false;
        public string? EquipoRequerido { get; private set; }
        public bool EsMovil { get; private set; } = false;
        public int? TiempoLimpiezaMinutos { get; private set; }

        private TipoEstacion() { }

        public TipoEstacion(string codigo, string nombre, string? descripcion = null, 
            int capacidadMaxima = 1, bool requiereEquipoEspecial = false, 
            string? equipoRequerido = null, bool esMovil = false, 
            int? tiempoLimpiezaMinutos = null, string? color = null, 
            string? icono = null, int orden = 0, bool esPorDefecto = false, 
            bool esSistema = false)
            : base(codigo, nombre, descripcion, color, icono, orden, esPorDefecto, esSistema)
        {
            CapacidadMaxima = capacidadMaxima;
            RequiereEquipoEspecial = requiereEquipoEspecial;
            EquipoRequerido = equipoRequerido;
            EsMovil = esMovil;
            TiempoLimpiezaMinutos = tiempoLimpiezaMinutos;
        }

        public void ActualizarEspecificaciones(int capacidadMaxima, bool requiereEquipoEspecial, 
            string? equipoRequerido = null, bool esMovil = false, 
            int? tiempoLimpiezaMinutos = null)
        {
            CapacidadMaxima = capacidadMaxima;
            RequiereEquipoEspecial = requiereEquipoEspecial;
            EquipoRequerido = equipoRequerido;
            EsMovil = esMovil;
            TiempoLimpiezaMinutos = tiempoLimpiezaMinutos;
        }
    }
}
