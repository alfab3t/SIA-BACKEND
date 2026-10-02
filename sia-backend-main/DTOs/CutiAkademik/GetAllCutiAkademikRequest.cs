using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.CutiAkademik
{
    public class GetAllCutiAkademikRequest
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string SearchKeyword { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int KonsentrasiId { get; set; } = 0;

        public string MahasiswaId { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string RolId { get; set; } = string.Empty;

        public string OrderBy { get; set; } = "tanggal_desc";
    }
}
