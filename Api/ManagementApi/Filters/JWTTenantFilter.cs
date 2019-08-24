using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Services;
using System.Linq;

namespace ManagementApi.Filters
{
    public class JWTTenantFilter : ActionFilterAttribute
    {
        public const string TENANT_KEY = "tenant";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId");

            if (userId == null)
            {
                context.Result = new StatusCodeResult(404);
                base.OnActionExecuting(context);
            }

            TenantsService tenantsService = new TenantsService();

            Guid tenantId = Guid.Empty;
            Guid.TryParse(userId.Value, out tenantId);

            var tenant = tenantsService.GetTenantFromAdmin(tenantId);

            context.RouteData.Values.Add(TENANT_KEY, tenant);

            base.OnActionExecuting(context);
        }
    }
}