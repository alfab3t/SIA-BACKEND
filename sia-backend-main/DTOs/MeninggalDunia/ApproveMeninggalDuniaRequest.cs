using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class ApproveMeninggalDuniaRequest
    {
        [StringLength(50, ErrorMessage = "Role maksimal 50 karakter.")]
        public string Role { get; set; } = "wadir1";

        [StringLength(50, ErrorMessage = "Username maksimal 50 karakter.")]
        public string Username { get; set; } = string.Empty;
    }
}
