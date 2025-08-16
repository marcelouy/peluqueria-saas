using PeluqueriaSaaS.Domain.Entities.Base;

namespace PeluqueriaSaaS.Domain.Entities
{
    /// <summary>
    /// Representa un estado posible en el flujo de trabajo de servicios
    /// </summary>
    public class EstadoServicio : TenantEntityBase
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public bool PermiteCobro { get; set; }
        public bool EsFinal { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; } = true;

        // Constructor vacío para EF
        public EstadoServicio() { }
    }
}