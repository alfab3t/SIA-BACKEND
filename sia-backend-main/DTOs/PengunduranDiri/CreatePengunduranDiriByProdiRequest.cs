using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class CreatePengunduranDiriByProdiRequest
    {
        [Required(ErrorMessage = "Mahasiswa harus dipilih.")]
        [StringLength(50)]
        public string MhsId { get; set; } = string.Empty;

        [StringLength(500)]
        public string LampiranSuratPengajuan { get; set; } = string.Empty;

        [StringLength(500)]
        public string Lampiran { get; set; } = string.Empty;

        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
    }
}
