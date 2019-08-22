using System;
namespace ManagementApi.Models
{
    public class NewUserDTO
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
    }
}
