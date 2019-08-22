using System;

namespace ManagementApi.Models
{
    public class UserDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public Guid Id { get; set; }
    }
}
