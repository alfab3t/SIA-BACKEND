using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.DropOut
{
    public class ApproveDropOutRequest
    {
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Catatan { get; set; } = string.Empty;
    }
}
