using System;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Services.Implementation;
using Xunit;
using System.Linq;

namespace Tests.Services
{
    public class TenantsServiceTest : IDisposable
    {
        private DBContext dBContext;
        private TenantsService service;

        public TenantsServiceTest()
        {
            var options = new DbContextOptionsBuilder<DBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            this.dBContext = new DBContext(options);
            this.service = new TenantsService(this.dBContext);
        }

        public void Dispose()
        {
            this.dBContext.Database.EnsureDeleted();
            this.dBContext.Dispose();
        }

        [Fact]
        public void GetTenantThatNotExist()
        {
            var tenant = service.GetTenant(Guid.NewGuid());
            Assert.Equal(null, tenant);
        }

        [Fact]
        public void GetTenantThatExist()
        {
            var tenantId = Guid.NewGuid();

            this.dBContext.Tenant.Add(new Tenant { Id = tenantId });
            this.dBContext.SaveChanges();

            var tenant = service.GetTenant(tenantId);

            Assert.NotEqual(null, tenant);
            Assert.Equal(tenantId, tenant.Id);
        }

        [Fact]
        public void GetTenantFromAdmin()
        {
            var tenant = new Tenant() { Id = Guid.NewGuid(), AdminId = Guid.NewGuid() };

            this.dBContext.Tenant.Add(tenant);
            this.dBContext.SaveChanges();

            var tenantFromAdmin = service.GetTenantFromAdmin(tenant.AdminId.Value);

            Assert.NotEqual(null, tenantFromAdmin);
            Assert.Equal(tenantFromAdmin.Id, tenant.Id);
        }

        [Fact]
        public void GetTenants()
        {
            var tenant1 = new Tenant { Id = Guid.NewGuid() };
            var tenant2 = new Tenant { Id = Guid.NewGuid() };

            this.dBContext.Tenant.Add(tenant1);
            this.dBContext.Tenant.Add(tenant2);
            this.dBContext.SaveChanges();

            var tenants = service.GetTenants();

            Assert.NotEqual(null, tenants);
            Assert.Equal(2, tenants.Count);
            Assert.Equal(tenants.Any(x => x.Id == tenant1.Id), true);
            Assert.Equal(tenants.Any(x => x.Id == tenant2.Id), true);
        }

    }
}