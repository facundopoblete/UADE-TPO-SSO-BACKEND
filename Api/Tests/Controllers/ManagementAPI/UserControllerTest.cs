using System;
using DataAccess;
using ManagementApi.Controllers;
using Microsoft.EntityFrameworkCore;
using Services.Implementation;
using Xunit;

namespace Tests.Controllers.ManagementAPI
{
    public class UserControllerTest : IDisposable
    {
        private DBContext dBContext;
        private UsersController controller;

        public UserControllerTest()
        {
            var options = new DbContextOptionsBuilder<DBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            this.dBContext = new DBContext(options);
            var service = new UsersService(dBContext, null);

            this.controller = new UsersController(service);

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
