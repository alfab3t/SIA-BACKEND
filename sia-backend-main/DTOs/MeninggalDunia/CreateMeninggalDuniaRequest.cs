using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class CreateMeninggalDuniaRequest
    {
        [Required(ErrorMessage = "Mahasiswa harus dipilih.")]
        [StringLength(20, ErrorMessage = "ID Mahasiswa maksimal 20 karakter.")]
        public string MhsId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lampiran berkas surat kematian harus diunggah.")]
        public IFormFile? LampiranFile { get; set; }

        [StringLength(50, ErrorMessage = "CreatedBy maksimal 50 karakter.")]
        public string CreatedBy { get; set; } = string.Empty;
    }
}
