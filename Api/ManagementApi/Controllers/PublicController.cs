using System.Collections.Generic;
using ManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace ManagementApi.Controllers
{
    [Route("api/[controller]")]
    public class PublicController : Controller
    {
        TenantsService tenantService = new TenantsService();

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
    }
}
