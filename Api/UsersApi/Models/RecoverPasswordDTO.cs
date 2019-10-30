using System;
namespace UsersApi.Models
{
    public class RecoverPasswordDTO
    {
        public Guid Id { get; set; }

        public string Password { get; set; }
    }
}
