using astratech_apps_backend.DTOs.Identity;
using astratech_apps_backend.Repositories.Interfaces;
using astratech_apps_backend.Services.Interfaces;

namespace astratech_apps_backend.Services.Implementations
{
    public class EmployeeIdentityService : IEmployeeIdentityService
    {
        private readonly IEmployeeIdentityRepository _employeeIdentityRepository;

        public EmployeeIdentityService(IEmployeeIdentityRepository employeeIdentityRepository)
        {
            _employeeIdentityRepository = employeeIdentityRepository;
        }

        public async Task<EmployeeIdentityResponse?> GetEmployeeIdentityAsync(EmployeeIdentityRequest request)
        {
            return await _employeeIdentityRepository.GetEmployeeIdentityByUserAsync(request.Username);
        }

        public async Task<bool> IsWadirAsync(string username)
        {
            var identity = await _employeeIdentityRepository.GetEmployeeIdentityByUserAsync(username);
            
            if (identity?.ErrorMessage != null)
                return false;

            // Check if the employee has wadir position based on jab_main_id
            // Update these IDs based on your actual database values
            var wadirPositionIds = new[] { "4" }; // Only specific wadir positions
            
            return wadirPositionIds.Contains(identity.JabMainId);
        }

        public async Task<bool> IsFinanceAsync(string username)
        {
            var identity = await _employeeIdentityRepository.GetEmployeeIdentityByUserAsync(username);
            
            if (identity?.ErrorMessage != null)
                return false;

            // More specific check for finance - check username pattern or specific combination
            // Option 1: Check by username pattern
            if (username.ToLower().Contains("finance") || username.ToLower().Contains("user_finance"))
            {
                return true;
            }

            // Option 2: Check by specific combination of jabMainId and strMainId for finance only
            // You may need to adjust these based on your actual finance employee data
            var isFinancePosition = identity.JabMainId == "1" && identity.StrMainId == "27";
            var isFinanceUser = username.ToLower().Equals("user_finance");
            
            return isFinancePosition && isFinanceUser;
        }
    }
}