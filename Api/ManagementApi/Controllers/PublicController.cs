using DataAccess;
using ManagementApi.Filters;
using ManagementApi.Models;
using ManagementApi.Utils;
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

        /// <summary>
        /// Obtiene el token para poder hacer request desde el backend.
        /// </summary>
        /// <param name="requestToken">Info del tenant para conseguir el token</param>
        /// <returns>JWT token para usar desde el backend</returns>
        [HttpPost("token")]
        public IActionResult GetToken([FromBody] RequestTokenDTO requestToken)
        {
            var tenant = tenantService.GetTenant(requestToken.TenantId, requestToken.ClientSecret);

            if (tenant == null)
            {
                return NotFound();
            }

            return Ok(new {
                token = JWTUtils.CreateJWT(tenant)
            });
        }

        /// <summary>
        /// Obtiene la informacion del tenant para personalizar el login.
        /// </summary>
        /// <returns>Configuración del tenant</returns>
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
