using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace astratech_apps_backend.DTOs.CutiAkademik
{
    public class UploadSKRequest
    {
        [Required(ErrorMessage = "ID cuti akademik harus diisi.")]
        [StringLength(30, ErrorMessage = "ID cuti akademik maksimal 30 karakter.")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "File SK harus diunggah.")]
        public IFormFile FileSK { get; set; } = null!;

        [StringLength(100, ErrorMessage = "Nomor SK maksimal 100 karakter.")]
        public string NomorSK { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "UploadBy maksimal 50 karakter.")]
        public string UploadBy { get; set; } = string.Empty;
    }
}