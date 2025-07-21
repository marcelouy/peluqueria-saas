/*
 * Archivo: src/PeluqueriaSaaS.Web/Program.cs
 * Propósito: Configuración principal - SOLO AÑADIDAS 2 LÍNEAS PARA SERVICIOS
 * Fecha: Julio 2025
 * CAMBIOS: Añadidas SOLO las líneas 18-19 para IServicioRepository y ITipoServicioRepository
 */

using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Infrastructure.Data;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers y Views
builder.Services.AddControllersWithViews();

// 2. Base de datos
builder.Services.AddDbContext<PeluqueriaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
    typeof(PeluqueriaSaaS.Application.Features.Clientes.Commands.CrearClienteCommand).Assembly));

// 4. AutoMapper
builder.Services.AddAutoMapper(typeof(PeluqueriaSaaS.Application.DTOs.ClienteDto).Assembly);

// 5. DEPENDENCIAS - Usar RepositoryManager (NO RepositoryManagerTemp)
builder.Services.AddScoped<IRepositoryManagerTemp, RepositoryManager>();
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// ⭐ 6. NUEVAS DEPENDENCIAS PARA SERVICIOS (SOLO ESTAS 2 LÍNEAS AÑADIDAS)
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<ITipoServicioRepository, TipoServicioRepository>();

var app = builder.Build();

// Asegurar que la base de datos existe y se crea automáticamente
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PeluqueriaDbContext>();
    context.Database.EnsureCreated();
}

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// SEED DE DATOS DE DESARROLLO
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            await PeluqueriaSaaS.Infrastructure.Data.SeedRunner.RunSeedAsync(scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error durante el seed de datos");
        }
    }
}

app.Run();