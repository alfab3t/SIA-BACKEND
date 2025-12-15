namespace astratech_apps_backend.DTOs.CutiAkademik
{
    public class GenerateCutiProdiIdRequest
    {
        public string DraftId { get; set; } = "";
        public string ModifiedBy { get; set; } = "";
        public string ProdiId { get; set; } = "";  // kalau diperlukan SP
    }
}
