using System;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Services.Implementation;
using Xunit;
using System.Linq;
using System.Net.Mail;
using Services.Interface;

namespace Tests.Services
{
    public class UsersServiceTest : IDisposable
    {
        private DBContext dBContext;
        private UsersService service;

        private static Guid USER1_ID_TENANT1 = Guid.NewGuid();
        private static String USER1_EMAIL_TENANT1 = "test@test.com";
        private static Guid TENANT1 = Guid.NewGuid();

        private class MockEmailSenderService : IEmailSenderService
        {
            public void sendEmail(MailAddress toAddress, string subject, string body)
            {
                Console.WriteLine(String.Format("Email Sent: {0} {1} {2}", toAddress.Address, subject, body));
            }
        }

        public UsersServiceTest()
        {
            var options = new DbContextOptionsBuilder<DBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            this.dBContext = new DBContext(options);
            this.service = new UsersService(this.dBContext, new MockEmailSenderService());

            var tenant1 = new Tenant() { Id = TENANT1 };
            var user1Tenant1 = new User() { Id = USER1_ID_TENANT1, TenantId = tenant1.Id, Email = USER1_EMAIL_TENANT1 };
            var user2Tenant1 = new User() { Id = Guid.NewGuid(), TenantId = tenant1.Id };
            var user3Tenant1 = new User() { Id = Guid.NewGuid(), TenantId = tenant1.Id };

            var tenant2 = new Tenant() { Id = Guid.NewGuid() };
            var user1Tenant2 = new User() { Id = Guid.NewGuid(), TenantId = tenant2.Id };
            var user2Tenant2 = new User() { Id = Guid.NewGuid(), TenantId = tenant2.Id };
            var user3Tenant2 = new User() { Id = Guid.NewGuid(), TenantId = tenant2.Id };

            var tenant3 = new Tenant() { Id = Guid.NewGuid() };
            var user1Tenant3 = new User() { Id = Guid.NewGuid(), TenantId = tenant3.Id };
            var user2Tenant3 = new User() { Id = Guid.NewGuid(), TenantId = tenant3.Id };
            var user3Tenant3 = new User() { Id = Guid.NewGuid(), TenantId = tenant3.Id };

            this.dBContext.Add(tenant1);
            this.dBContext.Add(tenant2);
            this.dBContext.Add(tenant3);

            this.dBContext.Add(user1Tenant1);
            this.dBContext.Add(user2Tenant1);
            this.dBContext.Add(user3Tenant1);

            this.dBContext.Add(user1Tenant2);
            this.dBContext.Add(user2Tenant2);
            this.dBContext.Add(user3Tenant2);

            this.dBContext.Add(user1Tenant3);
            this.dBContext.Add(user2Tenant3);
            this.dBContext.Add(user3Tenant3);

            this.dBContext.SaveChanges();
        }

        public void Dispose()
        {
            this.dBContext.Database.EnsureDeleted();
            this.dBContext.Dispose();
        }

        [Fact]
        public void GetUsers()
        {
            var users = service.GetUsers(TENANT1);
            Assert.NotEqual(null, users);
            Assert.Equal(users.Any(x => x.Id == USER1_ID_TENANT1), true);
            Assert.Equal(users.Any(x => x.TenantId == TENANT1), true);
            Assert.Equal(users.Any(x => x.Email == USER1_EMAIL_TENANT1), true);

            Assert.Equal(users.Any(x => x.TenantId != TENANT1), false);
        }

        [Fact]
        public void GetUserFromEmail()
        {
            var user = service.GetUser(TENANT1, USER1_EMAIL_TENANT1);
            Assert.NotEqual(null, user);
            Assert.Equal(USER1_ID_TENANT1, user.Id);
            Assert.Equal(TENANT1, user.TenantId);
            Assert.Equal(USER1_EMAIL_TENANT1, user.Email);
        }

        [Fact]
        public void GetUserFromId()
        {
            var user = service.GetUser(TENANT1, USER1_ID_TENANT1);
            Assert.NotEqual(null, user);
            Assert.Equal(USER1_ID_TENANT1, user.Id);
            Assert.Equal(TENANT1, user.TenantId);
            Assert.Equal(USER1_EMAIL_TENANT1, user.Email);
        }

        [Fact]
        public void GetUsers_OtherTenantId()
        {
            var users = service.GetUsers(TENANT1);

            Assert.Equal(users.Any(x => x.TenantId != TENANT1), false);
        }

        [Fact]
        public void CreateUser()
        {
            var newUserFullName = "new-user-test";
            var newUserEmail = "test@test.com";
            var newUserPassword = "test123";

            var user = service.CreateUser(TENANT1, newUserFullName, newUserEmail, newUserPassword);
            var findUser = service.GetUser(TENANT1, user.Id);

            Assert.NotEqual(null, user);
            Assert.NotEqual(null, findUser);

            Assert.Equal(TENANT1, user.TenantId);
            Assert.Equal(newUserEmail, user.Email);
            Assert.Equal(newUserFullName, user.FullName);
        }

        [Fact]
        public void DeleteUser()
        {
            service.DeleteUser(TENANT1, USER1_ID_TENANT1);
            var findUser = service.GetUser(TENANT1, USER1_ID_TENANT1);

            Assert.Equal(null, findUser);
        }

        [Fact]
        public void UpdateUser()
        {
            
        }

    }
}