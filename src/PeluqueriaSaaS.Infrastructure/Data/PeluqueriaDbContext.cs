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
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<EmpleadoBasico> EmpleadosBasicos { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<TipoServicio> TiposServicio { get; set; }
        public DbSet<Estacion> Estaciones { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<CitaServicio> CitaServicios { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentaDetalle> VentaDetalles { get; set; }
        public DbSet<Settings> Settings { get; set; }
        public DbSet<Articulo> Articulos { get; set; }
        public DbSet<TipoImpuesto> TiposImpuestos { get; set; }
        public DbSet<TasaImpuesto> TasasImpuestos { get; set; }
        public DbSet<ArticuloImpuesto> ArticulosImpuestos { get; set; }
        public DbSet<ServicioImpuesto> ServiciosImpuestos { get; set; }
        public DbSet<HistoricoTasaImpuesto> HistoricoTasasImpuestos { get; set; }
        public DbSet<EstadoServicio> EstadosServicio { get; set; }
        public DbSet<Comprobante> Comprobantes { get; set; }
        public DbSet<ComprobanteDetalle> ComprobanteDetalles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TipoImpuesto>().ToTable("TiposImpuestos");
            modelBuilder.Entity<TasaImpuesto>().ToTable("TasasImpuestos");

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
                entity.Property(e => e.TipoServicioId).IsRequired();
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
                entity.HasKey(e => e.Id);
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
                entity.Property(v => v.Observaciones).HasMaxLength(500).IsRequired(false);
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
                entity.ToTable("VentaDetalles");
                entity.HasKey(vd => vd.VentaDetalleId);

                // Propiedades básicas
                entity.Property(vd => vd.VentaDetalleId).HasColumnName("VentaDetalleId");
                entity.Property(vd => vd.VentaId).HasColumnName("VentaId").IsRequired();
                entity.Property(vd => vd.ServicioId).HasColumnName("ServicioId").IsRequired();
                entity.Property(vd => vd.NombreServicio).IsRequired().HasMaxLength(100);
                entity.Property(vd => vd.PrecioUnitario).HasColumnType("decimal(10,2)");
                entity.Property(vd => vd.Cantidad).IsRequired();
                entity.Property(vd => vd.Subtotal).HasColumnType("decimal(10,2)");
                entity.Property(vd => vd.EmpleadoServicioId).HasColumnName("EmpleadoServicioId").IsRequired(false);
                entity.Property(vd => vd.NotasServicio).HasMaxLength(200).IsRequired(false);
                entity.Property(vd => vd.TenantId).IsRequired().HasMaxLength(50);
                entity.Property(vd => vd.FechaCreacion).IsRequired();

                // CRÍTICO: Prevenir shadow properties
                entity.Property(vd => vd.VentaId).ValueGeneratedNever();
                entity.Property(vd => vd.ServicioId).ValueGeneratedNever();
                entity.Property(vd => vd.EmpleadoServicioId).ValueGeneratedNever();

                // IGNORAR EXPLÍCITAMENTE cualquier ArticuloId
                entity.Ignore("ArticuloId");
                entity.Ignore("ArticuloId1");
                entity.Ignore("Articulo");

                // Relaciones EXPLÍCITAS con nombres de columna
                entity.HasOne(vd => vd.Venta)
                    .WithMany(v => v.VentaDetalles)
                    .HasForeignKey(vd => vd.VentaId)
                    .HasPrincipalKey(v => v.VentaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(vd => vd.Servicio)
                    .WithMany()
                    .HasForeignKey(vd => vd.ServicioId)
                    .HasPrincipalKey(s => s.Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(vd => vd.EmpleadoServicio)
                    .WithMany()
                    .HasForeignKey(vd => vd.EmpleadoServicioId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(vd => vd.EstadoServicioId)
                    .IsRequired()
                    .ValueGeneratedNever();

                entity.Property(vd => vd.EmpleadoAsignadoId)
                    .IsRequired(false)
                    .ValueGeneratedNever();

                entity.Property(vd => vd.InicioServicio)
                    .IsRequired(false);

                entity.Property(vd => vd.FinServicio)
                    .IsRequired(false);

                // Índices
                entity.HasIndex(vd => vd.VentaId);
                entity.HasIndex(vd => vd.ServicioId);
            });

            // ✅ NUEVA CONFIGURACIÓN SETTINGS
            modelBuilder.Entity<Settings>(entity =>
            {
                entity.HasKey(s => s.Id);

                // Configuración básica
                entity.Property(s => s.NombrePeluqueria).HasMaxLength(100);
                entity.Property(s => s.DireccionPeluqueria).HasMaxLength(200);
                entity.Property(s => s.TelefonoPeluqueria).HasMaxLength(20);
                entity.Property(s => s.EmailPeluqueria).HasMaxLength(100);
                entity.Property(s => s.ResumenEncabezado).HasMaxLength(500);
                entity.Property(s => s.ResumenPiePagina).HasMaxLength(1000);
                entity.Property(s => s.RutaLogo).HasMaxLength(200);
                entity.Property(s => s.ColorPrimario).HasMaxLength(50);
                entity.Property(s => s.ColorSecundario).HasMaxLength(50);
                entity.Property(s => s.TamanoFuente).HasMaxLength(20);
                entity.Property(s => s.SimboloMoneda).HasMaxLength(10);
                entity.Property(s => s.FormatoMoneda).HasMaxLength(50);
                entity.Property(s => s.CodigoPeluqueria).HasMaxLength(50);
                entity.Property(s => s.TemplateCustomHTML).HasMaxLength(2000).IsRequired(false);

                // Defaults
                entity.Property(s => s.ResumenServicioHabilitado).HasDefaultValue(false);
                entity.Property(s => s.Activo).HasDefaultValue(true);
                entity.Property(s => s.FechaCreacion).HasDefaultValueSql("GETDATE()");

                // Índices
                entity.HasIndex(s => s.CodigoPeluqueria).IsUnique();
            });

            // ==========================================
            // 🆕 CONFIGURACIONES DE IMPUESTOS (CORREGIDAS)
            // ==========================================

            // Configuración TipoImpuesto (con propiedades correctas)
            modelBuilder.Entity<TipoImpuesto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Codigo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.TipoCalculo).IsRequired().HasMaxLength(20).HasDefaultValue("PORCENTAJE");
                entity.Property(e => e.AplicaA).IsRequired().HasMaxLength(20).HasDefaultValue("AMBOS");
                entity.Property(e => e.OrdenAplicacion).IsRequired().HasDefaultValue(1);
                entity.Property(e => e.IncluidoEnPrecio).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
                // No configurar FechaCreacion, FechaActualizacion, CreadoPor, ActualizadoPor
                // Ya están definidas en EntityBase y funcionan correctamente

                entity.HasIndex(e => e.Codigo).IsUnique();
            });

            // Configuración TasaImpuesto (con propiedades correctas)
            modelBuilder.Entity<TasaImpuesto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TipoImpuestoId).IsRequired();
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Porcentaje).HasColumnType("decimal(5,2)").IsRequired();
                entity.Property(e => e.FechaInicio).IsRequired();
                entity.Property(e => e.FechaFin).IsRequired(false);
                entity.Property(e => e.CodigoTasa).HasMaxLength(50);
                entity.Property(e => e.EsTasaPorDefecto).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.DecretoLey).HasMaxLength(200);
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
                // No configurar FechaCreacion, FechaActualizacion, CreadoPor, ActualizadoPor
                // Ya están definidas en EntityBase y funcionan correctamente

                entity.HasOne(e => e.TipoImpuesto)
                    .WithMany(t => t.Tasas)
                    .HasForeignKey(e => e.TipoImpuestoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.TipoImpuestoId, e.FechaInicio });
            });

            // 🔧 CONFIGURACIÓN ARTICULO-IMPUESTO (RELACIÓN MANY-TO-MANY)
            modelBuilder.Entity<ArticuloImpuesto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ArticulosImpuestos");

                entity.Property(e => e.ArticuloId).IsRequired().HasColumnName("ArticuloId");
                entity.Property(e => e.TasaImpuestoId).IsRequired().HasColumnName("TasaImpuestoId");
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FechaInicioAplicacion).IsRequired();
                entity.Property(e => e.FechaFinAplicacion).IsRequired(false);
                entity.Property(e => e.PorcentajeEspecial).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Notas).HasMaxLength(500);
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
                // 🔴 AGREGAR ESTAS DOS LÍNEAS PARA PREVENIR ArticuloId1
                entity.Property(e => e.ArticuloId).ValueGeneratedNever();
                entity.Property(e => e.TasaImpuestoId).ValueGeneratedNever();
                entity.HasOne(e => e.Articulo)
                    .WithMany(a => a.ArticulosImpuestos)
                    .HasForeignKey(e => e.ArticuloId)
                    .HasPrincipalKey(a => a.Id)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.TasaImpuesto)
                    .WithMany(t => t.ArticulosImpuestos)
                    .HasForeignKey(e => e.TasaImpuestoId)
                    .HasPrincipalKey(t => t.Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.ArticuloId, e.TasaImpuestoId, e.TenantId, e.Activo })
                    .HasFilter("[Activo] = 1")
                    .IsUnique();
            });



            // 🔧 CONFIGURACIÓN SERVICIO-IMPUESTO (RELACIÓN MANY-TO-MANY)
            modelBuilder.Entity<ServicioImpuesto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ServiciosImpuestos");

                entity.Property(e => e.ServicioId).IsRequired().HasColumnName("ServicioId");
                entity.Property(e => e.TasaImpuestoId).IsRequired().HasColumnName("TasaImpuestoId");
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FechaInicioAplicacion).IsRequired();
                entity.Property(e => e.FechaFinAplicacion).IsRequired(false);
                entity.Property(e => e.PorcentajeEspecial).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Notas).HasMaxLength(500);
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
                // 🔴 AGREGAR TAMBIÉN AQUÍ
                entity.Property(e => e.ServicioId).ValueGeneratedNever();
                entity.Property(e => e.TasaImpuestoId).ValueGeneratedNever();
                entity.HasOne(e => e.Servicio)
                    .WithMany()
                    .HasForeignKey(e => e.ServicioId)
                    .HasPrincipalKey(s => s.Id)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.TasaImpuesto)
                    .WithMany(t => t.ServiciosImpuestos)
                    .HasForeignKey(e => e.TasaImpuestoId)
                    .HasPrincipalKey(t => t.Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.ServicioId, e.TasaImpuestoId, e.TenantId, e.Activo })
                    .HasFilter("[Activo] = 1")
                    .IsUnique();
            });


            // Configuración HistoricoTasaImpuesto (con propiedades correctas)
            modelBuilder.Entity<HistoricoTasaImpuesto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TasaImpuestoId).IsRequired();
                entity.Property(e => e.PorcentajeAnterior).HasColumnType("decimal(5,2)").IsRequired();
                entity.Property(e => e.PorcentajeNuevo).HasColumnType("decimal(5,2)").IsRequired();
                entity.Property(e => e.FechaCambio).IsRequired();
                entity.Property(e => e.Motivo).IsRequired().HasMaxLength(500);
                entity.Property(e => e.DecretoLey).HasMaxLength(200);
                entity.Property(e => e.ModificadoPor).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.TasaImpuesto)
                    .WithMany()
                    .HasForeignKey(e => e.TasaImpuestoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.TasaImpuestoId, e.FechaCambio });
            });

            // 🔧 CONFIGURACIÓN ARTICULO (con propiedades correctas)
            modelBuilder.Entity<Articulo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Codigo).HasMaxLength(50);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.Categoria).HasMaxLength(50);
                entity.Property(e => e.Marca).HasMaxLength(50);
                entity.Property(e => e.Proveedor).HasMaxLength(100);
                entity.Property(e => e.Precio).HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.PrecioCosto).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Margen).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Oferta).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.PrecioOferta).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Stock).IsRequired().HasDefaultValue(0);
                entity.Property(e => e.StockMinimo).IsRequired().HasDefaultValue(0);
                entity.Property(e => e.RequiereStock).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.Peso).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Contenido).HasMaxLength(50);
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).IsRequired();
                entity.Property(e => e.FechaActualizacion).IsRequired();

                // Índices
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Categoria });
                entity.HasIndex(e => new { e.TenantId, e.Marca });
                entity.HasIndex(e => new { e.TenantId, e.Activo });
            });

            // Configuración EstadoServicio (agregar después de otras configuraciones)
            modelBuilder.Entity<EstadoServicio>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("EstadoServicio");
                entity.Property(e => e.Codigo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Color).IsRequired().HasMaxLength(7);
                entity.Property(e => e.PermiteCobro).IsRequired();
                entity.Property(e => e.EsFinal).IsRequired();
                entity.Property(e => e.Orden).IsRequired();
                entity.Property(e => e.Activo).IsRequired();
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);

                entity.HasIndex(e => e.Codigo).IsUnique();
            });

            // Configuración de Comprobantes
            modelBuilder.Entity<Comprobante>(entity =>
            {
                entity.ToTable("Comprobantes");
                entity.Property(e => e.Id).HasColumnName("ComprobanteId");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Serie).HasMaxLength(10);
                entity.Property(e => e.MetodoPago).HasMaxLength(50);
                entity.Property(e => e.Estado).HasMaxLength(20);

                entity.HasMany(e => e.Detalles)
                    .WithOne(d => d.Comprobante)
                    .HasForeignKey(d => d.ComprobanteId);
            });

            // Configuración de ComprobanteDetalles
            modelBuilder.Entity<ComprobanteDetalle>(entity =>
            {
                entity.ToTable("ComprobanteDetalles");
                entity.Property(e => e.Id).HasColumnName("DetalleId");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.TipoItem).HasMaxLength(20);
            });
        }
    }
}