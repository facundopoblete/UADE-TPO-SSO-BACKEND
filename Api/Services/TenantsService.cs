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

        public Tenant GetTenantFromAdmin(Guid userId)
        {
            return dBContext.Tenant.FirstOrDefault(x => x.AdminId == userId);
        }

        public Tenant GetTenant(string clientId, string clientSecret)
        {
            return dBContext.Tenant.FirstOrDefault(x => x.ClientId == clientId && x.ClientSecret == clientSecret);
        }

        public List<Tenant> GetTenants()
        {
            return dBContext.Tenant.ToList();
        }

        public void CreateTenant(string tenantName)
        {
            var tenant = new Tenant
            {
                Name = tenantName,
                ManagementToken = Convert.ToBase64String(GenerateRandomBytes(128)),
                ClientId = Guid.NewGuid().ToString("N"),
                JwtSigningKey = Guid.NewGuid().ToString("N"),
                JwtDuration = 0,
                ClientSecret = Convert.ToBase64String(GenerateRandomBytes(200)),
                Id = Guid.NewGuid(),
            };

            dBContext.Add(tenant);
            dBContext.SaveChanges();
        }

        private byte[] GenerateRandomBytes(int size)
        {
            var rnd = new RNGCryptoServiceProvider();
            var buf = new byte[size];
            rnd.GetBytes(buf);

            return buf;
        }
    }
}
