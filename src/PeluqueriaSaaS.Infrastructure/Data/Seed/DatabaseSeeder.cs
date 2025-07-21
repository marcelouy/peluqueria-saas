using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Infrastructure.Data;

namespace PeluqueriaSaaS.Infrastructure.Data.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(PeluqueriaDbContext context)
        {
            if (await context.Empleados.AnyAsync())
                return;

            Console.WriteLine("🌱 Creando empleados de prueba...");

            // Solo empleados básicos - usar SQL directo porque las entidades son complejas
            await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO Empleados (Nombre, Apellido, Email, Telefono, FechaNacimiento, FechaRegistro, Cargo, Salario, FechaContratacion, Horario, Direccion, Ciudad, CodigoPostal, Notas, EsActivo, SucursalId)
                VALUES 
                ('Ana', 'García López', 'ana.garcia@demo.cl', '+56912345678', '1985-05-15', GETDATE(), 'Estilista Senior', 800000, '2020-01-15', 'Lun-Vie 9:00-18:00', 'Los Robles 456', 'Santiago', '8320000', 'Especialista en cortes modernos', 1, NULL),
                ('Carlos', 'Rodríguez Silva', 'carlos.rodriguez@demo.cl', '+56987654321', '1990-08-22', GETDATE(), 'Barbero', 650000, '2021-06-01', 'Mar-Sáb 10:00-19:00', 'San Martín 789', 'Santiago', '8330000', 'Experto en cortes masculinos', 1, NULL),
                ('María', 'González Pérez', 'maria.gonzalez@demo.cl', '+56976543210', '1988-12-03', GETDATE(), 'Manicurista', 550000, '2019-09-15', 'Lun-Vie 9:30-17:30', 'Lynch 321', 'Santiago', '8340000', 'Especialista en nail art', 1, NULL),
                ('Patricia', 'Martínez Soto', 'patricia.martinez@demo.cl', '+56965432109', '1982-03-28', GETDATE(), 'Colorista', 950000, '2018-04-10', 'Mié-Dom 10:00-18:00', 'Las Condes 654', 'Santiago', '7550000', '15 años experiencia', 1, NULL)
            ");

            Console.WriteLine("✅ 4 empleados creados");
        }
    }
}