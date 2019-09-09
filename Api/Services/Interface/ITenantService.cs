using System;
using System.Collections.Generic;
using DataAccess;

namespace Services.Interface
{
    public interface ITenantService
    {
        Tenant GetTenant(Guid tenantId);

        Tenant GetTenantFromAdmin(Guid userId);

        Tenant GetTenant(string tenantId, string clientSecret);

        List<Tenant> GetTenants();

        void CreateTenant(string tenantName, Guid adminId);

        void UpdateTenantSettings(Guid tenantId, string name, string signingKey, int duration, bool allowPublicUsers);
    }
}
