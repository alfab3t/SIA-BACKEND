namespace astratech_apps_backend.DTOs.Mahasiswa
{
    /// <summary>
    /// Response untuk profil lengkap mahasiswa dari sia_getProfilMahasiswa
    /// </summary>
    public class ProfilMahasiswaResponse
    {
        // Data Pribadi
        public string MhsNama { get; set; } = "";
        public string MhsJenisKelamin { get; set; } = "";
        public string Ttl { get; set; } = "";
        public string Prodi { get; set; } = "";
        public string MhsId { get; set; } = "";
        public string Awal { get; set; } = "";
        public string DulJalur { get; set; } = "";
        public string MhsStatusKuliah { get; set; } = "";
        public string StatusBeasiswa { get; set; } = "";
        public string MhsVaWisuda { get; set; } = "";
        public string MhsVaCuti { get; set; } = "";
        public string MhsVaIdcard { get; set; } = "";
        public string MhsVaLainnya { get; set; } = "";
        public string DulNik { get; set; } = "";
        public string DulAgama { get; set; } = "";
        public string DulKewarganegaraan { get; set; } = "";
        public string DulGolonganDarah { get; set; } = "";
        public string DulAlamat { get; set; } = "";
        public string DulKodepos { get; set; } = "";
        public string DulHp { get; set; } = "";
        
        // Pendidikan
        public string DulSd { get; set; } = "";
        public string DulSdTahunLulus { get; set; } = "";
        public string DulSmp { get; set; } = "";
        public string DulSmpTahunLulus { get; set; } = "";
        public string DulSma { get; set; } = "";
        public string DulSmaTahunLulus { get; set; } = "";
        public string DulPt { get; set; } = "";
        public string DulPtTahunLulus { get; set; } = "";
        public string DulKursus { get; set; } = "";
        
        // Lainnya
        public string DulHobby { get; set; } = "";
        public string DulPengalamanKerja { get; set; } = "";
        public string DulOrganisasi { get; set; } = "";
        public string DulStatusKawin { get; set; } = "";
        public string DulUkuranSepatu { get; set; } = "";
        public string DulUkuranKemeja { get; set; } = "";
        public string DulTinggiBadan { get; set; } = "";
        public string DulBeratBadan { get; set; } = "";
        
        // Data Ayah
        public string DulNamaAyah { get; set; } = "";
        public string DulNikAyah { get; set; } = "";
        public string DulStatusAyah { get; set; } = "";
        public string DulKewarganegaraanAyah { get; set; } = "";
        public string DulAgamaAyah { get; set; } = "";
        public string DulAlamatAyah { get; set; } = "";
        public string DulKodeposAyah { get; set; } = "";
        public string DulHpAyah { get; set; } = "";
        public string DulPendidikanAyah { get; set; } = "";
        public string DulPekerjaanAyah { get; set; } = "";
        public string DulPerusahaanAyah { get; set; } = "";
        public string DulAlamatPerusahaanAyah { get; set; } = "";
        public string DulPenghasilanAyah { get; set; } = "";
        
        // Data Ibu
        public string DulNamaIbu { get; set; } = "";
        public string DulNikIbu { get; set; } = "";
        public string DulStatusIbu { get; set; } = "";
        public string DulKewarganegaraanIbu { get; set; } = "";
        public string DulAgamaIbu { get; set; } = "";
        public string DulAlamatIbu { get; set; } = "";
        public string DulKodeposIbu { get; set; } = "";
        public string DulHpIbu { get; set; } = "";
        public string DulPendidikanIbu { get; set; } = "";
        public string DulPekerjaanIbu { get; set; } = "";
        public string DulPerusahaanIbu { get; set; } = "";
        public string DulAlamatPerusahaanIbu { get; set; } = "";
        public string DulPenghasilanIbu { get; set; } = "";
        
        // Data Wali
        public string DulNamaWali { get; set; } = "";
        public string DulNikWali { get; set; } = "";
        public string DulStatusWali { get; set; } = "";
        public string DulKewarganegaraanWali { get; set; } = "";
        public string DulAgamaWali { get; set; } = "";
        public string DulAlamatWali { get; set; } = "";
        public string DulKodeposWali { get; set; } = "";
        public string DulHpWali { get; set; } = "";
        public string DulPendidikanWali { get; set; } = "";
        public string DulPekerjaanWali { get; set; } = "";
        public string DulPerusahaanWali { get; set; } = "";
        public string DulAlamatPerusahaanWali { get; set; } = "";
        public string DulPenghasilanWali { get; set; } = "";
        
        // Data Keluarga
        public string DulJumlahSaudara { get; set; } = "";
        public string DulJumlahKakak { get; set; } = "";
        public string DulJumlahAdik { get; set; } = "";
        public string DulSaudaraSekolah { get; set; } = "";
        public string DulSaudaraBekerja { get; set; } = "";
        public string DulAstraGrup { get; set; } = "";
        public string DulAstraHubungan { get; set; } = "";
        public string DulAstraPerusahaan { get; set; } = "";
        
        // Dokumen
        public string DulPasFoto { get; set; } = "";
        public string DulKtpSim { get; set; } = "";
        public string DulAktaKelahiran { get; set; } = "";
        public string DulKartuKeluarga { get; set; } = "";
        public string DulIjazah { get; set; } = "";
        public string DulSkhun { get; set; } = "";
        public string DulBebasNarkoba { get; set; } = "";
        public string DulSanggupBayar { get; set; } = "";
        public string DulBuktiBayar { get; set; } = "";
        
        // Lainnya
        public string DulEmail { get; set; } = "";
        public string DulVaSumbangan { get; set; } = "";
        public string DulVaSpp { get; set; } = "";
        public string DulStatus { get; set; } = "";
        public string AtasNama { get; set; } = "";
        public string NoRek { get; set; } = "";
        public string NamaBank { get; set; } = "";
        public string MhsDosenAkademik { get; set; } = "";
        public string DulNisn { get; set; } = "";
    }
}
