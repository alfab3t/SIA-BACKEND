namespace astratech_apps_backend.DTOs.Common
{
    public class PaginationRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Keyword { get; set; }
        public string? Status { get; set; }
        public string? OrderBy { get; set; }
        public string? Konsentrasi { get; set; }
    }
}
