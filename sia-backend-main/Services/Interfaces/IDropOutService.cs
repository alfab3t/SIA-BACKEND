using astratech_apps_backend.DTOs.DropOut;

namespace astratech_apps_backend.Services.Interfaces
{
    public interface IDropOutService
    {
        //Task<string> CreateAsync(CreateDropOutRequest dto, string createdBy);
        Task<string?> CreatePengajuanDOAsync(CreatePengajuanDORequest dto, string createdBy);
        Task<IEnumerable<DropOutResponse>> GetAllAsync(string keyword, int page, int limit);
        Task<DropOutDetailResponse?> GetDetailAsync(string id);
        Task<DropOutResponse?> GetByIdAsync(string id);
        //Task<IEnumerable<DropOutRiwayatResponse>> GetRiwayatAsync(
        //string username, string keyword, string sortBy, string konsentrasi, string role, string sekprodi);
        Task<IEnumerable<DropOutRiwayatResponse>> GetRiwayatAsync(
        string username, string keyword, string sortBy, string konsentrasi, string role, string displayName);
        Task<IEnumerable<DropOutRiwayatExcelResponse>> GetRiwayatExcelAsync(
        string username, string keyword, string sortBy, string konsentrasi, string role, string sekprodi);
        Task<DropOutGetIdByDraftResponse?> GetIdByDraftAsync(string id);
        Task<bool> UpdateAsync(string id, UpdateDropOutRequest dto, string updatedBy);
        Task<bool> DeleteAsync(string id);
        Task<bool> ApproveByWadirAsync(string id, ApproveDropOutRequest dto);
        Task<bool> RejectByWadirAsync(string id, RejectDropOutRequest dto);
        Task<string?> CheckReportAsync(string id);
        Task<DropOutReportSuketResponse?> GetReportSuketAsync(string suratNo);
        Task<DropOutDownloadSkResponse?> DownloadSKAsync(string droId);
        Task<SKDOReportResponse?> GetReportSKDOAsync(string id);
        Task<List<SKDOReportSubResponse>> GetReportSKDOSubAsync(string id);
        Task<bool> UploadSKDOAsync(UploadSKDORequest request);

        Task<IEnumerable<DropOutPendingResponse>> GetPendingAsync(
          string username,
          string keyword,
          string sortBy,
          string konsentrasi,
          string role,
          string displayName
      );

        Task<IEnumerable<DropOutMahasiswaOptionResponse>>
    GetMahasiswaByKonsentrasiAsync(string konsentrasiId);

        Task<IEnumerable<DropOutProdiOptionResponse>> GetProdiAsync();
        Task<IEnumerable<DropOutKonsentrasiOptionResponse>>
    GetKonsentrasiByProdiAsync(string prodiId, string sekprodiUsername);

        Task<string?> GetAngkatanByMahasiswaAsync(string mhsId);

        Task<MahasiswaProfilResponse?> GetMahasiswaProfilAsync(string mhsId);









    }
}
