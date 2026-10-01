using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.CutiAkademik
{
    /// <summary>
    /// Request untuk menyetujui cuti akademik oleh prodi
    /// </summary>
    public class ApproveProdiCutiRequest
    {
        /// <summary>
        /// ID cuti akademik yang akan disetujui
        /// </summary>
        [Required(ErrorMessage = "ID cuti akademik harus diisi")]
        public string Id { get; set; } = "";
        
        /// <summary>
        /// Pertimbangan/alasan persetujuan (WAJIB diisi)
        /// </summary>
        [Required(ErrorMessage = "Menimbang/pertimbangan harus diisi")]
        [MinLength(10, ErrorMessage = "Menimbang minimal 10 karakter")]
        public string Menimbang { get; set; } = "";
        
        /// <summary>
        /// Username prodi yang menyetujui
        /// </summary>
        [Required(ErrorMessage = "ApprovedBy harus diisi")]
        public string ApprovedBy { get; set; } = "";
    }
}