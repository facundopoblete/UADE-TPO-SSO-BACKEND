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
    public class TenantControllerTest : IDisposable
    {
        private DBContext dBContext;
        private TenantController controller;

        public TenantControllerTest()
        {
            var options = new DbContextOptionsBuilder<DBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            this.dBContext = new DBContext(options);
            var service = new TenantsService(dBContext);

            this.controller = new TenantController(service);

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
        }
    }
}
