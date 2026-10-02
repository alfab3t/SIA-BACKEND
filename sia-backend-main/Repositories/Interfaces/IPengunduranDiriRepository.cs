using astratech_apps_backend.DTOs.Common;
using astratech_apps_backend.DTOs.PengunduranDiri;
using astratech_apps_backend.Models;

namespace astratech_apps_backend.Repositories.Interfaces
{
    public interface IPengunduranDiriRepository
    {
        Task<(IEnumerable<PengunduranDiriListResponse> Data, int TotalData)> GetPendingPaginatedAsync(
            string username, string keyword, string sortBy, string konsentrasi, string status, string role, int page, int pageSize);

        Task<IEnumerable<PengunduranDiriListResponse>> GetPendingAsync(
            string username, string keyword, string sortBy, string konsentrasi, string status, string role);

        Task<(IEnumerable<PengunduranDiriRiwayatResponse> Data, int TotalData)> GetRiwayatPaginatedAsync(
            string username, string status, string keyword, string sortBy, string konsentrasi, string role, int page, int pageSize);

        Task<IEnumerable<PengunduranDiriRiwayatResponse>> GetRiwayatAsync(
            string username, string status, string keyword, string sortBy, string konsentrasi, string role);

        Task<IEnumerable<PengunduranDiriRiwayatExcelResponse>> GetRiwayatExcelAsync(
            string konsentrasi, string sortBy);

        Task<PengunduranDiriDetailResponse?> GetDetailAsync(string id);
        Task<PengunduranDiri?> GetByIdAsync(string id);

        Task<(bool Success, string Message, string? PdiId)> CreatePengajuanAsync(CreatePengunduranDiriRequest dto, string createdBy);
        Task<(bool Success, string Message, string? PdiId)> CreateByProdiAsync(CreatePengunduranDiriByProdiRequest dto);
        Task<(bool Success, string Message, string? NoPengajuan)> SubmitDraftAsync(string id, string submittedBy);
        Task<(bool Success, string Message)> UpdateAsync(string id, UpdatePengunduranDiriRequest dto, string updatedBy);
        Task<(bool Success, string Message)> ApproveAsync(string id, string role, string approvedBy);
        Task<(bool Success, string Message)> RejectAsync(string id, string role, string reason, string rejectedBy);
        Task<(bool Success, string Message)> UploadSKAsync(string id, UploadSKPengunduranDiriRequest dto, string updatedBy);
        Task<(bool Success, string Message)> DeleteAsync(string id, string deletedBy);

        // Helper Dropdowns & Master Data
        Task<IEnumerable<MahasiswaListResponse>> GetMahasiswaListAsync();
        Task<IEnumerable<MahasiswaByProdiResponse>> GetMahasiswaByProdiAsync(string userId);
        Task<MahasiswaProdiResponse?> GetMahasiswaProdiAsync(string mhsId);
        Task<MahasiswaAngkatanResponse?> GetMahasiswaAngkatanAsync(string mhsId);
        Task<IEnumerable<MahasiswaListResponse>> GetMahasiswaByKonsentrasiAsync(string username);
        Task<IEnumerable<ProdiOptionResponse>> GetProdiByUserAsync(string username);
        Task<IEnumerable<ProdiOptionResponse>> GetListProdiAsync();
        Task<BebasTanggunganResponse?> CekBebasTanggunganAsync(string mhsId);
        Task<MahasiswaProfilDetailResponse?> GetProfilMahasiswaAsync(string mhsId);
    }
}
