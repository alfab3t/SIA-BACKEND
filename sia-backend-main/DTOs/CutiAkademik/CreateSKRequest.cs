using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.CutiAkademik
{
    /// <summary>
    /// Request untuk membuat SK Cuti Akademik
    /// </summary>
    public class CreateSKRequest
    {
        /// <summary>
        /// ID cuti akademik yang akan dibuatkan SK-nya
        /// </summary>
        [Required(ErrorMessage = "ID cuti akademik harus diisi")]
        public string Id { get; set; } = "";
        
        /// <summary>
        /// Nomor SK (opsional, akan digenerate otomatis jika kosong)
        /// </summary>
        public string? NoSK { get; set; }
        
        /// <summary>
        /// Username admin yang membuat SK
        /// </summary>
        [Required(ErrorMessage = "CreatedBy harus diisi")]
        public string CreatedBy { get; set; } = "";
    }
}
