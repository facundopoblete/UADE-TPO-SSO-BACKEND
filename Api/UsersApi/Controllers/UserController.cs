using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
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

        /// <summary>
        /// Login de un usuario
        /// </summary>
        /// <param name="login"></param>
        /// <returns>El JWT del usuario</returns>
        /// <response code="401">Las credenciales no son validas.</response>
        [HttpPost("login")]
        [TenantFilter]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            if (RouteData.Values[TenantFilter.TENANT_KEY] == null)
            {
                return Unauthorized();
            }

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

        /// <summary>
        /// Registro de un nuevo usuario
        /// </summary>
        /// <param name="signup"></param>
        /// <returns>El JWT del usuario</returns>
        /// <response code="401">Las credenciales no son validas.</response>
        /// <response code="403">El tenant no permite creación de usuarios.</response>
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

            User user;
            try
            {
                user = usersService.CreateUser(tenant.Id, signup.FullName, signup.Email, signup.Password);
            }
            catch (FormatException)
            {
                return Conflict("Invalid email format");
            }
            
            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(new TokenDTO
            {
                Token = JWTUtils.CreateJWT(tenant, user)
            });
        }

        /// <summary>
        /// Reseteo de password para un usuario
        /// </summary>
        /// <param name="userEmail">Email del usuario</param>
        /// <returns>Ok</returns>
        [HttpPost("password/forgot")]
        [TenantFilter]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequestDTO userEmail)
        {
            Tenant tenant = RouteData.Values[TenantFilter.TENANT_KEY] as Tenant;

            usersService.UserForgotPassword(tenant.Id, userEmail.Email);

            return Ok();
        }

        /// <summary>
        /// Reseteo de password para un usuario
        /// </summary>
        /// <param name="recoverPassword">Email del usuario</param>
        /// <returns>Ok</returns>
        [HttpPut("password/forgot")]
        [TenantFilter]
        public IActionResult ForgotPasswordChange([FromBody] RecoverPasswordDTO recoverPassword)
        {
            Tenant tenant = RouteData.Values[TenantFilter.TENANT_KEY] as Tenant;

            var result = usersService.ChangeUserPasswordFromRecover(tenant.Id, recoverPassword.Id, recoverPassword.Password);

            if (!result)
            {
                return Conflict();
            }

            return Ok();
        }

        /// <summary>
        /// Cambia el password del usuario
        /// </summary>
        /// <returns>Información del usuario</returns>
        /// <response code="401">El JWT no es valido.</response>
        /// /// <response code="403">Password actual invalido.</response>
        [HttpGet("me/password")]
        [Authorize]
        public IActionResult UserChangePassword([FromBody] ChangePasswordDTO changePassword)
        {
            var audienceClaim = HttpContext.User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Aud).FirstOrDefault();
            var userClaim = HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault();

            if (audienceClaim == null || userClaim == null)
            {
                return Unauthorized();
            }

            var tenantId = Guid.Parse(audienceClaim.Value);
            var userId = Guid.Parse(userClaim.Value);

            var user = usersService.GetUser(tenantId, userId);

            if (user == null)
            {
                return Unauthorized();
            }

            if (!PasswordUtils.IsValidPassword(user, changePassword.CurrentPassword))
            {
                return Conflict();
            }

            try
            {
                usersService.ChangeUserPassword(tenantId, userId, changePassword.Password);
            }
            catch (Exception e)
            {
                return NotFound();
            }

            return Ok();
        }

        /// <summary>
        /// Obtiene información del usuario
        /// </summary>
        /// <returns>Información del usuario</returns>
        /// <response code="401">El JWT no es valido.</response>
        [HttpGet("me")]
        [Authorize]
        public IActionResult UserInfo()
        {
            if (HttpContext == null)
            {
                return Unauthorized();
            }

            var audience = HttpContext.User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Aud).FirstOrDefault();
            var userId = HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault();

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