namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class CreatePengunduranDiriByProdiRequest
    {
        public string MhsId { get; set; } = "";
        public string Alasan { get; set; } = "";
        public string Catatan { get; set; } = "";
        public string CreatedBy { get; set; } = "";
        public string ProdiNpk { get; set; } = ""; // @p5
    }
}
