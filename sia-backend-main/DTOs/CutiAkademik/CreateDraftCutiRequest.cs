using Microsoft.AspNetCore.Http;

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
