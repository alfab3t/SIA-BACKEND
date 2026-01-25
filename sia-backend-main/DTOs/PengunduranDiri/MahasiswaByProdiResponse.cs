namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class MahasiswaByProdiResponse
    {
        public string Value { get; set; } = "";  // mhs_id
        public string Text { get; set; } = "";   // mhs_nama
        public string NimNama { get; set; } = ""; // mhs_id + ' - ' + mhs_nama
        public string ProdiId { get; set; } = ""; // pro_id
        public string ProdiNama { get; set; } = ""; // pro_nama
        public string KonsentrasiId { get; set; } = ""; // kon_id
    }
}