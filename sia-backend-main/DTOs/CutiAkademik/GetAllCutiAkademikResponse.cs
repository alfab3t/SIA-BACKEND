namespace astratech_apps_backend.DTOs.CutiAkademik
{
    public class GetAllCutiAkademikResponse
    {
        public List<CutiAkademikListResponse> Data { get; set; } = [];
        public int TotalData { get; set; } = 0;
        public int TotalHalaman { get; set; } = 0;
    }
}
