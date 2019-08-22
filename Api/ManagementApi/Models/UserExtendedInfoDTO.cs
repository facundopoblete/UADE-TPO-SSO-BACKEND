using System;
using System.Collections.Generic;

namespace ManagementApi.Models
{
    public class UserExtendedInfoDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public Guid Id { get; set; }

        public List<UserEventDTO> Events { get; set; }
    }
}
