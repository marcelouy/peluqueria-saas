using System.ComponentModel.DataAnnotations;

namespace PeluqueriaSaaS.Domain.Entities.Base
{
    public abstract class EntityBase
    {
        [Key]
        public int Id { get; protected set; }
        
        public bool Activo { get; protected set; } = true;
        
        public DateTime FechaCreacion { get; protected set; } = DateTime.UtcNow;
        
        public DateTime FechaActualizacion { get; protected set; } = DateTime.UtcNow;
        
        // ✅ FIXED: Nullable strings to handle NULL values from BD
        public string? CreadoPor { get; protected set; }
        
        public string? ActualizadoPor { get; protected set; }

        protected EntityBase()
        {
            FechaCreacion = DateTime.UtcNow;
            FechaActualizacion = DateTime.UtcNow;
            Activo = true;
        }

        public virtual void MarcarCreacion(string? usuario = null)
        {
            CreadoPor = usuario;
            FechaCreacion = DateTime.UtcNow;
            FechaActualizacion = DateTime.UtcNow;
        }

        public virtual void MarcarActualizacion(string? usuario = null)
        {
            ActualizadoPor = usuario;
            FechaActualizacion = DateTime.UtcNow;
        }

        public virtual void Activar()
        {
            Activo = true;
            MarcarActualizacion();
        }

        public virtual void Desactivar()
        {
            Activo = false;
            MarcarActualizacion();
        }
    }
}