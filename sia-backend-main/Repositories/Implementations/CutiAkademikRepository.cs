using astratech_apps_backend.DTOs.CutiAkademik;
using astratech_apps_backend.Models;
using astratech_apps_backend.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace astratech_apps_backend.Repositories.Implementations
{
    public class CutiAkademikRepository : ICutiAkademikRepository
    {
        private readonly string _conn;
        private readonly IHttpContextAccessor _http;

        public CutiAkademikRepository(IConfiguration config, IHttpContextAccessor http)
        {
            _http = http;
            _conn = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
                config.GetConnectionString("DefaultConnection")!,
                Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
            );
        }



        // ------------------------- STEP 1 -------------------------
        public async Task<string?> CreateDraftAsync(CreateDraftCutiRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", "STEP1");
            cmd.Parameters.AddWithValue("@p2", dto.TahunAjaran);
            cmd.Parameters.AddWithValue("@p3", dto.Semester);
            cmd.Parameters.AddWithValue("@p4", dto.LampiranSuratPengajuan ?? "");
            cmd.Parameters.AddWithValue("@p5", dto.Lampiran ?? "");
            cmd.Parameters.AddWithValue("@p6", dto.MhsId);

            // p7–p50 kosong
            for (int i = 7; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            // Ambil draft id terbaru
            var cmd2 = new SqlCommand(
                @"SELECT TOP 1 cak_id FROM sia_mscutiakademik 
                  WHERE cak_created_by = @mhsId ORDER BY cak_created_date DESC", conn);

            cmd2.Parameters.AddWithValue("@mhsId", dto.MhsId);

            return (string?)await cmd2.ExecuteScalarAsync();
        }

        // ------------------------- STEP 2 -------------------------
        public async Task<string?> GenerateIdAsync(GenerateCutiIdRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", "STEP2");
            cmd.Parameters.AddWithValue("@p2", dto.DraftId);
            cmd.Parameters.AddWithValue("@p3", dto.ModifiedBy);

            for (int i = 4; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            // Ambil ID final
            var cmd2 = new SqlCommand(
                @"SELECT TOP 1 cak_id 
                  FROM sia_mscutiakademik 
                  WHERE cak_created_date IS NOT NULL 
                  ORDER BY cak_created_date DESC", conn);

            return (string?)await cmd2.ExecuteScalarAsync();
        }

        // ---------------------------------------------------------
        // STEP 1 — Create Draft by Prodi
        // ---------------------------------------------------------
        public async Task<string?> CreateDraftByProdiAsync(CreateCutiProdiRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createCutiAkademikByProdi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", "STEP1");
            cmd.Parameters.AddWithValue("@p2", dto.TahunAjaran);
            cmd.Parameters.AddWithValue("@p3", dto.Semester);
            cmd.Parameters.AddWithValue("@p4", dto.LampiranSuratPengajuan);
            cmd.Parameters.AddWithValue("@p5", dto.Lampiran);
            cmd.Parameters.AddWithValue("@p6", dto.MhsId);
            cmd.Parameters.AddWithValue("@p7", dto.Menimbang);
            cmd.Parameters.AddWithValue("@p8", dto.ApprovalProdi);

            // p9–p50 kosong
            for (int i = 9; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString();
        }

        // ---------------------------------------------------------
        // STEP 2 — Generate Final ID (After Draft)
        // ---------------------------------------------------------
        public async Task<string?> GenerateIdByProdiAsync(GenerateCutiProdiIdRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createCutiAkademikByProdi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", "STEP2");
            cmd.Parameters.AddWithValue("@p2", dto.DraftId);
            cmd.Parameters.AddWithValue("@p3", dto.ModifiedBy);

            // p4–p50 kosong
            for (int i = 4; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString();
        }

        public async Task<bool> CreateSKCutiAkademikAsync(CreateSKCutiAkademikRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createSKCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", dto.CutiId);
            cmd.Parameters.AddWithValue("@p2", dto.SK);
            cmd.Parameters.AddWithValue("@p3", dto.ModifiedBy);

            // p4–p50 kosong
            for (int i = 4; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }

        public async Task<List<CutiAkademikListResponse>> GetListResponseAsync(
      string? status,
      string? search,
      string? urut,
      int pageNumber,
      int pageSize)
        {
            var result = new List<CutiAkademikListResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // ============================================================
            // 🚀 AMBIL ROLE & USER DARI HTTP CONTEXT
            // ============================================================
            var role = _http.HttpContext?.Items["Role"]?.ToString()?.ToUpper() ?? "";
            var userId = _http.HttpContext?.Items["UserId"]?.ToString() ?? "";
            var mhsId = _http.HttpContext?.Items["MhsId"]?.ToString() ?? "";
            Console.Write(role);

            // ============================================================
            // 🚀 MAPPING ROLE KE PARAMETER SP
            // ============================================================
            string p1 = ""; // mhs_id
            string p3 = ""; // kry_id (prodi)

            if (role == "ROL23")
            {
                p1 = mhsId;        // mahasiswa melihat pengajuannya sendiri
                status = "";
                p3 = "";
            }
            else if (role == "PRODI")
            {
                p1 = "";
                p3 = userId;       // prodi melihat yang dia handle
            }
            else if (role == "ADMIN")
            {
                p1 = "";
                p3 = "";
            }

            cmd.Parameters.AddWithValue("@p1", p1);
            cmd.Parameters.AddWithValue("@p2", status ?? "");
            cmd.Parameters.AddWithValue("@p3", p3);
            cmd.Parameters.AddWithValue("@p4", search ?? "");
            cmd.Parameters.AddWithValue("@p5", urut ?? "");

            for (int i = 6; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new CutiAkademikListResponse
                {
                    Id = reader["cak_id"]?.ToString() ?? "",
                    IdDisplay = reader["id"]?.ToString() ?? "",
                    MhsId = reader["mhs_id"]?.ToString() ?? "",
                    TahunAjaran = reader["cak_tahunajaran"]?.ToString() ?? "",
                    Semester = reader["cak_semester"]?.ToString() ?? "",
                    ApproveProdi = reader["approve_prodi"]?.ToString() ?? "",
                    ApproveDir1 = reader["approve_dir1"]?.ToString() ?? "",
                    Tanggal = reader["tanggal"]?.ToString() ?? "",
                    SuratNo = reader["srt_no"]?.ToString() ?? "",
                    Status = reader["status"]?.ToString() ?? ""
                });
            }

            return result;
        }

        public async Task<IEnumerable<CutiAkademikRiwayatResponse>> GetRiwayatAsync(
        string username, string status, string keyword)
        {
            var result = new List<CutiAkademikRiwayatResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataRiwayatCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // p1 = username
            // p2 = status filter
            // p4 = keyword
            cmd.Parameters.AddWithValue("@p1", username);
            cmd.Parameters.AddWithValue("@p2", status);
            cmd.Parameters.AddWithValue("@p3", "");
            cmd.Parameters.AddWithValue("@p4", keyword);

            // p5–p50 kosong
            for (int i = 5; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new CutiAkademikRiwayatResponse
                {
                    Id = reader["cak_id"].ToString(),
                    IdDisplay = reader["id"].ToString(),
                    MhsId = reader["mhs_id"].ToString(),
                    NamaMahasiswa = reader["mhs_nama"].ToString(),
                    Konsentrasi = reader["kon_singkatan"].ToString(),
                    TahunAjaran = reader["cak_tahunajaran"].ToString(),
                    Semester = reader["cak_semester"].ToString(),
                    ApproveProdi = reader["approve_prodi"].ToString(),
                    ApproveDir1 = reader["approve_dir1"].ToString(),
                    Tanggal = reader["tanggal"].ToString(),
                    TanggalDisetujui = reader["tanggal_disetujui"].ToString(),
                    SuratNo = reader["srt_no"].ToString(),
                    Status = reader["status"].ToString()
                });
            }

            return result;
        }



        public async Task<CutiAkademikNotifResponse?> GetDetailNotifAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_detailCutiAkademikNotif", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // p1 = id, p2..p50 = ""
            cmd.Parameters.AddWithValue("@p1", id);
            for (int i = 2; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new CutiAkademikNotifResponse
            {
                Id = reader.GetString(0),
                MhsId = reader.GetString(1),
                NamaMahasiswa = reader.GetString(2),
                Konsentrasi = reader.GetString(3),
                Angkatan = reader.GetString(4),

                CreatedBy1 = reader.GetString(5),
                CreatedBy2 = reader.GetString(6),
                CreatedBy3 = reader.GetString(7),
                CreatedBy4 = reader.GetString(8),
                CreatedBy5 = reader.GetString(9),
                CreatedBy6 = reader.GetString(10),
                CreatedBy7 = reader.GetString(11),
                CreatedBy8 = reader.GetString(12),
            };
        }


        public async Task<CutiAkademikDetailResponse?> GetDetailAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_detailCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // @p1 = id, p2–p50 kosong
            cmd.Parameters.AddWithValue("@p1", id);
            for (int i = 2; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new CutiAkademikDetailResponse
            {
                Id = reader.GetString(0),
                MhsId = reader.GetString(1),
                Mahasiswa = reader.GetString(2),
                Konsentrasi = reader.GetString(3),
                Angkatan = reader.GetString(4),
                KonsentrasiSingkatan = reader.GetString(5),
                TahunAjaran = reader.GetString(6),
                Semester = reader.GetString(7),
                LampiranSP = reader.GetString(8),
                Lampiran = reader.GetString(9),
                Status = reader.GetString(10),
                CreatedBy = reader.GetString(11),
                TglPengajuan = reader.GetString(12),
                Sk = reader.GetString(13),
                SrtNo = reader.GetString(14),
                ProdiNama = reader.GetString(15),
                Kaprodi = reader.GetString(16),
                AppProdiDate = reader.GetString(17),
                ApprovalProdi = reader.GetString(18),
                AppDir1Date = reader.GetString(19),
                ApprovalDir1 = reader.GetString(20),
                Alamat = reader.GetString(21),
                Menimbang = reader.GetString(24),
                BulanCuti = reader.GetString(25),
                Direktur = reader.GetString(26),
                Wadir1 = reader.GetString(27),
                Wadir2 = reader.GetString(28),
                Wadir3 = reader.GetString(29),
                KodePos = reader.GetString(30)
            };
        }

        public async Task<IEnumerable<CutiAkademikRiwayatExcelResponse>> GetRiwayatExcelAsync()
        {
            var list = new List<CutiAkademikRiwayatExcelResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataRiwayatCutiAkademikExcel", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // isi p1–p50 kosong
            for (int i = 1; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new CutiAkademikRiwayatExcelResponse
                {
                    NIM = reader["NIM"].ToString(),
                    NamaMahasiswa = reader["Nama Mahasiswa"].ToString(),
                    Konsentrasi = reader["Konsentrasi"].ToString(),
                    TanggalPengajuan = reader["Tanggal Pengajuan"].ToString(),
                    NoSK = reader["No SK"].ToString(),
                    NoPengajuan = reader["No Pengajuan"].ToString()
                });
            }

            return list;
        }



        public async Task<bool> UpdateAsync(string id, UpdateCutiAkademikRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_editCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", id);
            cmd.Parameters.AddWithValue("@p2", dto.TahunAjaran);
            cmd.Parameters.AddWithValue("@p3", dto.Semester);
            cmd.Parameters.AddWithValue("@p4", dto.LampiranSuratPengajuan);
            cmd.Parameters.AddWithValue("@p5", dto.Lampiran);
            cmd.Parameters.AddWithValue("@p6", dto.ModifiedBy);

            // p7–p50 kosong
            for (int i = 7; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }


        public async Task<bool> DeleteAsync(string id, string modifiedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_deleteCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // parameter SP
            cmd.Parameters.AddWithValue("@p1", id);
            cmd.Parameters.AddWithValue("@p2", modifiedBy);

            // p3 sampai p50 = ""
            for (int i = 3; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }

        public async Task<bool> ApproveAsync(string id, string role, string username)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_setujuiCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", id);
            cmd.Parameters.AddWithValue("@p2", role);
            cmd.Parameters.AddWithValue("@p3", username);

            // p4–p50 kosong
            for (int i = 4; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<bool> ApproveProdiAsync(string id, string menimbang, string username)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_setujuiCutiAkademikProdi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", id);
            cmd.Parameters.AddWithValue("@p2", menimbang);
            cmd.Parameters.AddWithValue("@p3", username);

            // sisa p4–p50 kosong
            for (int i = 4; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }

        public async Task<bool> RejectAsync(string id, string rejectedBy, string reason)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_tolakCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", id);
            cmd.Parameters.AddWithValue("@p2", rejectedBy);
            cmd.Parameters.AddWithValue("@p3", reason);

            // sisa p4–p50
            for (int i = 4; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }




    }
}
