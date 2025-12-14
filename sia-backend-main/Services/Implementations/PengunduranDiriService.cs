using astratech_apps_backend.DTOs.PengunduranDiri;
using astratech_apps_backend.Repositories.Interfaces;
using astratech_apps_backend.Services.Interfaces;

namespace astratech_apps_backend.Services.Implementations
{
    public class PengunduranDiriService : IPengunduranDiriService
    {
        private readonly IPengunduranDiriRepository _repo;

        public PengunduranDiriService(IPengunduranDiriRepository repo)
        {
            _repo = repo;
        }


        public Task<string> CreateStep1Async(string mhsId, string createdBy)
        {
            return _repo.CreateStep1Async(mhsId, createdBy);
        }

        public Task<CreatePengunduranDiriResponse?> CreateStep2Async(string draftId, string createdBy)
        {
            return _repo.CreateStep2Async(draftId, createdBy);
        }

        public async Task<IEnumerable<PengunduranDiriListResponse>> GetAllAsync(string p1, string status, string userId)
        {
            return await _repo.GetAllAsync(p1, status, userId);
        }

        public async Task<PengunduranDiriResponse?> GetByIdAsync(string id)
        {
            var x = await _repo.GetByIdAsync(id);
            if (x == null) return null;

            return new PengunduranDiriResponse
            {
                Id = x.Id,
                MhsId = x.MhsId,
                LampiranSuratPengajuan = x.LampiranSuratPengajuan,
                Lampiran = x.Lampiran,
                Keterangan = x.Keterangan,
                ApprovalProdiBy = x.ApprovalProdiBy,
                AppProdiDate = x.AppProdiDate,
                ApprovalDir1By = x.ApprovalDir1By,
                AppDir1Date = x.AppDir1Date,
                SrtNo = x.SrtNo,
                NoSkpb = x.NoSkpb,
                Sk = x.Sk,
                Skpb = x.Skpb,
                Status = x.Status,
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate,
                ModifiedBy = x.ModifiedBy,
                ModifiedDate = x.ModifiedDate
            };
        }

        public async Task<bool> UpdateAsync(string id, UpdatePengunduranDiriRequest dto, string updatedBy)
        {
            return await _repo.UpdateAsync(id, dto, updatedBy);
        }


        public async Task<bool> SoftDeleteAsync(string id, string updatedBy)
        {
            return await _repo.SoftDeleteAsync(id, updatedBy);
        }


        public async Task<string?> CheckReportAsync(string pdiId)
        {
            return await _repo.CheckReportAsync(pdiId);
        }

        public async Task<CreatePengunduranDiriByProdiResponse> CreateByProdiAsync(CreatePengunduranDiriByProdiRequest dto)
        {
            return await _repo.CreateByProdiAsync(dto);
        }

        public async Task<bool> CreateSKAsync(string id, UploadSKPengunduranDiriRequest dto, string updatedBy)
        {
            return await _repo.CreateSKAsync(id, dto, updatedBy);
        }

        public async Task<PengunduranDiriDetailResponse?> GetDetailAsync(string id)
        {
            return await _repo.GetDetailAsync(id);
        }

        public async Task<PengunduranDiriNotifResponse?> GetNotifAsync(string id)
        {
            return await _repo.GetNotifAsync(id);
        }

        public async Task<IEnumerable<PengunduranDiriRiwayatResponse>> GetRiwayatAsync(
        string username,
        string status,
        string keyword,
        string orderBy,
        string konsentrasi)
        {
            return await _repo.GetRiwayatAsync(
                username,
                status,
                keyword,
                orderBy,
                konsentrasi
            );
        }

        public async Task<IEnumerable<PengunduranDiriRiwayatExcelResponse>> GetRiwayatExcelAsync(
        string orderBy,
        string konsentrasi
        )
        {
            return await _repo.GetRiwayatExcelAsync(orderBy, konsentrasi);
        }

        public async Task<bool> ApproveAsync(string id, ApprovePengunduranDiriRequest dto)
        {
            return await _repo.ApproveAsync(id, dto);
        }

        public async Task<bool> RejectAsync(string id, RejectPengunduranDiriRequest dto)
        {
            return await _repo.RejectAsync(id, dto);
        }




    }
}
