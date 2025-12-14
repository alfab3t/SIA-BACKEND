namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class ApprovePengunduranDiriRequest
    {
        public string Role { get; set; } = ""; // 'prodi' atau 'wadir1'
        public string ApprovedBy { get; set; } = "";
    }
}
