namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class UploadSKPdiFileRequest
    {
        public string PdiId { get; set; } = "";
        public IFormFile? SkFile { get; set; }
        public IFormFile? SkpbFile { get; set; }
    }
}
