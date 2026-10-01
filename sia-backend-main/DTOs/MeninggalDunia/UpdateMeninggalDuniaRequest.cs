namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class UpdateMeninggalDuniaRequest
    {
        public string? MhsId { get; set; }
        public string? Lampiran { get; set; }
        public IFormFile? LampiranFile { get; set; }
    }
}
