namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class CreatePengunduranDiriRequest
    {
        public string Step { get; set; } = ""; // STEP1 / STEP2
        public string DraftId { get; set; } = ""; // p2
        public string MhsId { get; set; } = ""; // p4
    }
}
