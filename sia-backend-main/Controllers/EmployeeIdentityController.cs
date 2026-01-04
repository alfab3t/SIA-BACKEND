using astratech_apps_backend.DTOs.Identity;
using astratech_apps_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace astratech_apps_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeIdentityController : ControllerBase
    {
        private readonly IEmployeeIdentityService _employeeIdentityService;

        public EmployeeIdentityController(IEmployeeIdentityService employeeIdentityService)
        {
            _employeeIdentityService = employeeIdentityService;
        }

        [HttpGet("get-identity/{username}")]
        public async Task<IActionResult> GetIdentity(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Username is required");
            }

            var request = new EmployeeIdentityRequest { Username = username };
            var result = await _employeeIdentityService.GetEmployeeIdentityAsync(request);
            
            if (result?.ErrorMessage != null)
                return BadRequest(new { message = result.ErrorMessage });

            var isWadir = await _employeeIdentityService.IsWadirAsync(username);
            var isFinance = await _employeeIdentityService.IsFinanceAsync(username);

            return Ok(new 
            { 
                identity = result,
                roles = new 
                {
                    isWadir,
                    isFinance,
                    roleType = isWadir ? "wadir" : isFinance ? "finance" : "other"
                }
            });
        }
    }
}