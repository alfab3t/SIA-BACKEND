namespace astratech_apps_backend.Models
{
    public class Institusi
    {
        public short Id { get; set; }
        public long RowNumber { get; set; } = 0;
        public string NamaInstitusi { get; set; } = string.Empty;
        public string NamaDirektur { get; set; } = string.Empty;
        public string NamaWadir1 { get; set; } = string.Empty;
        public string NamaWadir2 { get; set; } = string.Empty;
        public string NamaWadir3 { get; set; } = string.Empty;
        public string NamaWadir4 { get; set; } = string.Empty;
        public string Alamat { get; set; } = string.Empty;
        public string KodePos { get; set; } = string.Empty;
        public string Telepon { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public DateTime TanggalBerdiri { get; set; }
        public string NomorSK { get; set; } = string.Empty;
        public DateTime TanggalSK { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
