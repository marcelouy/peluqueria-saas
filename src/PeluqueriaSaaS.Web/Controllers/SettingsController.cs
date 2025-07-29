// ACTUALIZAR COMPLETO: src/PeluqueriaSaaS.Web/Controllers/SettingsController.cs
// AGREGAR DEPENDENCIES + MÉTODO GENERARRESUMEN

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Data;

namespace PeluqueriaSaaS.Web.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly PeluqueriaDbContext _context;

        // ✅ CONSTRUCTOR ACTUALIZADO: Agregar PeluqueriaDbContext dependency
        public SettingsController(ISettingsRepository settingsRepository, PeluqueriaDbContext context)
        {
            _settingsRepository = settingsRepository;
            _context = context;
        }

        // GET: Settings
        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine("Settings Index called");

                var settings = await _settingsRepository.GetOrCreateDefaultSettingsAsync();

                Console.WriteLine($"Settings loaded: {settings?.NombrePeluqueria ?? "null"}");

                return View(settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Settings Index: {ex.Message}");
                TempData["Error"] = "Error al cargar la configuración";

                var defaultSettings = CreateDefaultSettings();
                return View(defaultSettings);
            }
        }

        // GET: Settings/Edit/5
        public async Task<IActionResult> Edit(int id = 1)
        {
            try
            {
                Console.WriteLine($"Settings Edit called with ID: {id}");

                var settings = await _settingsRepository.GetOrCreateDefaultSettingsAsync();

                Console.WriteLine($"Settings for edit loaded: {settings?.NombrePeluqueria ?? "null"}");

                return View(settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Settings Edit: {ex.Message}");
                TempData["Error"] = "Error al cargar la configuración para edición";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Settings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Settings settings)
        {
            try
            {
                Console.WriteLine($"Settings Edit POST called for: {settings?.NombrePeluqueria ?? "null"}");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState is invalid");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine($"Validation error: {error.ErrorMessage}");
                    }
                    return View(settings);
                }

                settings.FechaActualizacion = DateTime.Now;

                var result = await _settingsRepository.UpdateSettingsAsync(settings);

                if (result != null)
                {
                    TempData["Success"] = "Configuración guardada correctamente";
                    Console.WriteLine("Settings updated successfully");
                }
                else
                {
                    TempData["Error"] = "Error al guardar la configuración";
                    Console.WriteLine("Settings update failed");
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
                TempData["Error"] = "Error al guardar la configuración";
                return View(settings);
            }
        }

        // GET: Settings/PreviewResumen
        public async Task<IActionResult> PreviewResumen()
        {
            try
            {
                Console.WriteLine("Settings PreviewResumen called");

                var settings = await _settingsRepository.GetOrCreateDefaultSettingsAsync();

                return View(settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PreviewResumen: {ex.Message}");
                TempData["Error"] = "Error al cargar la vista previa";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Settings/Toggle
        [HttpPost]
        public async Task<IActionResult> Toggle(bool enabled)
        {
            try
            {
                Console.WriteLine($"Settings Toggle called: {enabled}");

                var settings = await _settingsRepository.GetOrCreateDefaultSettingsAsync();

                settings.ResumenServicioHabilitado = enabled;
                settings.FechaActualizacion = DateTime.Now;

                await _settingsRepository.UpdateSettingsAsync(settings);

                TempData["Success"] = $"Resumen de servicio {(enabled ? "habilitado" : "deshabilitado")} correctamente";

                return Json(new { success = true, enabled = enabled });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling resumen: {ex.Message}");
                return Json(new { success = false, message = "Error al cambiar configuración" });
            }
        }

        // POST: Settings/Reset
        [HttpPost]
        public async Task<IActionResult> Reset()
        {
            try
            {
                Console.WriteLine("Settings Reset called");

                var settings = await _settingsRepository.GetSettingsAsync();

                if (settings == null)
                {
                    settings = CreateDefaultSettings();
                    settings = await _settingsRepository.CreateSettingsAsync(settings);
                }
                else
                {
                    ResetToDefaults(settings);
                    settings.FechaActualizacion = DateTime.Now;
                    await _settingsRepository.UpdateSettingsAsync(settings);
                }

                TempData["Success"] = "Configuración restaurada a valores por defecto";

                return Json(new { success = true, message = "Configuración restaurada correctamente" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting settings: {ex.Message}");
                return Json(new { success = false, message = "Error al restaurar configuración" });
            }
        }

        // ✅ NUEVO MÉTODO CRÍTICO: GENERAR RESUMEN
        [HttpPost]
        public async Task<IActionResult> GenerarResumen(int ventaId)
        {
            try
            {
                Console.WriteLine($"🎯 Generando resumen para VentaId: {ventaId}");

                // ✅ QUERY CON NULL SAFETY EXPLÍCITO - USANDO ARCHIVOS REALES
                var venta = await _context.Ventas
                    .Where(v => v.VentaId == ventaId && v.TenantId == "default")
                    .Select(v => new
                    {
                        v.VentaId,
                        v.FechaVenta,
                        v.NumeroVenta,
                        v.Total,
                        v.SubTotal,
                        v.Descuento,
                        v.EstadoVenta,
                        // ✅ NULL SAFETY: Manejar Observaciones NULL (ahora nullable en Entity)
                        Observaciones = v.Observaciones ?? "Sin observaciones",
                        ClienteNombre = v.Cliente.Nombre ?? "Cliente no especificado",
                        ClienteTelefono = v.Cliente.Telefono ?? "",
                        EmpleadoNombre = v.Empleado.Nombre ?? "Empleado no especificado"
                    })
                    .FirstOrDefaultAsync();

                if (venta == null)
                {
                    Console.WriteLine($"❌ Venta {ventaId} no encontrada");
                    return Json(new { success = false, message = "Venta no encontrada" });
                }

                Console.WriteLine($"✅ Venta encontrada: {venta.NumeroVenta}");

                // ✅ OBTENER DETALLES VENTA
                var detalles = await _context.VentaDetalles
                    .Where(vd => vd.VentaId == ventaId)
                    .Select(vd => new
                    {
                        ServicioNombre = vd.Servicio.Nombre ?? "Servicio no especificado",
                        vd.PrecioUnitario,
                        vd.Cantidad,
                        Subtotal = vd.PrecioUnitario * vd.Cantidad
                    })
                    .ToListAsync();

                Console.WriteLine($"✅ Detalles encontrados: {detalles.Count}");

                // ✅ OBTENER SETTINGS
                var settings = await _settingsRepository.GetOrCreateDefaultSettingsAsync();

                var resumenData = new
                {
                    // Datos Settings
                    NombrePeluqueria = settings?.NombrePeluqueria ?? "Mi Peluquería",
                    DireccionPeluqueria = settings?.DireccionPeluqueria ?? "",
                    TelefonoPeluqueria = settings?.TelefonoPeluqueria ?? "",
                    ColorPrimario = settings?.ColorPrimario ?? "#007bff",
                    ColorSecundario = settings?.ColorSecundario ?? "#6c757d",
                    SimboloMoneda = settings?.SimboloMoneda ?? "$U",

                    // Datos Venta con NULL SAFETY
                    VentaId = venta.VentaId,
                    NumeroVenta = venta.NumeroVenta ?? $"V-{venta.VentaId:D3}",
                    FechaVenta = venta.FechaVenta.ToString("dd/MM/yyyy HH:mm"),
                    ClienteNombre = venta.ClienteNombre,
                    ClienteTelefono = venta.ClienteTelefono,
                    EmpleadoNombre = venta.EmpleadoNombre,
                    Observaciones = venta.Observaciones,
                    SubTotal = venta.SubTotal,
                    Descuento = venta.Descuento,
                    Total = venta.Total,
                    EstadoVenta = venta.EstadoVenta ?? "Completada",

                    // Servicios
                    Servicios = detalles
                };

                Console.WriteLine($"✅ Resumen generado exitosamente para venta {venta.NumeroVenta}");

                return Json(new { success = true, data = resumenData });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generando resumen: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // HELPER METHODS
        private Settings CreateDefaultSettings()
        {
            return new Settings
            {
                NombrePeluqueria = "Mi Peluquería",
                DireccionPeluqueria = "Avenida Principal 123, Montevideo",
                TelefonoPeluqueria = "099 123 456",
                EmailPeluqueria = "info@mipeluqueria.com.uy",
                ResumenServicioHabilitado = false,
                ResumenEncabezado = "Gracias por elegir nuestros servicios",
                ResumenPiePagina = "¡Esperamos verte pronto!",
                MostrarLogoEnResumen = false,
                RutaLogo = "/images/logo.png",
                MostrarDatosCliente = true,
                MostrarEmpleadoServicio = true,
                MostrarFechaHora = true,
                MostrarDetalleServicios = true,
                MostrarTotalServicio = true,
                ColorPrimario = "#007bff",
                ColorSecundario = "#6c757d",
                TamanoFuente = "12px",
                SimboloMoneda = "$U",
                FormatoMoneda = "N2",
                NotificacionesEmail = true,
                BackupAutomatico = true,
                DiasRetencionVentas = 365,
                FechaCreacion = DateTime.Now,
                Activo = true,
                CodigoPeluqueria = "MAIN",
                UsarTemplateCustom = false,
                TemplateCustomHTML = string.Empty
            };
        }

        private void ResetToDefaults(Settings settings)
        {
            settings.NombrePeluqueria = "Mi Peluquería";
            settings.DireccionPeluqueria = "Avenida Principal 123, Montevideo";
            settings.TelefonoPeluqueria = "099 123 456";
            settings.EmailPeluqueria = "info@mipeluqueria.com.uy";
            settings.ResumenServicioHabilitado = false;
            settings.ResumenEncabezado = "Gracias por elegir nuestros servicios";
            settings.ResumenPiePagina = "¡Esperamos verte pronto!";
            settings.MostrarLogoEnResumen = false;
            settings.RutaLogo = "/images/logo.png";
            settings.MostrarDatosCliente = true;
            settings.MostrarEmpleadoServicio = true;
            settings.MostrarFechaHora = true;
            settings.MostrarDetalleServicios = true;
            settings.MostrarTotalServicio = true;
            settings.ColorPrimario = "#007bff";
            settings.ColorSecundario = "#6c757d";
            settings.TamanoFuente = "12px";
            settings.SimboloMoneda = "$U";
            settings.FormatoMoneda = "N2";
            settings.NotificacionesEmail = true;
            settings.BackupAutomatico = true;
            settings.DiasRetencionVentas = 365;
            settings.UsarTemplateCustom = false;
            settings.TemplateCustomHTML = string.Empty;
        }
        
        // AGREGAR AL FINAL DE SettingsController.cs
        // ANTES DEL ÚLTIMO } (antes de cerrar la clase)

        // ABRIR: src/PeluqueriaSaaS.Web/Controllers/SettingsController.cs
        // AGREGAR ESTE MÉTODO AL FINAL (antes del último })

        [HttpGet]
        public async Task<IActionResult> ExportarPDF(int ventaId)
        {
            try
            {
                Console.WriteLine($"🎯 Exportando PDF para VentaId: {ventaId}");

                var venta = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Empleado)
                    .FirstOrDefaultAsync(v => v.VentaId == ventaId);

                if (venta == null)
                {
                    return NotFound("Venta no encontrada");
                }

                var detalles = await _context.VentaDetalles
                    .Include(vd => vd.Servicio)
                    .Where(vd => vd.VentaId == ventaId)
                    .ToListAsync();

                var settings = await _settingsRepository.GetOrCreateDefaultSettingsAsync();

                // Crear contenido simple del PDF
                var content = $@"
                RESUMEN DE SERVICIO - {settings?.NombrePeluqueria ?? "Mi Peluquería"}
                =====================================================

                Venta #: {venta.VentaId}
                Fecha: {venta.FechaVenta:dd/MM/yyyy HH:mm}
                Cliente: {venta.Cliente?.Nombre ?? "N/A"}
                Empleado: {venta.Empleado?.Nombre ?? "N/A"}

                SERVICIOS:
                ";

                        foreach (var detalle in detalles)
                        {
                            content += $"- {detalle.Servicio?.Nombre ?? detalle.NombreServicio}: $U {detalle.PrecioUnitario:N2}\n";
                        }

                        content += $@"

                TOTAL: $U {venta.Total:N2}

                Comprobante interno - Sin valor fiscal
                Generado: {DateTime.Now:dd/MM/yyyy HH:mm}
                ";

                var bytes = System.Text.Encoding.UTF8.GetBytes(content);
                var fileName = $"resumen_venta_{ventaId}.txt";

                Console.WriteLine($"✅ PDF generado como texto: {fileName}");
                
                return File(bytes, "text/plain", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // ✅ HELPER METHOD - GENERAR HTML PARA PDF
        private string GenerarHTMLResumen(dynamic venta, dynamic detalles, Settings settings)
        {
            var html = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <title>Resumen de Servicio - Venta #{venta.VentaId}</title>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 20px; color: #333; }}
                .header {{ text-align: center; border-bottom: 2px solid {settings?.ColorPrimario ?? "#007bff"}; padding-bottom: 10px; margin-bottom: 20px; }}
                .company-name {{ font-size: 24px; color: {settings?.ColorPrimario ?? "#007bff"}; font-weight: bold; }}
                .company-info {{ margin: 10px 0; font-size: 14px; }}
                .section {{ margin: 15px 0; }}
                .section-title {{ font-size: 16px; color: {settings?.ColorPrimario ?? "#007bff"}; font-weight: bold; margin-bottom: 8px; }}
                .info-row {{ margin: 5px 0; }}
                .services-table {{ width: 100%; border-collapse: collapse; margin: 10px 0; }}
                .services-table th, .services-table td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                .services-table th {{ background-color: {settings?.ColorPrimario ?? "#007bff"}; color: white; }}
                .total-row {{ font-weight: bold; font-size: 18px; color: {settings?.ColorPrimario ?? "#007bff"}; text-align: right; margin: 15px 0; }}
                .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; border-top: 1px solid #ddd; padding-top: 10px; }}
            </style>
        </head>
        <body>
            <div class='header'>
                <div class='company-name'>{settings?.NombrePeluqueria ?? "Mi Peluquería"}</div>
                <div class='company-info'>{settings?.DireccionPeluqueria ?? ""}</div>
                <div class='company-info'>Tel: {settings?.TelefonoPeluqueria ?? ""} | Email: {settings?.EmailPeluqueria ?? ""}</div>
            </div>

            <div class='section'>
                <div class='section-title'>RESUMEN DE SERVICIO</div>
                <div class='info-row'><strong>Venta #:</strong> {venta.VentaId}</div>
                <div class='info-row'><strong>Número:</strong> {venta.NumeroVenta}</div>
                <div class='info-row'><strong>Fecha y Hora:</strong> {venta.FechaVenta:dd/MM/yyyy HH:mm}</div>
                <div class='info-row'><strong>Cliente:</strong> {venta.ClienteNombre}</div>
                <div class='info-row'><strong>Atendido por:</strong> {venta.EmpleadoNombre}</div>
            </div>

            <div class='section'>
                <div class='section-title'>Servicios Realizados:</div>
                <table class='services-table'>
                    <thead>
                        <tr>
                            <th>Servicio</th>
                            <th>Precio</th>
                            <th>Cantidad</th>
                            <th>Subtotal</th>
                        </tr>
                    </thead>
                    <tbody>";

            foreach (var detalle in detalles)
            {
                html += $@"
                        <tr>
                            <td>{detalle.ServicioNombre}</td>
                            <td>{settings?.SimboloMoneda ?? "$U"} {detalle.PrecioUnitario:N2}</td>
                            <td>{detalle.Cantidad}</td>
                            <td>{settings?.SimboloMoneda ?? "$U"} {detalle.Subtotal:N2}</td>
                        </tr>";
            }

            html += $@"
                    </tbody>
                </table>
            </div>

            <div class='total-row'>
                TOTAL: {settings?.SimboloMoneda ?? "$U"} {venta.Total:N2}
            </div>

            <div class='footer'>
                <p>¡Esperamos verte pronto!</p>
                <p><em>Comprobante interno - Sin valor fiscal</em></p>
                <p>Generado el {DateTime.Now:dd/MM/yyyy HH:mm}</p>
            </div>
        </body>
        </html>";

            return html;
        }

        // ✅ HELPER METHOD - GENERAR PDF FROM HTML (SIMPLE VERSION)
        private byte[] GenerarPDFFromHTML(string htmlContent)
        {
            // ✅ VERSIÓN SIMPLE SIN EXTERNAL LIBRARIES
            // Para production, usar IronPDF, wkhtmltopdf, o PuppeteerSharp
            
            // Por ahora, generar texto simple que simule PDF para testing
            var textContent = $@"
        RESUMEN DE SERVICIO - PDF EXPORT
        ================================

        {htmlContent.Replace("<", "").Replace(">", "").Replace("&nbsp;", " ")}

        Este es un PDF de prueba generado desde el sistema.
        Para implementación completa, agregar library PDF generation.
        ";
            
            return System.Text.Encoding.UTF8.GetBytes(textContent);
        }
    }
}