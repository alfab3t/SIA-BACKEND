using astratech_apps_backend.DTOs.CutiAkademik;
using astratech_apps_backend.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace astratech_apps_backend.Repositories.Implementations
{
    public class CutiAkademikRepository(IConfiguration config) : ICutiAkademikRepository
    {
        private readonly string _conn = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
            config.GetConnectionString("DefaultConnection")!,
            Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
        );

        public async Task<(IEnumerable<CutiAkademikListResponse>, int totalData)> GetAllAsync(GetAllCutiAkademikRequest dto)
        {
            var list = new List<CutiAkademikListResponse>();
            int totalData = 0;

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@SearchKeyword", dto.SearchKeyword ?? string.Empty);
            cmd.Parameters.AddWithValue("@Status", dto.Status ?? string.Empty);
            cmd.Parameters.AddWithValue("@KonsentrasiId", dto.KonsentrasiId);
            cmd.Parameters.AddWithValue("@MahasiswaId", dto.MahasiswaId ?? string.Empty);
            cmd.Parameters.AddWithValue("@UserId", dto.UserId ?? string.Empty);
            cmd.Parameters.AddWithValue("@RolId", dto.RolId ?? string.Empty);
            cmd.Parameters.AddWithValue("@OrderBy", string.IsNullOrEmpty(dto.OrderBy) ? "tanggal_desc" : dto.OrderBy);
            cmd.Parameters.AddWithValue("@PageNumber", dto.PageNumber < 1 ? 1 : dto.PageNumber);
            cmd.Parameters.AddWithValue("@PageSize", dto.PageSize < 1 ? 10 : dto.PageSize);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (totalData == 0 && !reader.IsDBNull(reader.GetOrdinal("TotalData")))
                {
                    totalData = Convert.ToInt32(reader["TotalData"]);
                }

                list.Add(new CutiAkademikListResponse
                {
                    Id = reader["cak_id"]?.ToString() ?? string.Empty,
                    IdDisplay = reader["id_display"]?.ToString() ?? string.Empty,
                    MhsId = reader["mhs_id"]?.ToString() ?? string.Empty,
                    NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? string.Empty,
                    Prodi = reader["kon_nama"]?.ToString() ?? string.Empty,
                    TahunAjaran = reader["cak_tahunajaran"]?.ToString() ?? string.Empty,
                    Semester = reader["cak_semester"]?.ToString() ?? string.Empty,
                    ApproveProdi = reader["approve_prodi"]?.ToString() ?? string.Empty,
                    ApproveDir1 = reader["approve_dir1"]?.ToString() ?? string.Empty,
                    Tanggal = reader["tanggal"]?.ToString() ?? string.Empty,
                    SuratNo = reader["srt_no"]?.ToString() ?? string.Empty,
                    Status = reader["status"]?.ToString() ?? string.Empty
                });
            }

            return (list, totalData);
        }

        public async Task<(IEnumerable<CutiAkademikListResponse>, int totalData)> GetRiwayatAsync(GetAllCutiAkademikRequest dto)
        {
            var list = new List<CutiAkademikListResponse>();
            int totalData = 0;

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataRiwayatCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@SearchKeyword", dto.SearchKeyword ?? string.Empty);
            cmd.Parameters.AddWithValue("@Status", dto.Status ?? string.Empty);
            cmd.Parameters.AddWithValue("@KonsentrasiId", dto.KonsentrasiId);
            cmd.Parameters.AddWithValue("@UserId", dto.UserId ?? string.Empty);
            cmd.Parameters.AddWithValue("@OrderBy", string.IsNullOrEmpty(dto.OrderBy) ? "tanggal_desc" : dto.OrderBy);
            cmd.Parameters.AddWithValue("@PageNumber", dto.PageNumber < 1 ? 1 : dto.PageNumber);
            cmd.Parameters.AddWithValue("@PageSize", dto.PageSize < 1 ? 10 : dto.PageSize);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (totalData == 0 && !reader.IsDBNull(reader.GetOrdinal("TotalData")))
                {
                    totalData = Convert.ToInt32(reader["TotalData"]);
                }

                list.Add(new CutiAkademikListResponse
                {
                    Id = reader["cak_id"]?.ToString() ?? string.Empty,
                    IdDisplay = reader["id_display"]?.ToString() ?? string.Empty,
                    MhsId = reader["mhs_id"]?.ToString() ?? string.Empty,
                    NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? string.Empty,
                    Prodi = reader["kon_nama"]?.ToString() ?? string.Empty,
                    TahunAjaran = reader["cak_tahunajaran"]?.ToString() ?? string.Empty,
                    Semester = reader["cak_semester"]?.ToString() ?? string.Empty,
                    ApproveProdi = reader["approve_prodi"]?.ToString() ?? string.Empty,
                    ApproveDir1 = reader["approve_dir1"]?.ToString() ?? string.Empty,
                    Tanggal = reader["tanggal"]?.ToString() ?? string.Empty,
                    SuratNo = reader["srt_no"]?.ToString() ?? string.Empty,
                    Status = reader["status"]?.ToString() ?? string.Empty
                });
            }

            return (list, totalData);
        }

        public async Task<CutiAkademikDetailResponse?> GetDetailAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_detailCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@CutiAkademikId", id);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new CutiAkademikDetailResponse
                {
                    Id = reader["cak_id"]?.ToString(),
                    MhsId = reader["mhs_id"]?.ToString(),
                    Mahasiswa = reader["mhs_nama"]?.ToString(),
                    Konsentrasi = reader["kon_nama"]?.ToString(),
                    Angkatan = reader["mhs_angkatan"]?.ToString(),
                    KonsentrasiSingkatan = reader["kon_singkatan"]?.ToString(),
                    TahunAjaran = reader["cak_tahunajaran"]?.ToString(),
                    Semester = reader["cak_semester"]?.ToString(),
                    LampiranSP = reader["cak_lampiran_suratpengajuan"]?.ToString(),
                    Lampiran = reader["cak_lampiran"]?.ToString(),
                    Status = reader["cak_status"]?.ToString(),
                    CreatedBy = reader["cak_created_by"]?.ToString(),
                    TglPengajuan = reader["tgl"]?.ToString(),
                    Sk = reader["cak_sk"]?.ToString(),
                    SrtNo = reader["srt_no"]?.ToString(),
                    ProdiNama = reader["pro_nama"]?.ToString(),
                    Kaprodi = reader["kaprod"]?.ToString(),
                    AppProdiDate = reader["cak_app_prodi_date"]?.ToString(),
                    ApprovalProdi = reader["cak_approval_prodi"]?.ToString(),
                    AppDir1Date = reader["cak_app_dir1_date"]?.ToString(),
                    ApprovalDir1 = reader["cak_approval_dir1"]?.ToString(),
                    AppDakapDate = reader["cak_app_dakap_date"]?.ToString(),
                    ApprovalDakap = reader["cak_approval_dakap"]?.ToString(),
                    Alamat = reader["mhs_alamat"]?.ToString(),
                    Menimbang = reader["cak_menimbang"]?.ToString(),
                    Keterangan = reader["cak_keterangan"]?.ToString(),
                    BulanCuti = reader["BulanCuti"]?.ToString(),
                    Direktur = reader["direktur"]?.ToString(),
                    Wadir1 = reader["wadir1"]?.ToString(),
                    Wadir2 = reader["wadir2"]?.ToString(),
                    Wadir3 = reader["wadir3"]?.ToString(),
                    KodePos = reader["mhs_kodepos"]?.ToString()
                };
            }

            return null;
        }

        public async Task<string?> CreateDraftAsync(CreateDraftCutiRequest dto, string lampiranSuratPengajuanPath, string lampiranPath, string createdBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Step", "STEP1");
            cmd.Parameters.AddWithValue("@TahunAjaran", dto.TahunAjaran);
            cmd.Parameters.AddWithValue("@Semester", dto.Semester);
            cmd.Parameters.AddWithValue("@LampiranSuratPengajuan", lampiranSuratPengajuanPath);
            cmd.Parameters.AddWithValue("@Lampiran", lampiranPath);
            cmd.Parameters.AddWithValue("@MahasiswaId", dto.MhsId);
            cmd.Parameters.AddWithValue("@DraftId", string.Empty);
            cmd.Parameters.AddWithValue("@ModifiedBy", createdBy);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString();
        }

        public async Task<string?> GenerateIdAsync(GenerateCutiIdRequest dto, string modifiedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Step", "STEP2");
            cmd.Parameters.AddWithValue("@TahunAjaran", string.Empty);
            cmd.Parameters.AddWithValue("@Semester", string.Empty);
            cmd.Parameters.AddWithValue("@LampiranSuratPengajuan", string.Empty);
            cmd.Parameters.AddWithValue("@Lampiran", string.Empty);
            cmd.Parameters.AddWithValue("@MahasiswaId", string.Empty);
            cmd.Parameters.AddWithValue("@DraftId", dto.DraftId);
            cmd.Parameters.AddWithValue("@ModifiedBy", string.IsNullOrEmpty(modifiedBy) ? dto.ModifiedBy : modifiedBy);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString();
        }

        public async Task<string?> CreateDraftByProdiAsync(CreateCutiProdiRequest dto, string lampiranSuratPengajuanPath, string lampiranPath, string createdBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createCutiAkademikByProdi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Step", "STEP1");
            cmd.Parameters.AddWithValue("@TahunAjaran", dto.TahunAjaran);
            cmd.Parameters.AddWithValue("@Semester", dto.Semester);
            cmd.Parameters.AddWithValue("@LampiranSuratPengajuan", lampiranSuratPengajuanPath);
            cmd.Parameters.AddWithValue("@Lampiran", lampiranPath);
            cmd.Parameters.AddWithValue("@MahasiswaId", dto.MhsId);
            cmd.Parameters.AddWithValue("@Menimbang", dto.Menimbang ?? string.Empty);
            cmd.Parameters.AddWithValue("@ApprovalProdi", string.IsNullOrEmpty(dto.ApprovalProdi) ? createdBy : dto.ApprovalProdi);
            cmd.Parameters.AddWithValue("@DraftId", string.Empty);
            cmd.Parameters.AddWithValue("@ModifiedBy", createdBy);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString();
        }

        public async Task<string?> GenerateIdByProdiAsync(GenerateCutiProdiIdRequest dto, string modifiedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createCutiAkademikByProdi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Step", "STEP2");
            cmd.Parameters.AddWithValue("@TahunAjaran", string.Empty);
            cmd.Parameters.AddWithValue("@Semester", string.Empty);
            cmd.Parameters.AddWithValue("@LampiranSuratPengajuan", string.Empty);
            cmd.Parameters.AddWithValue("@Lampiran", string.Empty);
            cmd.Parameters.AddWithValue("@MahasiswaId", string.Empty);
            cmd.Parameters.AddWithValue("@Menimbang", string.Empty);
            cmd.Parameters.AddWithValue("@ApprovalProdi", string.Empty);
            cmd.Parameters.AddWithValue("@DraftId", dto.DraftId);
            cmd.Parameters.AddWithValue("@ModifiedBy", string.IsNullOrEmpty(modifiedBy) ? dto.ModifiedBy : modifiedBy);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString();
        }

        public async Task<(bool success, string message, string newStatus)> ApproveCutiAsync(string id, string role, string approvedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_setujuiCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CutiAkademikId", id);
            cmd.Parameters.AddWithValue("@Role", role);
            cmd.Parameters.AddWithValue("@ApprovedBy", approvedBy);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var success = Convert.ToInt32(reader["Success"]) == 1;
                var message = reader["Message"]?.ToString() ?? string.Empty;
                var newStatus = reader["NewStatus"]?.ToString() ?? string.Empty;
                return (success, message, newStatus);
            }

            return (false, "Gagal memproses persetujuan cuti akademik.", string.Empty);
        }

        public async Task<(bool success, string message)> RejectCutiAsync(string id, string role, string keterangan, string modifiedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_tolakCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CutiAkademikId", id);
            cmd.Parameters.AddWithValue("@Role", role);
            cmd.Parameters.AddWithValue("@Keterangan", keterangan);
            cmd.Parameters.AddWithValue("@ModifiedBy", modifiedBy);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var success = Convert.ToInt32(reader["Success"]) == 1;
                var message = reader["Message"]?.ToString() ?? string.Empty;
                return (success, message);
            }

            return (false, "Gagal memproses penolakan cuti akademik.");
        }

        public async Task<(bool success, string message)> UploadSKAsync(string id, string nomorSk, string modifiedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createSKCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CutiAkademikId", id);
            cmd.Parameters.AddWithValue("@NomorSK", nomorSk);
            cmd.Parameters.AddWithValue("@ModifiedBy", modifiedBy);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var success = Convert.ToInt32(reader["Success"]) == 1;
                var message = reader["Message"]?.ToString() ?? string.Empty;
                return (success, message);
            }

            return (false, "Gagal menyimpan SK cuti akademik.");
        }

        public async Task<bool> UpdateAsync(UpdateCutiAkademikRequest dto, string lampiranSuratPengajuanPath, string lampiranPath, string modifiedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_editCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CutiAkademikId", dto.Id);
            cmd.Parameters.AddWithValue("@TahunAjaran", dto.TahunAjaran);
            cmd.Parameters.AddWithValue("@Semester", dto.Semester);
            cmd.Parameters.AddWithValue("@LampiranSuratPengajuan", lampiranSuratPengajuanPath ?? string.Empty);
            cmd.Parameters.AddWithValue("@Lampiran", lampiranPath ?? string.Empty);
            cmd.Parameters.AddWithValue("@ModifiedBy", modifiedBy);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return Convert.ToInt32(reader["Success"]) == 1;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(string id, string modifiedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_deleteCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CutiAkademikId", id);
            cmd.Parameters.AddWithValue("@ModifiedBy", modifiedBy);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return Convert.ToInt32(reader["Success"]) == 1;
            }

            return false;
        }

        public async Task<IEnumerable<CutiAkademikRiwayatExcelResponse>> GetRiwayatExcelAsync(string userId)
        {
            var list = new List<CutiAkademikRiwayatExcelResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataRiwayatCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@SearchKeyword", string.Empty);
            cmd.Parameters.AddWithValue("@Status", string.Empty);
            cmd.Parameters.AddWithValue("@KonsentrasiId", 0);
            cmd.Parameters.AddWithValue("@UserId", userId ?? string.Empty);
            cmd.Parameters.AddWithValue("@OrderBy", "tanggal_desc");
            cmd.Parameters.AddWithValue("@PageNumber", 1);
            cmd.Parameters.AddWithValue("@PageSize", 10000); // Ambil seluruh data untuk export Excel

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new CutiAkademikRiwayatExcelResponse
                {
                    NIM = reader["mhs_id"]?.ToString() ?? string.Empty,
                    NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? string.Empty,
                    Konsentrasi = reader["kon_nama"]?.ToString() ?? string.Empty,
                    TanggalPengajuan = reader["tanggal"]?.ToString() ?? string.Empty,
                    NoSK = reader["cak_sk"]?.ToString() ?? string.Empty,
                    NoPengajuan = reader["srt_no"]?.ToString() ?? reader["cak_id"]?.ToString() ?? string.Empty
                });
            }

            return list;
        }
    }
}
