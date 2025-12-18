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
            // Generate unique draft ID dengan retry mechanism
            // =============================
            string newDraftId = await GenerateUniqueDraftIdAsync(conn);

            // =============================
            // Simpan file terlebih dahulu
            // =============================
            var fileSP = SaveFile(dto.LampiranSuratPengajuan);
            var fileLampiran = SaveFile(dto.Lampiran);

            // =============================
            // Insert langsung ke tabel (bypass SP untuk draft)
            // =============================
            var insertSql = @"
                INSERT INTO sia_mscutiakademik (
                    cak_id, 
                    mhs_id, 
                    cak_tahunajaran, 
                    cak_semester, 
                    cak_lampiran_suratpengajuan, 
                    cak_lampiran, 
                    cak_status, 
                    cak_created_date, 
                    cak_created_by
                ) VALUES (
                    @cak_id, 
                    @mhs_id, 
                    @tahunajaran, 
                    @semester, 
                    @lampiran_sp, 
                    @lampiran, 
                    'Draft', 
                    GETDATE(), 
                    @created_by
                )";

            var cmd = new SqlCommand(insertSql, conn);
            cmd.Parameters.AddWithValue("@cak_id", newDraftId);
            cmd.Parameters.AddWithValue("@mhs_id", dto.MhsId);
            cmd.Parameters.AddWithValue("@tahunajaran", dto.TahunAjaran ?? "");
            cmd.Parameters.AddWithValue("@semester", dto.Semester ?? "");
            cmd.Parameters.AddWithValue("@lampiran_sp", fileSP ?? "");
            cmd.Parameters.AddWithValue("@lampiran", fileLampiran ?? "");
            cmd.Parameters.AddWithValue("@created_by", dto.MhsId);

            try
            {
                await cmd.ExecuteNonQueryAsync();
                return newDraftId;
            }
            catch (SqlException ex) when (ex.Number == 2627) // Primary key violation
            {
                // Jika masih ada collision, coba generate ID baru
                newDraftId = await GenerateUniqueDraftIdAsync(conn);
                cmd.Parameters["@cak_id"].Value = newDraftId;
                await cmd.ExecuteNonQueryAsync();
                return newDraftId;
            }
        }

        // =============================
        // Helper method untuk generate unique draft ID
        // =============================
        private async Task<string> GenerateUniqueDraftIdAsync(SqlConnection conn)
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                // Generate ID berdasarkan timestamp + random untuk menghindari collision
                var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                var random = new Random().Next(100, 999);
                var candidateId = $"{timestamp}{random}";

                // Cek apakah ID sudah ada
                var checkCmd = new SqlCommand("SELECT COUNT(*) FROM sia_mscutiakademik WHERE cak_id = @id", conn);
                checkCmd.Parameters.AddWithValue("@id", candidateId);

                var count = (int)await checkCmd.ExecuteScalarAsync();
                if (count == 0)
                {
                    return candidateId;
                }
            }

            // Fallback: gunakan GUID jika semua attempt gagal
            return Guid.NewGuid().ToString("N")[..10];
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
            
            // If status is empty, use direct query to show all data (similar to riwayat logic)
            if (string.IsNullOrEmpty(status))
            {
                var sql = @"
                    SELECT a.cak_id,
                           (case when CHARINDEX('PMA',a.cak_id) > 0 then a.cak_id else 'DRAFT' end) as id,
                           a.mhs_id,
                           a.cak_tahunajaran,
                           a.cak_semester,
                           a.cak_approval_prodi as approve_prodi,
                           a.cak_approval_dir1 as approve_dir1,
                           CONVERT(VARCHAR(11),a.cak_created_date,106) AS tanggal,
                           a.srt_no,
                           a.cak_status as status
                    FROM sia_mscutiakademik a
                    WHERE a.cak_status != 'Dihapus'
                      AND (@mhsId = '%' OR a.mhs_id LIKE '%' + @mhsId + '%')
                      AND (@search = '' OR a.mhs_id LIKE '%' + @search + '%' OR a.cak_id LIKE '%' + @search + '%')
                    ORDER BY a.cak_created_date DESC";

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@mhsId", mhsId ?? "%");
                cmd.Parameters.AddWithValue("@search", search ?? "");

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
            }
            else
            {
                // Use stored procedure when status is specified
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
            }

            return result;
        }

        // ============================================================
        // GET DETAIL (Hybrid: SP untuk final ID, Direct SQL untuk draft ID)
        // ============================================================
        public async Task<CutiAkademikDetailResponse?> GetDetailAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            // Tentukan apakah ini draft ID atau final ID
            bool isDraftId = !id.Contains("PMA") && !id.Contains("CA");

            if (isDraftId)
            {
                // Untuk draft ID, gunakan direct SQL query
                var sql = @"
                    SELECT a.cak_id, a.mhs_id, b.mhs_nama, c.kon_nama, b.mhs_angkatan,
                           c.kon_singkatan, a.cak_tahunajaran, a.cak_semester,
                           a.cak_lampiran_suratpengajuan, a.cak_lampiran, a.cak_status,
                           a.cak_created_by, CONVERT(VARCHAR(11),a.cak_created_date,106) as tgl,
                           a.cak_sk, a.srt_no, d.pro_nama, '' as kaprod,
                           CONVERT(VARCHAR(11),a.cak_app_prodi_date,106) as cak_app_prodi_date,
                           a.cak_approval_prodi, CONVERT(VARCHAR(11),a.cak_app_dir1_date,106) as cak_app_dir1_date,
                           a.cak_approval_dir1, b.mhs_alamat, a.cak_menimbang, '' as BulanCuti,
                           '' as direktur, '' as wadir1, '' as wadir2, '' as wadir3, b.mhs_kodepos
                    FROM sia_mscutiakademik a
                    LEFT JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
                    LEFT JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
                    LEFT JOIN sia_msprodi d ON c.pro_id = d.pro_id
                    WHERE a.cak_id = @id";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return null;

                return new CutiAkademikDetailResponse
                {
                    Id = reader["cak_id"].ToString(),
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
            else
            {
                // Untuk final ID, gunakan stored procedure
                var cmd = new SqlCommand("sia_detailCutiAkademik", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@p1", id);
                for (int i = 2; i <= 50; i++)
                    cmd.Parameters.AddWithValue($"@p{i}", "");

                var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    return null;

                return new CutiAkademikDetailResponse
                {
                    Id = reader["cak_id"].ToString(),
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
        }


        // ============================================================
        // UPDATE / EDIT CUTI (Hybrid: SP untuk final ID, Direct SQL untuk draft ID)
        // ============================================================
        public async Task<bool> UpdateAsync(string id, UpdateCutiAkademikRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            // -----------------------------
            // 1️⃣ Cek apakah data ada dan ambil info
            // -----------------------------
            var checkCmd = new SqlCommand(@"
                SELECT cak_lampiran_suratpengajuan, cak_lampiran, cak_id 
                FROM sia_mscutiakademik 
                WHERE cak_id = @id", conn);
            checkCmd.Parameters.AddWithValue("@id", id);

            string? oldFileSP = null;
            string? oldFileLampiran = null;
            bool dataExists = false;

            using var reader = await checkCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                dataExists = true;
                oldFileSP = reader["cak_lampiran_suratpengajuan"]?.ToString();
                oldFileLampiran = reader["cak_lampiran"]?.ToString();
            }
            reader.Close();

            if (!dataExists)
                throw new Exception($"Data dengan ID {id} tidak ditemukan.");

            // -----------------------------
            // 2️⃣ Handle file upload
            // -----------------------------
            string? fileSP = oldFileSP;
            string? fileLampiran = oldFileLampiran;

            if (dto.LampiranSuratPengajuan != null)
                fileSP = SaveFile(dto.LampiranSuratPengajuan);

            if (dto.Lampiran != null)
                fileLampiran = SaveFile(dto.Lampiran);

            // -----------------------------
            // 3️⃣ Tentukan apakah ini draft ID atau final ID
            // -----------------------------
            bool isDraftId = !id.Contains("PMA") && !id.Contains("CA");

            if (isDraftId)
            {
                // Untuk draft ID, gunakan direct SQL (lebih reliable)
                var updateSql = @"
                    UPDATE sia_mscutiakademik 
                    SET cak_tahunajaran = @tahunajaran,
                        cak_semester = @semester,
                        cak_lampiran_suratpengajuan = @lampiran_sp,
                        cak_lampiran = @lampiran,
                        cak_modif_date = GETDATE(),
                        cak_modif_by = @modified_by
                    WHERE cak_id = @id";

                var updateCmd = new SqlCommand(updateSql, conn);
                updateCmd.Parameters.AddWithValue("@id", id);
                updateCmd.Parameters.AddWithValue("@tahunajaran", dto.TahunAjaran ?? "");
                updateCmd.Parameters.AddWithValue("@semester", dto.Semester ?? "");
                updateCmd.Parameters.AddWithValue("@lampiran_sp", fileSP ?? "");
                updateCmd.Parameters.AddWithValue("@lampiran", fileLampiran ?? "");
                updateCmd.Parameters.AddWithValue("@modified_by", dto.ModifiedBy ?? "");

                var rows = await updateCmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            else
            {
                // Untuk final ID, gunakan stored procedure
                var cmd = new SqlCommand("sia_editCutiAkademik", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@p1", id);
                cmd.Parameters.AddWithValue("@p2", dto.TahunAjaran ?? "");
                cmd.Parameters.AddWithValue("@p3", dto.Semester ?? "");
                cmd.Parameters.AddWithValue("@p4", fileSP ?? "");
                cmd.Parameters.AddWithValue("@p5", fileLampiran ?? "");
                cmd.Parameters.AddWithValue("@p6", dto.ModifiedBy ?? "");

                // p7 – p50 kosong sesuai kebutuhan SP
                for (int i = 7; i <= 50; i++)
                    cmd.Parameters.AddWithValue($"@p{i}", "");

                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
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
            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            // =============================
            // Generate unique draft ID
            // =============================
            string newDraftId = await GenerateUniqueDraftIdAsync(conn);

            // =============================
            // Insert langsung ke tabel (bypass SP untuk draft)
            // =============================
            var insertSql = @"
                INSERT INTO sia_mscutiakademik (
                    cak_id, 
                    mhs_id, 
                    cak_tahunajaran, 
                    cak_semester, 
                    cak_lampiran_suratpengajuan, 
                    cak_lampiran, 
                    cak_menimbang,
                    cak_approval_prodi,
                    cak_status, 
                    cak_created_date, 
                    cak_created_by
                ) VALUES (
                    @cak_id, 
                    @mhs_id, 
                    @tahunajaran, 
                    @semester, 
                    @lampiran_sp, 
                    @lampiran, 
                    @menimbang,
                    @approval_prodi,
                    'Draft', 
                    GETDATE(), 
                    @created_by
                )";

            var cmd = new SqlCommand(insertSql, conn);
            cmd.Parameters.AddWithValue("@cak_id", newDraftId);
            cmd.Parameters.AddWithValue("@mhs_id", dto.MhsId ?? "");
            cmd.Parameters.AddWithValue("@tahunajaran", dto.TahunAjaran ?? "");
            cmd.Parameters.AddWithValue("@semester", dto.Semester ?? "");
            cmd.Parameters.AddWithValue("@lampiran_sp", dto.LampiranSuratPengajuan ?? "");
            cmd.Parameters.AddWithValue("@lampiran", dto.Lampiran ?? "");
            cmd.Parameters.AddWithValue("@menimbang", dto.Menimbang ?? "");
            cmd.Parameters.AddWithValue("@approval_prodi", dto.ApprovalProdi ?? "");
            cmd.Parameters.AddWithValue("@created_by", dto.MhsId ?? "");

            try
            {
                await cmd.ExecuteNonQueryAsync();
                return newDraftId;
            }
            catch (SqlException ex) when (ex.Number == 2627) // Primary key violation
            {
                // Jika masih ada collision, coba generate ID baru
                newDraftId = await GenerateUniqueDraftIdAsync(conn);
                cmd.Parameters["@cak_id"].Value = newDraftId;
                await cmd.ExecuteNonQueryAsync();
                return newDraftId;
            }
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
            
            // If status is empty, get all records with a direct query
            if (string.IsNullOrEmpty(status))
            {
                var sql = @"
                    SELECT a.cak_id,
                           (case when CHARINDEX('PMA',a.cak_id) > 0 then a.cak_id else 'DRAFT' end) as id,
                           a.mhs_id,
                           a.cak_tahunajaran,
                           a.cak_semester,
                           a.cak_approval_prodi as approve_prodi,
                           a.cak_approval_dir1 as approve_dir1,
                           CONVERT(VARCHAR(11),a.cak_created_date,106) AS tanggal,
                           a.srt_no,
                           a.cak_status as status
                    FROM sia_mscutiakademik a
                    WHERE a.cak_status != 'Dihapus'
                      AND (@search = '' OR a.mhs_id LIKE '%' + @search + '%' OR a.cak_id LIKE '%' + @search + '%')
                    ORDER BY a.cak_created_date DESC";

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@search", search ?? "");

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
            }
            else
            {
                // Use the riwayat SP when status is specified
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

    
