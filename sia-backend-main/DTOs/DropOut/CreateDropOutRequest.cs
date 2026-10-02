using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.DropOut
{
    public class CreateDropOutRequest
    {
        [Required(ErrorMessage = "Mahasiswa harus dipilih.")]
        [StringLength(50)]
        public string MhsId { get; set; } = string.Empty;

        [StringLength(4000)]
        public string Menimbang { get; set; } = string.Empty;

        [StringLength(4000)]
        public string Mengingat { get; set; } = string.Empty;

        [StringLength(500)]
        public string Lampiran { get; set; } = string.Empty;

        [StringLength(500)]
        public string LampiranSuratPengajuan { get; set; } = string.Empty;
    }
}
