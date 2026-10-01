namespace astratech_apps_backend.DTOs.Mahasiswa
{
    /// <summary>
    /// Response untuk daftar Mahasiswa berdasarkan Prodi
    /// </summary>
    public class MahasiswaListResponse
    {
        /// <summary>
        /// ID Mahasiswa (mhs_id)
        /// </summary>
        public string MhsId { get; set; } = "";
        
        /// <summary>
        /// Nama Mahasiswa (mhs_nama)
        /// </summary>
        public string MhsNama { get; set; } = "";
        
        /// <summary>
        /// Angkatan Mahasiswa (opsional)
        /// </summary>
        public string? Angkatan { get; set; }
        
        /// <summary>
        /// Nama Konsentrasi (opsional)
        /// </summary>
        public string? KonNama { get; set; }
    }
}