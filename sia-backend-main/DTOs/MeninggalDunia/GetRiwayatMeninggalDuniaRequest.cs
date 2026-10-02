using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class GetRiwayatMeninggalDuniaRequest
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        [StringLength(100)]
        public string Keyword { get; set; } = string.Empty;

        [StringLength(50)]
        public string Sort { get; set; } = "mdu_created_date desc";

        [StringLength(50)]
        public string Konsentrasi { get; set; } = string.Empty;

        [StringLength(50)]
        public string RoleId { get; set; } = string.Empty;
    }
}
