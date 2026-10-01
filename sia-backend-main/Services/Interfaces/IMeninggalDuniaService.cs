using astratech_apps_backend.DTOs.MeninggalDunia;
using astratech_apps_backend.Models;


namespace astratech_apps_backend.Services.Interfaces
{
    public interface IMeninggalDuniaService
    {
        //CREATE
        Task<string> CreateAsync(CreateMeninggalDuniaRequest dto, string createdBy);

        //FINALIZE DRAFT TO OFFICIAL
        Task<string> FinalizeAsync(string draftId, string updatedBy);

        //DROPDOWN DATA
        Task<IEnumerable<MahasiswaDropdownDto>> GetMahasiswaListAsync(string? search = null);
        Task<IEnumerable<ProgramStudiDropdownDto>> GetProgramStudiListAsync();
        Task<MahasiswaDetailDto?> GetMahasiswaDetailAsync(string mhsId);

        //STORED PROCEDURE METHODS
        Task<IEnumerable<MahasiswaDropdownSPDto>> GetMahasiswaDropdownAsync();
        Task<MahasiswaProdiDto?> GetMahasiswaProdiAsync(string mhsId);

        //READ ALL
        //Task<IEnumerable<MeninggalDuniaListResponse>> GetAllAsync(string status, string roleId);
        Task<MeninggalDuniaResponse> GetAllAsync(GetAllMeninggalDuniaRequest req);


        //READ SEARCH BY ID
        //Task<MeninggalDuniaResponse?> GetByIdAsync(string id);

        Task<MeninggalDuniaDetailResponse?> GetDetailAsync(string id);

        Task<MeninggalDuniaReportResponse?> GetReportAsync(string id);



        //UPDATE
        Task<bool> UpdateAsync(string id, UpdateMeninggalDuniaRequest dto, string updatedBy);


        //DELETE
        Task<bool> SoftDeleteAsync(string id, string updatedBy);

        //UPDATE BY SK
        Task<bool> UpdateSKAsync(string id, UpdateSKMeninggalDuniaRequest dto, string updatedBy);

        //UPLOAD BY SK
        Task<bool> UploadSKAsync(string id, IFormFile skFile, IFormFile spkbFile, string updatedBy);

        Task<GetRiwayatMeninggalDuniaResponse> GetRiwayatAsync(GetRiwayatMeninggalDuniaRequest req);


        Task<IEnumerable<RiwayatMeninggalDuniaExcelResponse>> GetRiwayatExcelAsync(
        string sort,
        string konsentrasi
        );

        // Method UploadSKMeninggalAsync sudah tidak diperlukan karena kita menggunakan UploadSKAsync
        // Task<bool> UploadSKMeninggalAsync(UploadSKMeninggalRequest request);

        Task<bool> ApproveAsync(string id, ApproveMeninggalDuniaRequest dto);

        Task<bool> RejectAsync(string id, RejectMeninggalDuniaRequest dto);

        //ROLE DETECTION
        Task<string> DetectUserRoleAsync(string username); 



    }
}
