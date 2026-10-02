using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class UpdatePengunduranDiriRequest
    {
        [StringLength(500)]
        public string LampiranSuratPengajuan { get; set; } = string.Empty;

        [StringLength(500)]
        public string Lampiran { get; set; } = string.Empty;
    }
}
