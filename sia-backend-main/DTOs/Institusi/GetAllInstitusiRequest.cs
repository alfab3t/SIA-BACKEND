using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.Institusi
{
    public class GetAllInstitusiRequest
    {
        [Required(ErrorMessage = "Nomor halaman harus diisi.")]
        public int PageNumber { get; set; } = 1;

        [Required(ErrorMessage = "Ukuran data per halaman harus diisi.")]
        public int PageSize { get; set; } = 10;

        public string SearchKeyword { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jenis urut harus diisi.")]
        public string Urut { get; set; } = string.Empty;
    }
}
