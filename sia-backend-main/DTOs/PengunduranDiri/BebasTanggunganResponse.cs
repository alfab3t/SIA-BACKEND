namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class BebasTanggunganResponse
    {
        public string MhsId { get; set; } = "";
        public string Status { get; set; } = ""; // OK atau NOK
        public bool IsBebasTanggungan { get; set; } = false;
        public string Message { get; set; } = "";
        public string StatusKeuangan { get; set; } = ""; // OK atau NOK
        public string StatusJam { get; set; } = ""; // OK atau NOK  
        public string StatusPeminjamanAlat { get; set; } = ""; // OK atau NOK
    }
}
