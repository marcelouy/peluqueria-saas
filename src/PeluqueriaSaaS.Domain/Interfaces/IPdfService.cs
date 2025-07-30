using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Domain.Interfaces
{
    /// <summary>
    /// ✅ PDF SERVICE INTERFACE - Domain Layer (Business Contracts)
    /// Location: src/PeluqueriaSaaS.Domain/Interfaces/IPdfService.cs
    /// Purpose: Define contracts for PDF generation business logic
    /// Business Value: Professional PDF generation capability - unique market differentiator
    /// </summary>
    public interface IPdfService
    {
        /// <summary>
        /// Genera PDF profesional del resumen de servicio usando configuración activa
        /// </summary>
        /// <param name="ventaId">ID de la venta</param>
        /// <returns>PDF bytes array para download</returns>
        Task<byte[]> GenerateServiceSummaryPdfAsync(int ventaId);

        /// <summary>
        /// Genera PDF profesional con configuración específica personalizada
        /// </summary>
        /// <param name="ventaId">ID de la venta</param>
        /// <param name="settings">Configuración específica del PDF (colores, textos, etc.)</param>
        /// <returns>PDF bytes array para download</returns>
        Task<byte[]> GenerateServiceSummaryPdfAsync(int ventaId, Settings settings);

        /// <summary>
        /// Genera PDF optimizado para impresora térmica (58mm o 80mm)
        /// </summary>
        /// <param name="ventaId">ID de la venta</param>
        /// <param name="printerWidth">Ancho impresora en mm (58 o 80)</param>
        /// <returns>PDF bytes array optimizado para impresión térmica</returns>
        Task<byte[]> GenerateThermalReceiptPdfAsync(int ventaId, int printerWidth = 80);

        /// <summary>
        /// Valida si el servicio PDF está disponible y funcionando
        /// </summary>
        /// <returns>True si está disponible, false si hay problemas</returns>
        Task<bool> IsServiceAvailableAsync();
    }
}