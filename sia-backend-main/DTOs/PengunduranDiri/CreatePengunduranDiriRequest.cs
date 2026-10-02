using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class CreatePengunduranDiriRequest
    {
        [StringLength(50)]
        public string Step { get; set; } = string.Empty;

        [StringLength(50)]
        public string DraftId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mahasiswa harus dipilih.")]
        [StringLength(50)]
        public string MhsId { get; set; } = string.Empty;

        [StringLength(500)]
        public string LampiranSuratPengajuan { get; set; } = string.Empty;

        [StringLength(500)]
        public string Lampiran { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CreatedBy { get; set; }
    }
}
