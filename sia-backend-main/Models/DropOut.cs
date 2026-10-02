namespace astratech_apps_backend.Models
{
    public class DropOut
    {
        public string Id { get; set; } = string.Empty;
        public long RowNumber { get; set; } = 0;
        public string NoPengajuan { get; set; } = string.Empty;
        public string MhsId { get; set; } = string.Empty;
        public string MhsNama { get; set; } = string.Empty;
        public string MhsAngkatan { get; set; } = string.Empty;
        public string MhsEmail { get; set; } = string.Empty;
        public string KonId { get; set; } = string.Empty;
        public string KonNama { get; set; } = string.Empty;
        public string ProId { get; set; } = string.Empty;
        public string ProNama { get; set; } = string.Empty;
        public string Menimbang { get; set; } = string.Empty;
        public string Mengingat { get; set; } = string.Empty;
        public string Lampiran { get; set; } = string.Empty;
        public string LampiranSuratPengajuan { get; set; } = string.Empty;
        public string Sk { get; set; } = string.Empty;
        public string Skpb { get; set; } = string.Empty;
        public string CatatanWadir1 { get; set; } = string.Empty;
        public string CatatanDirektur { get; set; } = string.Empty;
        public DateTime? TglAccWadir1 { get; set; }
        public DateTime? TglAccDirektur { get; set; }
        public string AlasanTolak { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }
    }
}
