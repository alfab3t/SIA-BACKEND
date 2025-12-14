namespace astratech_apps_backend.DTOs.CutiAkademik
{
    public class CreateCutiProdiRequest
    {
        public string TahunAjaran { get; set; } = "";
        public string Semester { get; set; } = "";
        public string LampiranSuratPengajuan { get; set; } = "";
        public string Lampiran { get; set; } = "";
        public string MhsId { get; set; } = "";
        public string Menimbang { get; set; } = "";
        public string ApprovalProdi { get; set; } = ""; // username prodi
    }
}
