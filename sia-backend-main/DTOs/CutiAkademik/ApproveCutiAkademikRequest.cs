namespace astratech_apps_backend.DTOs.CutiAkademik
{
    /// <summary>
    /// Request untuk menyetujui cuti akademik
    /// </summary>
    public class ApproveCutiAkademikRequest
    {
        /// <summary>
        /// ID cuti akademik yang akan disetujui
        /// </summary>
        public string Id { get; set; } = "";
        
        /// <summary>
        /// Role yang menyetujui: "prodi", "wadir1", "finance"
        /// </summary>
        public string Role { get; set; } = "";
        
        /// <summary>
        /// Username yang menyetujui
        /// </summary>
        public string ApprovedBy { get; set; } = "";
    }
}