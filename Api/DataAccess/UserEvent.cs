using System;
using System.Collections.Generic;

namespace DataAccess
{
    public partial class UserEvent
    {
        public Guid Tenantid { get; set; }
        public Guid Userid { get; set; }
        public DateTime When { get; set; }
        public string Event { get; set; }
        public string Info { get; set; }
        public Guid Id { get; set; }

        public virtual User User { get; set; }
    }
}
