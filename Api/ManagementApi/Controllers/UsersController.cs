using System;
using DataAccess;
using ManagementApi.Filters;
using ManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Linq;

namespace ManagementApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [JWTTenantFilter]
    public class UsersController : Controller
    {
        UsersService usersService = new UsersService();

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
                Id = user.Id
            });
        }

        [HttpPost]
        public IActionResult Post([FromBody]NewUserDTO user)
        {
            Tenant tenant = RouteData.Values[JWTTenantFilter.TENANT_KEY] as Tenant;

            var newUser = usersService.CreateUser(tenant.Id, user.FullName, user.Email, user.Password);

            if (newUser == null)
            {
                return NotFound();
            }

            return Ok(newUser);
        }

        [HttpPut("{userId}")]
        public IActionResult Put(Guid userId, [FromBody]NewUserDTO newUserData)
        {
            Tenant tenant = RouteData.Values[JWTTenantFilter.TENANT_KEY] as Tenant;

            var user = usersService.GetUser(tenant.Id, userId);

            if (user == null)
            {
                return NotFound();
            }

            usersService.UpdateUser(tenant.Id, userId, newUserData.FullName);

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
