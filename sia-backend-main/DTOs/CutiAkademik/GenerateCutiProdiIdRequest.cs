using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.CutiAkademik
{
    public class GenerateCutiProdiIdRequest
    {
        [Required(ErrorMessage = "Draft ID harus diisi.")]
        [StringLength(30, ErrorMessage = "Draft ID maksimal 30 karakter.")]
        public string DraftId { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "ModifiedBy maksimal 50 karakter.")]
        public string ModifiedBy { get; set; } = string.Empty;
    }
}