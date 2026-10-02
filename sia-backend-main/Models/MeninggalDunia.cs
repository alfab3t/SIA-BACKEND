namespace astratech_apps_backend.Models
{
    public class MeninggalDunia
    {
        public string Id { get; set; } = string.Empty;
        public long RowNumber { get; set; } = 0;
        public string MhsId { get; set; } = string.Empty;
        public string MhsNama { get; set; } = string.Empty;
        public string KonNama { get; set; } = string.Empty;
        public string ProNama { get; set; } = string.Empty;
        public string Lampiran { get; set; } = string.Empty;
        public string ApproveDir1By { get; set; } = string.Empty;
        public DateTime? ApproveDir1Date { get; set; }
        public string SrtNo { get; set; } = string.Empty;
        public string NoSpkb { get; set; } = string.Empty;
        public string Sk { get; set; } = string.Empty;
        public string Spkb { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }
    }
}
