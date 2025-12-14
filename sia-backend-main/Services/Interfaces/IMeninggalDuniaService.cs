using astratech_apps_backend.DTOs.MeninggalDunia;
using astratech_apps_backend.Models;


namespace astratech_apps_backend.Services.Interfaces
{
    public interface IMeninggalDuniaService
    {
        //CREATE
        Task<string> CreateAsync(CreateMeninggalDuniaRequest dto, string createdBy);

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

        Task<bool> UploadSKMeninggalAsync(UploadSKMeninggalRequest request);

        Task<bool> ApproveAsync(string id, ApproveMeninggalDuniaRequest dto);

        Task<bool> RejectAsync(string id, RejectMeninggalDuniaRequest dto); 



    }
}
