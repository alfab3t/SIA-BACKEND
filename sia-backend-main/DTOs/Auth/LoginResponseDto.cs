using astratech_apps_backend.Models;

namespace astratech_apps_backend.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public List<Aplikasi> ListAplikasi { get; set; } = [];
        public DateTime ExpiresAt { get; set; } = default!;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
