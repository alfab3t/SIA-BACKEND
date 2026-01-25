namespace astratech_apps_backend.DTOs.PengunduranDiri
{
    public class MahasiswaProfilDetailResponse
    {
        // Data Pribadi
        public string MhsId { get; set; } = "";
        public string MhsNama { get; set; } = "";
        public string JenisKelamin { get; set; } = "";
        public string Ttl { get; set; } = "";
        public string Prodi { get; set; } = "";
        public string Angkatan { get; set; } = "";
        public string JalurMasuk { get; set; } = "";
        public string StatusKuliah { get; set; } = "";
        public string StatusBeasiswa { get; set; } = "";
        public string DosenWali { get; set; } = "";
        public string Email { get; set; } = "";
        
        // VA
        public string VaWisuda { get; set; } = "";
        public string VaCuti { get; set; } = "";
        public string VaIdcard { get; set; } = "";
        public string VaLainnya { get; set; } = "";
        
        // Data KTP
        public string Nik { get; set; } = "";
        public string Nisn { get; set; } = "";
        public string Agama { get; set; } = "";
        public string Kewarganegaraan { get; set; } = "";
        public string GolonganDarah { get; set; } = "";
        public string Alamat { get; set; } = "";
        public string Kodepos { get; set; } = "";
        public string Hp { get; set; } = "";
        
        // Pendidikan
        public string Sd { get; set; } = "";
        public string SdTahunLulus { get; set; } = "";
        public string Smp { get; set; } = "";
        public string SmpTahunLulus { get; set; } = "";
        public string Sma { get; set; } = "";
        public string SmaTahunLulus { get; set; } = "";
        public string Pt { get; set; } = "";
        public string PtTahunLulus { get; set; } = "";
        public string Kursus { get; set; } = "";
        
        // Lainnya
        public string Hobby { get; set; } = "";
        public string PengalamanKerja { get; set; } = "";
        public string Organisasi { get; set; } = "";
        public string StatusKawin { get; set; } = "";
        public string UkuranSepatu { get; set; } = "";
        public string UkuranKemeja { get; set; } = "";
        public string TinggiBadan { get; set; } = "";
        public string BeratBadan { get; set; } = "";

        // Data Ayah
        public string NamaAyah { get; set; } = "";
        public string NikAyah { get; set; } = "";
        public string StatusAyah { get; set; } = "";
        public string KewarganegaraanAyah { get; set; } = "";
        public string AgamaAyah { get; set; } = "";
        public string AlamatAyah { get; set; } = "";
        public string KodeposAyah { get; set; } = "";
        public string HpAyah { get; set; } = "";
        public string PendidikanAyah { get; set; } = "";
        public string PekerjaanAyah { get; set; } = "";
        public string PerusahaanAyah { get; set; } = "";
        public string AlamatPerusahaanAyah { get; set; } = "";
        public string PenghasilanAyah { get; set; } = "";
        
        // Data Ibu
        public string NamaIbu { get; set; } = "";
        public string NikIbu { get; set; } = "";
        public string StatusIbu { get; set; } = "";
        public string KewarganegaraanIbu { get; set; } = "";
        public string AgamaIbu { get; set; } = "";
        public string AlamatIbu { get; set; } = "";
        public string KodeposIbu { get; set; } = "";
        public string HpIbu { get; set; } = "";
        public string PendidikanIbu { get; set; } = "";
        public string PekerjaanIbu { get; set; } = "";
        public string PerusahaanIbu { get; set; } = "";
        public string AlamatPerusahaanIbu { get; set; } = "";
        public string PenghasilanIbu { get; set; } = "";
        
        // Data Wali
        public string NamaWali { get; set; } = "";
        public string NikWali { get; set; } = "";
        public string StatusWali { get; set; } = "";
        public string KewarganegaraanWali { get; set; } = "";
        public string AgamaWali { get; set; } = "";
        public string AlamatWali { get; set; } = "";
        public string KodeposWali { get; set; } = "";
        public string HpWali { get; set; } = "";
        public string PendidikanWali { get; set; } = "";
        public string PekerjaanWali { get; set; } = "";
        public string PerusahaanWali { get; set; } = "";
        public string AlamatPerusahaanWali { get; set; } = "";
        public string PenghasilanWali { get; set; } = "";
        
        // Data Saudara
        public string JumlahSaudara { get; set; } = "";
        public string JumlahKakak { get; set; } = "";
        public string JumlahAdik { get; set; } = "";
        public string SaudaraSekolah { get; set; } = "";
        public string SaudaraBekerja { get; set; } = "";
        
        // Astra
        public string AstraGrup { get; set; } = "";
        public string AstraHubungan { get; set; } = "";
        public string AstraPerusahaan { get; set; } = "";
        
        // Dokumen
        public string PasFoto { get; set; } = "";
        public string KtpSim { get; set; } = "";
        public string AktaKelahiran { get; set; } = "";
        public string KartuKeluarga { get; set; } = "";
        public string Ijazah { get; set; } = "";
        public string Skhun { get; set; } = "";
        public string BebasNarkoba { get; set; } = "";
        public string SanggupBayar { get; set; } = "";
        public string BuktiBayar { get; set; } = "";
        
        // Rekening
        public string AtasNama { get; set; } = "";
        public string NoRek { get; set; } = "";
        public string NamaBank { get; set; } = "";
        
        // VA Lainnya
        public string VaSumbangan { get; set; } = "";
        public string VaSpp { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
