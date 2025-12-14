using astratech_apps_backend.DTOs.PengunduranDiri;
using astratech_apps_backend.Models;

namespace astratech_apps_backend.Repositories.Interfaces
{
    public interface IPengunduranDiriRepository
    {
        
        Task<string> CreateStep1Async(string mhsId, string createdBy);
        Task<CreatePengunduranDiriResponse?> CreateStep2Async(string draftId, string createdBy);
        Task<IEnumerable<PengunduranDiriListResponse>> GetAllAsync(string p1, string status, string userId);
        Task<PengunduranDiri?> GetByIdAsync(string id);
        Task<bool> UpdateAsync(string id, UpdatePengunduranDiriRequest dto, string updatedBy);
        Task<bool> SoftDeleteAsync(string id, string updatedBy);
        Task<string?> CheckReportAsync(string pdiId);
        Task<CreatePengunduranDiriByProdiResponse> CreateByProdiAsync(CreatePengunduranDiriByProdiRequest dto);
        Task<bool> CreateSKAsync(string id, UploadSKPengunduranDiriRequest dto, string updatedBy);
        Task<PengunduranDiriDetailResponse?> GetDetailAsync(string id);
        Task<PengunduranDiriNotifResponse?> GetNotifAsync(string id);
        Task<IEnumerable<PengunduranDiriRiwayatResponse>> GetRiwayatAsync(
        string username,
        string status,
        string keyword,
        string orderBy,
        string konsentrasi
        );
        Task<IEnumerable<PengunduranDiriRiwayatExcelResponse>> GetRiwayatExcelAsync(
        string orderBy,
        string konsentrasi
        );
        Task<bool> ApproveAsync(string id, ApprovePengunduranDiriRequest dto);
        Task<bool> RejectAsync(string id, RejectPengunduranDiriRequest dto);








    }
}
