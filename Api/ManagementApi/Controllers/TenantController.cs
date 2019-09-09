using System;
using DataAccess;
using ManagementApi.Filters;
using ManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Services.Interface;

namespace ManagementApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [JWTTenantFilter]
    public class TenantController : Controller
    {
        ITenantService tenantsService;

        public TenantController(ITenantService tenantService)
        {
            this.tenantsService = tenantService;
        }

        /// <summary>
        /// Obtiene las settings de un tenant
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Las settings del tenant</response>
        [HttpGet]
        public IActionResult GetSettings()
        {
            Guid tenantId = (Guid)RouteData.Values[JWTTenantFilter.TENANT_KEY];

            var tenant = tenantsService.GetTenant(tenantId);

            if (tenant == null)
            {
                return NotFound();
            }

            return Ok(new TenantSettingsDTO()
            {
                AllowPublicUsers = tenant.AllowPublicUsers.Value,
                ClientId = tenant.ClientId,
                JwtDuration = tenant.JwtDuration,
                JwtSigningKey = tenant.JwtSigningKey,
                Name = tenant.Name,
                Id = tenant.Id
            });
        }

        /// <summary>
        /// Modifica las settings de un tenant
        /// </summary>
        /// <param name="settings">Las nuevas settings para aplicarle al tenant</param>
        /// <returns></returns>
        /// <response code="200">Si se pudo modificar el tenant correctamente</response>
        [HttpPut]
        public IActionResult UpdateSettings([FromBody] UpdateTenantSettingsDTO settings)
        {
            Tenant tenant = RouteData.Values[JWTTenantFilter.TENANT_KEY] as Tenant;

            tenantsService.UpdateTenantSettings(tenant.Id, settings.Name, settings.JwtSigningKey, settings.JwtDuration, settings.AllowPublicUsers);

            return Ok();
        }

        /// <summary>
        /// Crea un nuevo tenant para el admin
        /// </summary>
        /// <param name="newTenant"></param>
        /// <returns></returns>
        /// <response code="200">Si se pudo crear el tenant correctamente</response>
        /// <response code="409">Si no se pudo encontrar el usuario o tenant</response>
        [HttpPost]
        public IActionResult CreateTenant([FromBody] NewTenantDTO newTenant)
        {
            if (RouteData.Values[JWTTenantFilter.USER_KEY] == null)
            {
                return Conflict();
            }

            Guid userId = (Guid)RouteData.Values[JWTTenantFilter.USER_KEY];

            var tenant = tenantsService.GetTenantFromAdmin(userId);

            if (tenant != null)
            {
                return Conflict();
            }

            tenantsService.CreateTenant(newTenant.Name, userId);

            return Ok();
        }
    }
}
