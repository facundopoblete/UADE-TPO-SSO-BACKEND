namespace ManagementApi.Models
{
    public class UpdateTenantSettingsDTO
    {
        public string Name { get; set; }
        public string JwtSigningKey { get; set; }
        public int JwtDuration { get; set; }
        public bool AllowPublicUsers { get; set; }
    }
}
