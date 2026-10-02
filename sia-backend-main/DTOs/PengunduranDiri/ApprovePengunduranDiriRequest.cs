using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class ApprovePengunduranDiriRequest
    {
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;

        [StringLength(100)]
        public string ApprovedBy { get; set; } = string.Empty;
    }
}
