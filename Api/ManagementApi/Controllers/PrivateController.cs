using DataAccess;
using ManagementApi.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementApi.Controllers
{
    [Route("api/[controller]")]
    public class PrivateController : Controller
    {
        [HttpGet("existsTenant")]
        [Authorize]
        [JWTTenantFilter]
        public IActionResult ExistTenant()
        {
            Tenant tenant = RouteData.Values[JWTTenantFilter.TENANT_KEY] as Tenant;

            if (tenant == null)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
