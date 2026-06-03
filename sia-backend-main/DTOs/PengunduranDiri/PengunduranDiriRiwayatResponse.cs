namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class PengunduranDiriRiwayatResponse
    {
        public string PdiId { get; set; } = "";
        public string MhsId { get; set; } = "";
        public string ApproveProdi { get; set; } = "";
        public string ApproveDir1 { get; set; } = "";
        public string Tanggal { get; set; } = "";
        public string TanggalDisetujui { get; set; } = "";
        public string SuratNo { get; set; } = "";
        public string NamaMahasiswa { get; set; } = "";
        public string ProdiNama { get; set; } = ""; // Format: "Teknik Informatika" (tanpa jenjang)
        public string Konsentrasi { get; set; } = ""; // Format: "SE", "DS", dll
        public string Status { get; set; } = "";
    }
}
