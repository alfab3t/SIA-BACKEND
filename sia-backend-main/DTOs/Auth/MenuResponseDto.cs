using astratech_apps_backend.Models;

namespace astratech_apps_backend.DTOs.Auth
{
    public class MenuResponseDto
    {
        public List<Menu> ListMenu { get; set; } = [];
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
