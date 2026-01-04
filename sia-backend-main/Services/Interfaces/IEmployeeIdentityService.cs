using astratech_apps_backend.DTOs.Identity;

namespace astratech_apps_backend.Services.Interfaces
{
    public interface IEmployeeIdentityService
    {
        Task<EmployeeIdentityResponse?> GetEmployeeIdentityAsync(EmployeeIdentityRequest request);
        Task<bool> IsWadirAsync(string username);
        Task<bool> IsFinanceAsync(string username);
        Task<bool> IsProdiAsync(string username);
        Task<string> GetUserRoleTypeAsync(string username);
    }
}