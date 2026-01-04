using astratech_apps_backend.DTOs.Identity;

namespace astratech_apps_backend.Repositories.Interfaces
{
    public interface IEmployeeIdentityRepository
    {
        Task<EmployeeIdentityResponse?> GetEmployeeIdentityByUserAsync(string username);
    }
}