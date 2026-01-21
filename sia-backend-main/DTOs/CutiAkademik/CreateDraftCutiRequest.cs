using Microsoft.AspNetCore.Http;

namespace astratech_apps_backend.DTOs.CutiAkademik
{
    public class CreateDraftCutiRequest
    {
        public string MhsId { get; set; } = string.Empty;
        public string TahunAjaran { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;

        // WAJIB sama dengan BE: LampiranSuratPengajuan
        public IFormFile? LampiranSuratPengajuan { get; set; }

        // Lampiran opsional
        public IFormFile? Lampiran { get; set; }

        public string CreatedBy { get; set; } = string.Empty;
    }
}