namespace astratech_apps_backend.Models
{
    public class CutiAkademik
    {
        public string Id { get; set; } = string.Empty;
        public long RowNumber { get; set; } = 0;
        public string TahunAjaran { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string MhsId { get; set; } = string.Empty;
        public string MhsNama { get; set; } = string.Empty;
        public int KonId { get; set; }
        public string KonNama { get; set; } = string.Empty;
        public string VaCutiAkademik { get; set; } = string.Empty;
        public string ApprovalProdi { get; set; } = string.Empty;
        public DateTime? AppProdiDate { get; set; }
        public string ApprovalDir1 { get; set; } = string.Empty;
        public DateTime? AppDir1Date { get; set; }
        public string ApprovalDakap { get; set; } = string.Empty;
        public DateTime? AppDakapDate { get; set; }
        public string Keterangan { get; set; } = string.Empty;
        public string StatusPembayaran { get; set; } = string.Empty;
        public string StatusCuti { get; set; } = string.Empty;
        public string LampiranSuratPengajuan { get; set; } = string.Empty;
        public string Lampiran { get; set; } = string.Empty;
        public string Menimbang { get; set; } = string.Empty;
        public string SrtNo { get; set; } = string.Empty;
        public string Sk { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }
    }
}
