namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class UploadSKMeninggalRequest
    {
        public string MduId { get; set; } = "";
        public string SK { get; set; } = "";      // mdu_sk
        public string SKPB { get; set; } = "";    // mdu_spkb
        public string ModifiedBy { get; set; } = "";
    }
}
