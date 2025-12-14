using astratech_apps_backend.DTOs.CutiAkademik;
using astratech_apps_backend.DTOs.MeninggalDunia;

namespace astratech_apps_backend.Services.Interfaces
{
    public interface ICutiAkademikService
    {
        Task<string?> CreateDraftAsync(CreateDraftCutiRequest dto);
        Task<string?> GenerateIdAsync(GenerateCutiIdRequest dto);
        Task<string?> CreateDraftByProdiAsync(CreateCutiProdiRequest dto);
        Task<string?> GenerateIdByProdiAsync(GenerateCutiProdiIdRequest dto);
        Task<bool> CreateSKCutiAkademikAsync(CreateSKCutiAkademikRequest dto);
        Task<List<CutiAkademikListResponse>> GetListResponseAsync(
          string? status,
          string? search,
          string? urut,
          int pageNumber,
          int pageSize
           );
        Task<CutiAkademikDetailResponse?> GetDetailAsync(string id);
        Task<CutiAkademikNotifResponse?> GetDetailNotifAsync(string id);
        Task<IEnumerable<CutiAkademikRiwayatResponse>> GetRiwayatAsync(
        string username, string status, string keyword);
        Task<bool> UpdateAsync(string id, UpdateCutiAkademikRequest dto);
        Task<bool> DeleteAsync(string id, string modifiedBy);
        Task<bool> ApproveAsync(string id, CutiAkademikApproveRequest request);
        Task<bool> ApproveProdiAsync(string id, CutiAkademikApproveProdiRequest request);
        Task<bool> RejectAsync(string id, CutiAkademikRejectRequest request);



    }
}
