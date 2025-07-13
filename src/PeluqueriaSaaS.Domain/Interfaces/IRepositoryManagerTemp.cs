using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Domain.Interfaces;

public interface IRepositoryManagerTemp
{
    IRepository<ClienteBasico> Cliente { get; }
    IRepository<EmpleadoBasico> Empleado { get; }
    IRepository<ServicioBasico> Servicio { get; }
    IRepository<CitaBasica> Cita { get; }
    Task<bool> SaveChangesAsync();
}

// Entidades básicas en Domain también
public class ClienteBasico
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class EmpleadoBasico
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class ServicioBasico
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

public class CitaBasica
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int EmpleadoId { get; set; }
    public DateTime FechaHora { get; set; }
}
