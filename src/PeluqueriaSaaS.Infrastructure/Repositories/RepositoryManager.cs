// src/PeluqueriaSaaS.Infrastructure/Repositories/RepositoryManager.cs
using Microsoft.EntityFrameworkCore;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Infrastructure.Data;

namespace PeluqueriaSaaS.Infrastructure.Repositories
{
    public class RepositoryManager : IRepositoryManagerTemp
    {
        private readonly PeluqueriaDbContext _context;

        public RepositoryManager(PeluqueriaDbContext context)
        {
            _context = context;
        }

        // Métodos para ClienteBasico
        public async Task<IEnumerable<ClienteBasico>> GetAllClientesAsync()
        {
            return await _context.ClientesBasicos.ToListAsync();
        }

        public async Task<ClienteBasico?> GetClienteByIdAsync(int id)
        {
            return await _context.ClientesBasicos.FindAsync(id);
        }

        public async Task<ClienteBasico> AddClienteAsync(ClienteBasico cliente)
        {
            _context.ClientesBasicos.Add(cliente);
            await _context.SaveChangesAsync();
            return cliente;
        }

        public async Task<ClienteBasico> UpdateClienteAsync(ClienteBasico cliente)
        {
            _context.Entry(cliente).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return cliente;
        }

        public async Task<bool> DeleteClienteAsync(int id)
        {
            var cliente = await _context.ClientesBasicos.FindAsync(id);
            if (cliente != null)
            {
                _context.ClientesBasicos.Remove(cliente);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        // Métodos para EmpleadoBasico
        public async Task<IEnumerable<EmpleadoBasico>> GetAllEmpleadosAsync()
        {
            return await _context.EmpleadosBasicos.ToListAsync();
        }

        public async Task<EmpleadoBasico?> GetEmpleadoByIdAsync(int id)
        {
            return await _context.EmpleadosBasicos.FindAsync(id);
        }

        public async Task<EmpleadoBasico> AddEmpleadoAsync(EmpleadoBasico empleado)
        {
            _context.EmpleadosBasicos.Add(empleado);
            await _context.SaveChangesAsync();
            return empleado;
        }

        public async Task<EmpleadoBasico> UpdateEmpleadoAsync(EmpleadoBasico empleado)
        {
            _context.Entry(empleado).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return empleado;
        }

        public async Task<bool> DeleteEmpleadoAsync(int id)
        {
            var empleado = await _context.EmpleadosBasicos.FindAsync(id);
            if (empleado != null)
            {
                _context.EmpleadosBasicos.Remove(empleado);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
