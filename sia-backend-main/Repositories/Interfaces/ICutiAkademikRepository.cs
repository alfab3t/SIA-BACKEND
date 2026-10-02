using astratech_apps_backend.DTOs.CutiAkademik;

namespace astratech_apps_backend.Repositories.Interfaces
{
    public interface ICutiAkademikRepository
    {
        Task<(IEnumerable<CutiAkademikListResponse>, int totalData)> GetAllAsync(GetAllCutiAkademikRequest dto);
        Task<(IEnumerable<CutiAkademikListResponse>, int totalData)> GetRiwayatAsync(GetAllCutiAkademikRequest dto);
        Task<CutiAkademikDetailResponse?> GetDetailAsync(string id);
        Task<string?> CreateDraftAsync(CreateDraftCutiRequest dto, string lampiranSuratPengajuanPath, string lampiranPath, string createdBy);
        Task<string?> GenerateIdAsync(GenerateCutiIdRequest dto, string modifiedBy);
        Task<string?> CreateDraftByProdiAsync(CreateCutiProdiRequest dto, string lampiranSuratPengajuanPath, string lampiranPath, string createdBy);
        Task<string?> GenerateIdByProdiAsync(GenerateCutiProdiIdRequest dto, string modifiedBy);
        Task<(bool success, string message, string newStatus)> ApproveCutiAsync(string id, string role, string approvedBy);
        Task<(bool success, string message)> RejectCutiAsync(string id, string role, string keterangan, string modifiedBy);
        Task<(bool success, string message)> UploadSKAsync(string id, string nomorSk, string modifiedBy);
        Task<bool> UpdateAsync(UpdateCutiAkademikRequest dto, string lampiranSuratPengajuanPath, string lampiranPath, string modifiedBy);
        Task<bool> DeleteAsync(string id, string modifiedBy);
        Task<IEnumerable<CutiAkademikRiwayatExcelResponse>> GetRiwayatExcelAsync(string userId);
    }
}