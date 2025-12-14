using Microsoft.AspNetCore.Mvc;

namespace astratech_apps_backend.DTOs.MeninggalDunia
{
    public class GetAllMeninggalDuniaRequest
    {
        public string? UserId { get; set; }
        public string? SearchKeyword { get; set; }
        public string? Status { get; set; }
        public string? Sort { get; set; }
        public string? RoleId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
