using System;

namespace astratech_apps_backend.Models
{
    public class MeninggalDunia
    {
        public string Id { get; set; } = "";
        public string MhsId { get; set; } = "";
        public string Lampiran { get; set; } = "";
        public string ApproveDir1By { get; set; } = "";
        public DateTime? ApproveDir1Date { get; set; }
        public string SrtNo { get; set; } = "";
        public string NoSpkb { get; set; } = "";
        public string Sk { get; set; } = "";
        public string Spkb { get; set; } = "";
        public string Status { get; set; } = "";
        public string CreatedBy { get; set; } = "";
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; } = "";
        public DateTime? ModifiedDate { get; set; }
    }
}
