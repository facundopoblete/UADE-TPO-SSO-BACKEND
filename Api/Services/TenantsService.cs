using System;
using DataAccess;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Services
{
    public class TenantsService
    {
        DBContext dBContext = new DBContext();

        public Tenant GetTenant(Guid tenantId)
        {
            return dBContext.Tenant.FirstOrDefault(x => x.Id == tenantId);
        }

        public List<Tenant> GetTenants()
        {
            return dBContext.Tenant.ToList();
        }

        public void CreateTenant(string tenantName)
        {
            var rnd = new RNGCryptoServiceProvider();
            var buf = new byte[128];
            rnd.GetBytes(buf);

            var tenant = new Tenant
            {
                Name = tenantName,
                ManagementToken = Convert.ToBase64String(buf),
                ClientId = Guid.NewGuid().ToString("N"),
                JwtSigningKey = Guid.NewGuid().ToString("N"),
                JwtDuration = 0,
                Id = Guid.NewGuid(),
            };

            dBContext.Add(tenant);
            dBContext.SaveChanges();
        }
    }
}
