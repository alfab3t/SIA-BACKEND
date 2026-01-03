using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.CutiAkademik
{
    /// <summary>
    /// Request untuk menolak cuti akademik
    /// </summary>
    public class RejectCutiAkademikRequest
    {
        /// <summary>
        /// ID cuti akademik yang akan ditolak
        /// </summary>
        [Required(ErrorMessage = "ID cuti akademik harus diisi")]
        public string Id { get; set; } = "";
        
        /// <summary>
        /// Role yang menolak: "prodi", "wadir1", "finance"
        /// </summary>
        [Required(ErrorMessage = "Role harus diisi")]
        public string Role { get; set; } = "";
        
        /// <summary>
        /// Username yang menolak
        /// </summary>
        [Required(ErrorMessage = "Username harus diisi")]
        public string Username { get; set; } = "";
        
        /// <summary>
        /// Keterangan/alasan penolakan (WAJIB diisi)
        /// </summary>
        [Required(ErrorMessage = "Keterangan/alasan penolakan harus diisi")]
        [MinLength(5, ErrorMessage = "Keterangan minimal 5 karakter")]
        public string Keterangan { get; set; } = "";
    }
}