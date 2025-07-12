using System;

namespace PeluqueriaSaaS.Domain.Entities.Base
{
    public abstract class EntityBase
    {
        public int Id { get; protected set; }
        public DateTime FechaCreacion { get; protected set; }
        public DateTime? FechaActualizacion { get; protected set; }
        public string CreadoPor { get; protected set; }
        public string ActualizadoPor { get; protected set; }
        public bool Activo { get; protected set; }

        protected EntityBase()
        {
            FechaCreacion = DateTime.UtcNow;
            Activo = true;
        }

        public void MarcarActualizacion(string usuario)
        {
            FechaActualizacion = DateTime.UtcNow;
            ActualizadoPor = usuario;
        }

        public void MarcarCreacion(string usuario)
        {
            CreadoPor = usuario;
        }

        public virtual void Desactivar()
        {
            Activo = false;
        }

        public virtual void Activar()
        {
            Activo = true;
        }
    }
}
