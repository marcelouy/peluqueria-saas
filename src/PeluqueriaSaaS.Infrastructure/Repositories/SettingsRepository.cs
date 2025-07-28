// src/PeluqueriaSaaS.Infrastructure/Repositories/SettingsRepository.cs
using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Data;

namespace PeluqueriaSaaS.Infrastructure.Repositories
{
    /// <summary>
    /// ✅ SETTINGS REPOSITORY IMPLEMENTATION - Infrastructure layer
    /// Propósito: Settings data access implementation
    /// Pattern: Consistente con EmpleadoRepository, ServicioRepository del proyecto
    /// DbContext: PeluqueriaDbContext (el correcto del proyecto)
    /// </summary>
    public class SettingsRepository : ISettingsRepository
    {
        private readonly PeluqueriaDbContext _context;

        public SettingsRepository(PeluqueriaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// ✅ Obtener settings principal (primer registro activo)
        /// </summary>
        public async Task<Settings?> GetSettingsAsync()
        {
            try
            {
                var settings = await _context.Settings
                    .AsNoTracking()
                    .Where(s => s.Activo)
                    .OrderBy(s => s.Id)
                    .FirstOrDefaultAsync();

                return settings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error GetSettingsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// ✅ Obtener settings por código peluquería (multi-tenant futuro)
        /// </summary>
        public async Task<Settings?> GetSettingsByCodigoAsync(string codigo)
        {
            try
            {
                if (string.IsNullOrEmpty(codigo))
                    return null;

                var settings = await _context.Settings
                    .AsNoTracking()
                    .Where(s => s.CodigoPeluqueria == codigo && s.Activo)
                    .FirstOrDefaultAsync();

                return settings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error GetSettingsByCodigoAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// ✅ Crear nuevos settings
        /// </summary>
        public async Task<Settings> CreateSettingsAsync(Settings settings)
        {
            try
            {
                if (settings == null)
                    throw new ArgumentNullException(nameof(settings));

                settings.FechaCreacion = DateTime.Now;
                settings.Activo = true;

                _context.Settings.Add(settings);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Settings creado: ID {settings.Id}, Código: {settings.CodigoPeluqueria}");
                return settings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error CreateSettingsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// ✅ Actualizar settings existente
        /// </summary>
        public async Task<Settings> UpdateSettingsAsync(Settings settings)
        {
            try
            {
                if (settings == null)
                    throw new ArgumentNullException(nameof(settings));

                var existingSettings = await _context.Settings
                    .Where(s => s.Id == settings.Id)
                    .FirstOrDefaultAsync();

                if (existingSettings == null)
                    throw new InvalidOperationException($"Settings con ID {settings.Id} no encontrado");

                // Actualizar propiedades
                existingSettings.NombrePeluqueria = settings.NombrePeluqueria;
                existingSettings.DireccionPeluqueria = settings.DireccionPeluqueria;
                existingSettings.TelefonoPeluqueria = settings.TelefonoPeluqueria;
                existingSettings.EmailPeluqueria = settings.EmailPeluqueria;
                
                // Configuración resumen
                existingSettings.ResumenServicioHabilitado = settings.ResumenServicioHabilitado;
                existingSettings.ResumenEncabezado = settings.ResumenEncabezado;
                existingSettings.ResumenPiePagina = settings.ResumenPiePagina;
                existingSettings.MostrarLogoEnResumen = settings.MostrarLogoEnResumen;
                existingSettings.RutaLogo = settings.RutaLogo;
                
                // Configuración formato
                existingSettings.MostrarDatosCliente = settings.MostrarDatosCliente;
                existingSettings.MostrarEmpleadoServicio = settings.MostrarEmpleadoServicio;
                existingSettings.MostrarFechaHora = settings.MostrarFechaHora;
                existingSettings.MostrarDetalleServicios = settings.MostrarDetalleServicios;
                existingSettings.MostrarTotalServicio = settings.MostrarTotalServicio;
                
                // Personalización
                existingSettings.ColorPrimario = settings.ColorPrimario;
                existingSettings.ColorSecundario = settings.ColorSecundario;
                existingSettings.TamanoFuente = settings.TamanoFuente;
                existingSettings.SimboloMoneda = settings.SimboloMoneda;
                existingSettings.FormatoMoneda = settings.FormatoMoneda;
                
                // Sistema
                existingSettings.NotificacionesEmail = settings.NotificacionesEmail;
                existingSettings.BackupAutomatico = settings.BackupAutomatico;
                existingSettings.DiasRetencionVentas = settings.DiasRetencionVentas;
                
                // Template custom
                existingSettings.TemplateCustomHTML = settings.TemplateCustomHTML;
                existingSettings.UsarTemplateCustom = settings.UsarTemplateCustom;
                
                existingSettings.ActualizarFecha();

                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Settings actualizado: ID {existingSettings.Id}");
                return existingSettings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error UpdateSettingsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// ✅ Eliminar settings (soft delete)
        /// </summary>
        public async Task<bool> DeleteSettingsAsync(int id)
        {
            try
            {
                var settings = await _context.Settings
                    .Where(s => s.Id == id)
                    .FirstOrDefaultAsync();

                if (settings == null)
                    return false;

                settings.Activo = false;
                settings.ActualizarFecha();

                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Settings eliminado (soft): ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error DeleteSettingsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// ✅ Verificar si existen settings
        /// </summary>
        public async Task<bool> ExistenSettingsAsync()
        {
            try
            {
                var count = await _context.Settings
                    .AsNoTracking()
                    .Where(s => s.Activo)
                    .CountAsync();

                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error ExistenSettingsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// ✅ Obtener o crear settings default
        /// Útil para inicialización automática
        /// </summary>
        public async Task<Settings> GetOrCreateDefaultSettingsAsync()
        {
            try
            {
                // Intentar obtener settings existente
                var settings = await GetSettingsAsync();
                
                if (settings != null)
                {
                    Console.WriteLine($"✅ Settings existente encontrado: ID {settings.Id}");
                    return settings;
                }

                // Crear settings default si no existe
                Console.WriteLine("🔄 No existen settings, creando default...");
                
                var defaultSettings = new Settings
                {
                    NombrePeluqueria = "Mi Peluquería",
                    DireccionPeluqueria = "Avenida Principal 123, Montevideo",
                    TelefonoPeluqueria = "099 123 456",
                    EmailPeluqueria = "info@mipeluqueria.com.uy",
                    ResumenServicioHabilitado = false,
                    ResumenEncabezado = "COMPROBANTE INTERNO - SIN VALOR FISCAL",
                    ResumenPiePagina = "Gracias por su visita. Este comprobante es solo para control interno.",
                    MostrarLogoEnResumen = false,
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
                    CodigoPeluqueria = "MAIN",
                    UsarTemplateCustom = false
                };

                var createdSettings = await CreateSettingsAsync(defaultSettings);
                Console.WriteLine($"✅ Settings default creado: ID {createdSettings.Id}");
                
                return createdSettings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error GetOrCreateDefaultSettingsAsync: {ex.Message}");
                throw;
            }
        }
    }
}