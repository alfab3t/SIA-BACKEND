namespace astratech_apps_backend.DTOs.CutiAkademik
{
    /// <summary>
    /// Request untuk menolak cuti akademik
    /// </summary>
    public class RejectCutiAkademikRequest
    {
        /// <summary>
        /// ID cuti akademik yang akan ditolak
        /// </summary>
        public string Id { get; set; } = "";
        
        /// <summary>
        /// Role yang menolak: "prodi", "wadir1", "finance"
        /// </summary>
        public string Role { get; set; } = "";
        
        /// <summary>
        /// Keterangan/alasan penolakan
        /// </summary>
        public string Keterangan { get; set; } = "";
    }
}