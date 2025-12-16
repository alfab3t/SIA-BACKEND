using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.DTOs.CutiAkademik
{
    /// <summary>
    /// Response model untuk list data cuti akademik
    /// </summary>
    public class CutiAkademikListResponse
    {
        /// <summary>
        /// ID unik cuti akademik
        /// </summary>
        /// <example>012/PMA/CA/IX/2025</example>
        public string Id { get; set; } = "";
        
        /// <summary>
        /// ID untuk display (DRAFT jika belum final)
        /// </summary>
        /// <example>012/PMA/CA/IX/2025</example>
        public string IdDisplay { get; set; } = "";
        
        /// <summary>
        /// NIM Mahasiswa
        /// </summary>
        /// <example>0420240032</example>
        public string MhsId { get; set; } = "";
        
        /// <summary>
        /// Tahun ajaran cuti
        /// </summary>
        /// <example>2024/2025</example>
        public string TahunAjaran { get; set; } = "";
        
        /// <summary>
        /// Semester cuti (Ganjil/Genap)
        /// </summary>
        /// <example>Genap</example>
        public string Semester { get; set; } = "";
        
        /// <summary>
        /// Username yang menyetujui dari prodi
        /// </summary>
        /// <example>andreas_e</example>
        public string ApproveProdi { get; set; } = "";
        
        /// <summary>
        /// Username yang menyetujui dari wadir 1
        /// </summary>
        /// <example>tonny.pongoh</example>
        public string ApproveDir1 { get; set; } = "";
        
        /// <summary>
        /// Tanggal pengajuan
        /// </summary>
        /// <example>11 Sep 2025</example>
        public string Tanggal { get; set; } = "";
        
        /// <summary>
        /// Nomor surat keputusan
        /// </summary>
        /// <example>008/PA-WADIR-I/SKC/IX/2025</example>
        public string SuratNo { get; set; } = "";
        
        /// <summary>
        /// Status cuti akademik
        /// </summary>
        /// <example>Disetujui</example>
        public string Status { get; set; } = "";
    }
}
