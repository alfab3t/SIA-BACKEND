namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class RejectMeninggalDuniaRequest
    {
        public string Role { get; set; } = "";      // contoh: "Prodi", "Wadir 1", "Direktur"
        public string Username { get; set; } = "";  // user yang menolak
    }
}
