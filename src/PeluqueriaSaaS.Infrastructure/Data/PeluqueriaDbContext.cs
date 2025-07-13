using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Infrastructure.Data;

public class PeluqueriaDbContext : DbContext
{
    public PeluqueriaDbContext(DbContextOptions<PeluqueriaDbContext> options) : base(options) { }

    public DbSet<ClienteBasico> Clientes { get; set; }
    public DbSet<EmpleadoBasico> Empleados { get; set; }
    public DbSet<ServicioBasico> Servicios { get; set; }
    public DbSet<CitaBasica> Citas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClienteBasico>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.ToTable("Clientes");
        });

        modelBuilder.Entity<EmpleadoBasico>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.ToTable("Empleados");
        });

        modelBuilder.Entity<ServicioBasico>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.ToTable("Servicios");
        });

        modelBuilder.Entity<CitaBasica>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClienteId).IsRequired();
            entity.Property(e => e.EmpleadoId).IsRequired();
            entity.Property(e => e.FechaHora).IsRequired();
            entity.ToTable("Citas");
        });
    }
}
