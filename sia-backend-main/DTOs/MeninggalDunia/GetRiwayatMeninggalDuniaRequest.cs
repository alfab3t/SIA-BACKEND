namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class GetRiwayatMeninggalDuniaRequest
    {
        public string Keyword { get; set; } = "";
        public string Sort { get; set; } = "mdu_created_date desc";
        public string Konsentrasi { get; set; } = "";
        public string RoleId { get; set; } = "";

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
