using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interface;
using Services.Utils;
using UsersApi.Filters;
using UsersApi.Models;
using UsersApi.Utils;

namespace UsersApi.Controllers
{
    [Route("api")]
    public class UserController : Controller
    {
        public IUserService usersService;

        public UserController(IUserService usersService)
        {
            this.usersService = usersService;
        }

        [HttpPost("login")]
        [TenantFilter]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            Tenant tenant = RouteData.Values[TenantFilter.TENANT_KEY] as Tenant;

            var user = usersService.GetUser(tenant.Id, login.Email);

            if (user == null)
            {
                return Unauthorized();
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
        [TenantFilter]
        public IActionResult Signup([FromBody] SignupDTO signup)
        {
            Tenant tenant = RouteData.Values[TenantFilter.TENANT_KEY] as Tenant;

            if (!tenant.AllowPublicUsers.Value)
            {
                return Forbid();
            }

            var existUser = usersService.GetUser(tenant.Id, signup.Email);

            if (existUser != null)
            {
                return Conflict();
            }

            var user = usersService.CreateUser(tenant.Id, signup.FullName, signup.Email, signup.Password);

            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(new TokenDTO
            {
                Token = JWTUtils.CreateJWT(tenant, user)
            });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult UserInfo()
        {
            var audience = HttpContext.User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Aud).FirstOrDefault();
            var userId = HttpContext.User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sub).FirstOrDefault();

            if (audience == null || userId == null)
            {
                return Unauthorized();
            }

            var user = usersService.GetUser(Guid.Parse(audience.Value), Guid.Parse(userId.Value));

            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(new UserDTO
            {
                FullName = user.FullName,
                Email = user.Email,
                ExtraClaim = user.ExtraClaims,
                Metadata = user.Metadata,
                Id = user.Id
            });
        }
    }
}