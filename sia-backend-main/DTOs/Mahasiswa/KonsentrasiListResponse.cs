namespace astratech_apps_backend.DTOs.Mahasiswa
{
    /// <summary>
    /// Response DTO untuk daftar konsentrasi berdasarkan sekprodi
    /// </summary>
    public class KonsentrasiListResponse
    {
        /// <summary>
        /// ID Konsentrasi
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nama konsentrasi dengan format: "Program Name (Singkatan)"
        /// </summary>
        public string Nama { get; set; } = string.Empty;
    }
}