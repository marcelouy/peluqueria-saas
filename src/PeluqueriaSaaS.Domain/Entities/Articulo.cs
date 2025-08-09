using PeluqueriaSaaS.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace PeluqueriaSaaS.Domain.Entities
{
    public class Articulo : TenantEntityBase
    {
        [StringLength(50)]
        public string? Codigo { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        public decimal Precio { get; set; }

        public decimal? PrecioCosto { get; set; }

        public decimal? Margen { get; set; }

        public bool Oferta { get; set; } = false;

        public decimal? PrecioOferta { get; set; }

        [Required]
        public int Stock { get; set; } = 0;

        public int StockMinimo { get; set; } = 0;

        public bool RequiereStock { get; set; } = true;

        [StringLength(50)]
        public string? Categoria { get; set; }

        [StringLength(50)]
        public string? Marca { get; set; }

        [StringLength(100)]
        public string? Proveedor { get; set; }

        public decimal? Peso { get; set; }

        [StringLength(50)]
        public string? Contenido { get; set; }

        // Navegación
        public virtual ICollection<VentaDetalle> VentaDetalles { get; set; } = new List<VentaDetalle>();

        // Métodos negocio
        public void CalcularMargen()
        {
            if (PrecioCosto.HasValue && PrecioCosto > 0)
            {
                Margen = Math.Round(((Precio - PrecioCosto.Value) / PrecioCosto.Value) * 100, 2);
            }
            else
            {
                Margen = null;
            }
        }

        public decimal PrecioEfectivo => Oferta && PrecioOferta.HasValue ? PrecioOferta.Value : Precio;
    }
}