namespace astratech_apps_backend.DTOs.DropOut
{
    public class RejectDropOutRequest
    {
        public string Username { get; set; } = ""; // username (p2)
        public string Reason { get; set; } = ""; // alasan tolak (p3)
    }
}
