namespace astratech_apps_backend.DTOs.DropOut
{
    public class DropOutResponse
    {
        public string Id { get; set; } = string.Empty;
        public long RowNumber { get; set; } = 0;
        public string MhsId { get; set; } = string.Empty;
        public string Menimbang { get; set; } = string.Empty;
        public string Mengingat { get; set; } = string.Empty;
        public string ApproveWadir1 { get; set; } = string.Empty;
        public DateTime? ApproveWadir1Date { get; set; }
        public string ApproveDir { get; set; } = string.Empty;
        public DateTime? ApproveDirDate { get; set; }
        public string SrtNo { get; set; } = string.Empty;
        public string SrtKetNo { get; set; } = string.Empty;
        public string Sk { get; set; } = string.Empty;
        public string Skpb { get; set; } = string.Empty;
        public string AlasanTolak { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }
    }
}
