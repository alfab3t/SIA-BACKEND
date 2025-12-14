namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class MeninggalDuniaListResponse
    {
        public string Id { get; set; } = "";
        public string IdAlternative { get; set; } = "";
        public string MhsId { get; set; } = "";
        public string ApproveDir1By { get; set; } = "";
        public string CreatedDate { get; set; } = ""; // hasil CONVERT(VARCHAR(11),...)
        public DateTime TanggalBuat { get; set; }
        public string SuratNo { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
