using System;
using DataAccess;
using ManagementApi.Controllers;
using ManagementApi.Filters;
using ManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Services.Implementation;
using Xunit;
using System.Linq;

namespace Tests.Controllers.ManagementAPI
{
    public class TenantControllerTest : IDisposable
    {
        private static Tenant TENANT1 = new Tenant() { Id = Guid.NewGuid(), JwtDuration = 0, JwtSigningKey = "abcdefghijklmnopqrst", ClientSecret = "12345678901234567", AllowPublicUsers = true, Name = "Test Tenant" };
        private static User USER1 = new User() { Id = Guid.NewGuid() };

        private DBContext dBContext;
        private TenantController controller;

        public TenantControllerTest()
        {
            var options = new DbContextOptionsBuilder<DBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            this.dBContext = new DBContext(options);
            var service = new TenantsService(dBContext);

            this.controller = new TenantController(service);

            dBContext.Add(TENANT1);
            dBContext.Add(USER1);

            dBContext.SaveChanges();
        }

        public void Dispose()
        {
            this.dBContext.Database.EnsureDeleted();
            this.dBContext.Dispose();
        }

        [Fact]
        public void GetTenant()
        {
            RouteData routeData = new RouteData();
            routeData.Values.Add(JWTTenantFilter.TENANT_KEY, TENANT1);
            ControllerContext controllerContext = new ControllerContext { RouteData = routeData };
            controller.ControllerContext = controllerContext;

            var result = controller.GetSettings();

            var objectResult = result as ObjectResult;

            Assert.NotEqual(null, objectResult);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Equal(true, objectResult.Value is TenantSettingsDTO);
        }

        [Fact]
        public void UpdateTenantSettings()
        {
            RouteData routeData = new RouteData();
            routeData.Values.Add(JWTTenantFilter.TENANT_KEY, TENANT1);
            ControllerContext controllerContext = new ControllerContext { RouteData = routeData };
            controller.ControllerContext = controllerContext;

            controller.UpdateSettings(new UpdateTenantSettingsDTO()
            {
                AllowPublicUsers = false,
                JwtDuration = 43,
                JwtSigningKey = "updated_signing_key",
                Name = "updated_name"
            });

            var result = controller.GetSettings();

            var objectResult = result as ObjectResult;

            Assert.NotEqual(null, objectResult);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Equal(true, objectResult.Value is TenantSettingsDTO);

            var settings = objectResult.Value as TenantSettingsDTO;

            Assert.Equal("updated_signing_key", settings.JwtSigningKey);
            Assert.Equal("updated_name", settings.Name);
            Assert.Equal(43, settings.JwtDuration);
            Assert.Equal(false, settings.AllowPublicUsers);
        }

        [Fact]
        public void CreateNewTenant()
        {
            RouteData routeData = new RouteData();
            routeData.Values.Add(JWTTenantFilter.USER_KEY, USER1.Id);
            ControllerContext controllerContext = new ControllerContext { RouteData = routeData };
            controller.ControllerContext = controllerContext;

            controller.CreateTenant(new NewTenantDTO() { Name = "test" });

            var tenant = this.dBContext.Tenant.Where(x => x.AdminId == USER1.Id).FirstOrDefault();

            Assert.NotEqual(null, tenant);
            Assert.Equal("test", tenant.Name);
        }

    }
}
