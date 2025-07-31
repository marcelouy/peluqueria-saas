using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace PeluqueriaSaaS.Infrastructure.Services
{
    public class PuppeteerPdfService : IPdfService
    {
        private readonly IBrowserPool _browserPool;
        private readonly ISettingsRepository _settingsRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PuppeteerPdfService> _logger;

        public PuppeteerPdfService(
            IBrowserPool browserPool,
            ISettingsRepository settingsRepository,
            IMemoryCache cache,
            ILogger<PuppeteerPdfService> logger)
        {
            _browserPool = browserPool;
            _settingsRepository = settingsRepository;
            _cache = cache;
            _logger = logger;
        }

        // ✅ SOBRECARGA 1: Solo ventaId (requerida por interface)
        public async Task<byte[]> GenerateServiceSummaryPdfAsync(int ventaId)
        {
            return await GenerateServiceSummaryPdfAsync(ventaId, "A4");
        }

        // ✅ SOBRECARGA 2: ventaId + Settings (requerida por interface)  
        public async Task<byte[]> GenerateServiceSummaryPdfAsync(int ventaId, Settings settings)
        {
            try
            {
                _logger.LogInformation("Generando PDF profesional para venta {VentaId} con settings personalizados", ventaId);

                // Get template HTML with custom settings
                var htmlContent = await GenerateServiceSummaryHtmlWithSettingsAsync(ventaId, settings);
                
                // Convert to PDF
                var pdfBytes = await ConvertHtmlToPdfAsync(htmlContent, "A4");
                
                _logger.LogInformation("PDF generado exitosamente. Tamaño: {Size} bytes", pdfBytes.Length);
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando PDF para venta {VentaId} con settings", ventaId);
                throw;
            }
        }

        // ✅ MÉTODO INTERNO: Para compatibilidad con formato (mantener funcionalidad existente)
        private async Task<byte[]> GenerateServiceSummaryPdfAsync(int ventaId, string formato = "A4")
        {
            try
            {
                _logger.LogInformation("Generando PDF profesional para venta {VentaId} en formato {Formato}", ventaId, formato);

                // Get template HTML
                var htmlContent = await GenerateServiceSummaryHtmlAsync(ventaId);
                
                // Convert to PDF
                var pdfBytes = await ConvertHtmlToPdfAsync(htmlContent, formato);
                
                _logger.LogInformation("PDF generado exitosamente. Tamaño: {Size} bytes", pdfBytes.Length);
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando PDF para venta {VentaId}", ventaId);
                throw;
            }
        }

        public async Task<byte[]> GenerateThermalReceiptPdfAsync(int ventaId, int anchoMm = 80)
        {
            try
            {
                _logger.LogInformation("Generando PDF térmico para venta {VentaId} con ancho {Ancho}mm", ventaId, anchoMm);

                // Get template HTML optimized for thermal
                var htmlContent = await GenerateServiceSummaryHtmlAsync(ventaId);
                
                // Convert to PDF with thermal settings
                var pdfBytes = await ConvertHtmlToPdfAsync(htmlContent, $"thermal_{anchoMm}mm");
                
                _logger.LogInformation("PDF térmico generado exitosamente. Tamaño: {Size} bytes", pdfBytes.Length);
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando PDF térmico para venta {VentaId}", ventaId);
                throw;
            }
        }

        public async Task<bool> IsServiceAvailableAsync()
        {
            try
            {
                return await _browserPool.IsHealthyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando disponibilidad del servicio PDF");
                return false;
            }
        }

        private Task<string> GenerateServiceSummaryHtmlWithSettingsAsync(int ventaId, Settings customSettings)
        {
            // Use cache for performance
            var cacheKey = $"pdf_html_custom_{ventaId}_{customSettings.GetHashCode()}";
            if (_cache.TryGetValue(cacheKey, out string? cachedHtml) && cachedHtml != null)
            {
                _logger.LogDebug("HTML recuperado del cache para venta {VentaId} con settings custom", ventaId);
                return Task.FromResult(cachedHtml);
            }

            // Mock data for now - replace with actual repository calls when available
            var ventaData = GetMockVentaData(ventaId);
            
            var html = GenerateHtmlTemplate(ventaData, customSettings);
            
            // Cache for 2 hours
            _cache.Set(cacheKey, html, TimeSpan.FromHours(2));
            
            return Task.FromResult(html);
        }

        private async Task<string> GenerateServiceSummaryHtmlAsync(int ventaId)
        {
            // Use cache for performance
            var cacheKey = $"pdf_html_{ventaId}";
            if (_cache.TryGetValue(cacheKey, out string? cachedHtml) && cachedHtml != null)
            {
                _logger.LogDebug("HTML recuperado del cache para venta {VentaId}", ventaId);
                return cachedHtml;
            }

            // Get settings for template
            var settings = await _settingsRepository.GetSettingsAsync();
            
            // Mock data for now - replace with actual repository calls when available
            var ventaData = GetMockVentaData(ventaId);
            
            var html = GenerateHtmlTemplate(ventaData, settings ?? GetDefaultSettings());
            
            // Cache for 2 hours
            _cache.Set(cacheKey, html, TimeSpan.FromHours(2));
            
            return html;
        }

        private Venta GetMockVentaData(int ventaId)
        {
            return new Venta
            {
                VentaId = ventaId,
                NumeroVenta = $"V-{ventaId:000}",
                FechaVenta = DateTime.Now,
                SubTotal = 1500.00m,
                Descuento = 150.00m,
                Total = 1350.00m,
                EstadoVenta = "Completada",
                Cliente = new Cliente("María", "González", "maria.gonzalez@email.com", "099123456", new DateTime(1985, 5, 15)),
                Empleado = new Empleado 
                { 
                    Id = 1, 
                    Nombre = "Ana", 
                    Apellido = "Martínez",
                    Cargo = "Estilista Senior"
                },
                VentaDetalles = new List<VentaDetalle>
                {
                    new VentaDetalle
                    {
                        VentaDetalleId = 1,
                        NombreServicio = "Corte de Cabello",
                        PrecioUnitario = 800.00m,
                        Cantidad = 1,
                        Subtotal = 800.00m
                    },
                    new VentaDetalle
                    {
                        VentaDetalleId = 2,
                        NombreServicio = "Barba y Bigote",
                        PrecioUnitario = 700.00m,
                        Cantidad = 1,
                        Subtotal = 700.00m
                    }
                }
            };
        }

        private Settings GetDefaultSettings()
        {
            return new Settings
            {
                NombrePeluqueria = "Peluquería SaaS",
                DireccionPeluqueria = "Montevideo, Uruguay",
                TelefonoPeluqueria = "+598 99 123 456",
                EmailPeluqueria = "info@peluqueriasaas.uy",
                ColorPrimario = "#2563eb",
                ColorSecundario = "#1e40af"
            };
        }

        private string GenerateHtmlTemplate(Venta venta, Settings settings)
        {
            var colorPrimario = settings.ColorPrimario ?? "#2563eb";
            var colorSecundario = settings.ColorSecundario ?? "#1e40af";
            var nombrePeluqueria = settings.NombrePeluqueria ?? "Peluquería SaaS";
            var direccion = settings.DireccionPeluqueria ?? "Montevideo, Uruguay";
            var telefono = settings.TelefonoPeluqueria ?? "+598 99 123 456";

            var html = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Resumen de Servicios - {venta.NumeroVenta}</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: 'Arial', sans-serif;
            line-height: 1.4;
            color: #333;
            background: white;
        }}
        
        .container {{
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
        }}
        
        .header {{
            text-align: center;
            margin-bottom: 30px;
            border-bottom: 3px solid {colorPrimario};
            padding-bottom: 20px;
        }}
        
        .logo {{
            font-size: 28px;
            font-weight: bold;
            color: {colorPrimario};
            margin-bottom: 10px;
        }}
        
        .empresa-info {{
            color: #666;
            font-size: 14px;
        }}
        
        .venta-info {{
            display: flex;
            justify-content: space-between;
            margin-bottom: 30px;
            background: #f8fafc;
            padding: 15px;
            border-radius: 8px;
        }}
        
        .servicios-table {{
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 30px;
        }}
        
        .servicios-table th {{
            background: {colorPrimario};
            color: white;
            padding: 12px;
            text-align: left;
            font-weight: bold;
        }}
        
        .servicios-table td {{
            padding: 10px 12px;
            border-bottom: 1px solid #e2e8f0;
        }}
        
        .servicios-table tr:nth-child(even) {{
            background: #f8fafc;
        }}
        
        .total-section {{
            text-align: right;
            margin-bottom: 30px;
        }}
        
        .total-row {{
            display: flex;
            justify-content: space-between;
            margin-bottom: 8px;
            font-size: 16px;
        }}
        
        .total-final {{
            font-size: 20px;
            font-weight: bold;
            color: {colorPrimario};
            border-top: 2px solid {colorPrimario};
            padding-top: 10px;
        }}
        
        .footer {{
            text-align: center;
            margin-top: 40px;
            padding-top: 20px;
            border-top: 1px solid #e2e8f0;
            color: #666;
            font-size: 12px;
        }}
        
        .disclaimer {{
            background: #fef3c7;
            border: 1px solid #f59e0b;
            padding: 10px;
            border-radius: 4px;
            margin-top: 20px;
            font-size: 11px;
            text-align: center;
        }}
        
        @media print {{
            .container {{
                max-width: none;
                margin: 0;
                padding: 10px;
            }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>{nombrePeluqueria}</div>
            <div class='empresa-info'>
                📍 {direccion}<br>
                📞 {telefono}<br>
                📧 {settings.EmailPeluqueria ?? "info@peluqueriasaas.uy"}
            </div>
        </div>
        
        <div class='venta-info'>
            <div>
                <strong>Cliente:</strong> {venta.Cliente?.NombreCompleto}<br>
                <strong>Teléfono:</strong> {venta.Cliente?.Telefono}<br>
                <strong>Atendido por:</strong> {venta.Empleado?.NombreCompleto}
            </div>
            <div>
                <strong>Comprobante:</strong> {venta.NumeroVenta}<br>
                <strong>Fecha:</strong> {venta.FechaVenta:dd/MM/yyyy HH:mm}<br>
                <strong>Estado:</strong> {venta.EstadoVenta}
            </div>
        </div>
        
        <table class='servicios-table'>
            <thead>
                <tr>
                    <th>Servicio</th>
                    <th>Precio Unit.</th>
                    <th>Cant.</th>
                    <th>Subtotal</th>
                </tr>
            </thead>
            <tbody>";

            foreach (var detalle in venta.VentaDetalles)
            {
                html += $@"
                <tr>
                    <td>{detalle.NombreServicio}</td>
                    <td>${detalle.PrecioUnitario:N2}</td>
                    <td>{detalle.Cantidad}</td>
                    <td>${detalle.Subtotal:N2}</td>
                </tr>";
            }

            html += $@"
            </tbody>
        </table>
        
        <div class='total-section'>
            <div class='total-row'>
                <span>Subtotal:</span>
                <span>${venta.SubTotal:N2}</span>
            </div>
            <div class='total-row'>
                <span>Descuento:</span>
                <span>-${venta.Descuento:N2}</span>
            </div>
            <div class='total-row total-final'>
                <span>TOTAL:</span>
                <span>${venta.Total:N2} UYU</span>
            </div>
        </div>
        
        <div class='footer'>
            <p>¡Gracias por confiar en {nombrePeluqueria}!</p>
            <p>Tu próxima cita nos espera 💇‍♀️✨</p>
            
            <div class='disclaimer'>
                <strong>⚠️ COMPROBANTE INTERNO - SIN VALOR FISCAL</strong><br>
                Este documento es solo para control interno y no tiene validez fiscal según normativa uruguaya.
                Para facturación oficial, solicitar comprobante fiscal correspondiente.
            </div>
        </div>
    </div>
</body>
</html>";
            
            return html;
        }

        private async Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent, string formato)
        {
            using var browser = await _browserPool.GetBrowserAsync();
            using var page = await browser.NewPageAsync();
            
            await page.SetContentAsync(htmlContent);
            
            var pdfOptions = GetPdfOptionsForFormat(formato);
            var pdfBytes = await page.PdfDataAsync(pdfOptions);
            
            return pdfBytes;
        }

        private PdfOptions GetPdfOptionsForFormat(string formato)
        {
            return formato.ToLower() switch
            {
                "a4" => new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "10mm",
                        Bottom = "10mm",
                        Left = "10mm",
                        Right = "10mm"
                    }
                },
                var thermal when thermal.StartsWith("thermal_") => new PdfOptions
                {
                    Width = thermal.Contains("58") ? "58mm" : "80mm",
                    PrintBackground = true,
                    PreferCSSPageSize = true, // Ajusta al contenido si hay @page en CSS
                    MarginOptions = new MarginOptions
                    {
                        Top = "2mm",
                        Bottom = "2mm",
                        Left = "2mm",
                        Right = "2mm"
                    }
                },
                _ => new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true
                }
            };
        }
    }
}