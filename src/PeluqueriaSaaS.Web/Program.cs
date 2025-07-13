using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Infrastructure.Data;
using PeluqueriaSaaS.Infrastructure.Repositories;
using PeluqueriaSaaS.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers y Views
builder.Services.AddControllersWithViews();

// 2. Base de datos
builder.Services.AddDbContext<PeluqueriaDbContext>(options =>
    options.UseInMemoryDatabase("PeluqueriaDB"));

// 3. MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
    typeof(PeluqueriaSaaS.Application.Features.Clientes.Commands.CrearClienteCommand).Assembly));

// 4. AutoMapper - busca todos los profiles automáticamente
builder.Services.AddAutoMapper(typeof(PeluqueriaSaaS.Application.DTOs.ClienteDto).Assembly);

// 5. Repository Pattern
builder.Services.AddScoped<IRepositoryManagerTemp, RepositoryManager>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

var app = builder.Build();

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

// Ruta principal
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Clientes}/{action=Index}/{id?}");

app.Run();



