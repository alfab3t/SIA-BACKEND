using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace astratech_apps_backend.DTOs.DropOut
{
    public class UploadSKFileRequest
    {
        [Required(ErrorMessage = "ID Drop Out harus diisi.")]
        [StringLength(50)]
        public string DroId { get; set; } = string.Empty;

        [Required(ErrorMessage = "File SK harus diupload.")]
        public IFormFile SkFile { get; set; } = null!;

        public IFormFile? SkpbFile { get; set; }
    }
}
