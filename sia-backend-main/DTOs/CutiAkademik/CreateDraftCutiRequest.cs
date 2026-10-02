using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace astratech_apps_backend.DTOs.CutiAkademik
{
    public class CreateDraftCutiRequest
    {
        [Required(ErrorMessage = "Mahasiswa harus dipilih.")]
        [StringLength(20, ErrorMessage = "ID Mahasiswa maksimal 20 karakter.")]
        public string MhsId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tahun akademik wajib diisi.")]
        [StringLength(10, ErrorMessage = "Tahun akademik maksimal 10 karakter.")]
        public string TahunAjaran { get; set; } = string.Empty;

        [Required(ErrorMessage = "Semester wajib diisi.")]
        [StringLength(10, ErrorMessage = "Semester maksimal 10 karakter.")]
        public string Semester { get; set; } = string.Empty;

        public IFormFile? LampiranSuratPengajuan { get; set; }

        public IFormFile? Lampiran { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; } = string.Empty;
    }
}