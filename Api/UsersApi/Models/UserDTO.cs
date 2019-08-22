using System;
namespace UsersApi.Models
{
    public class UserDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public Guid Id { get; set; }
        public dynamic Metadata { get; set; }
        public dynamic ExtraClaim { get; set; }
    }
}
