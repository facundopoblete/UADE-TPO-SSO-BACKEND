using DataAccess;
using ManagementApi.Filters;
using ManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace ManagementApi.Controllers
{
    [Route("api/[controller]")]
    public class PublicController : Controller
    {
        ITenantService tenantService;

        public PublicController(ITenantService tenantService)
        {
            this.tenantService = tenantService;
        }

        [HttpPost]
        public IActionResult GetToken([FromBody] RequestTokenDTO requestToken)
        {
            var tenant = tenantService.GetTenant(requestToken.ClientId, requestToken.ClientSecret);

            if (tenant == null)
            {
                return NotFound();
            }

            return Ok();
        }

        [TenantFilter]
        [HttpGet("loginSettings")]
        public IActionResult GetTenantCustomLogin()
        {
            Tenant tenant = RouteData.Values[JWTTenantFilter.TENANT_KEY] as Tenant;

            if (tenant == null)
            {
                return NotFound();
            }

            return Ok(new TenantCustomLogin()
            {
                AllowPublicUsers = tenant.AllowPublicUsers.Value,
                LoginText = tenant.Name
            });
        }
    }
}
