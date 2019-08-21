using System;
using DataAccess;
using ManagementApi.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace ManagementApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [TenantFilter]
    public class UsersController : Controller
    {
        UsersService usersService = new UsersService();

        [HttpGet]
        public IActionResult Get()
        {
            Tenant tenant = RouteData.Values[TenantFilter.TENANT_KEY] as Tenant;

            return Ok(usersService.GetUsers(tenant.Id));
        }

        [HttpGet("{userId}")]
        public IActionResult Get(Guid userId)
        {
            Tenant tenant = RouteData.Values[TenantFilter.TENANT_KEY] as Tenant;

            var user = usersService.GetUser(tenant.Id, userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost]
        public IActionResult Post([FromBody]User user)
        {
            Tenant tenant = RouteData.Values[TenantFilter.TENANT_KEY] as Tenant;

            // TODO: add password
            var newUser = usersService.CreateUser(tenant.Id, user.FullName, user.Email, "");

            if (newUser == null)
            {
                return NotFound();
            }

            return Ok(newUser);
        }

        [HttpPut("{userId}")]
        public IActionResult Put(Guid userId, [FromBody]User newUserData)
        {
            Tenant tenant = RouteData.Values[TenantFilter.TENANT_KEY] as Tenant;

            var user = usersService.GetUser(tenant.Id, userId);

            if (user == null)
            {
                return NotFound();
            }

            usersService.UpdateUser(tenant.Id, newUserData.Id, newUserData.FullName);

            return Ok();
        }

        [HttpDelete("{userId}")]
        public IActionResult Delete(Guid userId)
        {
            Tenant tenant = RouteData.Values[TenantFilter.TENANT_KEY] as Tenant;

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