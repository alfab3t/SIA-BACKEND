using astratech_apps_backend.DTOs.CutiAkademik;
using astratech_apps_backend.Repositories.Interfaces;
using astratech_apps_backend.Services.Interfaces;

namespace astratech_apps_backend.Services.Implementations
{
    public class CutiAkademikService : ICutiAkademikService
    {
        private readonly ICutiAkademikRepository _repo;

        public CutiAkademikService(ICutiAkademikRepository repo)
        {
            _repo = repo;
        }

        public Task<string?> CreateDraftAsync(CreateDraftCutiRequest dto)
            => _repo.CreateDraftAsync(dto);

        public Task<string?> GenerateIdAsync(GenerateCutiIdRequest dto)
            => _repo.GenerateIdAsync(dto);
        public async Task<string?> CreateDraftByProdiAsync(CreateCutiProdiRequest dto)
            => await _repo.CreateDraftByProdiAsync(dto);

        public async Task<string?> GenerateIdByProdiAsync(GenerateCutiProdiIdRequest dto)
            => await _repo.GenerateIdByProdiAsync(dto);

        public Task<IEnumerable<CutiAkademikListResponse>> GetAllAsync(string m, string s, string u, string r, string search = "")
        {
            // Normalize status parameter to match stored procedure expectations
            string normalizedStatus = NormalizeStatus(s);
            return _repo.GetAllAsync(m, normalizedStatus, u, r, search);
        }

        public Task<CutiAkademikDetailResponse?> GetDetailAsync(string id)
            => _repo.GetDetailAsync(id);

        public Task<bool> UpdateAsync(string id, UpdateCutiAkademikRequest dto)
            => _repo.UpdateAsync(id, dto);

        public Task<bool> DeleteAsync(string id, string modified)
            => _repo.DeleteAsync(id, modified);
        public async Task<IEnumerable<CutiAkademikListResponse>> GetRiwayatAsync(string userId, string status, string search)
        {
            // Normalize status parameter to match stored procedure expectations
            string normalizedStatus = NormalizeStatus(status);
            return await _repo.GetRiwayatAsync(userId, normalizedStatus, search);
        }

        public async Task<IEnumerable<CutiAkademikRiwayatExcelResponse>> GetRiwayatExcelAsync(string userId)
        {
            return await _repo.GetRiwayatExcelAsync(userId);
        }

        // ============================================================
        // APPROVAL & REJECTION METHODS
        // ============================================================
        
        public async Task<bool> ApproveCutiAsync(ApproveCutiAkademikRequest dto)
        {
            return await _repo.ApproveCutiAsync(dto);
        }

        public async Task<bool> ApproveProdiCutiAsync(ApproveProdiCutiRequest dto)
        {
            return await _repo.ApproveProdiCutiAsync(dto);
        }

        public async Task<bool> RejectCutiAsync(RejectCutiAkademikRequest dto)
        {
            return await _repo.RejectCutiAsync(dto);
        }

        public async Task<string?> CreateSKAsync(CreateSKRequest dto)
        {
            return await _repo.CreateSKAsync(dto);
        }

        public async Task<bool> UploadSKAsync(UploadSKRequest dto)
        {
            return await _repo.UploadSKAsync(dto);
        }

        /// <summary>
        /// Normalize status parameter to match stored procedure expectations
        /// </summary>
        private string NormalizeStatus(string status)
        {
            if (string.IsNullOrEmpty(status))
                return "";

            // Convert to lowercase for comparison
            string lowerStatus = status.ToLower().Trim();

            // Map common status values to their expected stored procedure format
            return lowerStatus switch
            {
                "disetujui" => "Disetujui",
                "belum disetujui prodi" => "Belum Disetujui Prodi",
                "belum disetujui wadir 1" => "Belum Disetujui Wadir 1",
                "menunggu upload sk" => "Menunggu Upload SK",
                "belum disetujui finance" => "Belum Disetujui Finance",
                "draft" => "Draft",
                "dihapus" => "Dihapus",
                _ => status // Return original if no mapping found
            };
        }

        public async Task<string> DetectUserRoleAsync(string username)
        {
            return await _repo.DetectUserRoleAsync(username);
        }
    }
}