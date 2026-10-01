using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.CutiAkademik
{
    /// <summary>
    /// Request untuk upload SK Cuti Akademik
    /// </summary>
    public class UploadSKRequest
    {
        /// <summary>
        /// ID cuti akademik yang akan diupload SK-nya
        /// </summary>
        [Required(ErrorMessage = "ID cuti akademik harus diisi")]
        public string Id { get; set; } = "";
        
        /// <summary>
        /// File SK yang akan diupload
        /// </summary>
        [Required(ErrorMessage = "File SK harus diupload")]
        public IFormFile FileSK { get; set; } = null!;
        
        /// <summary>
        /// Username admin yang mengupload
        /// </summary>
        [Required(ErrorMessage = "UploadBy harus diisi")]
        public string UploadBy { get; set; } = "";
    }
}