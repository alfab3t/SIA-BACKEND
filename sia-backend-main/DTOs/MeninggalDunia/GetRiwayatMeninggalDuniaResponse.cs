namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class GetRiwayatMeninggalDuniaResponse
    {
        public List<RiwayatMeninggalDuniaListDto> Data { get; set; } = [];
        public int TotalData { get; set; }
        public int TotalHalaman { get; set; }
    }
}
