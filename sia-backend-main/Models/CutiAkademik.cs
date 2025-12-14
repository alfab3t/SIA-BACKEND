namespace astratech_apps_backend.Models
{
    public class CutiAkademik
    {
        public string Id { get; set; } = "";
        public string TahunAjaran { get; set; } = "";
        public string Semester { get; set; } = "";
        public string MhsId { get; set; } = "";
        public string VaCutiAkademik { get; set; } = "";
        public string ApprovalProdi { get; set; } = "";
        public DateTime? AppProdiDate { get; set; }
        public string ApprovalDir1 { get; set; } = "";
        public DateTime? AppDir1Date { get; set; }
        public string ApprovalDakap { get; set; } = "";
        public DateTime? AppDakapDate { get; set; }
        public string Keterangan { get; set; } = "";
        public string StatusPembayaran { get; set; } = "";
        public string StatusCuti { get; set; } = "";
        public string LampiranSuratPengaju { get; set; } = "";
        public string Lampiran { get; set; } = "";
        public string Menimbang { get; set; } = "";
        public string SrtNo { get; set; } = "";
        public string Sk { get; set; } = "";
        public string Status { get; set; } = "";
        public string CreatedBy { get; set; } = "";
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; } = "";
        public DateTime? ModifiedDate { get; set; }
    }
}
