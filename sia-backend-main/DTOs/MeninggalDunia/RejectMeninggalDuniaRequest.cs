using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class RejectMeninggalDuniaRequest
    {
        [StringLength(50, ErrorMessage = "Role maksimal 50 karakter.")]
        public string Role { get; set; } = "Wadir 1";

        [StringLength(50, ErrorMessage = "Username maksimal 50 karakter.")]
        public string Username { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Alasan penolakan maksimal 500 karakter.")]
        public string Keterangan { get; set; } = string.Empty;
    }
}
