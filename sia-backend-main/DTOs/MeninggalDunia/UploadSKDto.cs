namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class UploadSKMeninggalDuniaDto
    {
        public required IFormFile SkFile { get; set; }
        public required IFormFile SpkbFile { get; set; }
    }
}
