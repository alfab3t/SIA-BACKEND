using astratech_apps_backend.DTOs.CutiAkademik;

namespace astratech_apps_backend.Services.Interfaces
{
    public interface ICutiAkademikService
    {
        Task<string?> CreateDraftAsync(CreateDraftCutiRequest dto);
        Task<string?> GenerateIdAsync(GenerateCutiIdRequest dto);
        Task<IEnumerable<CutiAkademikListResponse>> GetAllAsync(string mhsId, string status, string userId, string role, string search = "");
        Task<CutiAkademikDetailResponse?> GetDetailAsync(string id);
        Task<bool> UpdateAsync(string id, UpdateCutiAkademikRequest dto);
        Task<bool> DeleteAsync(string id, string modifiedBy);
        Task<string?> CreateDraftByProdiAsync(CreateCutiProdiRequest dto);
        Task<string?> GenerateIdByProdiAsync(GenerateCutiProdiIdRequest dto);
        Task<IEnumerable<CutiAkademikListResponse>> GetRiwayatAsync(string userId, string status, string search);
        Task<IEnumerable<CutiAkademikRiwayatExcelResponse>> GetRiwayatExcelAsync(string userId);
        
        // APPROVAL & REJECTION
        Task<bool> ApproveCutiAsync(ApproveCutiAkademikRequest dto);
        Task<bool> ApproveProdiCutiAsync(ApproveProdiCutiRequest dto);
        Task<bool> RejectCutiAsync(RejectCutiAkademikRequest dto);
        
        // ROLE DETECTION
        Task<string> DetectUserRoleAsync(string username);
        
        // SK MANAGEMENT
        Task<string?> CreateSKAsync(CreateSKRequest dto);
        Task<bool> UploadSKAsync(UploadSKRequest dto);

    }
}