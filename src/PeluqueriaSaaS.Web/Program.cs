using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Infrastructure.Data;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Repositories;
using PeluqueriaSaaS.Infrastructure.Services;
using MediatR;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

//TODO: SACAR EN PRODUCCION 
// ✅ Fuerza Kestrel puro (sin IIS) Sacar esta linea para produccion o preguntar a claudeIA
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(5043); // Escucha en todas las IPs para HTTP
});


// 1. CONFIGURACIÓN BASE
builder.Services.AddControllersWithViews();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PeluqueriaSaaS.Application.Features.Clientes.Queries.ObtenerClientesQuery).Assembly));


// 2. ENTITY FRAMEWORK
builder.Services.AddDbContext<PeluqueriaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ PDF SERVICES REGISTRATION - Using Repository Manager Pattern
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IBrowserPool, BrowserPool>();
builder.Services.AddTransient<IPdfService, PuppeteerPdfService>();

// ✅ LOGGING CONFIGURATION - Verificar que existe
builder.Services.AddLogging();
// 4. AUTOMAPPER (SI SE USA)
// builder.Services.AddAutoMapper(typeof(Program));

// 5. DEPENDENCIAS - Repository Pattern
builder.Services.AddScoped<IRepositoryManagerTemp, RepositoryManager>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// 6. SERVICIOS ESPECÍFICOS - ORDEN CRÍTICO
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<ITipoServicioRepository, TipoServicioRepository>();
builder.Services.AddScoped<IVentaRepository, VentaRepository>();

// ✅ 7. SETTINGS REPOSITORY - CRÍTICO PARA /Settings ENDPOINT
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.Run();