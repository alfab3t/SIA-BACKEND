namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class CreatePengunduranDiriRequest
    {
        public string Step { get; set; } = ""; // STEP1 / STEP2
        public string DraftId { get; set; } = ""; // untuk submit
        public string MhsId { get; set; } = ""; // mhs_id
        public string? LampiranSuratPengajuan { get; set; } // berkas pernyataan
        public string? Lampiran { get; set; } // berkas lampiran
        public string? CreatedBy { get; set; } // untuk endpoint /create
    }
}
