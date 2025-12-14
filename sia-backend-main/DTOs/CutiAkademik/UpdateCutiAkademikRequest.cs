namespace astratech_apps_backend.DTOs.CutiAkademik
{
    public class UpdateCutiAkademikRequest
    {
        public string TahunAjaran { get; set; } = "";
        public string Semester { get; set; } = "";
        public string LampiranSuratPengajuan { get; set; } = "";
        public string Lampiran { get; set; } = "";
        public string ModifiedBy { get; set; } = "";
    }
}
