namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class CreateMeninggalDuniaRequest
    {
        public string MhsId { get; set; } = "";
        public string Lampiran { get; set; } = "";
        // Frontend usually only sends minimal fields; server fills metadata.
    }
}
