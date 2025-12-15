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

        public Task<IEnumerable<CutiAkademikListResponse>> GetAllAsync(string m, string s, string u, string r)
            => _repo.GetAllAsync(m, s, u, r);

        public Task<CutiAkademikDetailResponse?> GetDetailAsync(string id)
            => _repo.GetDetailAsync(id);

        public Task<bool> UpdateAsync(string id, UpdateCutiAkademikRequest dto)
            => _repo.UpdateAsync(id, dto);

        public Task<bool> DeleteAsync(string id, string modified)
            => _repo.DeleteAsync(id, modified);
        public async Task<IEnumerable<CutiAkademikListResponse>> GetRiwayatAsync(string userId, string status, string search)
        {
            return await _repo.GetRiwayatAsync(userId, status, search);
        }

        public async Task<IEnumerable<CutiAkademikRiwayatExcelResponse>> GetRiwayatExcelAsync(string userId)
        {
            return await _repo.GetRiwayatExcelAsync(userId);
        }


    }
}
