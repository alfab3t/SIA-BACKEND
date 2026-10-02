using System.Data;
using Microsoft.Data.SqlClient;
using astratech_apps_backend.DTOs.Common;
using astratech_apps_backend.DTOs.PengunduranDiri;
using astratech_apps_backend.Models;
using astratech_apps_backend.Repositories.Interfaces;

namespace astratech_apps_backend.Repositories.Implementations
{
    public class PengunduranDiriRepository : IPengunduranDiriRepository
    {
        private readonly string _conn;

        public PengunduranDiriRepository(IConfiguration configuration)
        {
            _conn = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("DefaultConnection string not found.");
        }

        public async Task<(IEnumerable<PengunduranDiriListResponse> Data, int TotalData)> GetPendingPaginatedAsync(
            string username, string keyword, string sortBy, string konsentrasi, string status, string role, int page, int pageSize)
        {
            var list = new List<PengunduranDiriListResponse>();
            int totalCount = 0;

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_PengunduranDiri_GetPending", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Username", username ?? string.Empty);
            cmd.Parameters.AddWithValue("@Keyword", keyword ?? string.Empty);
            cmd.Parameters.AddWithValue("@SortBy", string.IsNullOrWhiteSpace(sortBy) ? "a.pdi_created_date DESC" : sortBy);
            cmd.Parameters.AddWithValue("@Konsentrasi", konsentrasi ?? string.Empty);
            cmd.Parameters.AddWithValue("@Role", role ?? string.Empty);
            cmd.Parameters.AddWithValue("@DisplayName", string.Empty);
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

                var pdiId = reader["pdi_id"]?.ToString() ?? string.Empty;
                var mhsId = reader["mhs_id"]?.ToString() ?? string.Empty;
                var mhsNama = reader["mhs_nama"]?.ToString() ?? string.Empty;
                var approveProdi = reader["approve_prodi"]?.ToString() ?? string.Empty;
                var approveDir1 = reader["approve_dir1"]?.ToString() ?? string.Empty;
                var srtNo = reader["srt_no"]?.ToString() ?? string.Empty;
                var pdiStatus = reader["pdi_status"]?.ToString() ?? string.Empty;
                var createdBy = reader["pdi_created_by"]?.ToString() ?? string.Empty;
                var prodiNama = reader["prodi_nama"]?.ToString() ?? string.Empty;
                var konNama = reader["konsentrasi"]?.ToString() ?? (reader["kon_nama"]?.ToString() ?? string.Empty);

                var tanggal = reader["pdi_created_date"] is DBNull
                    ? string.Empty
                    : Convert.ToDateTime(reader["pdi_created_date"]).ToString("yyyy-MM-dd HH:mm");

                var tanggalDisetujui = reader["pdi_app_dir1_date"] is DBNull
                    ? null
                    : Convert.ToDateTime(reader["pdi_app_dir1_date"]).ToString("yyyy-MM-dd HH:mm");

                list.Add(new PengunduranDiriListResponse
                {
                    Id = pdiId,
                    PdiId = pdiId,
                    IdAlternative = pdiId,
                    MhsId = mhsId,
                    NamaMahasiswa = mhsNama,
                    Mahasiswa = mhsNama,
                    ApproveProdi = approveProdi,
                    ApproveDir1 = approveDir1,
                    Tanggal = tanggal,
                    TanggalDisetujui = tanggalDisetujui,
                    SuratNo = srtNo,
                    Status = pdiStatus,
                    CreatedBy = createdBy,
                    ProdiNama = prodiNama,
                    Prodi = prodiNama,
                    Konsentrasi = konNama
                });
            }

            return (list, totalCount);
        }

        public async Task<IEnumerable<PengunduranDiriListResponse>> GetPendingAsync(
            string username, string keyword, string sortBy, string konsentrasi, string status, string role)
        {
            var (data, _) = await GetPendingPaginatedAsync(username, keyword, sortBy, konsentrasi, status, role, 1, 1000);
            return data;
        }

        public async Task<(IEnumerable<PengunduranDiriRiwayatResponse> Data, int TotalData)> GetRiwayatPaginatedAsync(
            string username, string status, string keyword, string sortBy, string konsentrasi, string role, int page, int pageSize)
        {
            var list = new List<PengunduranDiriRiwayatResponse>();
            int totalCount = 0;

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_PengunduranDiri_GetRiwayat", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Username", username ?? string.Empty);
            cmd.Parameters.AddWithValue("@Keyword", keyword ?? string.Empty);
            cmd.Parameters.AddWithValue("@SortBy", string.IsNullOrWhiteSpace(sortBy) ? "a.pdi_created_date DESC" : sortBy);
            cmd.Parameters.AddWithValue("@Konsentrasi", konsentrasi ?? string.Empty);
            cmd.Parameters.AddWithValue("@Role", role ?? string.Empty);
            cmd.Parameters.AddWithValue("@DisplayName", string.Empty);
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

                var pdiId = reader["pdi_id"]?.ToString() ?? string.Empty;
                var mhsId = reader["mhs_id"]?.ToString() ?? string.Empty;
                var mhsNama = reader["mhs_nama"]?.ToString() ?? string.Empty;
                var approveProdi = reader["approve_prodi"]?.ToString() ?? string.Empty;
                var approveDir1 = reader["approve_dir1"]?.ToString() ?? string.Empty;
                var srtNo = reader["srt_no"]?.ToString() ?? string.Empty;
                var pdiStatus = reader["pdi_status"]?.ToString() ?? string.Empty;
                var prodiNama = reader["prodi_nama"]?.ToString() ?? string.Empty;
                var konNama = reader["konsentrasi"]?.ToString() ?? (reader["kon_nama"]?.ToString() ?? string.Empty);

                var tanggal = reader["pdi_created_date"] is DBNull
                    ? string.Empty
                    : Convert.ToDateTime(reader["pdi_created_date"]).ToString("yyyy-MM-dd HH:mm");

                var tanggalDisetujui = reader["pdi_app_dir1_date"] is DBNull
                    ? string.Empty
                    : Convert.ToDateTime(reader["pdi_app_dir1_date"]).ToString("yyyy-MM-dd HH:mm");

                list.Add(new PengunduranDiriRiwayatResponse
                {
                    Id = pdiId,
                    PdiId = pdiId,
                    MhsId = mhsId,
                    ApproveProdi = approveProdi,
                    ApproveDir1 = approveDir1,
                    Tanggal = tanggal,
                    TanggalDisetujui = tanggalDisetujui,
                    SuratNo = srtNo,
                    NamaMahasiswa = mhsNama,
                    Mahasiswa = mhsNama,
                    ProdiNama = prodiNama,
                    Prodi = prodiNama,
                    Konsentrasi = konNama,
                    Status = pdiStatus
                });
            }

            return (list, totalCount);
        }

        public async Task<IEnumerable<PengunduranDiriRiwayatResponse>> GetRiwayatAsync(
            string username, string status, string keyword, string sortBy, string konsentrasi, string role)
        {
            var (data, _) = await GetRiwayatPaginatedAsync(username, status, keyword, sortBy, konsentrasi, role, 1, 1000);
            return data;
        }

        public async Task<IEnumerable<PengunduranDiriRiwayatExcelResponse>> GetRiwayatExcelAsync(
            string konsentrasi, string sortBy)
        {
            var list = new List<PengunduranDiriRiwayatExcelResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_PengunduranDiri_GetExcelRiwayat", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Konsentrasi", konsentrasi ?? string.Empty);
            cmd.Parameters.AddWithValue("@OrderBy", string.IsNullOrWhiteSpace(sortBy) ? "a.pdi_created_date DESC" : sortBy);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var tanggal = reader["tanggal"] is DBNull
                    ? string.Empty
                    : Convert.ToDateTime(reader["tanggal"]).ToString("yyyy-MM-dd HH:mm");

                list.Add(new PengunduranDiriRiwayatExcelResponse
                {
                    NIM = reader["mhs_id"]?.ToString() ?? string.Empty,
                    NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? string.Empty,
                    Konsentrasi = reader["kon_nama"]?.ToString() ?? string.Empty,
                    TanggalPengajuan = tanggal,
                    NoSk = reader["no_sk"]?.ToString() ?? string.Empty,
                    NoPengajuan = reader["no_pengajuan"]?.ToString() ?? string.Empty
                });
            }

            return list;
        }

        public async Task<PengunduranDiriDetailResponse?> GetDetailAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_PengunduranDiri_GetById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@pdi_id", id ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            var pdiId = reader["pdi_id"]?.ToString() ?? string.Empty;
            var mhsId = reader["mhs_id"]?.ToString() ?? string.Empty;
            var mhsNama = reader["mhs_nama"]?.ToString() ?? string.Empty;
            var angkatan = reader["mhs_angkatan"]?.ToString() ?? string.Empty;
            var konSingkatan = reader["kon_singkatan"]?.ToString() ?? string.Empty;
            var konNama = reader["kon_nama"]?.ToString() ?? string.Empty;
            var proNama = reader["pro_nama"]?.ToString() ?? string.Empty;
            var lampiranSurat = reader["pdi_lampiransuratpengajuan"]?.ToString() ?? string.Empty;
            var lampiran = reader["pdi_lampiran"]?.ToString() ?? string.Empty;
            var pdiStatus = reader["pdi_status"]?.ToString() ?? string.Empty;
            var createdBy = reader["pdi_created_by"]?.ToString() ?? string.Empty;
            var sk = reader["pdi_sk"]?.ToString() ?? string.Empty;
            var skpb = reader["pdi_skpb"]?.ToString() ?? string.Empty;
            var srtNo = reader["srt_no"]?.ToString() ?? string.Empty;
            var appProdiBy = reader["pdi_approval_prodi_by"]?.ToString() ?? string.Empty;
            var appDir1By = reader["pdi_approval_dir1_by"]?.ToString() ?? string.Empty;
            var alasanTolak = reader["alasan_tolak"]?.ToString() ?? string.Empty;

            var appProdiDate = reader["pdi_app_prodi_date"] is DBNull
                ? string.Empty
                : Convert.ToDateTime(reader["pdi_app_prodi_date"]).ToString("yyyy-MM-dd HH:mm");

            var appDir1Date = reader["pdi_app_dir1_date"] is DBNull
                ? string.Empty
                : Convert.ToDateTime(reader["pdi_app_dir1_date"]).ToString("yyyy-MM-dd HH:mm");

            return new PengunduranDiriDetailResponse
            {
                Id = pdiId,
                PdiId = pdiId,
                MhsId = mhsId,
                NamaMahasiswa = mhsNama,
                KonsentrasiNama = konNama,
                Konsentrasi = konNama,
                Angkatan = angkatan,
                KonsentrasiSingkatan = konSingkatan,
                LampiranSuratPengajuan = lampiranSurat,
                Lampiran = lampiran,
                Status = pdiStatus,
                CreatedBy = createdBy,
                TanggalSekarang = DateTime.Now.ToString("dd/MM/yyyy"),
                SK = sk,
                Skpb = skpb,
                SuratNo = srtNo,
                NoSkpb = srtNo,
                ProdiNama = proNama,
                Prodi = proNama,
                AppProdiDate = appProdiDate,
                ApprovalProdiBy = appProdiBy,
                AppDir1Date = appDir1Date,
                ApprovalDir1By = appDir1By,
                AlasanTolak = alasanTolak
            };
        }

        public async Task<PengunduranDiri?> GetByIdAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_PengunduranDiri_GetById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@pdi_id", id ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new PengunduranDiri
            {
                Id = reader["pdi_id"]?.ToString() ?? string.Empty,
                MhsId = reader["mhs_id"]?.ToString() ?? string.Empty,
                MhsNama = reader["mhs_nama"]?.ToString() ?? string.Empty,
                MhsAngkatan = reader["mhs_angkatan"]?.ToString() ?? string.Empty,
                KonNama = reader["kon_nama"]?.ToString() ?? string.Empty,
                ProNama = reader["pro_nama"]?.ToString() ?? string.Empty,
                Konsentrasi = reader["kon_singkatan"]?.ToString() ?? string.Empty,
                LampiranSuratPengajuan = reader["pdi_lampiransuratpengajuan"]?.ToString() ?? string.Empty,
                Lampiran = reader["pdi_lampiran"]?.ToString() ?? string.Empty,
                Status = reader["pdi_status"]?.ToString() ?? string.Empty,
                ApprovalProdiBy = reader["pdi_approval_prodi_by"]?.ToString() ?? string.Empty,
                AppProdiDate = reader["pdi_app_prodi_date"] is DBNull ? null : Convert.ToDateTime(reader["pdi_app_prodi_date"]),
                ApprovalDir1By = reader["pdi_approval_dir1_by"]?.ToString() ?? string.Empty,
                AppDir1Date = reader["pdi_app_dir1_date"] is DBNull ? null : Convert.ToDateTime(reader["pdi_app_dir1_date"]),
                AlasanTolak = reader["alasan_tolak"]?.ToString() ?? string.Empty,
                Keterangan = reader["alasan_tolak"]?.ToString() ?? string.Empty,
                SrtNo = reader["srt_no"]?.ToString() ?? string.Empty,
                NoSkpb = reader["pdi_no_skpb"]?.ToString() ?? string.Empty,
                Sk = reader["pdi_sk"]?.ToString() ?? string.Empty,
                Skpb = reader["pdi_skpb"]?.ToString() ?? string.Empty,
                CreatedBy = reader["pdi_created_by"]?.ToString() ?? string.Empty,
                CreatedDate = reader["pdi_created_date"] is DBNull ? null : Convert.ToDateTime(reader["pdi_created_date"])
            };
        }

        public async Task<(bool Success, string Message, string? PdiId)> CreatePengajuanAsync(
            CreatePengunduranDiriRequest dto, string createdBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_PengunduranDiri_CreatePengajuan", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MhsId", dto.MhsId ?? string.Empty);
            cmd.Parameters.AddWithValue("@LampiranSuratPengajuan", dto.LampiranSuratPengajuan ?? string.Empty);
            cmd.Parameters.AddWithValue("@Lampiran", dto.Lampiran ?? string.Empty);
            cmd.Parameters.AddWithValue("@CreatedBy", createdBy ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var resultCode = Convert.ToInt32(reader["ResultCode"]);
                var message = reader["Message"]?.ToString() ?? string.Empty;
                var pdiId = reader["PdiId"]?.ToString();

                return (resultCode > 0, message, pdiId);
            }

            return (false, "Gagal membuat pengajuan Pengunduran Diri.", null);
        }

        public async Task<(bool Success, string Message, string? PdiId)> CreateByProdiAsync(
            CreatePengunduranDiriByProdiRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_PengunduranDiri_CreatePengajuan", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MhsId", dto.MhsId ?? string.Empty);
            cmd.Parameters.AddWithValue("@LampiranSuratPengajuan", dto.LampiranSuratPengajuan ?? string.Empty);
            cmd.Parameters.AddWithValue("@Lampiran", dto.Lampiran ?? string.Empty);
            cmd.Parameters.AddWithValue("@CreatedBy", dto.CreatedBy ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var resultCode = Convert.ToInt32(reader["ResultCode"]);
                var message = reader["Message"]?.ToString() ?? string.Empty;
                var pdiId = reader["PdiId"]?.ToString();

                return (resultCode > 0, message, pdiId);
            }

            return (false, "Gagal membuat pengajuan Pengunduran Diri oleh Prodi.", null);
        }

        public async Task<(bool Success, string Message, string? NoPengajuan)> SubmitDraftAsync(
            string id, string submittedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_PengunduranDiri_Submit", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@PdiId", id ?? string.Empty);
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

            return (false, "Gagal mengajukan pengajuan Pengunduran Diri.", null);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(
            string id, UpdatePengunduranDiriRequest dto, string updatedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_PengunduranDiri_Update", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@PdiId", id ?? string.Empty);
            cmd.Parameters.AddWithValue("@LampiranSuratPengajuan", dto.LampiranSuratPengajuan ?? string.Empty);
            cmd.Parameters.AddWithValue("@Lampiran", dto.Lampiran ?? string.Empty);
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

        public async Task<(bool Success, string Message)> ApproveAsync(
            string id, string role, string approvedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_PengunduranDiri_Setujui", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@pdi_id", id ?? string.Empty);
            cmd.Parameters.AddWithValue("@role", role ?? string.Empty);
            cmd.Parameters.AddWithValue("@approved_by", approvedBy ?? string.Empty);

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
            string id, string role, string reason, string rejectedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_PengunduranDiri_Tolak", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@pdi_id", id ?? string.Empty);
            cmd.Parameters.AddWithValue("@role", role ?? string.Empty);
            cmd.Parameters.AddWithValue("@alasan_tolak", reason ?? string.Empty);

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

        public async Task<(bool Success, string Message)> UploadSKAsync(
            string id, UploadSKPengunduranDiriRequest dto, string updatedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("dbo.SP_PengunduranDiri_UploadSK", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@pdi_id", id ?? string.Empty);
            cmd.Parameters.AddWithValue("@sk", dto.Sk ?? string.Empty);
            cmd.Parameters.AddWithValue("@skpb", dto.Skpb ?? string.Empty);
            cmd.Parameters.AddWithValue("@updated_by", updatedBy ?? string.Empty);

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
            await using var cmd = new SqlCommand("dbo.SP_PengunduranDiri_Delete", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@pdi_id", id ?? string.Empty);
            cmd.Parameters.AddWithValue("@deleted_by", deletedBy ?? string.Empty);

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

        public async Task<IEnumerable<MahasiswaListResponse>> GetMahasiswaListAsync()
        {
            var list = new List<MahasiswaListResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("lpm_getListMahasiswa", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new MahasiswaListResponse
                {
                    Value = reader["mhs_id"]?.ToString() ?? string.Empty,
                    Text = reader["mhs_nama"]?.ToString() ?? string.Empty
                });
            }

            return list;
        }

        public async Task<IEnumerable<MahasiswaByProdiResponse>> GetMahasiswaByProdiAsync(string userId)
        {
            var list = new List<MahasiswaByProdiResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("lpm_getListMahasiswaByProdi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@username", userId ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var mhsId = reader["mhs_id"]?.ToString() ?? string.Empty;
                var mhsNama = reader["mhs_nama"]?.ToString() ?? string.Empty;
                var proId = reader["pro_id"]?.ToString() ?? string.Empty;
                var proNama = reader["pro_nama"]?.ToString() ?? string.Empty;
                var konId = reader["kon_id"]?.ToString() ?? string.Empty;

                list.Add(new MahasiswaByProdiResponse
                {
                    Value = mhsId,
                    Text = mhsNama,
                    NimNama = $"{mhsId} - {mhsNama}",
                    ProdiId = proId,
                    ProdiNama = proNama,
                    KonsentrasiId = konId
                });
            }

            return list;
        }

        public async Task<MahasiswaProdiResponse?> GetMahasiswaProdiAsync(string mhsId)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("lpm_getListMahasiswaByProdi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@username", mhsId ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new MahasiswaProdiResponse
            {
                KonId = reader["kon_id"]?.ToString() ?? string.Empty,
                ProId = reader["pro_id"]?.ToString() ?? string.Empty,
                ProNama = reader["pro_nama"]?.ToString() ?? string.Empty
            };
        }

        public async Task<MahasiswaAngkatanResponse?> GetMahasiswaAngkatanAsync(string mhsId)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getListAngkatanByMahasiswa", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", mhsId ?? string.Empty);
            for (int i = 2; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", string.Empty);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            return new MahasiswaAngkatanResponse
            {
                DulAngkatan = result?.ToString() ?? string.Empty
            };
        }

        public async Task<IEnumerable<MahasiswaListResponse>> GetMahasiswaByKonsentrasiAsync(string username)
        {
            var list = new List<MahasiswaListResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getListMahasiswaByKonsentrasi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@username", username ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new MahasiswaListResponse
                {
                    Value = reader["mhs_id"]?.ToString() ?? string.Empty,
                    Text = reader["mhs_nama"]?.ToString() ?? string.Empty
                });
            }

            return list;
        }

        public async Task<IEnumerable<ProdiOptionResponse>> GetProdiByUserAsync(string username)
        {
            var list = new List<ProdiOptionResponse>();

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
                list.Add(new ProdiOptionResponse
                {
                    Value = reader["pro_id"]?.ToString() ?? string.Empty,
                    Text = reader["pro_nama"]?.ToString() ?? string.Empty
                });
            }

            return list;
        }

        public async Task<IEnumerable<ProdiOptionResponse>> GetListProdiAsync()
        {
            var list = new List<ProdiOptionResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getListProdi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new ProdiOptionResponse
                {
                    Value = reader["pro_id"]?.ToString() ?? string.Empty,
                    Text = reader["pro_nama"]?.ToString() ?? string.Empty
                });
            }

            return list;
        }

        public async Task<BebasTanggunganResponse?> CekBebasTanggunganAsync(string mhsId)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_checkBebasTanggungan", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@mhs_id", mhsId ?? string.Empty);

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

            var isBebas = reader["is_bebas"] != DBNull.Value && Convert.ToBoolean(reader["is_bebas"]);
            var message = reader["message"]?.ToString() ?? string.Empty;

            return new BebasTanggunganResponse
            {
                MhsId = mhsId ?? string.Empty,
                Status = isBebas ? "OK" : "NOK",
                IsBebasTanggungan = isBebas,
                Message = message,
                StatusKeuangan = isBebas ? "OK" : "NOK"
            };
        }

        public async Task<MahasiswaProfilDetailResponse?> GetProfilMahasiswaAsync(string mhsId)
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
