using astratech_apps_backend.DTOs.Auth;

namespace astratech_apps_backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto dto);
        Task<PermissionResponseDto?> GetPermissionAsync(PermissionRequestDto dto);
        Task<MenuResponseDto?> GetMenuAsync(PermissionRequestDto dto);
    }
}
