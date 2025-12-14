using astratech_apps_backend.DTOs.Auth;
using astratech_apps_backend.Helpers;
using astratech_apps_backend.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace astratech_apps_backend.Services.Implementations
{
    public class AuthService(IConfiguration config, ILdapService ldapService, IUserService userService, IHttpContextAccessor httpContextAccessor) : IAuthService
    {
        private readonly IConfiguration _config = config;
        private readonly ILdapService _ldapService = ldapService;
        private readonly IUserService _userService = userService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto dto)
        {
            var (IsLDAPSuccess, ErrorLDAPMessage) = await _ldapService.AuthenticateAsync(dto.Username, dto.Password);
            if (!IsLDAPSuccess)
            {
                return new LoginResponseDto { ErrorMessage = ErrorLDAPMessage! };
            }

            var (IsUserSuccess, ListAplikasi, ErrorUserMessage) = await _userService.AuthenticateAsync(dto.Username, dto.JenisAplikasi);
            if (!IsUserSuccess)
            {
                return new LoginResponseDto { ErrorMessage = ErrorUserMessage! };
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request is null)
            {
                return new LoginResponseDto { ErrorMessage = "Tidak dapat mendeteksi konteks request." };
            }

            var issuer = $"{request.Scheme}://{request.Host.Value}";
            var allowedIssuers = _config.GetSection("Key:jwtIssuer").Get<List<string>>() ?? [];
            if (!allowedIssuers.Contains(issuer))
            {
                return new LoginResponseDto { ErrorMessage = "Domain tidak diizinkan." };
            }

            var key = Environment.GetEnvironmentVariable("DECRYPT_KEY_JWT")!;
            var audience = _config["Key:jwtAudience"]!;
            var minutes = int.Parse(_config["Key:jwtLifeTime"] ?? "480");

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("namaakun", dto.Username),
            };

            var token = JwtHelper.GenerateToken(key, issuer, audience, TimeSpan.FromMinutes(minutes), claims);
            var nama = await _ldapService.GetDisplayNameAsync(dto.Username);
            nama ??= dto.Username;

            return new LoginResponseDto
            {
                Token = token,
                Nama = nama,
                ListAplikasi = ListAplikasi,
                ExpiresAt = DateTime.UtcNow.AddMinutes(minutes)
            };
        }

        public async Task<MenuResponseDto?> GetMenuAsync(PermissionRequestDto dto)
        {
            var (IsSuccess, ListMenu, ErrorMessage) = await _userService.GetListMenuAsync(dto.Username, dto.AppId, dto.RoleId);
            if (!IsSuccess)
            {
                return new MenuResponseDto { ErrorMessage = ErrorMessage! };
            }

            return new MenuResponseDto
            {
                ListMenu = ListMenu
            };
        }

        public async Task<PermissionResponseDto?> GetPermissionAsync(PermissionRequestDto dto)
        {
            var (IsSuccess, ListPermission, ErrorMessage) = await _userService.GetPermissionAsync(dto.Username, dto.AppId, dto.RoleId);
            if (!IsSuccess)
            {
                return new PermissionResponseDto { ErrorMessage = ErrorMessage! };
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request is null)
            {
                return new PermissionResponseDto { ErrorMessage = "Tidak dapat mendeteksi konteks request." };
            }

            var issuer = $"{request.Scheme}://{request.Host.Value}";
            var allowedIssuers = _config.GetSection("Key:jwtIssuer").Get<List<string>>() ?? [];
            if (!allowedIssuers.Contains(issuer))
            {
                return new PermissionResponseDto { ErrorMessage = "Domain tidak diizinkan." };
            }

            var key = Environment.GetEnvironmentVariable("DECRYPT_KEY_JWT")!;
            var audience = _config["Key:jwtAudience"]!;
            var minutes = int.Parse(_config["Key:jwtLifeTime"] ?? "480");

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("namaakun", dto.Username),
                new("idrole", dto.RoleId),
                new("idapp", dto.AppId),
            };

            var token = JwtHelper.GenerateToken(key, issuer, audience, TimeSpan.FromMinutes(minutes), claims);

            return new PermissionResponseDto
            {
                Token = token,
                ListPermission = ListPermission,
                ExpiresAt = DateTime.UtcNow.AddMinutes(minutes)
            };
        }
    }
}
