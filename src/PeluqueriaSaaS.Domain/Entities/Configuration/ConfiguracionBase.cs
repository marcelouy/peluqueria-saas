using PeluqueriaSaaS.Domain.Entities.Base;

namespace PeluqueriaSaaS.Domain.Entities.Configuration
{
    public abstract class ConfiguracionBase : TenantEntityBase
    {
        public string Codigo { get; protected set; } = string.Empty;
        public string Nombre { get; protected set; } = string.Empty;
        public string? Descripcion { get; protected set; }
        public string? Color { get; protected set; }
        public string? Icono { get; protected set; }
        public int Orden { get; protected set; }
        public bool EsActivo { get; protected set; } = true;
        public bool EsPorDefecto { get; protected set; } = false;
        public bool EsSistema { get; protected set; } = false;

        protected ConfiguracionBase() { }

        protected ConfiguracionBase(string codigo, string nombre, string? descripcion = null, 
            string? color = null, string? icono = null, int orden = 0, 
            bool esPorDefecto = false, bool esSistema = false)
        {
            Codigo = codigo;
            Nombre = nombre;
            Descripcion = descripcion;
            Color = color;
            Icono = icono;
            Orden = orden;
            EsPorDefecto = esPorDefecto;
            EsSistema = esSistema;
        }

        public virtual void ActualizarInformacion(string nombre, string? descripcion = null, 
            string? color = null, string? icono = null, int orden = 0)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            Color = color;
            Icono = icono;
            Orden = orden;
        }

        public virtual void Activar() => EsActivo = true;
        public virtual void Desactivar() => EsActivo = false;
        public virtual void MarcarComoPorDefecto() => EsPorDefecto = true;
        public virtual void QuitarPorDefecto() => EsPorDefecto = false;
    }
}
