namespace TaskManagementWeb.Api.Models
{
    public class AppJwtSettings
    {
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public required string SecretKey { get; set; }
    }
}
