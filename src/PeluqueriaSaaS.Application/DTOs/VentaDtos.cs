using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PeluqueriaSaaS.Application.DTOs
{
    /// <summary>
    /// DTOs para módulo de ventas POS
    /// FASE A: Transferencia datos + ViewModels para Views
    /// </summary>

    // ==========================================
    // VENTA DTOs
    // ==========================================

    /// <summary>
    /// DTO para crear nueva venta en POS
    /// </summary>
    public class CreateVentaDto
    {
        [Required(ErrorMessage = "La fecha de venta es obligatoria")]
        public DateTime FechaVenta { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Debe seleccionar un empleado")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        public int ClienteId { get; set; }

        [Range(0, 999999.99, ErrorMessage = "El descuento debe ser mayor o igual a 0")]
        public decimal Descuento { get; set; } = 0;

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observaciones { get; set; }

        /// <summary>
        /// Lista de servicios en la venta
        /// </summary>
        public List<CreateVentaDetalleDto> Detalles { get; set; } = new List<CreateVentaDetalleDto>();
    }

    /// <summary>
    /// DTO para mostrar venta existente
    /// </summary>
    public class VentaDto
    {
        public int VentaId { get; set; }
        public DateTime FechaVenta { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        
        // Datos empleado
        public int EmpleadoId { get; set; }
        public string EmpleadoNombre { get; set; } = string.Empty;
        
        // Datos cliente
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        
        // Totales
        public decimal SubTotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        
        public string EstadoVenta { get; set; } = "Completada";
        public string? Observaciones { get; set; }
        
        // Detalles
        public List<VentaDetalleDto> Detalles { get; set; } = new List<VentaDetalleDto>();
    }

    /// <summary>
    /// DTO para actualizar venta existente
    /// </summary>
    public class UpdateVentaDto
    {
        [Required]
        public int VentaId { get; set; }

        [Required(ErrorMessage = "La fecha de venta es obligatoria")]
        public DateTime FechaVenta { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un empleado")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        public int ClienteId { get; set; }

        [Range(0, 999999.99, ErrorMessage = "El descuento debe ser mayor o igual a 0")]
        public decimal Descuento { get; set; }

        [StringLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres")]
        public string EstadoVenta { get; set; } = "Completada";

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observaciones { get; set; }
    }

    // ==========================================
    // VENTA DETALLE DTOs
    // ==========================================

    /// <summary>
    /// DTO para crear detalle de venta (servicio en venta)
    /// </summary>
    public class CreateVentaDetalleDto
    {
        [Required(ErrorMessage = "Debe seleccionar un servicio")]
        public int ServicioId { get; set; }

        [Required(ErrorMessage = "El nombre del servicio es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string NombreServicio { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0, 999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal PrecioUnitario { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, 999, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; } = 1;

        public decimal Subtotal => PrecioUnitario * Cantidad;

        /// <summary>
        /// Empleado específico que realizó este servicio (opcional)
        /// </summary>
        public int? EmpleadoServicioId { get; set; }

        [StringLength(200, ErrorMessage = "Las notas no pueden exceder 200 caracteres")]
        public string? NotasServicio { get; set; }
    }

    /// <summary>
    /// DTO para mostrar detalle de venta existente
    /// </summary>
    public class VentaDetalleDto
    {
        public int VentaDetalleId { get; set; }
        public int VentaId { get; set; }
        
        // Datos servicio
        public int ServicioId { get; set; }
        public string NombreServicio { get; set; } = string.Empty;
        
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        
        public int? EmpleadoServicioId { get; set; }
        public string? EmpleadoServicioNombre { get; set; }
        public string? NotasServicio { get; set; }
    }

    // ==========================================
    // VIEW MODELS PARA POS
    // ==========================================

    /// <summary>
    /// ViewModel para pantalla principal POS
    /// </summary>
    public class PosViewModel
    {
        // Datos venta actual
        public CreateVentaDto VentaActual { get; set; } = new CreateVentaDto();
        
        // Datos para dropdowns
        public List<EmpleadoBasicoDto> Empleados { get; set; } = new List<EmpleadoBasicoDto>();
        public List<ClienteBasicoDto> Clientes { get; set; } = new List<ClienteBasicoDto>();
        public List<ServicioBasicoDto> Servicios { get; set; } = new List<ServicioBasicoDto>();
        
        public Dictionary<string, List<ServicioBasicoDto>> ServiciosAgrupados { get; set; } = new();
        
        // Cálculos
        public decimal SubTotalCalculado => VentaActual.Detalles.Sum(d => d.Subtotal);
        public decimal TotalCalculado => SubTotalCalculado - VentaActual.Descuento;
    }

    /// <summary>
    /// ViewModel para reportes básicos FASE A
    /// </summary>
    public class ReporteVentasViewModel
    {
        public DateTime FechaDesde { get; set; } = DateTime.Today;
        public DateTime FechaHasta { get; set; } = DateTime.Today;
        
        // Resultados
        public List<VentaDto> Ventas { get; set; } = new List<VentaDto>();
        public decimal TotalVentas { get; set; }
        public int CantidadVentas { get; set; }
        public decimal PromedioVenta { get; set; }
        
        // Filtros
        public int? EmpleadoId { get; set; }
        public int? ClienteId { get; set; }
        public string? EstadoVenta { get; set; }
    }

    // ==========================================
    // DTOs BÁSICOS AUXILIARES
    // ==========================================

    public class EmpleadoBasicoDto
    {
        public int EmpleadoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    public class ClienteBasicoDto
    {
        public int ClienteId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
    }

    public class ServicioBasicoDto
    {
        public int ServicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string TipoServicioNombre { get; set; } = string.Empty;
        public int DuracionMinutos { get; set; }
    }

    // ==========================================
    // RESPONSE DTOs PARA API (FUTURO)
    // ==========================================

    public class VentaResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public VentaDto? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class VentasListResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<VentaDto> Data { get; set; } = new List<VentaDto>();
        public int TotalRecords { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}