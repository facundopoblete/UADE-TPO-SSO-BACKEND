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

        [HttpGet]
        public IActionResult Get()
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

        [HttpGet("{userId}")]
        public IActionResult Get(Guid userId)
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

        [HttpPost]
        public IActionResult Post([FromBody]NewUserDTO user)
        {
            Tenant tenant = RouteData.Values[JWTTenantFilter.TENANT_KEY] as Tenant;

            var existUser = usersService.GetUser(tenant.Id, user.Email);

            if (existUser != null)
            {
                return Conflict();
            }

            var newUser = usersService.CreateUser(tenant.Id, user.FullName, user.Email, user.Password);

            return Ok(newUser);
        }

        [HttpPut("{userId}")]
        public IActionResult Put(Guid userId, [FromBody]UpdateUserDTO newUserData)
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

        [HttpDelete("{userId}")]
        public IActionResult Delete(Guid userId)
        {
            Tenant tenant = RouteData.Values[JWTTenantFilter.TENANT_KEY] as Tenant;

            var user = usersService.GetUser(tenant.Id, userId);

            if (user == null)
            {
                return NotFound();
            }

            usersService.DeleteUser(tenant.Id, userId);

            return Ok();
        }
    }
}