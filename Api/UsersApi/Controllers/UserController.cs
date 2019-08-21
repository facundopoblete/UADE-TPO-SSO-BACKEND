using DataAccess;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Utils;
using UsersApi.Filters;
using UsersApi.Models;
using UsersApi.Utils;

namespace UsersApi.Controllers
{
    [Route("api")]
    [TenantFilter]
    public class UserController : Controller
    {
        public UsersService usersService = new UsersService();

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            Tenant tenant = RouteData.Values[TenantFilter.TENANT_KEY] as Tenant;

            var user = usersService.GetUser(tenant.Id, login.Email);

            if (user == null)
            {
                return NotFound();
            }

            if (!PasswordUtils.IsValidPassword(user, login.Password))
            {
                usersService.RegisterUserEvent(tenant.Id, user.Id, UserEvents.LOGIN_FAILED);
                return Unauthorized();
            }

            usersService.RegisterUserEvent(tenant.Id, user.Id, UserEvents.LOGIN_SUCCESS);

            return Ok(new TokenDTO
            {
                Token = JWTUtils.CreateJWT(tenant, user)
            });
        }

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] SignupDTO signup)
        {
            Tenant tenant = RouteData.Values[TenantFilter.TENANT_KEY] as Tenant;

            var user = usersService.CreateUser(tenant.Id, signup.FullName, signup.Email, signup.Password);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new TokenDTO
            {
                Token = JWTUtils.CreateJWT(tenant, user)
            });
        }
    }
}