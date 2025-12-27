using astratech_apps_backend.DTOs.MeninggalDunia;
using astratech_apps_backend.Models;
using astratech_apps_backend.Repositories.Interfaces;
using astratech_apps_backend.Services.Interfaces;

namespace astratech_apps_backend.Services.Implementations
{
    public class MeninggalDuniaService : IMeninggalDuniaService
    {
        private readonly IMeninggalDuniaRepository _repo;

        public MeninggalDuniaService(IMeninggalDuniaRepository repo)
        {
            _repo = repo;
        }


        //CREATE
        public async Task<string> CreateAsync(CreateMeninggalDuniaRequest dto, string createdBy)
        {
            // Get mahasiswa data untuk auto-fill program studi dan angkatan
            var mahasiswaData = await _repo.GetMahasiswaDetailAsync(dto.MhsId);
            if (mahasiswaData == null)
            {
                throw new ArgumentException("Data mahasiswa tidak ditemukan");
            }

            // Handle file upload
            string fileName = "";
            if (dto.LampiranFile != null)
            {
                var folder = Path.Combine("uploads", "meninggal", "lampiran");
                Directory.CreateDirectory(folder);

                fileName = $"{Guid.NewGuid()}_{dto.LampiranFile.FileName}";
                var filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.LampiranFile.CopyToAsync(stream);
            }

            // Create dengan data lengkap
            return await _repo.CreateWithMahasiswaDataAsync(dto.MhsId, fileName, mahasiswaData, createdBy);
        }

        //FINALIZE DRAFT TO OFFICIAL
        public async Task<string> FinalizeAsync(string draftId, string updatedBy)
        {
            return await _repo.FinalizeAsync(draftId, updatedBy);
        }

        //DROPDOWN DATA
        public async Task<IEnumerable<MahasiswaDropdownDto>> GetMahasiswaListAsync(string? search = null)
        {
            return await _repo.GetMahasiswaListAsync(search);
        }

        public async Task<IEnumerable<ProgramStudiDropdownDto>> GetProgramStudiListAsync()
        {
            return await _repo.GetProgramStudiListAsync();
        }

        public async Task<MahasiswaDetailDto?> GetMahasiswaDetailAsync(string mhsId)
        {
            return await _repo.GetMahasiswaDetailAsync(mhsId);
        }

        //STORED PROCEDURE METHODS
        public async Task<IEnumerable<MahasiswaDropdownSPDto>> GetMahasiswaDropdownAsync()
        {
            return await _repo.GetMahasiswaDropdownSPAsync();
        }

        public async Task<MahasiswaProdiDto?> GetMahasiswaProdiAsync(string mhsId)
        {
            return await _repo.GetMahasiswaProdiSPAsync(mhsId);
        }


        public async Task<MeninggalDuniaResponse> GetAllAsync(GetAllMeninggalDuniaRequest req)
        {
            var result = await _repo.GetAllAsync(req);

            return new MeninggalDuniaResponse
            {
                Data = result.Data.ToList(),
                TotalData = result.TotalData,
                TotalHalaman = (int)Math.Ceiling(result.TotalData / (double)req.PageSize)
            };
        }


        ////READ SEARCH BY ID
        //public async Task<MeninggalDuniaResponse?> GetByIdAsync(string id)
        //{
        //    var x = await _repo.GetByIdAsync(id);
        //    if (x == null) return null;

        //    return new MeninggalDuniaResponse
        //    {
        //        Id = x.Id,
        //        MhsId = x.MhsId,
        //        Lampiran = x.Lampiran,
        //        ApproveDir1By = x.ApproveDir1By,
        //        ApproveDir1Date = x.ApproveDir1Date,
        //        SrtNo = x.SrtNo,
        //        NoSpkb = x.NoSpkb,
        //        Sk = x.Sk,
        //        Spkb = x.Spkb,
        //        Status = x.Status,
        //        CreatedBy = x.CreatedBy,
        //        CreatedDate = x.CreatedDate,
        //        ModifiedBy = x.ModifiedBy,
        //        ModifiedDate = x.ModifiedDate
        //    };
        //}

        public async Task<MeninggalDuniaDetailResponse?> GetDetailAsync(string id)
        {
            return await _repo.GetDetailAsync(id);
        }

        public async Task<MeninggalDuniaReportResponse?> GetReportAsync(string id)
        {
            return await _repo.GetReportAsync(id);
        }





        //UPDATE
        public async Task<bool> UpdateAsync(string id, UpdateMeninggalDuniaRequest dto, string updatedBy)
        {
            return await _repo.UpdateAsync(id, dto, updatedBy);
        }


        //DELETE
        public async Task<bool> SoftDeleteAsync(string id, string updatedBy)
        {
            return await _repo.SoftDeleteAsync(id, updatedBy);
        }



        //UPDATE SK
        public async Task<bool> UpdateSKAsync(string id, UpdateSKMeninggalDuniaRequest dto, string updatedBy)
        {
            return await _repo.UpdateSKAsync(id, dto.Sk, dto.Spkb, updatedBy);
        }


        //UPLOAD SK
        public async Task<bool> UploadSKAsync(string id, IFormFile skFile, IFormFile spkbFile, string updatedBy)
        {
            var folder = Path.Combine("uploads", "meninggal");
            Directory.CreateDirectory(folder);

            string sk = "";
            string spkb = "";

            if (skFile != null)
            {
                sk = $"{Guid.NewGuid()}_{skFile.FileName}";
                var path = Path.Combine(folder, sk);
                using var stream = new FileStream(path, FileMode.Create);
                await skFile.CopyToAsync(stream);
            }

            if (spkbFile != null)
            {
                spkb = $"{Guid.NewGuid()}_{spkbFile.FileName}";
                var path = Path.Combine(folder, spkb);
                using var stream = new FileStream(path, FileMode.Create);
                await spkbFile.CopyToAsync(stream);
            }

            return await _repo.UploadSKAsync(id, sk, spkb, updatedBy);
        }

        public async Task<GetRiwayatMeninggalDuniaResponse>
        GetRiwayatAsync(GetRiwayatMeninggalDuniaRequest req)
        {
            var (data, total) = await _repo.GetRiwayatAsync(req);

            return new GetRiwayatMeninggalDuniaResponse
            {
                Data = data.ToList(),
                TotalData = total,
                TotalHalaman = (int)Math.Ceiling(total / (double)req.PageSize)
            };
        }



        public async Task<IEnumerable<RiwayatMeninggalDuniaExcelResponse>> GetRiwayatExcelAsync(
        string sort,
        string konsentrasi
        )
        {
            return await _repo.GetRiwayatExcelAsync(sort, konsentrasi);
        }

        public async Task<bool> UploadSKMeninggalAsync(UploadSKMeninggalRequest request)
        {
            return await _repo.UploadSKMeninggalAsync(request);
        }

        public async Task<bool> ApproveAsync(string id, ApproveMeninggalDuniaRequest dto)
        {
            return await _repo.ApproveAsync(id, dto);
        }

        public async Task<bool> RejectAsync(string id, RejectMeninggalDuniaRequest dto)
        {
            return await _repo.RejectAsync(id, dto);
        }




    }
}
