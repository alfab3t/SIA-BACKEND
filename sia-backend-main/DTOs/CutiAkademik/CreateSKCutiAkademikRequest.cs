using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.CutiAkademik
{
    /// <summary>
    /// Request untuk membuat SK Cuti Akademik menggunakan stored procedure sia_createSKCutiAkademik
    /// </summary>
    public class CreateSKCutiAkademikRequest
    {
        /// <summary>
        /// ID cuti akademik yang akan dibuatkan SK-nya (parameter @p1)
        /// </summary>
        [Required(ErrorMessage = "ID cuti akademik harus diisi")]
        public string Id { get; set; } = "";
        
        /// <summary>
        /// Nomor SK atau filename SK (parameter @p2)
        /// </summary>
        [Required(ErrorMessage = "Nomor SK atau filename harus diisi")]
        public string SkNumber { get; set; } = "";
        
        /// <summary>
        /// Username admin yang membuat SK (parameter @p3)
        /// </summary>
        [Required(ErrorMessage = "ModifiedBy harus diisi")]
        public string ModifiedBy { get; set; } = "";
    }
}