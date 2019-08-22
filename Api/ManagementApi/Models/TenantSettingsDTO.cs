namespace ManagementApi.Models
{
    public class TenantSettingsDTO
    {
        public string Name { get; set; }
        public string JwtSigningKey { get; set; }
        public int JwtDuration { get; set; }
        public bool AllowPublicUsers { get; set; }
        public string ClientId { get; set; }
    }
}
