namespace astratech_apps_backend.DTOs.Mahasiswa
{
    /// <summary>
    /// Response DTO untuk data mahasiswa berdasarkan NIM
    /// </summary>
    public class MahasiswaByNIMResponse
    {
        /// <summary>
        /// Nama Mahasiswa
        /// </summary>
        public string MhsNama { get; set; } = string.Empty;

        /// <summary>
        /// Nama konsentrasi dengan format: "Program Name (Singkatan)"
        /// </summary>
        public string KonNama { get; set; } = string.Empty;

        /// <summary>
        /// Angkatan mahasiswa
        /// </summary>
        public int MhsAngkatan { get; set; }

        /// <summary>
        /// ID Kelas
        /// </summary>
        public string Kelas { get; set; } = string.Empty;
    }
}