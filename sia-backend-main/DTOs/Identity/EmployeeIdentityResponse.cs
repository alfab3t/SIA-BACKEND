namespace astratech_apps_backend.DTOs.Identity
{
    public class EmployeeIdentityResponse
    {
        public string KryUsername { get; set; } = string.Empty;
        public string JabMainId { get; set; } = string.Empty;
        public string StrMainId { get; set; } = string.Empty;
        public string RolId { get; set; } = string.Empty;
        public string KryId { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}