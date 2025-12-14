namespace astratech_apps_backend.DTOs.DropOut
{
    public class DropOutRiwayatResponse
    {
        //public string Id { get; set; } = "";
        //public string MhsId { get; set; } = "";
        //public string Mahasiswa { get; set; } = "";
        //public string Konsentrasi { get; set; } = "";
        //public string Tanggal { get; set; } = "";
        //public string CreatedBy { get; set; } = "";
        //public string SuratNo { get; set; } = "";
        //public string Status { get; set; } = "";

        public string DroId { get; set; }
        public string TanggalPengajuan { get; set; }
        public string DibuatOleh { get; set; }
        public string NamaMahasiswa { get; set; }
        public string Prodi { get; set; }
        public string NoSkDo { get; set; }
        public string Status { get; set; }
    }
}
