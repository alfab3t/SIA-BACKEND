namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class PengunduranDiriListResponse
    {
        public string Id { get; set; } = string.Empty;
        public string PdiId { get; set; } = string.Empty;
        public string IdAlternative { get; set; } = string.Empty;
        public string MhsId { get; set; } = string.Empty;
        public string NamaMahasiswa { get; set; } = string.Empty;
        public string Mahasiswa { get; set; } = string.Empty;
        public string ApproveProdi { get; set; } = string.Empty;
        public string ApproveDir1 { get; set; } = string.Empty;
        public string Tanggal { get; set; } = string.Empty;
        public string? TanggalDisetujui { get; set; }
        public string SuratNo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string ProdiNama { get; set; } = string.Empty;
        public string Prodi { get; set; } = string.Empty;
        public string Konsentrasi { get; set; } = string.Empty;
    }
}
