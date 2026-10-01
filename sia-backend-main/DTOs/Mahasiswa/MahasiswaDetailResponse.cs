namespace astratech_apps_backend.DTOs.Mahasiswa
{
    /// <summary>
    /// Response untuk detail profil mahasiswa lengkap
    /// </summary>
    public class MahasiswaDetailResponse
    {
        /// <summary>
        /// Nomor Pendaftaran
        /// </summary>
        public string DulNoPendaftaran { get; set; } = "";
        
        /// <summary>
        /// Nama Mahasiswa
        /// </summary>
        public string MhsNama { get; set; } = "";
        
        /// <summary>
        /// Program Studi dan Konsentrasi
        /// </summary>
        public string KonNama { get; set; } = "";
        
        /// <summary>
        /// Tempat Lahir
        /// </summary>
        public string MhsTempatLahir { get; set; } = "";
        
        /// <summary>
        /// Tanggal Lahir (format: YYYY-MM-DD)
        /// </summary>
        public string MhsTglLahir { get; set; } = "";
        
        /// <summary>
        /// Jenis Kelamin
        /// </summary>
        public string MhsJenisKelamin { get; set; } = "";
        
        /// <summary>
        /// Alamat Mahasiswa
        /// </summary>
        public string MhsAlamat { get; set; } = "";
        
        /// <summary>
        /// Kode Pos
        /// </summary>
        public string MhsKodepos { get; set; } = "";
        
        /// <summary>
        /// Nomor HP
        /// </summary>
        public string MhsHp { get; set; } = "";
        
        /// <summary>
        /// Email
        /// </summary>
        public string MhsEmail { get; set; } = "";
        
        /// <summary>
        /// Tanggal Masuk (format: YYYY-MM-DD)
        /// </summary>
        public string MhsTglMasuk { get; set; } = "";
        
        /// <summary>
        /// Tanggal Lulus (format: YYYY-MM-DD)
        /// </summary>
        public string MhsTglLulus { get; set; } = "";
        
        /// <summary>
        /// Angkatan
        /// </summary>
        public int MhsAngkatan { get; set; }
        
        /// <summary>
        /// Status Kuliah
        /// </summary>
        public string MhsStatusKuliah { get; set; } = "";
        
        /// <summary>
        /// Nama Ayah
        /// </summary>
        public string DulNamaAyah { get; set; } = "";
        
        /// <summary>
        /// HP Ayah
        /// </summary>
        public string DulHpAyah { get; set; } = "";
        
        /// <summary>
        /// Status Ayah
        /// </summary>
        public string DulStatusAyah { get; set; } = "";
        
        /// <summary>
        /// Alamat Ayah
        /// </summary>
        public string DulAlamatAyah { get; set; } = "";
        
        /// <summary>
        /// Kode Pos Ayah
        /// </summary>
        public string DulKodeposAyah { get; set; } = "";
        
        /// <summary>
        /// Nama Ibu
        /// </summary>
        public string DulNamaIbu { get; set; } = "";
        
        /// <summary>
        /// HP Ibu
        /// </summary>
        public string DulHpIbu { get; set; } = "";
        
        /// <summary>
        /// Status Ibu
        /// </summary>
        public string DulStatusIbu { get; set; } = "";
        
        /// <summary>
        /// Alamat Ibu
        /// </summary>
        public string DulAlamatIbu { get; set; } = "";
        
        /// <summary>
        /// Kode Pos Ibu
        /// </summary>
        public string DulKodeposIbu { get; set; } = "";
        
        /// <summary>
        /// Nama Wali
        /// </summary>
        public string DulNamaWali { get; set; } = "";
        
        /// <summary>
        /// HP Wali
        /// </summary>
        public string DulHpWali { get; set; } = "";
        
        /// <summary>
        /// Status Wali
        /// </summary>
        public string DulStatusWali { get; set; } = "";
        
        /// <summary>
        /// Alamat Wali
        /// </summary>
        public string DulAlamatWali { get; set; } = "";
        
        /// <summary>
        /// Kode Pos Wali
        /// </summary>
        public string DulKodeposWali { get; set; } = "";
        
        /// <summary>
        /// ID Konsentrasi
        /// </summary>
        public int KonId { get; set; }
        
        /// <summary>
        /// Jenis Mahasiswa (Beasiswa/Reguler)
        /// </summary>
        public string MhsJenis { get; set; } = "";
        
        /// <summary>
        /// Jalur Masuk
        /// </summary>
        public string DulJalur { get; set; } = "";
        
        /// <summary>
        /// Atas Nama Rekening
        /// </summary>
        public string AtasNama { get; set; } = "";
        
        /// <summary>
        /// Nomor Rekening
        /// </summary>
        public string NoRek { get; set; } = "";
        
        /// <summary>
        /// Nama Bank
        /// </summary>
        public string NamaBank { get; set; } = "";
        
        /// <summary>
        /// NISN
        /// </summary>
        public string DulNisn { get; set; } = "";
        
        /// <summary>
        /// ID Kelurahan
        /// </summary>
        public string KelId { get; set; } = "";
        
        /// <summary>
        /// NIK
        /// </summary>
        public string DulNik { get; set; } = "";
        
        /// <summary>
        /// ID Mahasiswa
        /// </summary>
        public string MhsId { get; set; } = "";
        
        /// <summary>
        /// RFID Aktif
        /// </summary>
        public string RfidAktif { get; set; } = "";
    }
}