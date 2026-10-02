using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.DropOut
{
    public class GetAllDropOutRequest
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        [StringLength(100)]
        public string SearchKeyword { get; set; } = string.Empty;

        [StringLength(50)]
        public string Konsentrasi { get; set; } = string.Empty;

        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [StringLength(50)]
        public string Sort { get; set; } = string.Empty;

        [StringLength(50)]
        public string RoleId { get; set; } = string.Empty;

        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;
    }
}
