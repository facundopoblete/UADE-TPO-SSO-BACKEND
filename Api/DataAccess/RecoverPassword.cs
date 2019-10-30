using System;
using System.Collections.Generic;

namespace DataAccess
{
    public partial class RecoverPassword
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public bool? IsValid { get; set; }

        public virtual Tenant Tenant { get; set; }
        public virtual User User { get; set; }
    }
}
