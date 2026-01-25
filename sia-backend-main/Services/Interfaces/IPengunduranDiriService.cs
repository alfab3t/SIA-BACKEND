using astratech_apps_backend.DTOs.PengunduranDiri;

namespace astratech_apps_backend.Services.Interfaces
{
    public interface IPengunduranDiriService
    {
        Task<IEnumerable<PengunduranDiriListResponse>> GetAllAsync(string p1, string status, string userId);
        Task<PengunduranDiriResponse?> GetByIdAsync(string id);
        Task<bool> UpdateAsync(string id, UpdatePengunduranDiriRequest dto, string updatedBy);
        Task<bool> SoftDeleteAsync(string id, string updatedBy);
        Task<string?> CheckReportAsync(string pdiId);
        Task<string> CreateStep1Async(string mhsId, string createdBy, string? lampiranSuratPengajuan = "", string? lampiran = "");
        Task<CreatePengunduranDiriResponse?> CreateStep2Async(string draftId, string createdBy);
        Task<CreatePengunduranDiriByProdiResponse> CreateByProdiAsync(CreatePengunduranDiriByProdiRequest dto);
        Task<string> CreateByProdiStep1Async(string mhsId, string createdBy, string? lampiranSuratPengajuan = "", string? lampiran = "");
        Task<CreatePengunduranDiriByProdiResponse?> CreateByProdiStep2Async(string draftId, string modifiedBy);
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
        Task<IEnumerable<MahasiswaListResponse>> GetMahasiswaListAsync();
        Task<MahasiswaProdiResponse?> GetMahasiswaProdiAsync(string mhsId);
        Task<MahasiswaAngkatanResponse?> GetMahasiswaAngkatanAsync(string mhsId);
        Task<IEnumerable<MahasiswaListResponse>> GetMahasiswaByKonsentrasiAsync(string username);
        Task<IEnumerable<ProdiOptionResponse>> GetProdiByUserAsync(string username);
        Task<IEnumerable<ProdiOptionResponse>> GetListProdiAsync();
        Task<BebasTanggunganResponse?> CekBebasTanggunganAsync(string mhsId);
        Task<MahasiswaProfilDetailResponse?> GetProfilMahasiswaAsync(string mhsId);
    }
}
