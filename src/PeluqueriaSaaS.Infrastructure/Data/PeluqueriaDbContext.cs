// src/PeluqueriaSaaS.Infrastructure/Data/PeluqueriaDbContext.cs
using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Infrastructure.Data
{
    public class PeluqueriaDbContext : DbContext
    {
        public PeluqueriaDbContext(DbContextOptions<PeluqueriaDbContext> options) : base(options)
        {
        }

        public DbSet<ClienteBasico> ClientesBasicos { get; set; }
        public DbSet<Empleado> Empleados { get; set; } = null!;
        public DbSet<EmpleadoBasico> EmpleadosBasicos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configuración para ClienteBasico
            modelBuilder.Entity<ClienteBasico>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.FechaRegistro).IsRequired();
                
                // Nuevos campos
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.Property(e => e.Ciudad).HasMaxLength(100);
                entity.Property(e => e.CodigoPostal).HasMaxLength(10);
                entity.Property(e => e.Notas).HasMaxLength(500);
                entity.Property(e => e.EsActivo).IsRequired().HasDefaultValue(true);
            });
            
            // Configuración para EmpleadoBasico
            modelBuilder.Entity<EmpleadoBasico>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.FechaContratacion).IsRequired();
                entity.Property(e => e.Cargo).HasMaxLength(100);
                entity.Property(e => e.Salario).HasColumnType("decimal(18,2)");
            });
        }
    }
}

