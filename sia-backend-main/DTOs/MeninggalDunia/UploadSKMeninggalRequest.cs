using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class UploadSKMeninggalRequest
    {
        [Required(ErrorMessage = "ID meninggal dunia harus diisi.")]
        [StringLength(30, ErrorMessage = "ID meninggal dunia maksimal 30 karakter.")]
        public string MduId { get; set; } = string.Empty;

        [Required(ErrorMessage = "File Surat Keputusan (SK) harus diunggah.")]
        public IFormFile? SK { get; set; }

        [Required(ErrorMessage = "File Surat Keterangan Pernah Berkuliah (SKPB) harus diunggah.")]
        public IFormFile? SKPB { get; set; }

        [StringLength(50, ErrorMessage = "ModifiedBy maksimal 50 karakter.")]
        public string ModifiedBy { get; set; } = string.Empty;
    }
}
