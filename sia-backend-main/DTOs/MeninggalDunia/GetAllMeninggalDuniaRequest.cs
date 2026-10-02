using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class GetAllMeninggalDuniaRequest
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        [StringLength(100)]
        public string SearchKeyword { get; set; } = string.Empty;

        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [StringLength(50)]
        public string Sort { get; set; } = string.Empty;

        [StringLength(50)]
        public string RoleId { get; set; } = string.Empty;

        [StringLength(50)]
        public string UserId { get; set; } = string.Empty;
    }
}
