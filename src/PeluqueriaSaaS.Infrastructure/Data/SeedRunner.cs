using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PeluqueriaSaaS.Infrastructure.Data;
using PeluqueriaSaaS.Infrastructure.Data.Seed;

namespace PeluqueriaSaaS.Infrastructure.Data
{
    public static class SeedRunner
    {
        public static async Task RunSeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PeluqueriaDbContext>();

            Console.WriteLine("🌱 Iniciando proceso de seed...");
            
            try
            {
                // Asegurar que la BD existe y está actualizada
                await context.Database.EnsureCreatedAsync();
                
                // Ejecutar seed solo si no hay datos
                await DatabaseSeeder.SeedAsync(context);
                
                Console.WriteLine("✅ Proceso de seed completado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en seed: {ex.Message}");
                Console.WriteLine($"📋 Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}