using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Entities.Configuration;
using PeluqueriaSaaS.Domain.ValueObjects;

namespace PeluqueriaSaaS.Infrastructure.Data
{
    public class PeluqueriaDbContext : DbContext
    {
        public PeluqueriaDbContext(DbContextOptions<PeluqueriaDbContext> options) : base(options) { }

        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Sucursal> Sucursales { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<ClienteBasico> ClientesBasicos { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<EmpleadoBasico> EmpleadosBasicos { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<TipoServicio> TiposServicio { get; set; }
        public DbSet<Estacion> Estaciones { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<CitaServicio> CitaServicios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IGNORAR TODOS LOS VALUEOBJECTS
            modelBuilder.Ignore<Email>();
            modelBuilder.Ignore<Telefono>();
            modelBuilder.Ignore<Dinero>();

            // CONFIGURACION DINERO EN SERVICIO
            modelBuilder.Entity<Servicio>()
                .OwnsOne(s => s.Precio, dinero =>
                {
                    dinero.Property(d => d.Valor).HasColumnName("Precio").HasPrecision(18, 2);
                    dinero.Property(d => d.Moneda).HasColumnName("Moneda").HasMaxLength(3).HasDefaultValue("CLP");
                });

            // CONFIGURACION DINERO EN CITA - MONTOTOTAL
            modelBuilder.Entity<Cita>()
                .OwnsOne(c => c.MontoTotal, dinero =>
                {
                    dinero.Property(d => d.Valor).HasColumnName("MontoTotal").HasPrecision(18, 2);
                    dinero.Property(d => d.Moneda).HasColumnName("MonedaTotal").HasMaxLength(3).HasDefaultValue("CLP");
                });

            // CONFIGURACION DINERO EN CITA - MONTOPAGADO
            modelBuilder.Entity<Cita>()
                .OwnsOne(c => c.MontoPagado, dinero =>
                {
                    dinero.Property(d => d.Valor).HasColumnName("MontoPagado").HasPrecision(18, 2);
                    dinero.Property(d => d.Moneda).HasColumnName("MonedaPago").HasMaxLength(3).HasDefaultValue("CLP");
                });

            // CONFIGURACION DINERO EN CITASERVICIO - PRECIOFINAL
            modelBuilder.Entity<CitaServicio>()
                .OwnsOne(cs => cs.PrecioFinal, dinero =>
                {
                    dinero.Property(d => d.Valor).HasColumnName("PrecioFinal").HasPrecision(18, 2);
                    dinero.Property(d => d.Moneda).HasColumnName("MonedaServicio").HasMaxLength(3).HasDefaultValue("CLP");
                });

            // ✅ CONFIGURACIÓN CORREGIDA DE SERVICIO
            modelBuilder.Entity<Servicio>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.DuracionMinutos).IsRequired();
                entity.Property(e => e.TipoServicioId).IsRequired(); // ✅ CORREGIDO: ahora es int (compatible con TipoServicio.Id)
                entity.Property(e => e.TenantId).IsRequired();
                entity.Property(e => e.FechaCreacion).IsRequired();
                entity.Property(e => e.FechaActualizacion).IsRequired();
                entity.Property(e => e.EsActivo).IsRequired().HasDefaultValue(true);
                
                // ✅ RELACIÓN CORREGIDA: int → int (compatible)
                entity.HasOne(s => s.TipoServicio)
                    .WithMany()
                    .HasForeignKey(s => s.TipoServicioId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índices para performance
                entity.HasIndex(e => new { e.TenantId, e.Nombre }).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.TipoServicioId });
                entity.HasIndex(e => new { e.TenantId, e.EsActivo });
            });

            // ✅ CONFIGURACIÓN DE TIPOSERVICIO (para verificar)
            modelBuilder.Entity<TipoServicio>(entity =>
            {
                entity.HasKey(e => e.Id); // ✅ Id es int (hereda de EntityBase)
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TenantId).IsRequired();
                
                // Índice único por tenant
                entity.HasIndex(e => new { e.TenantId, e.Nombre }).IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}