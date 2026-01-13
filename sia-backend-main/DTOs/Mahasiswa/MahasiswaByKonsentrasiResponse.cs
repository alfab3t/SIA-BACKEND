namespace astratech_apps_backend.DTOs.Mahasiswa
{
    /// <summary>
    /// Response DTO untuk daftar mahasiswa berdasarkan konsentrasi
    /// </summary>
    public class MahasiswaByKonsentrasiResponse
    {
        /// <summary>
        /// ID Mahasiswa
        /// </summary>
        public string MhsId { get; set; } = string.Empty;

        /// <summary>
        /// Nama mahasiswa dengan format: "ID - Nama"
        /// </summary>
        public string MhsNama { get; set; } = string.Empty;
    }
}