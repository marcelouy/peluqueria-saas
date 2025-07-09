namespace PeluqueriaSaaS.Domain.Entities.Base
{
    public abstract class TenantEntityBase : EntityBase, ITenantEntity
    {
        public string TenantId { get; private set; }

        public void SetTenant(string tenantId)
        {
            if (string.IsNullOrEmpty(TenantId))
            {
                TenantId = tenantId;
            }
        }
    }
}
