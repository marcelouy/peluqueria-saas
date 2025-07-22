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
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentaDetalle> VentaDetalles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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

            // Configuración Venta
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.HasKey(v => v.VentaId);
                entity.Property(v => v.NumeroVenta).IsRequired().HasMaxLength(20);
                entity.Property(v => v.SubTotal).HasColumnType("decimal(10,2)");
                entity.Property(v => v.Descuento).HasColumnType("decimal(10,2)");
                entity.Property(v => v.Total).HasColumnType("decimal(10,2)");
                entity.Property(v => v.EstadoVenta).IsRequired().HasMaxLength(20);
                entity.Property(v => v.Observaciones).HasMaxLength(500);
                entity.Property(v => v.TenantId).IsRequired().HasMaxLength(50);
                
                // Relaciones
                entity.HasOne(v => v.Empleado)
                    .WithMany()
                    .HasForeignKey(v => v.EmpleadoId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(v => v.Cliente)
                    .WithMany()
                    .HasForeignKey(v => v.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Índices
                entity.HasIndex(v => new { v.FechaVenta, v.TenantId });
                entity.HasIndex(v => v.NumeroVenta).IsUnique();
            });

            // Configuración VentaDetalle
            modelBuilder.Entity<VentaDetalle>(entity =>
            {
                entity.HasKey(vd => vd.VentaDetalleId);
                entity.Property(vd => vd.NombreServicio).IsRequired().HasMaxLength(100);
                entity.Property(vd => vd.PrecioUnitario).HasColumnType("decimal(10,2)");
                entity.Property(vd => vd.Subtotal).HasColumnType("decimal(10,2)");
                entity.Property(vd => vd.NotasServicio).HasMaxLength(200);
                entity.Property(vd => vd.TenantId).IsRequired().HasMaxLength(50);
                
                // Relaciones
                entity.HasOne(vd => vd.Venta)
                    .WithMany(v => v.VentaDetalles)
                    .HasForeignKey(vd => vd.VentaId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(vd => vd.Servicio)
                    .WithMany()
                    .HasForeignKey(vd => vd.ServicioId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(vd => vd.EmpleadoServicio)
                    .WithMany()
                    .HasForeignKey(vd => vd.EmpleadoServicioId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                // Índices
                entity.HasIndex(vd => vd.VentaId);
                entity.HasIndex(vd => vd.ServicioId);
            });
        }
    }
}