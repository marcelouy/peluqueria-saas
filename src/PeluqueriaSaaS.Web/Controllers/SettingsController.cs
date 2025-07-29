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

                // 🔧 FIX VALIDATION: Set default si field NULL
                if (string.IsNullOrEmpty(settings.TemplateCustomHTML))
                        settings.TemplateCustomHTML = string.Empty;

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


        [HttpGet]
        public async Task<IActionResult> GenerarHTMLProfessional(int ventaId)
        {
            try
            {
                Console.WriteLine($"🎯 Generando HTML Professional para VentaId: {ventaId}");

                // Obtener datos venta - SIN NumeroDocumento
                var venta = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Empleado)
                    .Where(v => v.VentaId == ventaId)
                    .Select(v => new
                    {
                        v.VentaId,
                        v.FechaVenta,
                        v.NumeroVenta,
                        v.Total,
                        v.SubTotal,
                        v.Descuento,
                        Observaciones = v.Observaciones ?? "",
                        ClienteNombre = v.Cliente.NombreCompleto ?? "Cliente no especificado",
                        ClienteTelefono = v.Cliente.Telefono ?? "",
                        ClienteEmail = v.Cliente.Email ?? "",
                        EmpleadoNombre = v.Empleado.Nombre ?? "Empleado no especificado"
                    })
                    .FirstOrDefaultAsync();

                if (venta == null)
                {
                    return NotFound("Venta no encontrada");
                }

                // Obtener detalles
                var detalles = await _context.VentaDetalles
                    .Include(vd => vd.Servicio)
                    .ThenInclude(s => s.TipoServicio)
                    .Where(vd => vd.VentaId == ventaId)
                    .Select(vd => new
                    {
                        ServicioNombre = vd.Servicio.Nombre ?? "Servicio",
                        ServicioCategoria = vd.Servicio.TipoServicio.Nombre ?? "General",
                        ServicioDuracion = vd.Servicio.DuracionMinutos + " min",
                        PrecioUnitario = vd.PrecioUnitario,
                        Cantidad = vd.Cantidad,
                        EmpleadoServicio = vd.EmpleadoServicio != null ? vd.EmpleadoServicio.Nombre : venta.EmpleadoNombre
                    })
                    .ToListAsync();

                // Obtener settings
                var settings = await _settingsRepository.GetOrCreateDefaultSettingsAsync();

                // Template HTML Professional
                var htmlTemplate = GetHTMLTemplateProfessional();

                // Reemplazar variables del template - SIN ClienteDocumento
                var html = htmlTemplate
                    // Company Info
                    .Replace("{{COMPANY_NAME}}", settings?.NombrePeluqueria ?? "Mi Peluquería")
                    .Replace("{{COMPANY_TAGLINE}}", settings?.ResumenEncabezado ?? "Servicios de belleza profesional")
                    .Replace("{{COMPANY_ADDRESS}}", settings?.DireccionPeluqueria ?? "")
                    .Replace("{{COMPANY_PHONE}}", settings?.TelefonoPeluqueria ?? "")
                    .Replace("{{COMPANY_EMAIL}}", settings?.EmailPeluqueria ?? "")
                    .Replace("{{LOGO}}", "💇‍♀️")

                    // Venta Info
                    .Replace("{{VENTA_ID}}", venta.VentaId.ToString())
                    .Replace("{{FECHA_VENTA}}", venta.FechaVenta.ToString("dd/MM/yyyy"))
                    .Replace("{{HORA_VENTA}}", venta.FechaVenta.ToString("HH:mm"))
                    .Replace("{{EMPLEADO_NOMBRE}}", venta.EmpleadoNombre)

                    // Cliente Info - SIN DOCUMENTO
                    .Replace("{{CLIENTE_NOMBRE}}", venta.ClienteNombre)
                    .Replace("{{CLIENTE_TELEFONO}}", venta.ClienteTelefono)
                    .Replace("{{CLIENTE_EMAIL}}", venta.ClienteEmail)

                    // Totales
                    .Replace("{{SUBTOTAL}}", venta.SubTotal.ToString("N2"))
                    .Replace("{{DESCUENTO}}", venta.Descuento.ToString("N2"))
                    .Replace("{{TOTAL}}", venta.Total.ToString("N2"));

                // Generar filas servicios
                var serviciosHtml = "";
                foreach (var detalle in detalles)
                {
                    serviciosHtml += $@"
                        <tr>
                            <td>
                                <div class=""service-name"">{detalle.ServicioNombre}</div>
                                <div class=""service-category"">{detalle.ServicioCategoria}</div>
                            </td>
                            <td>
                                <div class=""employee-name"">{detalle.EmpleadoServicio}</div>
                            </td>
                            <td>{detalle.ServicioDuracion}</td>
                            <td class=""price-cell"">${detalle.PrecioUnitario:N2}</td>
                        </tr>";
                }

                // Reemplazar tabla servicios
                html = html.Replace("{{SERVICIOS_TBODY}}", serviciosHtml);

                // Manejar observaciones
                if (!string.IsNullOrEmpty(venta.Observaciones))
                {
                    html = html.Replace("{{OBSERVACIONES}}", venta.Observaciones)
                            .Replace("style=\"display: none;\"", "");
                }

                // Aplicar colores custom CSS variables
                html = html.Replace(":root {", $@":root {{
                    --primary-color: {settings?.ColorPrimario ?? "#3b82f6"};
                    --secondary-color: {settings?.ColorSecundario ?? "#1e40af"};");

                Console.WriteLine($"✅ HTML Professional generado para venta {venta.VentaId}");

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        private string GetHTMLTemplateProfessional()
        {
            return @"<!DOCTYPE html>
        <html lang=""es"">
        <head>
            <meta charset=""UTF-8"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
            <title>Resumen de Servicio</title>
            <link rel=""stylesheet"" href=""/css/resumen-professional.css"">
            <style>
                :root {
                    --primary-color: #3b82f6;
                    --secondary-color: #1e40af;
                }
            </style>
        </head>
        <body>
            <div class=""resumen-container"">
                <header class=""header"">
                    <div class=""header-content"">
                        <div class=""company-info"">
                            <h1>{{COMPANY_NAME}}</h1>
                            <p class=""company-tagline"">{{COMPANY_TAGLINE}}</p>
                        </div>
                        <div class=""logo-container"">
                            <div class=""logo-placeholder"">{{LOGO}}</div>
                        </div>
                    </div>
                </header>

                <section class=""document-info"">
                    <h2>Resumen de Servicio</h2>
                    <div class=""info-grid"">
                        <div class=""info-item"">
                            <span class=""info-label"">Venta #</span>
                            <span class=""info-value"">{{VENTA_ID}}</span>
                        </div>
                        <div class=""info-item"">
                            <span class=""info-label"">Fecha</span>
                            <span class=""info-value"">{{FECHA_VENTA}}</span>
                        </div>
                        <div class=""info-item"">
                            <span class=""info-label"">Hora</span>
                            <span class=""info-value"">{{HORA_VENTA}}</span>
                        </div>
                        <div class=""info-item"">
                            <span class=""info-label"">Atendido por</span>
                            <span class=""info-value"">{{EMPLEADO_NOMBRE}}</span>
                        </div>
                    </div>
                </section>

                <main class=""main-content"">
                    <section class=""client-section"">
                        <h3 class=""section-title"">
                            <span class=""section-icon"">👤</span>
                            Información del Cliente
                        </h3>
                        <div class=""client-card"">
                            <div class=""info-item"">
                                <span class=""info-label"">Nombre</span>
                                <span class=""info-value"">{{CLIENTE_NOMBRE}}</span>
                            </div>
                            <div class=""info-item"">
                                <span class=""info-label"">Teléfono</span>
                                <span class=""info-value"">{{CLIENTE_TELEFONO}}</span>
                            </div>
                            <div class=""info-item"">
                                <span class=""info-label"">Email</span>
                                <span class=""info-value"">{{CLIENTE_EMAIL}}</span>
                            </div>
                        </div>
                    </section>

                    <section class=""services-section"">
                        <h3 class=""section-title"">
                            <span class=""section-icon"">✂️</span>
                            Servicios Realizados
                        </h3>
                        <table class=""services-table"">
                            <thead>
                                <tr>
                                    <th>Servicio</th>
                                    <th>Empleado</th>
                                    <th>Duración</th>
                                    <th>Precio</th>
                                </tr>
                            </thead>
                            <tbody>
                                {{SERVICIOS_TBODY}}
                            </tbody>
                        </table>
                    </section>

                    <section class=""totals-section"">
                        <div class=""totals-grid"">
                            <div class=""total-row"">
                                <span class=""total-label"">Subtotal</span>
                                <span class=""total-value"">${{SUBTOTAL}}</span>
                            </div>
                            <div class=""total-row"">
                                <span class=""total-label"">Descuento</span>
                                <span class=""total-value"">-${{DESCUENTO}}</span>
                            </div>
                            <div class=""total-row final"">
                                <span class=""total-label"">Total</span>
                                <span class=""total-value"">${{TOTAL}}</span>
                            </div>
                        </div>
                    </section>

                    <section class=""observations-section"" style=""display: none;"">
                        <h3 class=""section-title"">
                            <span class=""section-icon"">📝</span>
                            Observaciones
                        </h3>
                        <div class=""client-card"">
                            <p>{{OBSERVACIONES}}</p>
                        </div>
                    </section>
                </main>

                <footer class=""footer"">
                    <div class=""legal-notice"">
                        <strong>Comprobante interno - Sin valor fiscal</strong><br>
                        Este documento es únicamente para control interno y no constituye factura legal.
                    </div>
                    <div class=""contact-info"">
                        <div class=""contact-item"">{{COMPANY_ADDRESS}}</div>
                        <div class=""contact-item"">Tel: {{COMPANY_PHONE}}</div>
                        <div class=""contact-item"">{{COMPANY_EMAIL}}</div>
                    </div>
                </footer>
            </div>
        </body>
        </html>";
        }



    }
}