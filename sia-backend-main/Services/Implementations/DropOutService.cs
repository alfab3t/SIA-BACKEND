using astratech_apps_backend.DTOs.DropOut;
using astratech_apps_backend.Repositories.Interfaces;
using astratech_apps_backend.Services.Interfaces;

namespace astratech_apps_backend.Services.Implementations
{
    public class DropOutService : IDropOutService
    {
        private readonly IDropOutRepository _repo;

        public DropOutService(IDropOutRepository repo)
        {
            _repo = repo;
        }

        public async Task<string> CreateAsync(CreateDropOutRequest dto, string createdBy)
        {
            return await _repo.CreateAsync(dto, createdBy);
        }

        public async Task<string?> CreatePengajuanDOAsync(CreatePengajuanDORequest dto, string createdBy)
        {
            return await _repo.CreatePengajuanDOAsync(dto, createdBy);
        }


        public async Task<IEnumerable<DropOutResponse>> GetAllAsync(string keyword, int page, int limit)
        {
            var list = await _repo.GetAllAsync(keyword, page, limit);

            return list.Select(x => new DropOutResponse
            {
                Id = x.Id,
                MhsId = x.MhsId,
                Menimbang = x.Menimbang,
                Mengingat = x.Mengingat,
                ApproveWadir1 = x.ApproveWadir1,
                ApproveWadir1Date = x.ApproveWadir1Date,
                ApproveDir = x.ApproveDir,
                ApproveDirDate = x.ApproveDirDate,
                SrtNo = x.SrtNo,
                SrtKetNo = x.SrtKetNo,
                Sk = x.Sk,
                Skpb = x.Skpb,
                AlasanTolak = x.AlasanTolak,
                Status = x.Status,
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate,
                ModifiedBy = x.ModifiedBy,
                ModifiedDate = x.ModifiedDate
            });
        }

        public async Task<DropOutDetailResponse?> GetDetailAsync(string id)
        {
            return await _repo.GetDetailAsync(id);
        }

        public async Task<DropOutResponse?> GetByIdAsync(string id)
        {
            var x = await _repo.GetByIdAsync(id);
            if (x == null) return null;

            return new DropOutResponse
            {
                Id = x.Id,
                MhsId = x.MhsId,
                Menimbang = x.Menimbang,
                Mengingat = x.Mengingat,
                ApproveWadir1 = x.ApproveWadir1,
                ApproveWadir1Date = x.ApproveWadir1Date,
                ApproveDir = x.ApproveDir,
                ApproveDirDate = x.ApproveDirDate,
                SrtNo = x.SrtNo,
                SrtKetNo = x.SrtKetNo,
                Sk = x.Sk,
                Skpb = x.Skpb,
                AlasanTolak = x.AlasanTolak,
                Status = x.Status,
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate,
                ModifiedBy = x.ModifiedBy,
                ModifiedDate = x.ModifiedDate
            };
        }

        public async Task<bool> UpdateAsync(string id, UpdateDropOutRequest dto, string updatedBy)
        {
            return await _repo.UpdateAsync(id, dto, updatedBy);
        }


        public async Task<bool> DeleteAsync(string id)
        {
            return await _repo.DeleteAsync(id);
        }


        public async Task<bool> ApproveDropOutAsync(string id, ApproveDropOutRequest dto)
        {
            return await _repo.ApproveDropOutAsync(id, dto);
        }

        public async Task<string?> CheckReportAsync(string id)
        {
            return await _repo.CheckReportAsync(id);
        }

        public async Task<DropOutReportSuketResponse?> GetReportSuketAsync(string suratNo)
        {
            return await _repo.GetReportSuketAsync(suratNo);
        }

        public async Task<DropOutDownloadSkResponse?> DownloadSKAsync(string droId)
        {
            return await _repo.DownloadSKAsync(droId);
        }

        //public async Task<IEnumerable<DropOutRiwayatResponse>> GetRiwayatAsync(
        //string username, string keyword, string sortBy, string konsentrasi, string role, string sekprodi)
        //{
        //    return await _repo.GetRiwayatAsync(username, keyword, sortBy, konsentrasi, role, sekprodi);
        //}

        public Task<IEnumerable<DropOutRiwayatResponse>> GetRiwayatAsync(
        string username, string keyword, string sortBy, string konsentrasi, string role, string displayName)
        {
            return _repo.GetRiwayatAsync(username, keyword, sortBy, konsentrasi, role, displayName);
        }

        public async Task<IEnumerable<DropOutRiwayatExcelResponse>> GetRiwayatExcelAsync(
        string username, string keyword, string sortBy, string konsentrasi, string role, string sekprodi)
        {
            return await _repo.GetRiwayatExcelAsync(username, keyword, sortBy, konsentrasi, role, sekprodi);
        }

        public async Task<DropOutGetIdByDraftResponse?> GetIdByDraftAsync(string id)
        {
            return await _repo.GetIdByDraftAsync(id);
        }

        public async Task<bool> RejectAsync(string id, RejectDropOutRequest dto)
        {
            return await _repo.RejectAsync(id, dto);
        }

        public async Task<SKDOReportResponse?> GetReportSKDOAsync(string id)
        {
            return await _repo.GetReportSKDOAsync(id);
        }

        public async Task<List<SKDOReportSubResponse>> GetReportSKDOSubAsync(string id)
        {
            return await _repo.GetReportSKDOSubAsync(id);
        }

        public async Task<bool> UploadSKDOAsync(UploadSKDORequest request)
        {
            return await _repo.UploadSKDOAsync(request);
        }


        public async Task<IEnumerable<DropOutPendingResponse>> GetPendingAsync(
    string username,
    string keyword,
    string sortBy,
    string konsentrasi)
        {
            return await _repo.GetPendingAsync(username, keyword, sortBy, konsentrasi);
        }









    }
}
