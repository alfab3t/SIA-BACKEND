using astratech_apps_backend.DTOs.MeninggalDunia;
using astratech_apps_backend.Models;

namespace astratech_apps_backend.Repositories.Interfaces
{
    public interface IMeninggalDuniaRepository
    {

        //CREATE
        Task<string> CreateAsync(CreateMeninggalDuniaRequest dto, string createdBy);

        //CREATE WITH MAHASISWA DATA
        Task<string> CreateWithMahasiswaDataAsync(string mhsId, string lampiranFileName, MahasiswaDetailDto mahasiswaData, string createdBy);

        //GET MAHASISWA DETAIL
        Task<MahasiswaDetailDto?> GetMahasiswaDetailAsync(string mhsId);

        //FINALIZE DRAFT TO OFFICIAL
        Task<string> FinalizeAsync(string draftId, string updatedBy);

        //DROPDOWN DATA
        Task<IEnumerable<MahasiswaDropdownDto>> GetMahasiswaListAsync(string? search = null);
        Task<IEnumerable<ProgramStudiDropdownDto>> GetProgramStudiListAsync();

        //STORED PROCEDURE METHODS
        Task<IEnumerable<MahasiswaDropdownSPDto>> GetMahasiswaDropdownSPAsync();
        Task<MahasiswaProdiDto?> GetMahasiswaProdiSPAsync(string mhsId);

        //READ ALL
        Task<(IEnumerable<MeninggalDuniaListDto> Data, int TotalData)>GetAllAsync(GetAllMeninggalDuniaRequest req);

        Task<MeninggalDuniaDetailResponse?> GetDetailAsync(string id);

        Task<MeninggalDuniaReportResponse?> GetReportAsync(string id);



        //READ BY ID
        Task<MeninggalDunia?> GetByIdAsync(string id);

        //UPDATE
        Task<bool> UpdateAsync(string id, UpdateMeninggalDuniaRequest dto, string updatedBy);
        

        //DELETE
        Task<bool> SoftDeleteAsync(string id, string updatedBy);

        //UPDATE SK
        Task<bool> UpdateSKAsync(string id, string sk, string spkb, string updatedBy);

        //UPLOAD SK
        Task<bool> UploadSKAsync(string id, string sk, string spkb, string updatedBy);

        Task<(IEnumerable<RiwayatMeninggalDuniaListDto> Data, int TotalData)>   GetRiwayatAsync(GetRiwayatMeninggalDuniaRequest req);

        Task<IEnumerable<RiwayatMeninggalDuniaExcelResponse>> GetRiwayatExcelAsync(
        string sort,
        string konsentrasi
        );

        Task<bool> UploadSKMeninggalAsync(UploadSKMeninggalRequest request);

        Task<bool> ApproveAsync(string id, ApproveMeninggalDuniaRequest dto);

        Task<bool> RejectAsync(string id, RejectMeninggalDuniaRequest dto);

        //ROLE DETECTION
        Task<string> DetectUserRoleAsync(string username);




    }
}
