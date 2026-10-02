using astratech_apps_backend.DTOs.Common;
using astratech_apps_backend.DTOs.DropOut;
using astratech_apps_backend.DTOs.PengunduranDiri;
using astratech_apps_backend.Models;

namespace astratech_apps_backend.Repositories.Interfaces
{
    public interface IDropOutRepository
    {
        Task<(IEnumerable<DropOutPendingResponse> Data, int TotalData)> GetPendingPaginatedAsync(
            string username, string keyword, string sortBy, string konsentrasi, string role, string displayName, string status, int page, int pageSize);

        Task<IEnumerable<DropOutPendingResponse>> GetPendingAsync(
            string username, string keyword, string sortBy, string konsentrasi, string role, string displayName);

        Task<(IEnumerable<DropOutRiwayatResponse> Data, int TotalData)> GetRiwayatPaginatedAsync(
            string username, string keyword, string sortBy, string konsentrasi, string role, string displayName, string status, int page, int pageSize);

        Task<IEnumerable<DropOutRiwayatResponse>> GetRiwayatAsync(
            string username, string keyword, string sortBy, string konsentrasi, string role, string displayName);

        Task<IEnumerable<DropOutRiwayatExcelResponse>> GetRiwayatExcelAsync(
            string username, string keyword, string sortBy, string konsentrasi, string role, string displayName);

        Task<DropOutDetailResponse?> GetDetailAsync(string id);
        Task<DropOut?> GetByIdAsync(string id);

        Task<(bool Success, string Message, string? NewId)> CreatePengajuanDOAsync(CreatePengajuanDORequest dto, string createdBy);
        Task<(bool Success, string Message)> UpdateAsync(string id, UpdateDropOutRequest dto, string updatedBy);
        Task<(bool Success, string Message, string? NoPengajuan)> SubmitDraftAsync(string id, string submittedBy);
        Task<(bool Success, string Message)> ApproveAsync(string id, string role, string catatan, string approvedBy);
        Task<(bool Success, string Message)> RejectAsync(string id, string catatan, string rejectedBy);
        Task<(bool Success, string Message)> UploadSKDOAsync(UploadSKDORequest request);
        Task<(bool Success, string Message)> DeleteAsync(string id, string deletedBy);

        Task<DropOutDownloadSkResponse?> DownloadSKAsync(string droId);

        Task<IEnumerable<DropOutMahasiswaOptionResponse>> GetMahasiswaByKonsentrasiAsync(string konsentrasiId);
        Task<IEnumerable<DropOutProdiOptionResponse>> GetProdiAsync(string username);
        Task<IEnumerable<DropOutProdiOptionResponse>> GetListProdiAsync();
        Task<IEnumerable<DropOutKonsentrasiOptionResponse>> GetKonsentrasiByProdiAsync(string prodiId, string sekprodiUsername);
        Task<string?> GetAngkatanByMahasiswaAsync(string mhsId);
        Task<BebasTanggunganResponse?> CekBebasTanggunganAsync(string mhsId);
        Task<MahasiswaProfilDetailResponse?> GetProfilMahasiswaDetailAsync(string mhsId);
    }
}
