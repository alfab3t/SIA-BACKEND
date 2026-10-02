using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class UpdateMeninggalDuniaRequest
    {
        [StringLength(30, ErrorMessage = "ID maksimal 30 karakter.")]
        public string Id { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "ID Mahasiswa maksimal 20 karakter.")]
        public string MhsId { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Lampiran maksimal 255 karakter.")]
        public string Lampiran { get; set; } = string.Empty;

        public IFormFile? LampiranFile { get; set; }

        [StringLength(50, ErrorMessage = "ModifiedBy maksimal 50 karakter.")]
        public string ModifiedBy { get; set; } = string.Empty;
    }
}
