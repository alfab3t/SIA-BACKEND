using System;

namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class PengunduranDiriResponse
    {
        public string Id { get; set; } = "";
        public string MhsId { get; set; } = "";

        public string LampiranSuratPengajuan { get; set; } = "";
        public string Lampiran { get; set; } = "";

        public string ApprovalProdiBy { get; set; } = "";
        public DateTime? AppProdiDate { get; set; }

        public string ApprovalDir1By { get; set; } = "";
        public DateTime? AppDir1Date { get; set; }

        public string Keterangan { get; set; } = "";
        public string SrtNo { get; set; } = "";
        public string NoSkpb { get; set; } = "";
        public string Sk { get; set; } = "";
        public string Skpb { get; set; } = "";

        public string Status { get; set; } = "";

        public string CreatedBy { get; set; } = "";
        public DateTime? CreatedDate { get; set; }

        public string ModifiedBy { get; set; } = "";
        public DateTime? ModifiedDate { get; set; }
    }
}
