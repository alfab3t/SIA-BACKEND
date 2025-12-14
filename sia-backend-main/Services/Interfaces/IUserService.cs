using astratech_apps_backend.Models;

namespace astratech_apps_backend.Services.Interfaces
{
    public interface IUserService
    {
        Task<(bool IsSuccess, List<Aplikasi> ListAplikasi, string? ErrorMessage)> AuthenticateAsync(string username, string jenisAplikasi);
        Task<(bool IsSuccess, List<string> ListPermission, string? ErrorMessage)> GetPermissionAsync(string username, string aplikasi, string role);
        Task<bool> HasPermissionAsync(string username, string aplikasi, string role, string permission);
        Task<(bool IsSuccess, List<Menu> ListMenu, string? ErrorMessage)> GetListMenuAsync(string username, string aplikasi, string role);
    }
}
