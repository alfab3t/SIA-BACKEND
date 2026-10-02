using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace astratech_apps_backend.DTOs.CutiAkademik
{
    public class CreateCutiProdiRequest
    {
        [Required(ErrorMessage = "Tahun ajaran harus diisi.")]
        [StringLength(20, ErrorMessage = "Tahun ajaran maksimal 20 karakter.")]
        public string TahunAjaran { get; set; } = string.Empty;

        [Required(ErrorMessage = "Semester harus diisi.")]
        [StringLength(10, ErrorMessage = "Semester maksimal 10 karakter.")]
        public string Semester { get; set; } = string.Empty;

        public IFormFile? LampiranSuratPengajuan { get; set; }

        public IFormFile? Lampiran { get; set; }

        [Required(ErrorMessage = "Mahasiswa harus dipilih.")]
        [StringLength(20, ErrorMessage = "ID Mahasiswa maksimal 20 karakter.")]
        public string MhsId { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Menimbang maksimal 500 karakter.")]
        public string Menimbang { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Approval Prodi maksimal 50 karakter.")]
        public string ApprovalProdi { get; set; } = string.Empty;
    }
}
