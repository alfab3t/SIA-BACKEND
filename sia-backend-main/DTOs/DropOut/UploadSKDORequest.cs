using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.DropOut
{
    public class UploadSKDORequest
    {
        [Required(ErrorMessage = "ID Drop Out harus diisi.")]
        [StringLength(50)]
        public string DroId { get; set; } = string.Empty;

        [Required(ErrorMessage = "File SK harus diisi.")]
        [StringLength(500)]
        public string SK { get; set; } = string.Empty;

        [StringLength(500)]
        public string SKPB { get; set; } = string.Empty;

        [StringLength(100)]
        public string ModifiedBy { get; set; } = string.Empty;
    }
}
