using System;
namespace UsersApi.Models
{
    public class LoginMachineDTO
    {
        public Guid Id { get; set; }
        
        public string Secret { get; set; }
    }
}
