using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.CutiAkademik
{
    public class RejectCutiAkademikRequest
    {
        [Required(ErrorMessage = "ID cuti akademik harus diisi.")]
        [StringLength(30, ErrorMessage = "ID cuti akademik maksimal 30 karakter.")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role harus diisi.")]
        [StringLength(20, ErrorMessage = "Role maksimal 20 karakter.")]
        public string Role { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Username maksimal 50 karakter.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Alasan penolakan harus diisi.")]
        [StringLength(500, ErrorMessage = "Alasan penolakan maksimal 500 karakter.")]
        public string Keterangan { get; set; } = string.Empty;

        // Alias untuk fleksibilitas frontend jika mengirimkan field 'reason'
        public string Reason
        {
            get => Keterangan;
            set { if (!string.IsNullOrEmpty(value)) Keterangan = value; }
        }
    }
}