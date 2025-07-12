using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Entities.Configuration;
using PeluqueriaSaaS.Domain.ValueObjects;

namespace PeluqueriaSaaS.Infrastructure.Data
{
    public class PeluqueriaDbContext : DbContext
    {
        public PeluqueriaDbContext(DbContextOptions<PeluqueriaDbContext> options) : base(options) { }

        // Entidades principales
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Sucursal> Sucursales { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Estacion> Estaciones { get; set; }

        // Entidades de configuración
        public DbSet<TipoServicio> TiposServicio { get; set; }
        public DbSet<EstadoCita> EstadosCita { get; set; }
        public DbSet<EstadoEmpleado> EstadosEmpleado { get; set; }
        public DbSet<TipoEstacion> TiposEstacion { get; set; }

        // Entidades de relación
        public DbSet<CitaServicio> CitaServicios { get; set; }
        public DbSet<CitaEstacion> CitaEstaciones { get; set; }
        public DbSet<EmpleadoEstacion> EmpleadoEstaciones { get; set; }
        public DbSet<HistorialCliente> HistorialClientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración Value Objects
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.OwnsOne(c => c.Email, email =>
                {
                    email.Property(e => e.Valor).HasColumnName("Email").HasMaxLength(255);
                });
                
                entity.OwnsOne(c => c.Telefono, telefono =>
                {
                    telefono.Property(t => t.Numero).HasColumnName("TelefonoNumero").HasMaxLength(50);
                    telefono.Property(t => t.CodigoPais).HasColumnName("TelefonoCodigoPais").HasMaxLength(10);
                });
            });

            modelBuilder.Entity<Cita>(entity =>
            {
                entity.OwnsOne(c => c.MontoTotal, monto =>
                {
                    monto.Property(m => m.Valor).HasColumnName("MontoTotal").HasColumnType("decimal(18,2)");
                    monto.Property(m => m.Moneda).HasColumnName("MonedaTotal").HasMaxLength(3);
                });
                
                entity.OwnsOne(c => c.MontoPagado, monto =>
                {
                    monto.Property(m => m.Valor).HasColumnName("MontoPagado").HasColumnType("decimal(18,2)");
                    monto.Property(m => m.Moneda).HasColumnName("MonedaPagado").HasMaxLength(3);
                });
            });

            // Configuración multi-tenancy
            // modelBuilder.Entity<Cliente>().HasQueryFilter(e => e.TenantId == currentTenant);
            // modelBuilder.Entity<Empleado>().HasQueryFilter(e => e.TenantId == currentTenant);
            // modelBuilder.Entity<Cita>().HasQueryFilter(e => e.TenantId == currentTenant);
        }

        private Guid GetCurrentTenantId()
        {
            // TODO: Implementar obtención del tenant actual desde el contexto HTTP
            return Guid.Empty;
        }
    }
}
