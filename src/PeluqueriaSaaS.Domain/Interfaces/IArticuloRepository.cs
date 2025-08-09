// Domain/Interfaces/IArticuloRepository.cs
using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Domain.Interfaces
{
    public interface IArticuloRepository
    {
        // ===== CRUD BÁSICO =====
        Task<IEnumerable<Articulo>> GetAllAsync(string tenantId);
        Task<Articulo?> GetByIdAsync(int id, string tenantId);
        Task<Articulo> CreateAsync(Articulo articulo);
        Task<Articulo> UpdateAsync(Articulo articulo);
        Task<bool> DeleteAsync(int id, string tenantId);

        // ===== OPERACIONES ESPECÍFICAS NEGOCIO =====
        Task<IEnumerable<Articulo>> GetActivosAsync(string tenantId);
        Task<bool> ExisteCodigo(string codigo, string tenantId, int? excludeId = null);
        Task<IEnumerable<Articulo>> GetBajoStockAsync(string tenantId);
        Task<bool> DescontarStock(int articuloId, int cantidad, string tenantId);
        
        // ===== DROPDOWNS - FIXED: IEnumerable<string> en lugar de List<string> =====
        Task<IEnumerable<string>> GetCategoriasAsync(string tenantId);
        Task<IEnumerable<string>> GetMarcasAsync(string tenantId);
        Task<IEnumerable<string>> GetProveedoresAsync(string tenantId);
    }
}