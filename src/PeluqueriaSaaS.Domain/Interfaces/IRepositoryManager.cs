using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Entities.Configuration;

namespace PeluqueriaSaaS.Domain.Interfaces;

public interface IRepositoryManager
{
    IRepository<Cliente> Cliente { get; }
    IRepository<Empleado> Empleado { get; }
    IRepository<Cita> Cita { get; }
    IRepository<Empresa> Empresa { get; }
    IRepository<Sucursal> Sucursal { get; }
    IRepository<Servicio> Servicio { get; }
    IRepository<Estacion> Estacion { get; }
    IRepository<TipoServicio> TipoServicio { get; }
    IRepository<EstadoCita> EstadoCita { get; }
    IRepository<EstadoEmpleado> EstadoEmpleado { get; }
    IRepository<TipoEstacion> TipoEstacion { get; }
    IRepository<CitaServicio> CitaServicio { get; }
    IRepository<CitaEstacion> CitaEstacion { get; }
    IRepository<EmpleadoEstacion> EmpleadoEstacion { get; }
    IRepository<HistorialCliente> HistorialCliente { get; }
    
    Task<bool> SaveChangesAsync();
}
