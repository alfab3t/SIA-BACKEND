using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace astratech_apps_backend.DTOs.CutiAkademik
{
    public class UpdateCutiAkademikRequest
    {
        [Required(ErrorMessage = "ID Cuti Akademik harus diisi.")]
        [StringLength(30, ErrorMessage = "ID Cuti Akademik maksimal 30 karakter.")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tahun ajaran harus diisi.")]
        [StringLength(20, ErrorMessage = "Tahun ajaran maksimal 20 karakter.")]
        public string TahunAjaran { get; set; } = string.Empty;

        [Required(ErrorMessage = "Semester harus diisi.")]
        [StringLength(10, ErrorMessage = "Semester maksimal 10 karakter.")]
        public string Semester { get; set; } = string.Empty;

        public IFormFile? LampiranSuratPengajuan { get; set; }

        public IFormFile? Lampiran { get; set; }

        [StringLength(50, ErrorMessage = "ModifiedBy maksimal 50 karakter.")]
        public string ModifiedBy { get; set; } = string.Empty;
    }
}
