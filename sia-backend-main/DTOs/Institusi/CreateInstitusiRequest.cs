using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.Institusi
{
    public class CreateInstitusiRequest
    {
        [Required(ErrorMessage = "Nama institusi harus diisi.")]
        [StringLength(100)]
        public string NamaInstitusi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama direktur harus diisi.")]
        [StringLength(50)]
        public string NamaDirektur { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama wakil direktur 1 harus diisi.")]
        [StringLength(50)]
        public string NamaWadir1 { get; set; } = string.Empty;

        [StringLength(50)]
        public string NamaWadir2 { get; set; } = string.Empty;

        [StringLength(50)]
        public string NamaWadir3 { get; set; } = string.Empty;

        [StringLength(50)]
        public string NamaWadir4 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Alamat harus diisi.")]
        [StringLength(200)]
        public string Alamat { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kode pos harus diisi.")]
        [StringLength(5)]
        public string KodePos { get; set; } = string.Empty;

        [StringLength(15)]
        public string Telepon { get; set; } = string.Empty;

        [StringLength(15)]
        public string Fax { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [StringLength(50)]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        public string Website { get; set; } = string.Empty;

        public DateTime? TanggalBerdiri { get; set; }

        [Required(ErrorMessage = "Nomor SK harus diisi.")]
        [StringLength(50)]
        public string NomorSK { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tanggal SK harus diisi.")]
        [Range(typeof(DateTime), "1900-01-01", "2999-12-31", ErrorMessage = "Format tanggal SK tidak valid.")]
        public DateTime TanggalSK { get; set; }
    }
}
