// Archivo: src/PeluqueriaSaaS.Application/DTOs/AsignacionRapidaDto.cs

namespace PeluqueriaSaaS.Application.DTOs
{
    public class AsignacionRapidaDto
    {
        public int ClienteId { get; set; }
        public List<ServicioAsignacionDto> Asignaciones { get; set; } = new();
    }

    public class ServicioAsignacionDto
    {
        public int ServicioId { get; set; }
        public int EmpleadoAsignadoId { get; set; }
        public string? Notas { get; set; }
    }
}