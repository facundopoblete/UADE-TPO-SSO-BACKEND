using System;
using System.Collections.Generic;

namespace DataAccess
{
    public partial class Tenant
    {
        public Tenant()
        {
            User = new HashSet<User>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? AdminId { get; set; }
        public string ClientId { get; set; }
        public string ManagementToken { get; set; }
        public string JwtSigningKey { get; set; }
        public int JwtDuration { get; set; }
        public string ClientSecret { get; set; }

        public virtual ICollection<User> User { get; set; }
    }
}
