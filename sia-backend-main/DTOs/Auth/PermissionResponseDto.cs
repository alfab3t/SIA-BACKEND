namespace astratech_apps_backend.DTOs.Auth
{
    public class PermissionResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public List<string> ListPermission { get; set; } = [];
        public DateTime ExpiresAt { get; set; } = default!;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
