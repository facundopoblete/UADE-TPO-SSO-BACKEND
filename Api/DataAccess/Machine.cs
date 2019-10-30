using System;
using System.Collections.Generic;

namespace DataAccess
{
    public partial class Machine
    {
        public Guid Id { get; set; }
        public Guid? TenantId { get; set; }
        public string Secret { get; set; }
        public string Name { get; set; }

        public virtual Tenant Tenant { get; set; }
    }
}
