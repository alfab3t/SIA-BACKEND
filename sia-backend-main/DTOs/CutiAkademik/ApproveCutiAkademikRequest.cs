using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.CutiAkademik
{
    public class ApproveCutiAkademikRequest
    {
        [Required(ErrorMessage = "ID cuti akademik harus diisi.")]
        [StringLength(30, ErrorMessage = "ID cuti akademik maksimal 30 karakter.")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role harus diisi.")]
        [StringLength(20, ErrorMessage = "Role maksimal 20 karakter.")]
        public string Role { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "ApprovedBy maksimal 50 karakter.")]
        public string ApprovedBy { get; set; } = string.Empty;
    }
}