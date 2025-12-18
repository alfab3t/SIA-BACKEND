namespace astratech_apps_backend.DTOs.Mahasiswa
{
    /// <summary>
    /// Response untuk daftar Program Studi
    /// </summary>
    public class ProdiListResponse
    {
        /// <summary>
        /// ID Konsentrasi (kon_id)
        /// </summary>
        public string KonId { get; set; } = "";
        
        /// <summary>
        /// Nama Konsentrasi/Program Studi (kon_nama)
        /// </summary>
        public string KonNama { get; set; } = "";
    }
}