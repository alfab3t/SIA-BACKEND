using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.DropOut
{
    public class RejectDropOutRequest
    {
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Alasan penolakan harus diisi.")]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }
}
