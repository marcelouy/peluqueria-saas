// SETTINGSCONTROLLER.CS - CONTROLLER CORREGIDO CON MÉTODOS ASYNC EXACTOS
// Este archivo debe estar en: src\PeluqueriaSaaS.Web\Controllers\SettingsController.cs

using Microsoft.AspNetCore.Mvc;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Web.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ISettingsRepository _settingsRepository;

        public SettingsController(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        // GET: Settings
        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine("Settings Index called");
                
                // ✅ Usar GetOrCreateDefaultSettingsAsync del interface real
                var settings = await _settingsRepository.GetOrCreateDefaultSettingsAsync();
                
                Console.WriteLine($"Settings loaded: {settings?.NombrePeluqueria ?? "null"}");
                
                return View(settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Settings Index: {ex.Message}");
                TempData["Error"] = "Error al cargar la configuración";
                
                // Return default settings for error case
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
                
                // ✅ Usar GetOrCreateDefaultSettingsAsync
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
                
                // ✅ Usar UpdateSettingsAsync del interface real
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
                
                // ✅ Usar GetOrCreateDefaultSettingsAsync
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
                
                // ✅ Usar GetOrCreateDefaultSettingsAsync
                var settings = await _settingsRepository.GetOrCreateDefaultSettingsAsync();
                
                settings.ResumenServicioHabilitado = enabled;
                settings.FechaActualizacion = DateTime.Now;
                
                // ✅ Usar UpdateSettingsAsync
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
                
                // ✅ Usar GetSettingsAsync para obtener existente
                var settings = await _settingsRepository.GetSettingsAsync();
                
                if (settings == null)
                {
                    // Si no existe, crear uno nuevo con defaults
                    settings = CreateDefaultSettings();
                    settings = await _settingsRepository.CreateSettingsAsync(settings);
                }
                else
                {
                    // Reset existing settings to defaults
                    ResetToDefaults(settings);
                    settings.FechaActualizacion = DateTime.Now;
                    // ✅ Usar UpdateSettingsAsync
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
    }
}