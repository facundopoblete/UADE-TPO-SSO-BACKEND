using System;
using DataAccess;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Services.Interface;

namespace Services.Implementation
{
    public class TenantsService : ITenantService
    {
        private DBContext dBContext;

        public TenantsService(DBContext dBContext)
        {
            this.dBContext = dBContext;
        }
        
        public Tenant GetTenant(Guid tenantId)
        {
            return dBContext.Tenant.FirstOrDefault(x => x.Id == tenantId);
        }

        public Tenant GetTenantFromAdmin(Guid userId)
        {
            return dBContext.Tenant.FirstOrDefault(x => x.AdminId == userId);
        }

        public Tenant GetTenant(string tenantId, string clientSecret)
        {
            return dBContext.Tenant.FirstOrDefault(x => x.Id == Guid.Parse(tenantId) && x.ClientSecret == clientSecret);
        }

        public List<Tenant> GetTenants()
        {
            return dBContext.Tenant.ToList();
        }

        public void CreateTenant(string tenantName, Guid adminId)
        {
            var tenant = new Tenant
            {
                Name = tenantName,
                ClientId = Guid.NewGuid().ToString("N"),
                JwtSigningKey = Guid.NewGuid().ToString("N"),
                JwtDuration = 0,
                ClientSecret = Convert.ToBase64String(GenerateRandomBytes(200)),
                AdminId = adminId,
                Id = Guid.NewGuid(),
            };

            dBContext.Add(tenant);
            dBContext.SaveChanges();
        }

        public void UpdateTenantSettings(Guid tenantId, string name, string signingKey, int duration, bool allowPublicUsers)
        {
            var newTenantInfo = GetTenant(tenantId);

            newTenantInfo.Name = name;
            newTenantInfo.JwtSigningKey = signingKey;
            newTenantInfo.JwtDuration = duration;
            newTenantInfo.AllowPublicUsers = allowPublicUsers;
            newTenantInfo.Id = tenantId;

            dBContext.Update(newTenantInfo);
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
