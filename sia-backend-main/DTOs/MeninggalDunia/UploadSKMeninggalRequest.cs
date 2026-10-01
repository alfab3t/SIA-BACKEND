namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class UploadSKMeninggalRequest
    {
        public string MduId { get; set; } = "";
        public IFormFile? SK { get; set; }        // File upload untuk SK
        public IFormFile? SKPB { get; set; }      // File upload untuk SPKB  
        public string ModifiedBy { get; set; } = "";
    }
}
