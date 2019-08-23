using System;
using System.Collections.Generic;

namespace ManagementApi.Models
{
    public class UserExtendedInfoDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public Guid Id { get; set; }
        public string Metadata { get; set; }
        public string ExtraClaims { get; set; }
        public List<UserEventDTO> Events { get; set; }
    }
}
