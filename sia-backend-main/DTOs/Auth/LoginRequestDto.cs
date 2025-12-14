using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Nama akun harus diisi.")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kata sandi harus diisi.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jenis aplikasi harus diisi.")]
        public string JenisAplikasi { get; set; } = string.Empty;
    }
}
