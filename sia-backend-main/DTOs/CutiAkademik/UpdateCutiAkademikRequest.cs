using Microsoft.AspNetCore.Http;

public class UpdateCutiAkademikRequest
{
    public string Id { get; set; } = string.Empty;
    public string TahunAjaran { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;

    public IFormFile? LampiranSuratPengajuan { get; set; }
    public IFormFile? Lampiran { get; set; }

    public string ModifiedBy { get; set; } = string.Empty;
}
