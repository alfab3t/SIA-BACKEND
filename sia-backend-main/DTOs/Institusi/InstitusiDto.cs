namespace astratech_apps_backend.DTOs.Institusi
{
    public class InstitusiDto
    {
        public short Id { get; set; }
        public string NamaInstitusi { get; set; } = string.Empty;
        public string NamaDirektur { get; set; } = string.Empty;
        public DateTime TanggalSK { get; set; }
        public string NomorSK { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
