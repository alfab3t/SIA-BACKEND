namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class MahasiswaListResponse
    {
        public string Value { get; set; } = "";  // mhs_id
        public string Text { get; set; } = "";   // mhs_nama
        public string NimNama { get; set; } = ""; // mhs_id + ' - ' + mhs_nama
    }
}