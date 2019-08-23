namespace ManagementApi.Models
{
    public class UpdateUserDTO
    {
        public string FullName { get; set; }
        public string Password { get; set; }
        public dynamic Metadata { get; set; }
        public dynamic ExtraClaims { get; set; }
    }
}
