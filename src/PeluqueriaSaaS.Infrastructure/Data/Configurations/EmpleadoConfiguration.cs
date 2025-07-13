using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.ValueObjects;

namespace PeluqueriaSaaS.Infrastructure.Data.Configurations
{
    public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
    {
        public void Configure(EntityTypeBuilder<Empleado> builder)
        {
            builder.ToTable("Empleados");
            
            builder.HasKey(e => e.Id);
            
            // Configuración SOLO del Value Object Email para resolver el error
            builder.OwnsOne(e => e.Email, email =>
            {
                email.Property(em => em.Valor)
                    .HasColumnName("Email")
                    .HasMaxLength(255);
            });
            
            // Si existe Telefono, también configurarlo
            // Verificar si la entidad tiene esta propiedad antes de descomentar
            /*
            builder.OwnsOne(e => e.Telefono, telefono =>
            {
                telefono.Property(t => t.Numero)
                    .HasColumnName("Telefono")
                    .HasMaxLength(20);
            });
            */
        }
    }
}
