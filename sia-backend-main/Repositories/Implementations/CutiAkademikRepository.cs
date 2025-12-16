using astratech_apps_backend.DTOs.CutiAkademik;
using astratech_apps_backend.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace astratech_apps_backend.Repositories.Implementations
{
    public class CutiAkademikRepository : ICutiAkademikRepository
    {
        private readonly string _conn;

        public CutiAkademikRepository(IConfiguration config)
        {
            _conn = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
                config.GetConnectionString("DefaultConnection")!,
                Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
            );
        }

        // ============================================================
        // 🔍 Helper: Cari mhs_id berdasarkan input FE (nim/nama/mhs_id)
        // ============================================================
        private async Task<string?> ResolveMhsIdAsync(string input)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand(
                @"SELECT mhs_id 
          FROM sia_msmahasiswa 
          WHERE mhs_id = @x", conn);

            cmd.Parameters.AddWithValue("@x", input);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString();
        }



        // ============================================================
        // STEP 1 — Create Draft (SP: sia_createCutiAkademik)
        // ============================================================
        public async Task<string?> CreateDraftAsync(CreateDraftCutiRequest dto)
        {
            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            // =============================
            // FIX: hitung ID draft terbaru
            // =============================
            int newDraftId = 1;

            var getMaxCmd = new SqlCommand(@"
        SELECT MAX(CAST(cak_id AS INT))
        FROM sia_mscutiakademik
        WHERE ISNUMERIC(cak_id) = 1
    ", conn);

            var result = await getMaxCmd.ExecuteScalarAsync();
            if (result != DBNull.Value && result != null)
                newDraftId = Convert.ToInt32(result) + 1;

            // =============================
            // Kirim ke SP STEP1
            // =============================
            var cmd = new SqlCommand("sia_createCutiAkademik", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p1", "STEP1");
            cmd.Parameters.AddWithValue("@p2", dto.TahunAjaran);
            cmd.Parameters.AddWithValue("@p3", dto.Semester);
            var fileSP = SaveFile(dto.LampiranSuratPengajuan);
            var fileLampiran = SaveFile(dto.Lampiran);

            cmd.Parameters.AddWithValue("@p4", fileSP ?? "");
            cmd.Parameters.AddWithValue("@p5", fileLampiran ?? "");

            cmd.Parameters.AddWithValue("@p6", dto.MhsId);

            // p7 sampai p50 → empty
            for (int i = 7; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await cmd.ExecuteNonQueryAsync();

            return newDraftId.ToString();
        }




        // ============================================================
        // STEP 2 — Generate Final ID (SP: sia_createCutiAkademik)
        // ============================================================
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

            var cmd2 = new SqlCommand(
                @"SELECT TOP 1 cak_id 
                  FROM sia_mscutiakademik 
                  WHERE cak_id LIKE '%CA%'
                  ORDER BY cak_created_date DESC", conn);

            return (string?)await cmd2.ExecuteScalarAsync();
        }

        // ============================================================
        // GET ALL DATA
        // SP: sia_getDataCutiAkademik
        // ============================================================
        public async Task<IEnumerable<CutiAkademikListResponse>> GetAllAsync(
            string mhsId, string status, string userId, string role, string search = "")
        {
            var result = new List<CutiAkademikListResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", mhsId);
            cmd.Parameters.AddWithValue("@p2", status);
            cmd.Parameters.AddWithValue("@p3", userId);
            cmd.Parameters.AddWithValue("@p4", search ?? "");

            for (int i = 5; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new CutiAkademikListResponse
                {
                    Id = reader["cak_id"].ToString(),
                    IdDisplay = reader["id"].ToString(),
                    MhsId = reader["mhs_id"].ToString(),
                    TahunAjaran = reader["cak_tahunajaran"].ToString(),
                    Semester = reader["cak_semester"].ToString(),
                    ApproveProdi = reader["approve_prodi"].ToString(),
                    ApproveDir1 = reader["approve_dir1"].ToString(),
                    Tanggal = reader["tanggal"].ToString(),
                    SuratNo = reader["srt_no"].ToString(),
                    Status = reader["status"].ToString(),
                });
            }

            return result;
        }

        // ============================================================
        // GET DETAIL (SP: sia_detailCutiAkademik)
        // ============================================================
        public async Task<CutiAkademikDetailResponse?> GetDetailAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_detailCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", id);
            for (int i = 2; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new CutiAkademikDetailResponse
            {
                Id = reader["cak_id"].ToString(),
                //IdFinal = reader["cak_finalid"] != DBNull.Value ? reader["cak_finalid"].ToString() : "",   // <==== DITAMBAHKAN
                MhsId = reader["mhs_id"].ToString(),
                Mahasiswa = reader["mhs_nama"].ToString(),
                Konsentrasi = reader["kon_nama"].ToString(),
                Angkatan = reader["mhs_angkatan"].ToString(),
                KonsentrasiSingkatan = reader["kon_singkatan"].ToString(),
                TahunAjaran = reader["cak_tahunajaran"].ToString(),
                Semester = reader["cak_semester"].ToString(),
                LampiranSP = reader["cak_lampiran_suratpengajuan"].ToString(),
                Lampiran = reader["cak_lampiran"].ToString(),
                Status = reader["cak_status"].ToString(),
                CreatedBy = reader["cak_created_by"].ToString(),
                TglPengajuan = reader["tgl"].ToString(),
                Sk = reader["cak_sk"].ToString(),
                SrtNo = reader["srt_no"].ToString(),
                ProdiNama = reader["pro_nama"].ToString(),
                Kaprodi = reader["kaprod"].ToString(),
                AppProdiDate = reader["cak_app_prodi_date"].ToString(),
                ApprovalProdi = reader["cak_approval_prodi"].ToString(),
                AppDir1Date = reader["cak_app_dir1_date"].ToString(),
                ApprovalDir1 = reader["cak_approval_dir1"].ToString(),
                Alamat = reader["mhs_alamat"].ToString(),
                Menimbang = reader["cak_menimbang"].ToString(),
                BulanCuti = reader["BulanCuti"].ToString(),
                Direktur = reader["direktur"].ToString(),
                Wadir1 = reader["wadir1"].ToString(),
                Wadir2 = reader["wadir2"].ToString(),
                Wadir3 = reader["wadir3"].ToString(),
                KodePos = reader["mhs_kodepos"].ToString(),
            };
        }


        // ============================================================
        // UPDATE / EDIT CUTI (SP: sia_editCutiAkademik)
        // ============================================================
        public async Task<bool> UpdateAsync(string id, UpdateCutiAkademikRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_editCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // -----------------------------
            // 1️⃣ Ambil data lama
            // -----------------------------
            var old = await GetDetailAsync(id);

            if (old == null)
                throw new Exception("Data tidak ditemukan.");

            // -----------------------------
            // 2️⃣ Simpan file baru jika ada
            // -----------------------------
            string? fileSP = old.LampiranSP;      // default = file lama
            string? fileLampiran = old.Lampiran; // default = file lama

            if (dto.LampiranSuratPengajuan != null)
                fileSP = SaveFile(dto.LampiranSuratPengajuan);

            if (dto.Lampiran != null)
                fileLampiran = SaveFile(dto.Lampiran);

            // -----------------------------
            // 3️⃣ Set parameter ke SP
            // -----------------------------
            cmd.Parameters.AddWithValue("@p1", id);
            cmd.Parameters.AddWithValue("@p2", dto.TahunAjaran);
            cmd.Parameters.AddWithValue("@p3", dto.Semester);

            cmd.Parameters.AddWithValue("@p4", fileSP ?? "");
            cmd.Parameters.AddWithValue("@p5", fileLampiran ?? "");
            cmd.Parameters.AddWithValue("@p6", dto.ModifiedBy);

            // p7 – p50 kosong sesuai kebutuhan SP
            for (int i = 7; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            // -----------------------------
            // 4️⃣ Execute update
            // -----------------------------
            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }


        // ============================================================
        // DELETE (Soft Delete — SP: sia_deleteCutiAkademik)
        // ============================================================
        public async Task<bool> DeleteAsync(string id, string modifiedBy)
        {
            try
            {
                await using var conn = new SqlConnection(_conn);
                await using var cmd = new SqlCommand("sia_deleteCutiAkademik", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@p1", id);
                cmd.Parameters.AddWithValue("@p2", modifiedBy);

                for (int i = 3; i <= 50; i++)
                    cmd.Parameters.AddWithValue($"@p{i}", "");

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return true;   // 💚 anggap berhasil jika tidak error
            }
            catch
            {
                return false;  // ❌ hanya kalau SQL benar-benar error
            }
        }


        // ---------------------------------------------------------
        // STEP 1 — Create Draft by Prodi
        // ---------------------------------------------------------
        public async Task<string?> CreateDraftByProdiAsync(CreateCutiProdiRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", "STEP1");
            cmd.Parameters.AddWithValue("@p2", dto.TahunAjaran ?? "");
            cmd.Parameters.AddWithValue("@p3", dto.Semester ?? "");
            cmd.Parameters.AddWithValue("@p4", dto.LampiranSuratPengajuan ?? "");
            cmd.Parameters.AddWithValue("@p5", dto.Lampiran ?? "");
            cmd.Parameters.AddWithValue("@p6", dto.MhsId ?? "");
            cmd.Parameters.AddWithValue("@p7", dto.Menimbang ?? "");
            cmd.Parameters.AddWithValue("@p8", dto.ApprovalProdi ?? "");

            // p9–p50 kosong
            for (int i = 9; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            // Ambil draft id terbaru berdasarkan created_by (mhsId) — sama pola seperti STEP1 biasa
            var cmd2 = new SqlCommand(
                @"SELECT TOP 1 cak_id FROM sia_mscutiakademik 
                  WHERE cak_created_by = @mhsId ORDER BY cak_created_date DESC", conn);
            cmd2.Parameters.AddWithValue("@mhsId", dto.MhsId ?? "");

            return (string?)await cmd2.ExecuteScalarAsync();
        }

        // ---------------------------------------------------------
        // STEP 2 — Generate Final ID (After Draft) by Prodi
        // ---------------------------------------------------------
        public async Task<string?> GenerateIdByProdiAsync(GenerateCutiProdiIdRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", "STEP2");
            cmd.Parameters.AddWithValue("@p2", dto.DraftId ?? "");
            cmd.Parameters.AddWithValue("@p3", dto.ModifiedBy ?? "");
            // p4..p50 kosong
            for (int i = 4; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            var cmd2 = new SqlCommand(
                @"SELECT TOP 1 cak_id 
                  FROM sia_mscutiakademik 
                  WHERE cak_id LIKE '%CA%'
                  ORDER BY cak_created_date DESC", conn);

            return (string?)await cmd2.ExecuteScalarAsync();
        }

        public async Task<IEnumerable<CutiAkademikListResponse>> GetRiwayatAsync(
    string userId, string status, string search)
        {
            var result = new List<CutiAkademikListResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataRiwayatCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", userId);
            cmd.Parameters.AddWithValue("@p2", status);
            cmd.Parameters.AddWithValue("@p3", "");
            cmd.Parameters.AddWithValue("@p4", search);

            // sisanya p5 sampai p50 kosongi
            for (int i = 5; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new CutiAkademikListResponse
                {
                    Id = reader["cak_id"].ToString(),
                    IdDisplay = reader["id"].ToString(),
                    MhsId = reader["mhs_id"].ToString(),
                    TahunAjaran = reader["cak_tahunajaran"].ToString(),
                    Semester = reader["cak_semester"].ToString(),
                    ApproveProdi = reader["approve_prodi"].ToString(),
                    ApproveDir1 = reader["approve_dir1"].ToString(),
                    Tanggal = reader["tanggal"].ToString(),
                    SuratNo = reader["srt_no"].ToString(),
                    Status = reader["status"].ToString()
                });
            }

            return result;
        }

        public async Task<IEnumerable<CutiAkademikRiwayatExcelResponse>> GetRiwayatExcelAsync(string userId)
        {
            var result = new List<CutiAkademikRiwayatExcelResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataRiwayatCutiAkademikExcel", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", userId ?? "");

            for (int i = 2; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new CutiAkademikRiwayatExcelResponse
                {
                    NIM = reader["NIM"]?.ToString(),
                    NamaMahasiswa = reader["Nama Mahasiswa"]?.ToString(),
                    Konsentrasi = reader["Konsentrasi"]?.ToString(),
                    TanggalPengajuan = reader["Tanggal Pengajuan"]?.ToString(),
                    NoSK = reader["No SK"]?.ToString(),
                    NoPengajuan = reader["No Pengajuan"]?.ToString()
                });
            }

            return result;
        }

        private string? SaveFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/cuti");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return fileName;
        }

            
    }
}

    
