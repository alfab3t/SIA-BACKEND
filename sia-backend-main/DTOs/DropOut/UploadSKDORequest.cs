namespace astratech_apps_backend.DTOs.DropOut
{
    public class UploadSKDORequest
    {
        public string DroId { get; set; } = "";
        public string SK { get; set; } = "";      // dro_sk
        public string SKPB { get; set; } = "";    // dro_skpb
        public string ModifiedBy { get; set; } = "";
    }
}
