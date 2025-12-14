namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class PengunduranDiriListResponse
    {
        public string PdiId { get; set; } = "";
        public string IdAlternative { get; set; } = "";
        public string MhsId { get; set; } = "";
        public string ApproveProdi { get; set; } = "";
        public string ApproveDir1 { get; set; } = "";
        public string Tanggal { get; set; } = "";
        public string? TanggalDisetujui { get; set; }
        public string SuratNo { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
