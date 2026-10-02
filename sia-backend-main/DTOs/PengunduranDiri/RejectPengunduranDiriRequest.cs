using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class RejectPengunduranDiriRequest
    {
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Alasan penolakan harus diisi.")]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }
}
