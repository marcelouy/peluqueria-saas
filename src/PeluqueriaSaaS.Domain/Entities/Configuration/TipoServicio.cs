namespace PeluqueriaSaaS.Domain.Entities.Configuration
{
    public class TipoServicio : ConfiguracionBase
    {
        public int DuracionEstimadaMinutos { get; private set; }
        public decimal PrecioBase { get; private set; }
        public bool RequiereEspecialista { get; private set; } = false;
        public bool PermiteDescuentos { get; private set; } = true;
        public int? TiempoPreparacionMinutos { get; private set; }

        private TipoServicio() { }

        public TipoServicio(string codigo, string nombre, int duracionEstimadaMinutos, 
            decimal precioBase, string? descripcion = null, bool requiereEspecialista = false, 
            bool permiteDescuentos = true, int? tiempoPreparacionMinutos = null, 
            string? color = null, string? icono = null, int orden = 0, 
            bool esPorDefecto = false, bool esSistema = false)
            : base(codigo, nombre, descripcion, color, icono, orden, esPorDefecto, esSistema)
        {
            DuracionEstimadaMinutos = duracionEstimadaMinutos;
            PrecioBase = precioBase;
            RequiereEspecialista = requiereEspecialista;
            PermiteDescuentos = permiteDescuentos;
            TiempoPreparacionMinutos = tiempoPreparacionMinutos;
        }

        public void ActualizarConfiguracion(int duracionEstimadaMinutos, decimal precioBase, 
            bool requiereEspecialista = false, bool permiteDescuentos = true, 
            int? tiempoPreparacionMinutos = null)
        {
            DuracionEstimadaMinutos = duracionEstimadaMinutos;
            PrecioBase = precioBase;
            RequiereEspecialista = requiereEspecialista;
            PermiteDescuentos = permiteDescuentos;
            TiempoPreparacionMinutos = tiempoPreparacionMinutos;
        }
    }
}
