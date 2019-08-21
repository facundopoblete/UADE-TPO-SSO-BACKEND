using DataAccess;
using Microsoft.AspNetCore.Mvc;
using Services;
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
        public IActionResult Login(LoginDTO login)
        {
            Tenant tenant = RouteData.Values["tenant"] as Tenant;

            var user = usersService.GetUser(tenant.Id, login.Email);

            if (user == null)
            {
                return NotFound();
            }

            if (!usersService.IsValidPassword(user, login.Password))
            {
                usersService.RegisterUserEvent(tenant.Id, user.Id, "LOGIN FAILED");
                return Unauthorized();
            }

            usersService.RegisterUserEvent(tenant.Id, user.Id, "LOGIN SUCCESS");

            return Ok(new TokenDTO
            {
                Token = JWTUtils.CreateJWT(tenant, user)
            });
        }

        [HttpPost("signup")]
        public IActionResult Signup(SignupDTO signup)
        {
            Tenant tenant = RouteData.Values["tenant"] as Tenant;

            var user = usersService.CreateUser(tenant.Id, signup.FullName, signup.Email, signup.Password);

            if (user == null)
            {
                return null;
            }

            return Ok(new TokenDTO
            {
                Token = JWTUtils.CreateJWT(tenant, user)
            });
        }
    }
}