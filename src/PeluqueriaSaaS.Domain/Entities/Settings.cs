using System.ComponentModel.DataAnnotations;

namespace PeluqueriaSaaS.Domain.Entities
{
    /// <summary>
    /// ✅ SETTINGS ENTITY - Configuración sistema por peluquería
    /// Propósito: Resumen servicio opcional Uruguay + otras configuraciones
    /// Business Value: Feature diferenciador vs competencia
    /// </summary>
    public class Settings
    {
        [Key]
        public int Id { get; set; }

        // ✅ IDENTIFICACIÓN PELUQUERÍA
        [Required]
        [StringLength(100)]
        public string NombrePeluqueria { get; set; } = string.Empty;

        [StringLength(200)]
        public string DireccionPeluqueria { get; set; } = string.Empty;

        [StringLength(20)]
        public string TelefonoPeluqueria { get; set; } = string.Empty;

        [StringLength(100)]
        public string EmailPeluqueria { get; set; } = string.Empty;

        // ✅ CONFIGURACIÓN RESUMEN SERVICIO URUGUAY
        public bool ResumenServicioHabilitado { get; set; } = false;

        [StringLength(500)]
        public string ResumenEncabezado { get; set; } = "COMPROBANTE INTERNO - SIN VALOR FISCAL";

        [StringLength(1000)]
        public string ResumenPiePagina { get; set; } = "Gracias por su visita. Este comprobante es solo para control interno.";

        public bool MostrarLogoEnResumen { get; set; } = false;

        [StringLength(200)]
        public string RutaLogo { get; set; } = string.Empty;

        // ✅ CONFIGURACIÓN FORMATO RESUMEN
        public bool MostrarDatosCliente { get; set; } = true;
        
        public bool MostrarEmpleadoServicio { get; set; } = true;
        
        public bool MostrarFechaHora { get; set; } = true;
        
        public bool MostrarDetalleServicios { get; set; } = true;
        
        public bool MostrarTotalServicio { get; set; } = true;

        // ✅ CONFIGURACIÓN PERSONALIZACIÓN
        [StringLength(50)]
        public string ColorPrimario { get; set; } = "#007bff";

        [StringLength(50)]
        public string ColorSecundario { get; set; } = "#6c757d";

        [StringLength(20)]
        public string TamanoFuente { get; set; } = "12px";

        // ✅ CONFIGURACIÓN MONEDA URUGUAY
        [StringLength(10)]
        public string SimboloMoneda { get; set; } = "$U";

        [StringLength(50)]
        public string FormatoMoneda { get; set; } = "N2";

        // ✅ CONFIGURACIÓN ADICIONAL SISTEMA
        public bool NotificacionesEmail { get; set; } = true;
        
        public bool BackupAutomatico { get; set; } = true;
        
        public int DiasRetencionVentas { get; set; } = 365;

        // ✅ METADATOS
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        public DateTime? FechaActualizacion { get; set; }
        
        public bool Activo { get; set; } = true;

        // ✅ IDENTIFICADOR ÚNICO PELUQUERÍA (para multi-tenant futuro)
        [StringLength(50)]
        public string CodigoPeluqueria { get; set; } = "MAIN";

        // ✅ CONFIGURACIÓN RESUMEN TEMPLATE
        [StringLength(2000)]
        public string TemplateCustomHTML { get; set; } = string.Empty;

        public bool UsarTemplateCustom { get; set; } = false;

        // ✅ MÉTODOS HELPER
        public string GetNombreCompletoConDireccion()
        {
            var result = NombrePeluqueria;
            if (!string.IsNullOrEmpty(DireccionPeluqueria))
            {
                result += $" - {DireccionPeluqueria}";
            }
            return result;
        }

        public string GetContactoCompleto()
        {
            var contactos = new List<string>();
            
            if (!string.IsNullOrEmpty(TelefonoPeluqueria))
                contactos.Add($"Tel: {TelefonoPeluqueria}");
                
            if (!string.IsNullOrEmpty(EmailPeluqueria))
                contactos.Add($"Email: {EmailPeluqueria}");
                
            return string.Join(" | ", contactos);
        }

        public bool TieneLogoConfigurado()
        {
            return MostrarLogoEnResumen && !string.IsNullOrEmpty(RutaLogo);
        }

        public void ActualizarFecha()
        {
            FechaActualizacion = DateTime.Now;
        }
    }
}