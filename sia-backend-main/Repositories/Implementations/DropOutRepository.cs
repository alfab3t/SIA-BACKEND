using System.Data;
using Microsoft.Data.SqlClient;
using astratech_apps_backend.DTOs.Common;
using astratech_apps_backend.DTOs.DropOut;
using astratech_apps_backend.DTOs.PengunduranDiri;
using astratech_apps_backend.Models;
using astratech_apps_backend.Repositories.Interfaces;

namespace astratech_apps_backend.Repositories.Implementations
{
    public class DropOutRepository : IDropOutRepository
    {
        private readonly string _conn;

        public DropOutRepository(IConfiguration configuration)
        {
            _conn = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("DefaultConnection string not found.");
        }

        public async Task<(IEnumerable<DropOutPendingResponse> Data, int TotalData)> GetPendingPaginatedAsync(
            string username, string keyword, string sortBy, string konsentrasi, string role, string displayName, string status, int page, int pageSize)
        {
            var list = new List<DropOutPendingResponse>();
            int totalCount = 0;

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_DropOut_GetPending", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Username", username ?? string.Empty);
            cmd.Parameters.AddWithValue("@Keyword", keyword ?? string.Empty);
            cmd.Parameters.AddWithValue("@SortBy", string.IsNullOrWhiteSpace(sortBy) ? "a.dro_created_date DESC" : sortBy);
            cmd.Parameters.AddWithValue("@Konsentrasi", konsentrasi ?? string.Empty);
            cmd.Parameters.AddWithValue("@Role", role ?? string.Empty);
            cmd.Parameters.AddWithValue("@DisplayName", displayName ?? string.Empty);
            cmd.Parameters.AddWithValue("@Status", status ?? string.Empty);
            cmd.Parameters.AddWithValue("@Page", page < 1 ? 1 : page);
            cmd.Parameters.AddWithValue("@PageSize", pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize));

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (totalCount == 0 && reader["TotalCount"] != DBNull.Value)
                {
                    totalCount = Convert.ToInt32(reader["TotalCount"]);
                }

                var droId = reader["dro_id"]?.ToString() ?? string.Empty;
                var noPengajuan = reader["dro_no_pengajuan"]?.ToString() ?? string.Empty;
                var mhsId = reader["mhs_id"]?.ToString() ?? string.Empty;
                var mhsNama = reader["mhs_nama"]?.ToString() ?? string.Empty;
                var konNama = reader["kon_nama"]?.ToString() ?? string.Empty;
                var proNama = reader["pro_nama"]?.ToString() ?? string.Empty;
                var droStatus = reader["dro_status"]?.ToString() ?? string.Empty;
                var createdBy = reader["dro_created_by"]?.ToString() ?? string.Empty;
                var createdDate = reader["dro_created_date"] is DBNull
                    ? string.Empty
                    : Convert.ToDateTime(reader["dro_created_date"]).ToString("yyyy-MM-dd HH:mm");

                list.Add(new DropOutPendingResponse
                {
                    Id = droId,
                    DroId = droId,
                    NoPengajuan = noPengajuan,
                    SuratNo = noPengajuan,
                    MhsId = mhsId,
                    Mahasiswa = mhsNama,
                    NamaMahasiswa = mhsNama,
                    Konsentrasi = konNama,
                    Prodi = proNama,
                    CreatedDate = createdDate,
                    TanggalPengajuan = createdDate,
                    CreatedBy = createdBy,
                    DibuatOleh = createdBy,
                    Status = droStatus
                });
            }

            return (list, totalCount);
        }

        public async Task<IEnumerable<DropOutPendingResponse>> GetPendingAsync(
            string username, string keyword, string sortBy, string konsentrasi, string role, string displayName)
        {
            var list = new List<DropOutPendingResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_DropOut_GetPendingAll", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Username", username ?? string.Empty);
            cmd.Parameters.AddWithValue("@Keyword", keyword ?? string.Empty);
            cmd.Parameters.AddWithValue("@SortBy", string.IsNullOrWhiteSpace(sortBy) ? "a.dro_created_date DESC" : sortBy);
            cmd.Parameters.AddWithValue("@Konsentrasi", konsentrasi ?? string.Empty);
            cmd.Parameters.AddWithValue("@Role", role ?? string.Empty);
            cmd.Parameters.AddWithValue("@DisplayName", displayName ?? string.Empty);
            cmd.Parameters.AddWithValue("@Status", string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var droId = reader["dro_id"]?.ToString() ?? string.Empty;
                var noPengajuan = reader["dro_no_pengajuan"]?.ToString() ?? string.Empty;
                var mhsId = reader["mhs_id"]?.ToString() ?? string.Empty;
                var mhsNama = reader["mhs_nama"]?.ToString() ?? string.Empty;
                var konNama = reader["kon_nama"]?.ToString() ?? string.Empty;
                var proNama = reader["pro_nama"]?.ToString() ?? string.Empty;
                var droStatus = reader["dro_status"]?.ToString() ?? string.Empty;
                var createdBy = reader["dro_created_by"]?.ToString() ?? string.Empty;
                var createdDate = reader["dro_created_date"] is DBNull
                    ? string.Empty
                    : Convert.ToDateTime(reader["dro_created_date"]).ToString("yyyy-MM-dd HH:mm");

                list.Add(new DropOutPendingResponse
                {
                    Id = droId,
                    DroId = droId,
                    NoPengajuan = noPengajuan,
                    SuratNo = noPengajuan,
                    MhsId = mhsId,
                    Mahasiswa = mhsNama,
                    NamaMahasiswa = mhsNama,
                    Konsentrasi = konNama,
                    Prodi = proNama,
                    CreatedDate = createdDate,
                    TanggalPengajuan = createdDate,
                    CreatedBy = createdBy,
                    DibuatOleh = createdBy,
                    Status = droStatus
                });
            }

            return list;
        }

        public async Task<(IEnumerable<DropOutRiwayatResponse> Data, int TotalData)> GetRiwayatPaginatedAsync(
            string username, string keyword, string sortBy, string konsentrasi, string role, string displayName, string status, int page, int pageSize)
        {
            var list = new List<DropOutRiwayatResponse>();
            int totalCount = 0;

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_DropOut_GetRiwayat", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Username", username ?? string.Empty);
            cmd.Parameters.AddWithValue("@Keyword", keyword ?? string.Empty);
            cmd.Parameters.AddWithValue("@SortBy", string.IsNullOrWhiteSpace(sortBy) ? "a.dro_created_date DESC" : sortBy);
            cmd.Parameters.AddWithValue("@Konsentrasi", konsentrasi ?? string.Empty);
            cmd.Parameters.AddWithValue("@Role", role ?? string.Empty);
            cmd.Parameters.AddWithValue("@DisplayName", displayName ?? string.Empty);
            cmd.Parameters.AddWithValue("@Status", status ?? string.Empty);
            cmd.Parameters.AddWithValue("@Page", page < 1 ? 1 : page);
            cmd.Parameters.AddWithValue("@PageSize", pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize));

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (totalCount == 0 && reader["TotalCount"] != DBNull.Value)
                {
                    totalCount = Convert.ToInt32(reader["TotalCount"]);
                }

                var droId = reader["dro_id"]?.ToString() ?? string.Empty;
                var noPengajuan = reader["dro_no_pengajuan"]?.ToString() ?? string.Empty;
                var mhsId = reader["mhs_id"]?.ToString() ?? string.Empty;
                var mhsNama = reader["mhs_nama"]?.ToString() ?? string.Empty;
                var konNama = reader["kon_nama"]?.ToString() ?? string.Empty;
                var proNama = reader["pro_nama"]?.ToString() ?? string.Empty;
                var droStatus = reader["dro_status"]?.ToString() ?? string.Empty;
                var createdBy = reader["dro_created_by"]?.ToString() ?? string.Empty;
                var sk = reader["dro_sk"]?.ToString() ?? string.Empty;
                var createdDate = reader["dro_created_date"] is DBNull
                    ? string.Empty
                    : Convert.ToDateTime(reader["dro_created_date"]).ToString("yyyy-MM-dd HH:mm");

                list.Add(new DropOutRiwayatResponse
                {
                    Id = droId,
                    DroId = droId,
                    MhsId = mhsId,
                    TanggalPengajuan = createdDate,
                    CreatedDate = createdDate,
                    DibuatOleh = createdBy,
                    CreatedBy = createdBy,
                    NamaMahasiswa = mhsNama,
                    Mahasiswa = mhsNama,
                    Prodi = proNama,
                    Konsentrasi = konNama,
                    NoSkDo = sk,
                    SuratNo = noPengajuan,
                    Status = droStatus
                });
            }

            return (list, totalCount);
        }

        public async Task<IEnumerable<DropOutRiwayatResponse>> GetRiwayatAsync(
            string username, string keyword, string sortBy, string konsentrasi, string role, string displayName)
        {
            var (data, _) = await GetRiwayatPaginatedAsync(
                username, keyword, sortBy, konsentrasi, role, displayName, string.Empty, 1, 1000);
            return data;
        }

        public async Task<IEnumerable<DropOutRiwayatExcelResponse>> GetRiwayatExcelAsync(
            string username, string keyword, string sortBy, string konsentrasi, string role, string displayName)
        {
            var list = new List<DropOutRiwayatExcelResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_DropOut_GetExcelRiwayat", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Username", username ?? string.Empty);
            cmd.Parameters.AddWithValue("@Keyword", keyword ?? string.Empty);
            cmd.Parameters.AddWithValue("@SortBy", string.IsNullOrWhiteSpace(sortBy) ? "a.dro_created_date DESC" : sortBy);
            cmd.Parameters.AddWithValue("@Konsentrasi", konsentrasi ?? string.Empty);
            cmd.Parameters.AddWithValue("@Role", role ?? string.Empty);
            cmd.Parameters.AddWithValue("@DisplayName", displayName ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var createdDate = reader["dro_created_date"] is DBNull
                    ? string.Empty
                    : Convert.ToDateTime(reader["dro_created_date"]).ToString("yyyy-MM-dd HH:mm");

                list.Add(new DropOutRiwayatExcelResponse
                {
                    NIM = reader["mhs_id"]?.ToString() ?? string.Empty,
                    NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? string.Empty,
                    Konsentrasi = reader["kon_nama"]?.ToString() ?? string.Empty,
                    TanggalPengajuan = createdDate,
                    NoSK = reader["dro_sk"]?.ToString() ?? string.Empty,
                    NoPengajuan = reader["dro_no_pengajuan"]?.ToString() ?? string.Empty
                });
            }

            return list;
        }

        public async Task<DropOutDetailResponse?> GetDetailAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_DropOut_GetById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@DroId", id ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            var droId = reader["dro_id"]?.ToString() ?? string.Empty;
            var noPengajuan = reader["dro_no_pengajuan"]?.ToString() ?? string.Empty;
            var mhsId = reader["mhs_id"]?.ToString() ?? string.Empty;
            var mhsNama = reader["mhs_nama"]?.ToString() ?? string.Empty;
            var konNama = reader["kon_nama"]?.ToString() ?? string.Empty;
            var proNama = reader["pro_nama"]?.ToString() ?? string.Empty;
            var angkatan = reader["mhs_angkatan"]?.ToString() ?? string.Empty;
            var email = reader["mhs_email"]?.ToString() ?? string.Empty;
            var menimbang = reader["dro_menimbang"]?.ToString() ?? string.Empty;
            var mengingat = reader["dro_mengingat"]?.ToString() ?? string.Empty;
            var lampiran = reader["dro_lampiran"]?.ToString() ?? string.Empty;
            var lampiranSurat = reader["dro_lampiran_surat_pengajuan"]?.ToString() ?? string.Empty;
            var status = reader["dro_status"]?.ToString() ?? string.Empty;
            var createdBy = reader["dro_created_by"]?.ToString() ?? string.Empty;
            var sk = reader["dro_sk"]?.ToString() ?? string.Empty;
            var skpb = reader["dro_skpb"]?.ToString() ?? string.Empty;
            var alasanTolak = reader["dro_alasan_tolak"]?.ToString() ?? string.Empty;
            var catatanWadir1 = reader["dro_catatan_wadir1"]?.ToString() ?? string.Empty;
            var catatanDirektur = reader["dro_catatan_direktur"]?.ToString() ?? string.Empty;

            var createdDate = reader["dro_created_date"] is DBNull
                ? string.Empty
                : Convert.ToDateTime(reader["dro_created_date"]).ToString("yyyy-MM-dd HH:mm");
            var accWadir1Date = reader["dro_tgl_acc_wadir1"] is DBNull
                ? string.Empty
                : Convert.ToDateTime(reader["dro_tgl_acc_wadir1"]).ToString("yyyy-MM-dd HH:mm");
            var accDirDate = reader["dro_tgl_acc_direktur"] is DBNull
                ? string.Empty
                : Convert.ToDateTime(reader["dro_tgl_acc_direktur"]).ToString("yyyy-MM-dd HH:mm");

            return new DropOutDetailResponse
            {
                Id = droId,
                DroId = droId,
                NoPengajuan = noPengajuan,
                MhsId = mhsId,
                MhsText = $"{mhsId} - {mhsNama}",
                NamaMahasiswa = mhsNama,
                Konsentrasi = konNama,
                Prodi = proNama,
                Angkatan = angkatan,
                Email = email,
                Menimbang = menimbang,
                Mengingat = mengingat,
                Lampiran = lampiran,
                LampiranSuratPengajuan = lampiranSurat,
                Status = status,
                CreatedBy = createdBy,
                CreatedDate = createdDate,
                Sk = sk,
                Skpb = skpb,
                ApproveWadir1Date = accWadir1Date,
                ApproveWadir1By = catatanWadir1,
                ApproveDirDate = accDirDate,
                ApproveDirBy = catatanDirektur,
                AlasanTolak = alasanTolak,
                CatatanWadir1 = catatanWadir1,
                CatatanDirektur = catatanDirektur,
                SuratKeteranganNo = noPengajuan
            };
        }

        public async Task<DropOut?> GetByIdAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_DropOut_GetById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@DroId", id ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new DropOut
            {
                Id = reader["dro_id"]?.ToString() ?? string.Empty,
                NoPengajuan = reader["dro_no_pengajuan"]?.ToString() ?? string.Empty,
                MhsId = reader["mhs_id"]?.ToString() ?? string.Empty,
                MhsNama = reader["mhs_nama"]?.ToString() ?? string.Empty,
                MhsAngkatan = reader["mhs_angkatan"]?.ToString() ?? string.Empty,
                MhsEmail = reader["mhs_email"]?.ToString() ?? string.Empty,
                KonId = reader["kon_id"]?.ToString() ?? string.Empty,
                KonNama = reader["kon_nama"]?.ToString() ?? string.Empty,
                ProId = reader["pro_id"]?.ToString() ?? string.Empty,
                ProNama = reader["pro_nama"]?.ToString() ?? string.Empty,
                Menimbang = reader["dro_menimbang"]?.ToString() ?? string.Empty,
                Mengingat = reader["dro_mengingat"]?.ToString() ?? string.Empty,
                Lampiran = reader["dro_lampiran"]?.ToString() ?? string.Empty,
                LampiranSuratPengajuan = reader["dro_lampiran_surat_pengajuan"]?.ToString() ?? string.Empty,
                Sk = reader["dro_sk"]?.ToString() ?? string.Empty,
                Skpb = reader["dro_skpb"]?.ToString() ?? string.Empty,
                CatatanWadir1 = reader["dro_catatan_wadir1"]?.ToString() ?? string.Empty,
                CatatanDirektur = reader["dro_catatan_direktur"]?.ToString() ?? string.Empty,
                TglAccWadir1 = reader["dro_tgl_acc_wadir1"] is DBNull ? null : Convert.ToDateTime(reader["dro_tgl_acc_wadir1"]),
                TglAccDirektur = reader["dro_tgl_acc_direktur"] is DBNull ? null : Convert.ToDateTime(reader["dro_tgl_acc_direktur"]),
                AlasanTolak = reader["dro_alasan_tolak"]?.ToString() ?? string.Empty,
                Status = reader["dro_status"]?.ToString() ?? string.Empty,
                CreatedBy = reader["dro_created_by"]?.ToString() ?? string.Empty,
                CreatedDate = reader["dro_created_date"] is DBNull ? null : Convert.ToDateTime(reader["dro_created_date"]),
                ModifiedBy = reader["dro_updated_by"]?.ToString() ?? string.Empty,
                ModifiedDate = reader["dro_updated_date"] is DBNull ? null : Convert.ToDateTime(reader["dro_updated_date"])
            };
        }

        public async Task<(bool Success, string Message, string? NewId)> CreatePengajuanDOAsync(
            CreatePengajuanDORequest dto, string createdBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_DropOut_CreatePengajuan", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MhsId", dto.MhsId ?? string.Empty);
            cmd.Parameters.AddWithValue("@Menimbang", dto.Menimbang ?? string.Empty);
            cmd.Parameters.AddWithValue("@Mengingat", dto.Mengingat ?? string.Empty);
            cmd.Parameters.AddWithValue("@Lampiran", dto.Lampiran ?? string.Empty);
            cmd.Parameters.AddWithValue("@LampiranSurat", dto.LampiranSuratPengajuan ?? string.Empty);
            cmd.Parameters.AddWithValue("@CreatedBy", createdBy ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var resultCode = Convert.ToInt32(reader["ResultCode"]);
                var message = reader["Message"]?.ToString() ?? string.Empty;
                var droId = reader["DroId"]?.ToString();

                return (resultCode > 0, message, droId);
            }

            return (false, "Gagal membuat pengajuan Drop Out.", null);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(
            string id, UpdateDropOutRequest dto, string updatedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_DropOut_Update", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@DroId", id ?? string.Empty);
            cmd.Parameters.AddWithValue("@Menimbang", dto.Menimbang ?? string.Empty);
            cmd.Parameters.AddWithValue("@Mengingat", dto.Mengingat ?? string.Empty);
            cmd.Parameters.AddWithValue("@Lampiran", dto.Lampiran ?? string.Empty);
            cmd.Parameters.AddWithValue("@LampiranSurat", dto.LampiranSuratPengajuan ?? string.Empty);
            cmd.Parameters.AddWithValue("@ModifiedBy", updatedBy ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var resultCode = Convert.ToInt32(reader["ResultCode"]);
                var message = reader["Message"]?.ToString() ?? string.Empty;
                return (resultCode > 0, message);
            }

            return (false, "Gagal memperbarui pengajuan.");
        }

        public async Task<(bool Success, string Message, string? NoPengajuan)> SubmitDraftAsync(
            string id, string submittedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_DropOut_Submit", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@DroId", id ?? string.Empty);
            cmd.Parameters.AddWithValue("@SubmittedBy", submittedBy ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var resultCode = Convert.ToInt32(reader["ResultCode"]);
                var message = reader["Message"]?.ToString() ?? string.Empty;
                var noPengajuan = reader["NoPengajuan"]?.ToString();

                return (resultCode > 0, message, noPengajuan);
            }

            return (false, "Gagal mengajukan pengajuan Drop Out.", null);
        }

        public async Task<(bool Success, string Message)> ApproveAsync(
            string id, string role, string catatan, string approvedBy)
        {
            // Ambil detail untuk menentukan level persetujuan (Wadir 1 atau Direktur)
            var detail = await GetByIdAsync(id);
            if (detail == null)
                return (false, "Data Drop Out tidak ditemukan.");

            string spName = detail.Status == "Belum Disetujui Direktur" || role == "ROL03"
                ? "dbo.SP_DropOut_SetujuiDirektur"
                : "dbo.SP_DropOut_SetujuiWadir1";

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand(spName, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@DroId", id ?? string.Empty);
            cmd.Parameters.AddWithValue("@Catatan", catatan ?? string.Empty);
            cmd.Parameters.AddWithValue("@UpdatedBy", approvedBy ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var resultCode = Convert.ToInt32(reader["ResultCode"]);
                var message = reader["Message"]?.ToString() ?? string.Empty;
                return (resultCode > 0, message);
            }

            return (false, "Gagal menyetujui pengajuan.");
        }

        public async Task<(bool Success, string Message)> RejectAsync(
            string id, string catatan, string rejectedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_DropOut_Tolak", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@DroId", id ?? string.Empty);
            cmd.Parameters.AddWithValue("@Catatan", catatan ?? string.Empty);
            cmd.Parameters.AddWithValue("@UpdatedBy", rejectedBy ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var resultCode = Convert.ToInt32(reader["ResultCode"]);
                var message = reader["Message"]?.ToString() ?? string.Empty;
                return (resultCode > 0, message);
            }

            return (false, "Gagal menolak pengajuan.");
        }

        public async Task<(bool Success, string Message)> UploadSKDOAsync(UploadSKDORequest request)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_DropOut_UploadSK", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@DroId", request.DroId ?? string.Empty);
            cmd.Parameters.AddWithValue("@SkFile", request.SK ?? string.Empty);
            cmd.Parameters.AddWithValue("@SkpbFile", request.SKPB ?? string.Empty);
            cmd.Parameters.AddWithValue("@UpdatedBy", request.ModifiedBy ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var resultCode = Convert.ToInt32(reader["ResultCode"]);
                var message = reader["Message"]?.ToString() ?? string.Empty;
                return (resultCode > 0, message);
            }

            return (false, "Gagal menyimpan berkas SK.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(string id, string deletedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_DropOut_Delete", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@DroId", id ?? string.Empty);
            cmd.Parameters.AddWithValue("@DeletedBy", deletedBy ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var resultCode = Convert.ToInt32(reader["ResultCode"]);
                var message = reader["Message"]?.ToString() ?? string.Empty;
                return (resultCode > 0, message);
            }

            return (false, "Gagal menghapus pengajuan.");
        }

        public async Task<DropOutDownloadSkResponse?> DownloadSKAsync(string droId)
        {
            var detail = await GetByIdAsync(droId);
            if (detail == null) return null;

            return new DropOutDownloadSkResponse
            {
                Sk = detail.Sk,
                Skpb = detail.Skpb
            };
        }

        public async Task<IEnumerable<DropOutMahasiswaOptionResponse>> GetMahasiswaByKonsentrasiAsync(string konsentrasiId)
        {
            var result = new List<DropOutMahasiswaOptionResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_DropOut_GetMahasiswaByKonsentrasi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@KonsentrasiId", konsentrasiId ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new DropOutMahasiswaOptionResponse
                {
                    Value = reader["mhs_id"]?.ToString() ?? string.Empty,
                    Text = reader["mhs_nama"]?.ToString() ?? string.Empty
                });
            }

            return result;
        }

        public async Task<IEnumerable<DropOutProdiOptionResponse>> GetProdiAsync(string username)
        {
            var result = new List<DropOutProdiOptionResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getListProdibySekprod", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@username", username ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new DropOutProdiOptionResponse
                {
                    Value = reader["pro_id"]?.ToString() ?? string.Empty,
                    Text = reader["pro_nama"]?.ToString() ?? string.Empty
                });
            }

            return result;
        }

        public async Task<IEnumerable<DropOutProdiOptionResponse>> GetListProdiAsync()
        {
            var result = new List<DropOutProdiOptionResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getListProdi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new DropOutProdiOptionResponse
                {
                    Value = reader["pro_id"]?.ToString() ?? string.Empty,
                    Text = reader["pro_nama"]?.ToString() ?? string.Empty
                });
            }

            return result;
        }

        public async Task<IEnumerable<DropOutKonsentrasiOptionResponse>> GetKonsentrasiByProdiAsync(string prodiId, string sekprodiUsername)
        {
            var result = new List<DropOutKonsentrasiOptionResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getListKonsentrasiByProdi2", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@ProdiId", prodiId ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new DropOutKonsentrasiOptionResponse
                {
                    Value = reader["kon_id"]?.ToString() ?? string.Empty,
                    Text = reader["kon_nama"]?.ToString() ?? string.Empty
                });
            }

            return result;
        }

        public async Task<string?> GetAngkatanByMahasiswaAsync(string mhsId)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_DropOut_GetAngkatanByMahasiswa", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MhsId", mhsId ?? string.Empty);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString();
        }

        public async Task<BebasTanggunganResponse?> CekBebasTanggunganAsync(string mhsId)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_DropOut_CheckBebasTanggungan", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MhsId", mhsId ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return new BebasTanggunganResponse
                {
                    MhsId = mhsId ?? string.Empty,
                    Status = "NOK",
                    IsBebasTanggungan = false,
                    Message = "Data mahasiswa tidak ditemukan."
                };
            }

            var isBebas = Convert.ToInt32(reader["IsBebas"]) == 1;
            var message = reader["Message"]?.ToString() ?? string.Empty;

            return new BebasTanggunganResponse
            {
                MhsId = mhsId ?? string.Empty,
                Status = isBebas ? "OK" : "NOK",
                IsBebasTanggungan = isBebas,
                Message = message,
                StatusKeuangan = isBebas ? "OK" : "NOK"
            };
        }

        public async Task<MahasiswaProfilDetailResponse?> GetProfilMahasiswaDetailAsync(string mhsId)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getProfilMahasiswa", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@mhs_id", mhsId ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new MahasiswaProfilDetailResponse
            {
                MhsId = reader["mhs_id"]?.ToString() ?? string.Empty,
                MhsNama = reader["mhs_nama"]?.ToString() ?? string.Empty,
                Prodi = reader["pro_nama"]?.ToString() ?? (reader["kon_nama"]?.ToString() ?? string.Empty),
                Angkatan = reader["mhs_angkatan"]?.ToString() ?? string.Empty,
                StatusKuliah = reader["mhs_status"]?.ToString() ?? string.Empty,
                Email = reader["mhs_email"]?.ToString() ?? string.Empty,
                Hp = reader["mhs_tlp"]?.ToString() ?? string.Empty
            };
        }
    }
}
