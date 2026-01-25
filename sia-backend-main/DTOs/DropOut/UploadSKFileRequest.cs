namespace astratech_apps_backend.DTOs.DropOut
{
    public class UploadSKFileRequest
    {
        public string DroId { get; set; } = "";
        public IFormFile? SkFile { get; set; }
        public IFormFile? SkpbFile { get; set; }
    }
}
