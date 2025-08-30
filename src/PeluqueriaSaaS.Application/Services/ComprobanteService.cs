using Microsoft.Extensions.Logging;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeluqueriaSaaS.Application.Services
{
    public class ComprobanteService : IComprobanteService
    {
        private readonly IComprobanteRepository _comprobanteRepository;
        private readonly IVentaRepository _ventaRepository;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly ILogger<ComprobanteService> _logger;
        private const string DEFAULT_TENANT_ID = "default";
        private const string DEFAULT_SERIE = "A001";
        private const string DEFAULT_USUARIO = "María González";
        private const string EMPLEADO_SISTEMA_EMAIL = "sistema@peluqueria.com";
        
        private int? _empleadoSistemaId = null; // Cache del ID

        public ComprobanteService(
            IComprobanteRepository comprobanteRepository,
            IVentaRepository ventaRepository,
            IEmpleadoRepository empleadoRepository,
            ILogger<ComprobanteService> logger)
        {
            _comprobanteRepository = comprobanteRepository;
            _ventaRepository = ventaRepository;
            _empleadoRepository = empleadoRepository;
            _logger = logger;
        }
        
        private async Task<int> GetEmpleadoSistemaIdAsync()
        {
            if (_empleadoSistemaId == null)
            {
                var empleadoSistema = await _empleadoRepository.GetByEmailAsync(EMPLEADO_SISTEMA_EMAIL);
                
                if (empleadoSistema == null)
                {
                    _logger.LogWarning("Empleado Sistema no encontrado, creando uno nuevo...");
                    empleadoSistema = await _empleadoRepository.CreateSistemaAsync();
                }
                
                _empleadoSistemaId = empleadoSistema.Id;
                _logger.LogInformation($"Empleado Sistema ID: {_empleadoSistemaId}");
            }
            
            return _empleadoSistemaId.Value;
        }

        public async Task<Comprobante> GenerarComprobanteAsync(int ventaId)
        {
            try
            {
                _logger.LogInformation($"🧾 Iniciando generación de comprobante para venta {ventaId}");

                // Verificar si ya existe un comprobante para esta venta
                if (await _comprobanteRepository.ExistsAsync(ventaId))
                {
                    _logger.LogWarning($"Ya existe un comprobante para la venta {ventaId}");
                    throw new InvalidOperationException("Ya existe un comprobante para esta venta");
                }

                // Obtener la venta con sus detalles usando el repository
                var venta = await _ventaRepository.GetByIdAsync(ventaId, DEFAULT_TENANT_ID);

                if (venta == null)
                {
                    _logger.LogError($"No se encontró la venta con ID {ventaId}");
                    throw new ArgumentException($"No se encontró la venta con ID {ventaId}");
                }

                // Determinar el nombre del cliente
                string clienteNombre = "CLIENTE OCASIONAL";

                if (venta.Cliente != null)
                {
                    clienteNombre = $"{venta.Cliente.Nombre} {venta.Cliente.Apellido}".Trim();
                }

                // Calcular totales
                decimal subtotal = venta.SubTotal > 0 ? venta.SubTotal : venta.Total;
                decimal descuento = venta.Descuento;
                decimal total = venta.Total;

                // Obtener siguiente número
                var numero = await _comprobanteRepository.GetNextNumberAsync(DEFAULT_SERIE, DEFAULT_TENANT_ID);

                // ✅ USAR EL NUEVO CONSTRUCTOR SIMPLIFICADO (7 parámetros)
                var comprobante = new Comprobante(
                    ventaId: venta.VentaId,
                    clienteNombre: clienteNombre,
                    subtotal: subtotal,
                    total: total,
                    metodoPago: "EFECTIVO",
                    usuarioEmision: DEFAULT_USUARIO,
                    tenantId: DEFAULT_TENANT_ID
                );

                // Establecer el número correlativo usando el nuevo método
                comprobante.SetNumero(numero);

                // Si necesitamos una serie diferente, la establecemos
                comprobante.SetSerie(DEFAULT_SERIE);
                
                // ✅ NUEVO: Establecer los IDs de cliente y empleado
                // Usar reflexión para establecer las propiedades privadas
                var tipo = comprobante.GetType();
                
                // Establecer ClienteId
                var clienteIdProp = tipo.GetProperty("ClienteId");
                if (clienteIdProp != null)
                {
                    clienteIdProp.SetValue(comprobante, venta.ClienteId > 0 ? venta.ClienteId : (int?)null);
                }
                
                // Establecer EmpleadoId - usar el ID del empleado sistema si no hay empleado asignado
                var empleadoIdProp = tipo.GetProperty("EmpleadoId");
                if (empleadoIdProp != null)
                {
                    int empleadoId = venta.EmpleadoId > 0 ? venta.EmpleadoId : await GetEmpleadoSistemaIdAsync();
                    empleadoIdProp.SetValue(comprobante, empleadoId);
                }

                _logger.LogInformation($"✅ Comprobante preparado para guardar con número {DEFAULT_SERIE}-{numero:D8}");

                // Guardar el comprobante
                var comprobanteGuardado = await _comprobanteRepository.CreateAsync(comprobante);

                // Crear los detalles del comprobante si hay ventas detalles
                if (venta.VentaDetalles != null && venta.VentaDetalles.Any())
                {
                    foreach (var ventaDetalle in venta.VentaDetalles)
                    {
                        var detalle = new ComprobanteDetalle(
                            comprobanteId: comprobanteGuardado.Id,
                            tipoItem: "SERVICIO",
                            descripcion: ventaDetalle.NombreServicio ?? "Servicio",
                            cantidad: ventaDetalle.Cantidad,
                            precioUnitario: ventaDetalle.PrecioUnitario,
                            total: ventaDetalle.Subtotal
                        );

                        detalle.ItemId = ventaDetalle.ServicioId;
                        detalle.TenantId = DEFAULT_TENANT_ID;

                        // ✅ NUEVO: Establecer el EmpleadoId real en lugar de solo el texto
                        if (ventaDetalle.EmpleadoServicioId.HasValue && ventaDetalle.EmpleadoServicioId.Value > 0)
                        {
                            detalle.EmpleadoId = ventaDetalle.EmpleadoServicioId.Value;
                            
                            // Opcionalmente, obtener el nombre real del empleado
                            var empleado = await _empleadoRepository.GetByIdAsync(ventaDetalle.EmpleadoServicioId.Value);
                            if (empleado != null)
                            {
                                detalle.EmpleadoNombre = $"{empleado.Nombre} {empleado.Apellido}";
                            }
                            else
                            {
                                detalle.EmpleadoNombre = $"Empleado #{ventaDetalle.EmpleadoServicioId}";
                            }
                        }

                        // Guardar cada detalle individualmente
                        await _comprobanteRepository.GuardarDetalleAsync(detalle);
                    }

                    _logger.LogInformation($"✅ {venta.VentaDetalles.Count} detalles guardados para comprobante {comprobanteGuardado.Id}");
                }

                _logger.LogInformation($"✅ Comprobante guardado con ID: {comprobanteGuardado.Id} y número: {comprobanteGuardado.NumeroCompleto}");

                return comprobanteGuardado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al generar comprobante para venta {ventaId}");
                throw;
            }
        }

        public async Task<Comprobante?> ObtenerComprobanteAsync(int comprobanteId)
        {
            try
            {
                return await _comprobanteRepository.GetByIdAsync(comprobanteId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al obtener comprobante {comprobanteId}");
                throw;
            }
        }

        public async Task<Comprobante?> ObtenerComprobantePorVentaAsync(int ventaId)
        {
            try
            {
                return await _comprobanteRepository.GetByVentaIdAsync(ventaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al obtener comprobante para venta {ventaId}");
                throw;
            }
        }

        public async Task<IEnumerable<Comprobante>> ObtenerComprobantesAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                // Implementación pendiente en el repository
                // Por ahora retornamos lista vacía
                return await Task.FromResult(new List<Comprobante>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener comprobantes");
                throw;
            }
        }

        public async Task<IEnumerable<Comprobante>> ObtenerComprobantesRecientesAsync(int cantidad)
        {
            try
            {
                return await _comprobanteRepository.GetRecentAsync(cantidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al obtener {cantidad} comprobantes recientes");
                throw;
            }
        }

        public async Task<IEnumerable<Comprobante>> ObtenerComprobantesPorFechaAsync(DateTime fecha)
        {
            try
            {
                return await _comprobanteRepository.GetByFechaAsync(fecha);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al obtener comprobantes de fecha {fecha:yyyy-MM-dd}");
                throw;
            }
        }

        public async Task AnularComprobanteAsync(int comprobanteId, string motivo, string usuario)
        {
            try
            {
                var comprobante = await _comprobanteRepository.GetByIdAsync(comprobanteId);
                if (comprobante == null)
                {
                    _logger.LogWarning($"No se encontró el comprobante {comprobanteId}");
                    throw new ArgumentException($"No se encontró el comprobante con ID {comprobanteId}");
                }

                comprobante.Anular(motivo, usuario);
                await _comprobanteRepository.UpdateAsync(comprobante);

                _logger.LogInformation($"✅ Comprobante {comprobanteId} anulado por {usuario}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al anular comprobante {comprobanteId}");
                throw;
            }
        }

        public async Task<int> ObtenerSiguienteNumeroAsync(string serie)
        {
            try
            {
                return await _comprobanteRepository.GetNextNumberAsync(serie, DEFAULT_TENANT_ID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al obtener siguiente número para serie {serie}");
                throw;
            }
        }
    }
}