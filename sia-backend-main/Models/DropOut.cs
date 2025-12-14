using System;

namespace astratech_apps_backend.Models
{
    public class DropOut
    {
        public string Id { get; set; } = "";
        public string MhsId { get; set; } = "";

        public string Menimbang { get; set; } = "";
        public string Mengingat { get; set; } = "";

        public string ApproveWadir1 { get; set; } = "";
        public DateTime? ApproveWadir1Date { get; set; }

        public string ApproveDir { get; set; } = "";
        public DateTime? ApproveDirDate { get; set; }

        public string SrtNo { get; set; } = "";
        public string SrtKetNo { get; set; } = "";

        public string Sk { get; set; } = "";
        public string Skpb { get; set; } = "";

        public string AlasanTolak { get; set; } = "";
        public string Status { get; set; } = "";

        public string CreatedBy { get; set; } = "";
        public DateTime? CreatedDate { get; set; }

        public string ModifiedBy { get; set; } = "";
        public DateTime? ModifiedDate { get; set; }
    }
}
