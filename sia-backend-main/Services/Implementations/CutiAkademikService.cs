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

        public async Task<string?> CreateDraftAsync(CreateDraftCutiRequest dto)
        {
            return await _repo.CreateDraftAsync(dto);
        }

        public async Task<string?> GenerateIdAsync(GenerateCutiIdRequest dto)
        {
            return await _repo.GenerateIdAsync(dto);
        }

        public async Task<string?> CreateDraftByProdiAsync(CreateCutiProdiRequest dto)
        {
            return await _repo.CreateDraftByProdiAsync(dto);
        }

        public async Task<string?> GenerateIdByProdiAsync(GenerateCutiProdiIdRequest dto)
        {
            return await _repo.GenerateIdByProdiAsync(dto);
        }

        public async Task<bool> CreateSKCutiAkademikAsync(CreateSKCutiAkademikRequest dto)
        {
            return await _repo.CreateSKCutiAkademikAsync(dto);
        }

        public async Task<List<CutiAkademikListResponse>> GetListResponseAsync(
    string? status,
    string? search,
    string? urut,
    int pageNumber,
    int pageSize
)
        {
            // ⿡ Ambil data raw dari repository
            var data = await _repo.GetListResponseAsync(status, search, urut, pageNumber, pageSize);

            // Jika repo sudah kasih data kosong → return cepat
            if (data == null || data.Count == 0)
                return data ?? new List<CutiAkademikListResponse>();

            // ⿢ FILTER STATUS
            if (!string.IsNullOrWhiteSpace(status))
            {
                data = data
                    .Where(x => x.Status != null &&
                                x.Status.Contains(status, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // ⿣ SEARCH (id, nim, tanggal, surat)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower().Trim();
                data = data.Where(x =>
                    (x.IdDisplay?.ToLower().Contains(s) ?? false) ||
                    (x.MhsId?.ToLower().Contains(s) ?? false) ||
                    (x.Tanggal?.ToLower().Contains(s) ?? false) ||
                    (x.SuratNo?.ToLower().Contains(s) ?? false)
                ).ToList();
            }

            // ⿤ SORTING
            data = urut switch
            {
                "tanggal_desc" => data.OrderByDescending(x => ParseDate(x.Tanggal)).ToList(),
                "tanggal_asc" => data.OrderBy(x => ParseDate(x.Tanggal)).ToList(),
                "id_asc" => data.OrderBy(x => x.IdDisplay).ToList(),
                "id_desc" => data.OrderByDescending(x => x.IdDisplay).ToList(),
                _ => data.OrderByDescending(x => ParseDate(x.Tanggal)).ToList()
            };

            // ⿥ PAGING
            var paged = data
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return paged;
        }

        // Helper convert tanggal dari SP Astra
        private DateTime ParseDate(string? date)
        {
            if (DateTime.TryParse(date, out var dt))
                return dt;

            return DateTime.MinValue;
        }

        public async Task<CutiAkademikNotifResponse?> GetDetailNotifAsync(string id)
        {
            return await _repo.GetDetailNotifAsync(id);
        }

        public async Task<CutiAkademikDetailResponse?> GetDetailAsync(string id)
        {
            return await _repo.GetDetailAsync(id);
        }

        public async Task<IEnumerable<CutiAkademikRiwayatResponse>> GetRiwayatAsync(
        string username, string status, string keyword)
        {
            return await _repo.GetRiwayatAsync(username, status, keyword);
        }

        public async Task<IEnumerable<CutiAkademikRiwayatExcelResponse>> GetRiwayatExcelAsync()
        {
            return await _repo.GetRiwayatExcelAsync();
        }

        public async Task<bool> UpdateAsync(string id, UpdateCutiAkademikRequest dto)
        {
            return await _repo.UpdateAsync(id, dto);
        }

        public async Task<bool> DeleteAsync(string id, string modifiedBy)
        {
            return await _repo.DeleteAsync(id, modifiedBy);
        }

        public async Task<bool> ApproveAsync(string id, CutiAkademikApproveRequest request)
        {
            return await _repo.ApproveAsync(id, request.Role, request.Username);
        }

        public async Task<bool> ApproveProdiAsync(string id, CutiAkademikApproveProdiRequest request)
        {
            return await _repo.ApproveProdiAsync(id, request.Menimbang, request.Username);
        }

        public async Task<bool> RejectAsync(string id, CutiAkademikRejectRequest request)
        {
            return await _repo.RejectAsync(id, request.RejectedBy, request.Reason);
        }


    }
}
