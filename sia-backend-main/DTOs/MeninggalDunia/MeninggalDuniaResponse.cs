using System;

namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class MeninggalDuniaResponse
    {
        public List<MeninggalDuniaListDto> Data { get; set; } = [];
        public int TotalData { get; set; } = 0;
        public int TotalHalaman { get; set; } = 0;
    }
}
