using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.ValueObjects;

namespace PeluqueriaSaaS.Infrastructure.Data.Configurations
{
    public class CitaServicioConfiguration : IEntityTypeConfiguration<CitaServicio>
    {
        public void Configure(EntityTypeBuilder<CitaServicio> builder)
        {
            builder.ToTable("CitaServicios");
            
            builder.HasKey(cs => cs.Id);
            
            // Configuración del ID (sin conversión si es int primitivo)
            // Si CitaServicio.Id es int, no necesita conversión
            // Si CitaServicio.Id es CitaServicioId, usar conversión
            
            // Configuración del Value Object Dinero (PrecioFinal)
            builder.OwnsOne(cs => cs.PrecioFinal, precio =>
            {
                precio.Property(p => p.Valor)
                    .HasColumnName("PrecioFinal")
                    .HasColumnType("decimal(18,2)");
                    
                precio.Property(p => p.Moneda)
                    .HasColumnName("Moneda")
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");
            });
            
            // Configuración de propiedades de relación
            // Solo aplicar conversión si las propiedades son Value Objects
            
            // Si CitaId y ServicioId son Guid primitivos, no convertir
            // Si son Value Objects, usar conversión
            
            // Relaciones
            builder.HasOne<Cita>()
                .WithMany()
                .HasForeignKey(cs => cs.CitaId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne<Servicio>()
                .WithMany()
                .HasForeignKey(cs => cs.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
