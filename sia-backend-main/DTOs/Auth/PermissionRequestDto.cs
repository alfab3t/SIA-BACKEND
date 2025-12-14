using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.Auth
{
    public class PermissionRequestDto
    {
        [Required(ErrorMessage = "Nama akun harus diisi.")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID aplikasi harus diisi.")]
        [StringLength(5)]
        public string AppId { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID role harus diisi.")]
        [StringLength(10)]
        public string RoleId { get; set; } = string.Empty;
    }
}
