using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Infrastructure.Data;

namespace PeluqueriaSaaS.Infrastructure.Repositories
{
    public class EmpleadoRepository : IEmpleadoRepository
    {
        private readonly PeluqueriaDbContext _context;

        public EmpleadoRepository(PeluqueriaDbContext context)
        {
            _context = context;
        }

        // ✅ MÉTODOS CRUD BÁSICOS ORIGINALES
        public async Task<IEnumerable<Empleado>> GetAllAsync()
        {
            return await _context.Empleados
                .OrderBy(e => e.Apellido)
                .ThenBy(e => e.Nombre)
                .ToListAsync();
        }

        public async Task<Empleado?> GetByIdAsync(int id)
        {
            return await _context.Empleados.FindAsync(id);
        }

        public async Task<Empleado> AddAsync(Empleado empleado)
        {
            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();
            return empleado;
        }

        public async Task<Empleado> UpdateAsync(Empleado empleado)
        {
            _context.Entry(empleado).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!(await ExistsAsync(empleado.Id)))
                {
                    throw new InvalidOperationException("El empleado no existe");
                }
                throw;
            }

            return empleado;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
            {
                return false;
            }

            _context.Empleados.Remove(empleado);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Empleados.AnyAsync(e => e.Id == id);
        }

        // ✅ MÉTODOS ADICIONALES ORIGINALES
        public async Task<Empleado> CreateAsync(Empleado empleado)
        {
            // CreateAsync es un alias para AddAsync
            return await AddAsync(empleado);
        }

        public async Task<IEnumerable<Empleado>> GetActivosAsync()
        {
            return await _context.Empleados
                .Where(e => e.EsActivo)
                .OrderBy(e => e.Apellido)
                .ThenBy(e => e.Nombre)
                .ToListAsync();
        }

        // ✅ MÉTODOS PARA USERIDENTIFICATIONSERVICE - CORREGIDOS PARA ENTIDAD REAL
        
        /// <summary>
        /// Buscar empleado por email - CORREGIDO para entidad real
        /// </summary>
        public async Task<Empleado?> GetByEmailAsync(string email, string tenantId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return null;

                return await _context.Empleados
                    .FirstOrDefaultAsync(e => 
                        e.EsActivo && 
                        e.Email != null &&
                        e.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetByEmailAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtener empleados activos con parámetro tenantId (por compatibilidad)
        /// </summary>
        public async Task<IEnumerable<Empleado>> GetActivosAsync(string tenantId)
        {
            try
            {
                // Usar método existente que YA FUNCIONA
                return await GetActivosAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetActivosAsync(tenantId): {ex.Message}");
                return new List<Empleado>();
            }
        }

        /// <summary>
        /// Buscar empleado por documento - SIMPLIFICADO para entidad real
        /// </summary>
        public async Task<Empleado?> GetByDocumentoAsync(string documento, string tenantId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(documento))
                    return null;

                // NOTA: Empleado actual no tiene campo Documento
                // Buscar por Email como alternativa
                return await GetByEmailAsync(documento, tenantId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetByDocumentoAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtener empleados administradores - CORREGIDO para entidad real
        /// </summary>
        public async Task<IEnumerable<Empleado>> GetAdministradoresAsync(string tenantId)
        {
            try
            {
                var cargosAdmin = new[] { "administrador", "gerente", "supervisor", "admin" };
                
                return await _context.Empleados
                    .Where(e => 
                        e.EsActivo &&
                        !string.IsNullOrEmpty(e.Cargo) &&
                        cargosAdmin.Any(cargo => e.Cargo.ToLower().Contains(cargo)))
                    .OrderBy(e => e.Nombre)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetAdministradoresAsync: {ex.Message}");
                return new List<Empleado>();
            }
        }

        /// <summary>
        /// Obtener lista de cargos únicos - CORREGIDO para entidad real
        /// </summary>
        public async Task<IEnumerable<string>> GetCargosAsync(string tenantId)
        {
            try
            {
                return await _context.Empleados
                    .Where(e => 
                        e.EsActivo && 
                        !string.IsNullOrEmpty(e.Cargo))
                    .Select(e => e.Cargo)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetCargosAsync: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Obtener empleados activos simplificado - CORREGIDO para entidad real
        /// </summary>
        public async Task<IEnumerable<Empleado>> GetEmpleadosActivosSimpleAsync(string tenantId)
        {
            try
            {
                return await _context.Empleados
                    .Where(e => e.EsActivo)
                    .Select(e => new Empleado
                    {
                        Id = e.Id,
                        Nombre = e.Nombre,
                        Apellido = e.Apellido,
                        Email = e.Email,
                        Cargo = e.Cargo
                    })
                    .OrderBy(e => e.Nombre)
                    .ThenBy(e => e.Apellido)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetEmpleadosActivosSimpleAsync: {ex.Message}");
                return new List<Empleado>();
            }
        }

        /// <summary>
        /// Verificar si empleado es administrador - CORREGIDO para entidad real
        /// </summary>
        public async Task<bool> IsEmpleadoAdminAsync(int empleadoId, string tenantId)
        {
            try
            {
                var empleado = await _context.Empleados
                    .FirstOrDefaultAsync(e => 
                        e.Id == empleadoId && 
                        e.EsActivo);

                if (string.IsNullOrEmpty(empleado?.Cargo))
                    return false;

                var cargo = empleado.Cargo.ToLower();
                return cargo.Contains("admin") || 
                       cargo.Contains("gerente") || 
                       cargo.Contains("supervisor");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en IsEmpleadoAdminAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Buscar empleado por username/email - CORREGIDO para entidad real
        /// </summary>
        public async Task<Empleado?> GetByUsernameAsync(string username, string tenantId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return null;

                // Buscar por email como username
                return await GetByEmailAsync(username, tenantId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetByUsernameAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Buscar empleados por término - CORREGIDO para entidad real
        /// </summary>
        public async Task<IEnumerable<Empleado>> SearchEmpleadosAsync(string searchTerm, string tenantId, int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new List<Empleado>();

                var term = searchTerm.ToLower();

                return await _context.Empleados
                    .Where(e => 
                        e.EsActivo &&
                        (e.Nombre.ToLower().Contains(term) ||
                         e.Apellido.ToLower().Contains(term) ||
                         e.Email.ToLower().Contains(term)))
                    .OrderBy(e => e.Nombre)
                    .ThenBy(e => e.Apellido)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en SearchEmpleadosAsync: {ex.Message}");
                return new List<Empleado>();
            }
        }
    }
}