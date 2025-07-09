namespace PeluqueriaSaaS.Domain.Entities.Base
{
    public interface ITenantEntity
    {
        string TenantId { get; }
        void SetTenant(string tenantId);
    }
}
