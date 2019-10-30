using System;
using System.Collections.Generic;

namespace DataAccess
{
    public partial class User
    {
        public User()
        {
            RecoverPassword = new HashSet<RecoverPassword>();
            UserEvent = new HashSet<UserEvent>();
        }

        public Guid TenantId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string Metadata { get; set; }
        public Guid Id { get; set; }
        public string ExtraClaims { get; set; }

        public virtual Tenant Tenant { get; set; }
        public virtual ICollection<RecoverPassword> RecoverPassword { get; set; }
        public virtual ICollection<UserEvent> UserEvent { get; set; }
    }
}
