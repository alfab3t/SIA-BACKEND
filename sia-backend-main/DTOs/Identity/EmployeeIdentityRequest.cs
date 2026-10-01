using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.Identity
{
    public class EmployeeIdentityRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
    }
}