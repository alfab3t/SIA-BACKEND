namespace astratech_apps_backend.Models
{
    public class PengunduranDiri
    {
        public string Id { get; set; } = string.Empty;
        public long RowNumber { get; set; } = 0;
        public string MhsId { get; set; } = string.Empty;
        public string MhsNama { get; set; } = string.Empty;
        public string MhsAngkatan { get; set; } = string.Empty;
        public string KonNama { get; set; } = string.Empty;
        public string ProNama { get; set; } = string.Empty;
        public string Konsentrasi { get; set; } = string.Empty;
        public string LampiranSuratPengajuan { get; set; } = string.Empty;
        public string Lampiran { get; set; } = string.Empty;
        public string ApprovalProdiBy { get; set; } = string.Empty;
        public DateTime? AppProdiDate { get; set; }
        public string ApprovalDir1By { get; set; } = string.Empty;
        public DateTime? AppDir1Date { get; set; }
        public string Keterangan { get; set; } = string.Empty;
        public string AlasanTolak { get; set; } = string.Empty;
        public string SrtNo { get; set; } = string.Empty;
        public string NoSkpb { get; set; } = string.Empty;
        public string Sk { get; set; } = string.Empty;
        public string Skpb { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }
    }
}
