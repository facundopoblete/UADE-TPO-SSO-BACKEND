using System;
using DataAccess;
using ManagementApi.Filters;
using ManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Services.Interface;

namespace ManagementApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [JWTTenantFilter]
    public class UsersController : Controller
    {
        public IUserService usersService;

        public UsersController(IUserService usersService)
        {
            this.usersService = usersService;
        }

        /// <summary>
        /// Obtiene todos los usuarios del tenant.
        /// </summary>
        /// <returns></returns>
        /// <response code="401">El JWT no es valido.</response>
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            Tenant tenant = RouteData.Values[JWTTenantFilter.TENANT_KEY] as Tenant;

            var users = usersService.GetUsers(tenant.Id);

            return Ok(users.Select(x => new UserDTO
            {
                Email = x.Email,
                FullName = x.FullName,
                Id = x.Id
            }));
        }

        /// <summary>
        /// Obtiene la información de un usuario
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <returns></returns>
        /// <response code="401">El JWT no es valido.</response>
        [HttpGet("{userId}")]
        public IActionResult GetUser(Guid userId)
        {
            Tenant tenant = RouteData.Values[JWTTenantFilter.TENANT_KEY] as Tenant;

            var user = usersService.GetUser(tenant.Id, userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new UserExtendedInfoDTO()
            {
                Email = user.Email,
                Events = user.UserEvent.OrderByDescending(x => x.When).Take(5).Select(x => new UserEventDTO()
                {
                    When = x.When,
                    Event = x.Event
                }).ToList(),
                FullName = user.FullName,
                Id = user.Id,
                ExtraClaims = user.ExtraClaims,
                Metadata = user.Metadata
            });
        }

        /// <summary>
        /// Crea un nuevo usuario en el tenant.
        /// </summary>
        /// <param name="user">Información del nuevo usuario</param>
        /// <returns>El nuevo usuario</returns>
        /// <response code="401">El JWT no es valido.</response>
        [HttpPost]
        public IActionResult CreateUser([FromBody]NewUserDTO user)
        {
            Tenant tenant = RouteData.Values[JWTTenantFilter.TENANT_KEY] as Tenant;

            var existUser = usersService.GetUser(tenant.Id, user.Email);

            if (existUser != null)
            {
                return Conflict();
            }

            var newUser = usersService.CreateUser(tenant.Id, user.FullName, user.Email, user.Password);

            return Ok(new UserDTO
            {
                Email = newUser.Email,
                FullName = newUser.FullName,
                Id = newUser.Id
            });
        }

        /// <summary>
        /// Modifica un usuario
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="newUserData">Información nueva del usuario</param>
        /// <returns></returns>
        /// <response code="401">El JWT no es valido.</response>
        [HttpPut("{userId}")]
        public IActionResult UpdateUser(Guid userId, [FromBody]UpdateUserDTO newUserData)
        {
            Tenant tenant = RouteData.Values[JWTTenantFilter.TENANT_KEY] as Tenant;

            var user = usersService.GetUser(tenant.Id, userId);

            if (user == null)
            {
                return NotFound();
            }

            usersService.UpdateUser(tenant.Id, userId, newUserData.FullName, newUserData.Password, newUserData.ExtraClaims?.ToString(), newUserData.Metadata?.ToString());

            return Ok();
        }

        /// <summary>
        /// Elimina un usuario del tenant
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <returns></returns>
        /// <response code="401">El JWT no es valido.</response>
        [HttpDelete("{userId}")]
        public IActionResult DeleteUser(Guid userId)
        {
            Tenant tenant = RouteData.Values[JWTTenantFilter.TENANT_KEY] as Tenant;

            usersService.DeleteUser(tenant.Id, userId);

            return Ok();
        }
    }
}