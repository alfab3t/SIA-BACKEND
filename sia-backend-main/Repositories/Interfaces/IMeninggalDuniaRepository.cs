using astratech_apps_backend.DTOs.MeninggalDunia;
using astratech_apps_backend.Models;

namespace astratech_apps_backend.Repositories.Interfaces
{
    public interface IMeninggalDuniaRepository
    {
        Task<(IEnumerable<MeninggalDuniaListDto> Data, int TotalData)> GetAllAsync(GetAllMeninggalDuniaRequest req);
        Task<(IEnumerable<RiwayatMeninggalDuniaListDto> Data, int TotalData)> GetRiwayatAsync(GetRiwayatMeninggalDuniaRequest req);
        Task<MeninggalDuniaDetailResponse?> GetDetailAsync(string id);
        Task<string?> CreateAsync(CreateMeninggalDuniaRequest dto, string lampiranFileName, string createdBy);
        Task<string?> FinalizeAsync(string draftId, string updatedBy);
        Task<bool> UpdateAsync(string id, string lampiranFileName, string updatedBy);
        Task<bool> SoftDeleteAsync(string id, string updatedBy);
        Task<bool> ApproveAsync(string id, string role, string username);
        Task<bool> RejectAsync(string id, string role, string username);
        Task<bool> UploadSKAsync(string id, string skFileName, string spkbFileName, string updatedBy);
        Task<IEnumerable<RiwayatMeninggalDuniaExcelResponse>> GetRiwayatExcelAsync(string sort, string konsentrasi);

        // Helper Dropdown Data Mahasiswa untuk Halaman Add
        Task<IEnumerable<MahasiswaDropdownDto>> GetMahasiswaListAsync(string? search = null);
        Task<MahasiswaDetailDto?> GetMahasiswaDetailAsync(string mhsId);
        Task<MahasiswaProdiDto?> GetMahasiswaProdiAsync(string mhsId);
    }
}
