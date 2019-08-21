using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Services;

namespace UsersApi.Filters
{
    public class TenantFilter : ActionFilterAttribute
    {
        public const string TENANT_HEADER = "TENANT-ID";
        public const string TENANT_KEY = "tenant";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var tenantHeader = context.HttpContext?.Request?.Headers?[TENANT_HEADER].ToString();
            Guid tenantGuid = Guid.Empty;
            Guid.TryParse(tenantHeader, out tenantGuid);

            if (tenantGuid == Guid.Empty)
            {
                context.Result = new StatusCodeResult(404);
                base.OnActionExecuting(context);
            }


            TenantsService tenantsService = new TenantsService();
            var tenant = tenantsService.GetTenant(tenantGuid);

            if (tenant == null)
            {
                context.Result = new StatusCodeResult(404);
                base.OnActionExecuting(context);
            }

            context.RouteData.Values.Add(TENANT_KEY, tenant);
        }
    }
}
