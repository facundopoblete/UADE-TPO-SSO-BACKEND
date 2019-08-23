using DataAccess;
using ManagementApi.Filters;
using ManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Services;

namespace ManagementApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [JWTTenantFilter]
    public class TenantController : Controller
    {
        TenantsService tenantsService = new TenantsService();

        [HttpGet]
        public IActionResult GetSettings()
        {
            Tenant tenant = RouteData.Values[JWTTenantFilter.TENANT_KEY] as Tenant;

            return Ok(new TenantSettingsDTO()
            {
                AllowPublicUsers = tenant.AllowPublicUsers.Value,
                ClientId = tenant.ClientId,
                JwtDuration = tenant.JwtDuration,
                JwtSigningKey = tenant.JwtSigningKey,
                Name = tenant.Name
            });
        }

        [HttpPut]
        public IActionResult UpdateSettings([FromBody] UpdateTenantSettingsDTO settings)
        {
            Tenant tenant = RouteData.Values[JWTTenantFilter.TENANT_KEY] as Tenant;

            tenantsService.UpdateTenantSettings(tenant.Id, settings.Name, settings.JwtSigningKey, settings.JwtDuration, settings.AllowPublicUsers);

            return Ok();
        }
    }


}
