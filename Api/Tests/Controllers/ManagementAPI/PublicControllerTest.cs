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

namespace Tests.Controllers.ManagementAPI
{
    public class PublicControllerTest : IDisposable
    {
        private static Tenant TENANT1 = new Tenant() { Id = Guid.NewGuid(), JwtDuration = 0, JwtSigningKey = "abcdefghijklmnopqrst", ClientSecret = "12345678901234567", AllowPublicUsers = true, Name = "Test Tenant" };
        private static Tenant TENANT2 = new Tenant() { Id = Guid.NewGuid(), JwtDuration = 0, JwtSigningKey = "abcdefghijklmnopqrst", ClientSecret = "12345678901234567", AllowPublicUsers = false, Name = "Test Tenant 2" };

        private DBContext dBContext;
        private PublicController controller;

        public PublicControllerTest()
        {
            var options = new DbContextOptionsBuilder<DBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            this.dBContext = new DBContext(options);
            var service = new TenantsService(dBContext);

            this.controller = new PublicController(service);

            dBContext.Add(TENANT1);

            dBContext.SaveChanges();
        }

        public void Dispose()
        {
            this.dBContext.Database.EnsureDeleted();
            this.dBContext.Dispose();
        }

        [Fact]
        public void GetTenantBackendToken()
        {
            RouteData routeData = new RouteData();
            routeData.Values.Add(JWTTenantFilter.TENANT_KEY, TENANT1);
            ControllerContext controllerContext = new ControllerContext { RouteData = routeData };
            controller.ControllerContext = controllerContext;

            var result = controller.GetToken(new RequestTokenDTO() { ClientSecret = TENANT1.ClientSecret, TenantId = TENANT1.Id.ToString() });

            var objectResult = result as ObjectResult;

            Assert.NotEqual(null, objectResult);
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public void GetTenantBackendTokenWithInvalidCredentials()
        {
            RouteData routeData = new RouteData();
            routeData.Values.Add(JWTTenantFilter.TENANT_KEY, TENANT1);
            ControllerContext controllerContext = new ControllerContext { RouteData = routeData };
            controller.ControllerContext = controllerContext;

            var result = controller.GetToken(new RequestTokenDTO() { ClientSecret = "invalid_secret", TenantId = TENANT1.Id.ToString() });

            var statusResult = result as StatusCodeResult;

            Assert.NotEqual(null, statusResult);
            Assert.Equal(404, statusResult.StatusCode);
        }

        [Fact]
        public void GetTenantPublicSettings()
        {
            RouteData routeData = new RouteData();
            routeData.Values.Add(JWTTenantFilter.TENANT_KEY, TENANT1);
            ControllerContext controllerContext = new ControllerContext { RouteData = routeData };
            controller.ControllerContext = controllerContext;

            var result = controller.GetTenantCustomLogin();

            var objectResult = result as ObjectResult;

            Assert.NotEqual(null, objectResult);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Equal(true, (result as ObjectResult).Value is TenantCustomLogin);

            var customLoginInfo = (result as ObjectResult).Value as TenantCustomLogin;

            Assert.Equal(TENANT1.Name, customLoginInfo.LoginText);
            Assert.Equal(TENANT1.AllowPublicUsers, customLoginInfo.AllowPublicUsers);
        }

        [Fact]
        public void GetTenantPublicSettingsWithInvalidTenant()
        {
            RouteData routeData = new RouteData();
            routeData.Values.Add(JWTTenantFilter.TENANT_KEY, null);
            ControllerContext controllerContext = new ControllerContext { RouteData = routeData };
            controller.ControllerContext = controllerContext;

            var result = controller.GetTenantCustomLogin();

            var statusResult = result as StatusCodeResult;

            Assert.NotEqual(null, statusResult);
            Assert.Equal(404, statusResult.StatusCode);
        }

    }
}