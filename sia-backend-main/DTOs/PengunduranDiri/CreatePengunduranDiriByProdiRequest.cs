namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class CreatePengunduranDiriByProdiRequest
    {
        public string MhsId { get; set; } = "";                    // @p4
        public string LampiranSuratPengajuan { get; set; } = "";   // @p2
        public string Lampiran { get; set; } = "";                 // @p3
        public string CreatedBy { get; set; } = "";                // @p5 (NPK prodi)
    }
}
