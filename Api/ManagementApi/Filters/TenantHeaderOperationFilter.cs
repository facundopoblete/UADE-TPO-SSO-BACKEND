using System.Collections.Generic;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ManagementApi.Filters
{
    public class TenantHeaderOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var isTenantFilter = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is TenantFilter);

            if (isTenantFilter)
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<IParameter>();

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = TenantFilter.TENANT_HEADER,
                    In = "header",
                    Description = "Tenant Id",
                    Required = true,
                    Type = "string"
                });
            }
        }
    }
}
