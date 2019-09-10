using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Services.Implementation;
using UsersApi.Controllers;
using UsersApi.Filters;
using UsersApi.Models;
using Xunit;

namespace Tests.Controllers.UsersAPI
{
    public class UserControllerTest : IDisposable
    {
        protected static Guid USER1_ID_TENANT1 = Guid.NewGuid();
        private static String USER1_EMAIL_TENANT1 = "test@test.com";
        private static String USER1_PASSWORD_TENANT1 = "password123";
        private static String USER1_NAME_TENANT1 = "User Test";

        private static String USER2_EMAIL_TENANT1 = "test2@test.com";
        private static String USER2_PASSWORD_TENANT1 = "password12345";
        private static String USER2_NAME_TENANT1 = "User Test 2";

        private static Tenant TENANT1 = new Tenant() { Id = Guid.NewGuid(), JwtDuration = 0, JwtSigningKey = "abcdefghijklmnopqrst", ClientSecret = "12345678901234567", AllowPublicUsers = true };
        private static Tenant TENANT2 = new Tenant() { Id = Guid.NewGuid(), JwtDuration = 0, JwtSigningKey = "12345678901234567", ClientSecret = "abcdefghijklmnopqrst", AllowPublicUsers = false };
        private static Tenant TENANT3 = new Tenant() { Id = Guid.NewGuid(), JwtDuration = 0, JwtSigningKey = "12345678901234567", ClientSecret = "abcdefghijklmnopqrst", AllowPublicUsers = true };

        private static User USER_TENANT1 = new User() { Id = Guid.NewGuid(), TenantId = TENANT1.Id };

        private DBContext dBContext;
        private UserController controller;

        public UserControllerTest()
        {
            var options = new DbContextOptionsBuilder<DBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            this.dBContext = new DBContext(options);
            var service = new UsersService(dBContext);

            this.controller = new UserController(service);

            dBContext.Add(TENANT1);
            dBContext.Add(TENANT2);
            dBContext.Add(TENANT3);
            dBContext.Add(USER_TENANT1);

            dBContext.SaveChanges();

            service.CreateUser(TENANT1.Id, USER1_NAME_TENANT1, USER1_EMAIL_TENANT1, USER1_PASSWORD_TENANT1);
        }

        public void Dispose()
        {
            this.dBContext.Database.EnsureDeleted();
            this.dBContext.Dispose();
        }

        [Fact]
        public void LoginWithValidCredentials()
        {
            RouteData routeData = new RouteData();
            routeData.Values.Add(TenantFilter.TENANT_KEY, TENANT1);
            ControllerContext controllerContext = new ControllerContext { RouteData = routeData };
            controller.ControllerContext = controllerContext;

            var result = controller.Login(new LoginDTO()
            {
                Email = USER1_EMAIL_TENANT1,
                Password = USER1_PASSWORD_TENANT1
            });

            var objectResult = result as ObjectResult;

            Assert.NotEqual(null, objectResult);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Equal(true, (result as ObjectResult).Value is TokenDTO);
        }

        [Fact]
        public void LoginWithInvalidCredentials()
        {
            RouteData routeData = new RouteData();
            routeData.Values.Add(TenantFilter.TENANT_KEY, TENANT1);
            ControllerContext controllerContext = new ControllerContext { RouteData = routeData };
            controller.ControllerContext = controllerContext;

            var result = controller.Login(new LoginDTO()
            {
                Email = USER1_EMAIL_TENANT1,
                Password = "invalid_password_test"
            });

            var statusResult = result as StatusCodeResult;

            Assert.Equal(401, statusResult.StatusCode);
        }

        [Fact]
        public void LoginWithInvalidTenant()
        {
            /*RouteData routeData = new RouteData();
            routeData.Values.Add(TenantFilter.TENANT_KEY, null);
            ControllerContext controllerContext = new ControllerContext { RouteData = routeData };
            controller.ControllerContext = controllerContext;

            var result = controller.Login(new UsersApi.Models.LoginDTO()
            {
                Email = USER1_EMAIL_TENANT1,
                Password = USER1_PASSWORD_TENANT1
            });

            var statusResult = result as StatusCodeResult;

            Assert.Equal(500, statusResult.StatusCode);*/
        }

        [Fact]
        public void LoginWithValidCredentialsButDifferentTenant()
        {
            RouteData routeData = new RouteData();
            routeData.Values.Add(TenantFilter.TENANT_KEY, TENANT2);
            ControllerContext controllerContext = new ControllerContext { RouteData = routeData };
            controller.ControllerContext = controllerContext;

            var result = controller.Login(new LoginDTO()
            {
                Email = USER1_EMAIL_TENANT1,
                Password = USER1_PASSWORD_TENANT1
            });

            var statusResult = result as StatusCodeResult;

            Assert.Equal(401, statusResult.StatusCode);
        }

        [Fact]
        public void CreateUserInPublicTenant()
        {
            RouteData routeData = new RouteData();
            routeData.Values.Add(TenantFilter.TENANT_KEY, TENANT1);
            ControllerContext controllerContext = new ControllerContext { RouteData = routeData };
            controller.ControllerContext = controllerContext;

            var result = controller.Signup(new SignupDTO()
            {
                Email = USER2_EMAIL_TENANT1,
                FullName = USER2_NAME_TENANT1,
                Password = USER2_PASSWORD_TENANT1
            });

            Assert.Equal(true, result is ObjectResult);
            Assert.Equal(true, (result as ObjectResult).Value is TokenDTO);
        }

        [Fact]
        public void CreateUserWithEmailThatAlreadyExistsInOtherTenant()
        {
            RouteData routeData = new RouteData();
            routeData.Values.Add(TenantFilter.TENANT_KEY, TENANT3);
            ControllerContext controllerContext = new ControllerContext { RouteData = routeData };
            controller.ControllerContext = controllerContext;

            var result = controller.Signup(new SignupDTO()
            {
                Email = USER1_EMAIL_TENANT1,
                FullName = USER1_NAME_TENANT1,
                Password = USER1_PASSWORD_TENANT1
            });

            Assert.Equal(true, result is ObjectResult);
            Assert.Equal(true, (result as ObjectResult).Value is TokenDTO);
        }

        [Fact]
        public void CreateUserWithEmailThatAlreadyExists()
        {
            RouteData routeData = new RouteData();
            routeData.Values.Add(TenantFilter.TENANT_KEY, TENANT1);
            ControllerContext controllerContext = new ControllerContext { RouteData = routeData };
            controller.ControllerContext = controllerContext;

            var result = controller.Signup(new SignupDTO()
            {
                Email = USER1_EMAIL_TENANT1,
                FullName = "name",
                Password = "pass"
            });

            var statusResult = result as StatusCodeResult;

            Assert.Equal(409, statusResult.StatusCode);
        }

        [Fact]
        public void CreateUserInPrivateTenant()
        {
            RouteData routeData = new RouteData();
            routeData.Values.Add(TenantFilter.TENANT_KEY, TENANT2);
            ControllerContext controllerContext = new ControllerContext { RouteData = routeData };
            controller.ControllerContext = controllerContext;

            var result = controller.Signup(new SignupDTO()
            {
                Email = USER1_EMAIL_TENANT1,
                FullName = USER1_NAME_TENANT1,
                Password = USER1_PASSWORD_TENANT1
            });

            Assert.Equal(true, result is ForbidResult);
        }

        [Fact]
        public void GetMeInfoValidUser()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Aud, TENANT1.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, USER_TENANT1.Id.ToString()),
            }, "Bearer"));

            RouteData routeData = new RouteData();
            routeData.Values.Add(TenantFilter.TENANT_KEY, TENANT1);
            ControllerContext controllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = user } };
            controller.ControllerContext = controllerContext;

            var result = controller.UserInfo();

            var objectResult = result as ObjectResult;

            Assert.NotEqual(null, objectResult);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Equal(true, (result as ObjectResult).Value is UserDTO);
        }

        [Fact]
        public void GetMeInfoInvalidUser()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Aud, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            }, "Bearer"));

            RouteData routeData = new RouteData();
            routeData.Values.Add(TenantFilter.TENANT_KEY, TENANT1);
            ControllerContext controllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = user } };
            controller.ControllerContext = controllerContext;

            var result = controller.UserInfo();

            Assert.Equal(true, result is StatusCodeResult);
            Assert.Equal(401, (result as StatusCodeResult).StatusCode);
        }

        [Fact]
        public void GetMeInfoNullUser()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            }, "Bearer"));

            RouteData routeData = new RouteData();
            routeData.Values.Add(TenantFilter.TENANT_KEY, TENANT1);
            ControllerContext controllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = user } };
            controller.ControllerContext = controllerContext;

            var result = controller.UserInfo();

            Assert.Equal(true, result is StatusCodeResult);
            Assert.Equal(401, (result as StatusCodeResult).StatusCode);
        }
    }
}