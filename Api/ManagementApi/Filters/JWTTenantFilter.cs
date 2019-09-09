using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ManagementApi.Filters
{
    public class JWTTenantFilter : ActionFilterAttribute
    {
        public const string TENANT_KEY = "TENANT";
        public const string USER_KEY = "USER";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var audience = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Aud);
            var userId = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            if (audience == null)
            {
                context.Result = new StatusCodeResult(404);
                base.OnActionExecuting(context);
            }

            Guid tenantId = Guid.Empty;
            Guid.TryParse(audience.Value, out tenantId);

            ITenantService tenantsService = context.HttpContext.RequestServices.GetService<ITenantService>();

            var tenant = tenantsService.GetTenant(tenantId);

            context.RouteData.Values.Add(TENANT_KEY, tenantId);

            if (userId != null)
            {
                context.RouteData.Values.Add(USER_KEY, Guid.Parse(userId.Value));
            }

            base.OnActionExecuting(context);
        }
    }
}