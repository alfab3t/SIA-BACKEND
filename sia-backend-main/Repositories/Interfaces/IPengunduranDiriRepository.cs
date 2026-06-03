using astratech_apps_backend.DTOs.PengunduranDiri;
using astratech_apps_backend.DTOs.Common;
using astratech_apps_backend.Models;

namespace astratech_apps_backend.Repositories.Interfaces
{
    public interface IPengunduranDiriRepository
    {
        
        Task<string> CreateStep1Async(string mhsId, string createdBy, string? lampiranSuratPengajuan = "", string? lampiran = "");
        Task<CreatePengunduranDiriResponse?> CreateStep2Async(string draftId, string createdBy);
        Task<IEnumerable<PengunduranDiriListResponse>> GetAllAsync(string p1, string keyword, string sortBy, string konId, string status, string userId);
        Task<PaginatedResponse<PengunduranDiriListResponse>> GetAllPaginatedAsync(string p1, string keyword, string sortBy, string konId, string status, string userId, int page, int pageSize);
        Task<PengunduranDiri?> GetByIdAsync(string id);
        Task<bool> UpdateAsync(string id, UpdatePengunduranDiriRequest dto, string updatedBy);
        Task<bool> SoftDeleteAsync(string id, string updatedBy);
        Task<string?> CheckReportAsync(string pdiId);
        Task<string> CreateByProdiStep1Async(string mhsId, string createdBy, string? lampiranSuratPengajuan = "", string? lampiran = "");
        Task<CreatePengunduranDiriByProdiResponse?> CreateByProdiStep2Async(string draftId, string modifiedBy);
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
        Task<PaginatedResponse<PengunduranDiriRiwayatResponse>> GetRiwayatPaginatedAsync(
        string username,
        string status,
        string keyword,
        string orderBy,
        string konsentrasi,
        int page,
        int pageSize
        );
        Task<IEnumerable<PengunduranDiriRiwayatExcelResponse>> GetRiwayatExcelAsync(
        string orderBy,
        string konsentrasi
        );
        Task<bool> ApproveAsync(string id, ApprovePengunduranDiriRequest dto);
        Task<bool> RejectAsync(string id, RejectPengunduranDiriRequest dto);
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
