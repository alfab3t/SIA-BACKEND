using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class UploadSKPengunduranDiriRequest
    {
        [Required(ErrorMessage = "File SK harus diisi.")]
        [StringLength(500)]
        public string Sk { get; set; } = string.Empty;

        [StringLength(500)]
        public string Skpb { get; set; } = string.Empty;
    }
}
