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
        // STEP 2 — Generate Final ID (FIXED: Bypass Buggy SP)
        // ============================================================
        public async Task<string?> GenerateIdAsync(GenerateCutiIdRequest dto)
        {
            try
            {
                Console.WriteLine($"[Repository] GenerateIdAsync - DraftId: '{dto.DraftId}', ModifiedBy: '{dto.ModifiedBy}'");
                
                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                
                // Validate draft record exists and is in Draft status
                var checkCmd = new SqlCommand(@"
                    SELECT cak_id, cak_status, mhs_id
                    FROM sia_mscutiakademik 
                    WHERE cak_id = @draftId", conn);
                checkCmd.Parameters.AddWithValue("@draftId", dto.DraftId);
                
                var reader = await checkCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    reader.Close();
                    throw new Exception($"Draft record dengan ID '{dto.DraftId}' tidak ditemukan.");
                }
                
                var status = reader["cak_status"].ToString();
                var mhsId = reader["mhs_id"].ToString();
                reader.Close();
                
                if (status != "Draft")
                {
                    throw new Exception($"Record dengan ID '{dto.DraftId}' bukan dalam status Draft (status: {status}).");
                }
                
                // Generate unique final ID using SAFE logic (not buggy SP)
                string finalId = await GenerateUniqueFinalIdSafeAsync(conn);
                Console.WriteLine($"[Repository] Generated unique final ID: {finalId}");
                
                // Update draft record to final ID with proper status
                var updateCmd = new SqlCommand(@"
                    UPDATE sia_mscutiakademik 
                    SET cak_id = @finalId,
                        cak_status = 'Belum Disetujui Prodi',
                        cak_modif_date = GETDATE(),
                        cak_modif_by = @modifiedBy
                    WHERE cak_id = @draftId", conn);
                
                updateCmd.Parameters.AddWithValue("@finalId", finalId);
                updateCmd.Parameters.AddWithValue("@modifiedBy", dto.ModifiedBy);
                updateCmd.Parameters.AddWithValue("@draftId", dto.DraftId);
                
                var rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                
                if (rowsAffected == 0)
                {
                    throw new Exception("Gagal mengupdate draft record ke final ID.");
                }
                
                Console.WriteLine($"[Repository] Successfully updated draft to final ID: {finalId}");
                return finalId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Repository] ERROR in GenerateIdAsync: {ex.Message}");
                throw;
            }
        }

        // ============================================================
        // Helper: Generate Unique Final ID (SAFE - No SP Dependency)
        // ============================================================
        private async Task<string> GenerateUniqueFinalIdSafeAsync(SqlConnection conn)
        {
            try
            {
                // Get current month in Roman numerals (same as SP logic)
                var now = DateTime.Now;
                var month = now.Month;
                var year = now.Year;
                
                // Convert month to Roman using same logic as SP function
                string romanMonth = ConvertToRoman(month);
                string kode = $"/PMA/CA/{romanMonth}/{year}";
                
                Console.WriteLine($"[Repository] Generating ID with kode: {kode}");
                
                // Get the HIGHEST sequence number for current year (FIXED: order by cak_id, not created_date)
                var getLastIdCmd = new SqlCommand(@"
                    SELECT TOP 1 cak_id 
                    FROM sia_mscutiakademik 
                    WHERE cak_id LIKE '%/PMA/CA/%' 
                      AND cak_id LIKE '%/' + CAST(@year AS VARCHAR(4))
                    ORDER BY CAST(LEFT(cak_id, 3) AS INT) DESC", conn);
                
                getLastIdCmd.Parameters.AddWithValue("@year", year);
                
                var lastId = await getLastIdCmd.ExecuteScalarAsync() as string;
                Console.WriteLine($"[Repository] Last ID found: {lastId}");
                
                int nextSequence = 1;
                
                if (!string.IsNullOrEmpty(lastId))
                {
                    // Extract sequence number from ID like "052/PMA/CA/XII/2025"
                    try
                    {
                        var sequenceStr = lastId.Substring(0, 3);
                        if (int.TryParse(sequenceStr, out int currentSequence))
                        {
                            nextSequence = currentSequence + 1;
                        }
                    }
                    catch
                    {
                        // If parsing fails, start from 1
                        nextSequence = 1;
                    }
                }
                
                Console.WriteLine($"[Repository] Next sequence: {nextSequence}");
                
                // Generate new ID with collision protection
                for (int attempt = 0; attempt < 20; attempt++)
                {
                    string candidateId;
                    
                    // Format sequence number with leading zeros (same as SP)
                    if (nextSequence < 10)
                        candidateId = $"00{nextSequence}{kode}";
                    else if (nextSequence < 100)
                        candidateId = $"0{nextSequence}{kode}";
                    else
                        candidateId = $"{nextSequence}{kode}";
                    
                    Console.WriteLine($"[Repository] Trying candidate ID: {candidateId}");
                    
                    // Check if this ID already exists
                    var checkExistCmd = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM sia_mscutiakademik 
                        WHERE cak_id = @candidateId", conn);
                    checkExistCmd.Parameters.AddWithValue("@candidateId", candidateId);
                    
                    var count = (int)await checkExistCmd.ExecuteScalarAsync();
                    if (count == 0)
                    {
                        Console.WriteLine($"[Repository] ID is unique: {candidateId}");
                        return candidateId;
                    }
                    
                    Console.WriteLine($"[Repository] ID collision detected, trying next sequence");
                    nextSequence++;
                }
                
                // If all attempts fail, use timestamp fallback
                var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds() % 1000;
                var fallbackId = $"{timestamp:D3}{kode}";
                Console.WriteLine($"[Repository] Using fallback ID: {fallbackId}");
                return fallbackId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Repository] ERROR in GenerateUniqueFinalIdSafeAsync: {ex.Message}");
                throw;
            }
        }

        // ============================================================
        // Helper: Convert Month to Roman (Same as SP fnConvertIntToRoman)
        // ============================================================
        private string ConvertToRoman(int month)
        {
            return month switch
            {
                1 => "I",
                2 => "II", 
                3 => "III",
                4 => "IV",
                5 => "V",
                6 => "VI",
                7 => "VII",
                8 => "VIII",
                9 => "IX",
                10 => "X",
                11 => "XI",
                12 => "XII",
                _ => "I"
            };
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
                           a.cak_created_by, 
                           a.cak_created_date,
                           FORMAT(a.cak_created_date, 'dd MMMM yyyy', 'id-ID') as tgl,
                           a.cak_sk, a.srt_no, d.pro_nama, '' as kaprod,
                           CASE 
                               WHEN a.cak_app_prodi_date IS NOT NULL THEN FORMAT(a.cak_app_prodi_date, 'dd MMMM yyyy', 'id-ID')
                               ELSE ''
                           END as cak_app_prodi_date,
                           a.cak_approval_prodi, 
                           CASE 
                               WHEN a.cak_app_dir1_date IS NOT NULL THEN FORMAT(a.cak_app_dir1_date, 'dd MMMM yyyy', 'id-ID')
                               ELSE ''
                           END as cak_app_dir1_date,
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
                // Untuk final ID, gunakan direct SQL query (sama seperti draft) 
                // karena stored procedure mungkin return data yang salah
                var sql = @"
                    SELECT a.cak_id, a.mhs_id, b.mhs_nama, c.kon_nama, b.mhs_angkatan,
                           c.kon_singkatan, a.cak_tahunajaran, a.cak_semester,
                           a.cak_lampiran_suratpengajuan, a.cak_lampiran, a.cak_status,
                           a.cak_created_by, 
                           a.cak_created_date,
                           FORMAT(a.cak_created_date, 'dd MMMM yyyy', 'id-ID') as tgl,
                           a.cak_sk, a.srt_no, d.pro_nama, '' as kaprod,
                           CASE 
                               WHEN a.cak_app_prodi_date IS NOT NULL THEN FORMAT(a.cak_app_prodi_date, 'dd MMMM yyyy', 'id-ID')
                               ELSE ''
                           END as cak_app_prodi_date,
                           a.cak_approval_prodi, 
                           CASE 
                               WHEN a.cak_app_dir1_date IS NOT NULL THEN FORMAT(a.cak_app_dir1_date, 'dd MMMM yyyy', 'id-ID')
                               ELSE ''
                           END as cak_app_dir1_date,
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
        // STEP 1 — Create Draft by Prodi (menggunakan SP khusus prodi)
        // ---------------------------------------------------------
        public async Task<string?> CreateDraftByProdiAsync(CreateCutiProdiRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            try
            {
                // =============================
                // Simpan file terlebih dahulu (sama seperti mahasiswa)
                // =============================
                var fileSP = SaveFile(dto.LampiranSuratPengajuan);
                var fileLampiran = SaveFile(dto.Lampiran);

                // =============================
                // Gunakan SP khusus untuk prodi: sia_createCutiAkademikByProdi
                // =============================
                var cmd = new SqlCommand("sia_createCutiAkademikByProdi", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@p1", "STEP1");
                cmd.Parameters.AddWithValue("@p2", dto.TahunAjaran ?? "");
                cmd.Parameters.AddWithValue("@p3", dto.Semester ?? "");
                cmd.Parameters.AddWithValue("@p4", fileSP ?? "");
                cmd.Parameters.AddWithValue("@p5", fileLampiran ?? "");
                cmd.Parameters.AddWithValue("@p6", dto.MhsId ?? "");
                cmd.Parameters.AddWithValue("@p7", dto.Menimbang ?? "");
                cmd.Parameters.AddWithValue("@p8", dto.ApprovalProdi ?? "");

                // p9–p50 kosong
                for (int i = 9; i <= 50; i++)
                    cmd.Parameters.AddWithValue($"@p{i}", "");

                await cmd.ExecuteNonQueryAsync();

                // Ambil draft id terbaru yang dibuat oleh SP
                var getDraftIdCmd = new SqlCommand(@"
                    SELECT TOP 1 cak_id 
                    FROM sia_mscutiakademik 
                    WHERE cak_created_by = @approval_prodi 
                      AND cak_status = 'Draft'
                      AND cak_id NOT LIKE '%CA%'
                    ORDER BY cak_created_date DESC", conn);
                getDraftIdCmd.Parameters.AddWithValue("@approval_prodi", dto.ApprovalProdi ?? "");

                var draftId = await getDraftIdCmd.ExecuteScalarAsync();
                return draftId?.ToString();
            }
            catch (SqlException ex) when (ex.Number == 2627) // Primary key violation
            {
                // Jika ada collision, coba lagi dengan retry mechanism
                Console.WriteLine($"Primary key collision in CreateDraftByProdiAsync: {ex.Message}");
                
                // Fallback: gunakan direct insert dengan unique ID
                return await CreateDraftByProdiDirectAsync(dto, conn);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateDraftByProdiAsync: {ex.Message}");
                throw;
            }
        }

        // ---------------------------------------------------------
        // Fallback method untuk prodi jika SP gagal
        // ---------------------------------------------------------
        private async Task<string?> CreateDraftByProdiDirectAsync(CreateCutiProdiRequest dto, SqlConnection conn)
        {
            // =============================
            // Simpan file terlebih dahulu (sama seperti mahasiswa)
            // =============================
            var fileSP = SaveFile(dto.LampiranSuratPengajuan);
            var fileLampiran = SaveFile(dto.Lampiran);

            // Generate unique draft ID
            string newDraftId = await GenerateUniqueDraftIdAsync(conn);

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
                    cak_app_prodi_date,
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
                    GETDATE(),
                    'Draft', 
                    GETDATE(), 
                    @created_by
                )";

            var cmd = new SqlCommand(insertSql, conn);
            cmd.Parameters.AddWithValue("@cak_id", newDraftId);
            cmd.Parameters.AddWithValue("@mhs_id", dto.MhsId ?? "");
            cmd.Parameters.AddWithValue("@tahunajaran", dto.TahunAjaran ?? "");
            cmd.Parameters.AddWithValue("@semester", dto.Semester ?? "");
            cmd.Parameters.AddWithValue("@lampiran_sp", fileSP ?? "");
            cmd.Parameters.AddWithValue("@lampiran", fileLampiran ?? "");
            cmd.Parameters.AddWithValue("@menimbang", dto.Menimbang ?? "");
            cmd.Parameters.AddWithValue("@approval_prodi", dto.ApprovalProdi ?? "");
            cmd.Parameters.AddWithValue("@created_by", dto.ApprovalProdi ?? "");

            await cmd.ExecuteNonQueryAsync();
            return newDraftId;
        }

        // ---------------------------------------------------------
        // STEP 2 — Generate Final ID (After Draft) by Prodi
        // ---------------------------------------------------------
        public async Task<string?> GenerateIdByProdiAsync(GenerateCutiProdiIdRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            try
            {
                // Gunakan SP khusus prodi untuk generate final ID
                var cmd = new SqlCommand("sia_createCutiAkademikByProdi", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@p1", "STEP2");
                cmd.Parameters.AddWithValue("@p2", dto.DraftId ?? "");
                cmd.Parameters.AddWithValue("@p3", dto.ModifiedBy ?? "");

                // p4..p50 kosong
                for (int i = 4; i <= 50; i++)
                    cmd.Parameters.AddWithValue($"@p{i}", "");

                await cmd.ExecuteNonQueryAsync();

                // SP akan return final ID di akhir
                var cmd2 = new SqlCommand(
                    @"SELECT TOP 1 cak_id 
                      FROM sia_mscutiakademik 
                      WHERE cak_id LIKE '%CA%'
                      ORDER BY cak_modif_date DESC", conn);

                return (string?)await cmd2.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GenerateIdByProdiAsync: {ex.Message}");
                throw;
            }
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

        // ============================================================
        // APPROVAL & REJECTION METHODS
        // ============================================================
        
        /// <summary>
        /// Menyetujui cuti akademik (prodi/wadir1/finance)
        /// </summary>
        public async Task<bool> ApproveCutiAsync(ApproveCutiAkademikRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_setujuiCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", dto.Id);
            cmd.Parameters.AddWithValue("@p2", dto.Role.ToLower());
            cmd.Parameters.AddWithValue("@p3", dto.ApprovedBy);

            // p4-p50 kosong
            for (int i = 4; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        /// <summary>
        /// Menyetujui cuti akademik oleh prodi
        /// </summary>
        public async Task<bool> ApproveProdiCutiAsync(ApproveProdiCutiRequest dto)
        {
            try
            {
                Console.WriteLine($"[ApproveProdiCutiAsync] Starting approval for ID: {dto.Id}");
                Console.WriteLine($"[ApproveProdiCutiAsync] Menimbang: '{dto.Menimbang}' (Length: {dto.Menimbang?.Length ?? 0})");
                Console.WriteLine($"[ApproveProdiCutiAsync] ApprovedBy: {dto.ApprovedBy}");

                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                // First, check if record exists and get current status
                var checkCmd = new SqlCommand(
                    "SELECT cak_id, cak_status FROM sia_mscutiakademik WHERE cak_id = @id", conn);
                checkCmd.Parameters.AddWithValue("@id", dto.Id);

                var reader = await checkCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    reader.Close();
                    Console.WriteLine($"[ApproveProdiCutiAsync] ERROR: Record not found for ID: {dto.Id}");
                    return false;
                }

                var currentStatus = reader["cak_status"].ToString();
                reader.Close();
                
                Console.WriteLine($"[ApproveProdiCutiAsync] Current status: {currentStatus}");

                // Try stored procedure first
                var spCmd = new SqlCommand("sia_setujuiCutiAkademikProdi", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                spCmd.Parameters.AddWithValue("@p1", dto.Id);
                spCmd.Parameters.AddWithValue("@p2", dto.Menimbang ?? "");
                spCmd.Parameters.AddWithValue("@p3", dto.ApprovedBy);

                // p4-p50 kosong
                for (int i = 4; i <= 50; i++)
                    spCmd.Parameters.AddWithValue($"@p{i}", "");

                Console.WriteLine($"[ApproveProdiCutiAsync] Trying stored procedure first...");
                var spRows = await spCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[ApproveProdiCutiAsync] SP rows affected: {spRows}");

                if (spRows > 0)
                {
                    Console.WriteLine($"[ApproveProdiCutiAsync] SP success!");
                    return true;
                }

                // If SP failed, try direct SQL update as fallback
                Console.WriteLine($"[ApproveProdiCutiAsync] SP failed, trying direct SQL update...");
                
                var directCmd = new SqlCommand(@"
                    UPDATE sia_mscutiakademik 
                    SET cak_menimbang = @menimbang,
                        cak_approval_prodi = @approvedBy,
                        cak_status = 'Belum Disetujui Wadir 1',
                        cak_app_prodi_date = GETDATE()
                    WHERE cak_id = @id", conn);

                directCmd.Parameters.AddWithValue("@id", dto.Id);
                directCmd.Parameters.AddWithValue("@menimbang", dto.Menimbang ?? "");
                directCmd.Parameters.AddWithValue("@approvedBy", dto.ApprovedBy);

                var directRows = await directCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[ApproveProdiCutiAsync] Direct SQL rows affected: {directRows}");

                var success = directRows > 0;
                Console.WriteLine($"[ApproveProdiCutiAsync] Final result: {success}");
                
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApproveProdiCutiAsync] ERROR: {ex.Message}");
                Console.WriteLine($"[ApproveProdiCutiAsync] Stack trace: {ex.StackTrace}");
                throw; // Re-throw to let controller handle it
            }
        }

        /// <summary>
        /// Menolak cuti akademik dengan keterangan
        /// </summary>
        public async Task<bool> RejectCutiAsync(RejectCutiAkademikRequest dto)
        {
            try
            {
                Console.WriteLine($"[RejectCutiAsync] Starting rejection for ID: {dto.Id}");
                Console.WriteLine($"[RejectCutiAsync] Role: {dto.Role}");
                Console.WriteLine($"[RejectCutiAsync] Keterangan: '{dto.Keterangan}' (Length: {dto.Keterangan?.Length ?? 0})");

                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                // First, check if record exists and get current status
                var checkCmd = new SqlCommand(
                    "SELECT cak_id, cak_status FROM sia_mscutiakademik WHERE cak_id = @id", conn);
                checkCmd.Parameters.AddWithValue("@id", dto.Id);

                var reader = await checkCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    reader.Close();
                    Console.WriteLine($"[RejectCutiAsync] ERROR: Record not found for ID: {dto.Id}");
                    return false;
                }

                var currentStatus = reader["cak_status"].ToString();
                reader.Close();
                
                Console.WriteLine($"[RejectCutiAsync] Current status: {currentStatus}");

                // Try stored procedure first
                var spCmd = new SqlCommand("sia_tolakCutiAkademik", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                spCmd.Parameters.AddWithValue("@p1", dto.Id);
                spCmd.Parameters.AddWithValue("@p2", dto.Role);
                spCmd.Parameters.AddWithValue("@p3", dto.Keterangan ?? "");

                // p4-p50 kosong
                for (int i = 4; i <= 50; i++)
                    spCmd.Parameters.AddWithValue($"@p{i}", "");

                Console.WriteLine($"[RejectCutiAsync] Trying stored procedure first...");
                var spRows = await spCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[RejectCutiAsync] SP rows affected: {spRows}");

                if (spRows > 0)
                {
                    Console.WriteLine($"[RejectCutiAsync] SP success!");
                    return true;
                }

                // If SP failed, try direct SQL update as fallback
                Console.WriteLine($"[RejectCutiAsync] SP failed, trying direct SQL update...");
                
                var directCmd = new SqlCommand(@"
                    UPDATE sia_mscutiakademik 
                    SET cak_keterangan = @keterangan,
                        cak_status = @newStatus
                    WHERE cak_id = @id", conn);

                directCmd.Parameters.AddWithValue("@id", dto.Id);
                directCmd.Parameters.AddWithValue("@keterangan", dto.Keterangan ?? "");
                directCmd.Parameters.AddWithValue("@newStatus", $"Ditolak {dto.Role}");

                var directRows = await directCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[RejectCutiAsync] Direct SQL rows affected: {directRows}");

                var success = directRows > 0;
                Console.WriteLine($"[RejectCutiAsync] Final result: {success}");
                
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RejectCutiAsync] ERROR: {ex.Message}");
                Console.WriteLine($"[RejectCutiAsync] Stack trace: {ex.StackTrace}");
                throw; // Re-throw to let controller handle it
            }
        }

        /// <summary>
        /// Create SK Cuti Akademik - Using stored procedure sia_createSKCutiAkademik
        /// </summary>
        public async Task<string?> CreateSKAsync(CreateSKRequest dto)
        {
            try
            {
                Console.WriteLine($"[CreateSKAsync] Starting SK creation for ID: {dto.Id}");
                Console.WriteLine($"[CreateSKAsync] NoSK: {dto.NoSK}, CreatedBy: {dto.CreatedBy}");

                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                // Check if record exists and has correct status
                var checkCmd = new SqlCommand(@"
                    SELECT cak_id, cak_status, srt_no 
                    FROM sia_mscutiakademik 
                    WHERE cak_id = @id", conn);
                checkCmd.Parameters.AddWithValue("@id", dto.Id);

                var reader = await checkCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    reader.Close();
                    Console.WriteLine($"[CreateSKAsync] ERROR: Record not found for ID: {dto.Id}");
                    return null;
                }

                var currentStatus = reader["cak_status"].ToString();
                var existingSrtNo = reader["srt_no"].ToString();
                reader.Close();
                
                Console.WriteLine($"[CreateSKAsync] Current status: {currentStatus}");
                Console.WriteLine($"[CreateSKAsync] Existing srt_no: {existingSrtNo}");

                // Validate status - harus sudah disetujui finance untuk bisa create SK
                if (currentStatus != "Belum Disetujui Finance" && currentStatus != "Menunggu Upload SK")
                {
                    Console.WriteLine($"[CreateSKAsync] ERROR: Invalid status for SK creation: {currentStatus}");
                    Console.WriteLine($"[CreateSKAsync] Expected status: 'Belum Disetujui Finance' or 'Menunggu Upload SK'");
                    return null;
                }

                // Generate nomor SK jika tidak disediakan
                string noSK = dto.NoSK ?? existingSrtNo;
                
                if (string.IsNullOrEmpty(noSK))
                {
                    // Generate nomor SK otomatis
                    var year = DateTime.Now.Year;
                    var month = DateTime.Now.Month;
                    
                    // Get last SK number for this month
                    var getLastNoCmd = new SqlCommand(@"
                        SELECT TOP 1 srt_no 
                        FROM sia_mscutiakademik 
                        WHERE srt_no LIKE @pattern 
                        ORDER BY srt_no DESC", conn);
                    getLastNoCmd.Parameters.AddWithValue("@pattern", $"%/SK-CA/{month:D2}/{year}");
                    
                    var lastNo = await getLastNoCmd.ExecuteScalarAsync();
                    int sequence = 1;
                    
                    if (lastNo != null)
                    {
                        var lastNoStr = lastNo.ToString();
                        var parts = lastNoStr?.Split('/');
                        if (parts != null && parts.Length > 0 && int.TryParse(parts[0], out int lastSeq))
                        {
                            sequence = lastSeq + 1;
                        }
                    }
                    
                    noSK = $"{sequence:D3}/SK-CA/{month:D2}/{year}";
                    Console.WriteLine($"[CreateSKAsync] Generated SK number: {noSK}");
                }

                // Use stored procedure sia_createSKCutiAkademik to finalize SK
                try
                {
                    var spCmd = new SqlCommand("sia_createSKCutiAkademik", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    spCmd.Parameters.AddWithValue("@p1", dto.Id);        // cak_id
                    spCmd.Parameters.AddWithValue("@p2", noSK);          // cak_sk (nomor SK)
                    spCmd.Parameters.AddWithValue("@p3", dto.CreatedBy); // cak_modif_by

                    // p4-p50 kosong
                    for (int i = 4; i <= 50; i++)
                        spCmd.Parameters.AddWithValue($"@p{i}", "");

                    Console.WriteLine($"[CreateSKAsync] Executing stored procedure with SK: {noSK}");
                    var spRows = await spCmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"[CreateSKAsync] SP rows affected: {spRows}");

                    if (spRows > 0)
                    {
                        Console.WriteLine($"[CreateSKAsync] SK created successfully using SP: {noSK}");
                        return noSK;
                    }
                }
                catch (Exception spEx)
                {
                    Console.WriteLine($"[CreateSKAsync] SP failed: {spEx.Message}");
                }

                // Fallback: Update record dengan nomor SK dan ubah status ke "Menunggu Upload SK"
                Console.WriteLine($"[CreateSKAsync] SP failed, using fallback direct SQL...");
                var updateCmd = new SqlCommand(@"
                    UPDATE sia_mscutiakademik 
                    SET srt_no = @noSK,
                        cak_status = 'Menunggu Upload SK',
                        cak_modif_date = GETDATE(),
                        cak_modif_by = @createdBy
                    WHERE cak_id = @id", conn);

                updateCmd.Parameters.AddWithValue("@id", dto.Id);
                updateCmd.Parameters.AddWithValue("@noSK", noSK);
                updateCmd.Parameters.AddWithValue("@createdBy", dto.CreatedBy);

                Console.WriteLine($"[CreateSKAsync] Executing fallback update with SK number: {noSK}");
                var rows = await updateCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[CreateSKAsync] Fallback rows affected: {rows}");

                if (rows > 0)
                {
                    Console.WriteLine($"[CreateSKAsync] SK created successfully with fallback: {noSK}");
                    return noSK;
                }
                
                Console.WriteLine($"[CreateSKAsync] Failed to create SK");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateSKAsync] ERROR: {ex.Message}");
                Console.WriteLine($"[CreateSKAsync] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Upload SK Cuti Akademik (untuk admin) - Using stored procedure sia_createSKCutiAkademik
        /// </summary>
        public async Task<bool> UploadSKAsync(UploadSKRequest dto)
        {
            try
            {
                Console.WriteLine($"[UploadSKAsync] Starting SK upload for ID: {dto.Id}");
                Console.WriteLine($"[UploadSKAsync] File: {dto.FileSK?.FileName}");
                Console.WriteLine($"[UploadSKAsync] UploadBy: {dto.UploadBy}");

                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                // First, check if record exists and has correct status
                var checkCmd = new SqlCommand(
                    "SELECT cak_id, cak_status FROM sia_mscutiakademik WHERE cak_id = @id", conn);
                checkCmd.Parameters.AddWithValue("@id", dto.Id);

                var reader = await checkCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    reader.Close();
                    Console.WriteLine($"[UploadSKAsync] ERROR: Record not found for ID: {dto.Id}");
                    return false;
                }

                var currentStatus = reader["cak_status"].ToString();
                reader.Close();
                
                Console.WriteLine($"[UploadSKAsync] Current status: {currentStatus}");

                if (currentStatus != "Menunggu Upload SK")
                {
                    Console.WriteLine($"[UploadSKAsync] ERROR: Invalid status for SK upload: {currentStatus}");
                    return false;
                }

                // Save file
                var fileName = SaveFile(dto.FileSK);
                if (string.IsNullOrEmpty(fileName))
                {
                    Console.WriteLine($"[UploadSKAsync] ERROR: Failed to save file");
                    return false;
                }

                Console.WriteLine($"[UploadSKAsync] File saved as: {fileName}");

                // Try stored procedure first (hybrid approach)
                try
                {
                    var spCmd = new SqlCommand("sia_createSKCutiAkademik", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    spCmd.Parameters.AddWithValue("@p1", dto.Id);        // cak_id
                    spCmd.Parameters.AddWithValue("@p2", fileName);      // cak_sk (filename)
                    spCmd.Parameters.AddWithValue("@p3", dto.UploadBy);  // cak_modif_by

                    // p4-p50 kosong
                    for (int i = 4; i <= 50; i++)
                        spCmd.Parameters.AddWithValue($"@p{i}", "");

                    Console.WriteLine($"[UploadSKAsync] Trying stored procedure first...");
                    var spRows = await spCmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"[UploadSKAsync] SP rows affected: {spRows}");

                    if (spRows > 0)
                    {
                        Console.WriteLine($"[UploadSKAsync] SP success!");
                        return true;
                    }
                }
                catch (Exception spEx)
                {
                    Console.WriteLine($"[UploadSKAsync] SP failed: {spEx.Message}");
                }

                // If SP failed, use direct SQL with all required fields (fallback)
                Console.WriteLine($"[UploadSKAsync] SP failed, trying direct SQL update...");
                
                var directCmd = new SqlCommand(@"
                    UPDATE sia_mscutiakademik 
                    SET cak_sk = @fileName,
                        cak_status = 'Disetujui',
                        cak_status_cuti = 'Cuti',
                        cak_approval_dakap = GETDATE(),
                        cak_modif_date = GETDATE(),
                        cak_modif_by = @uploadBy
                    WHERE cak_id = @id 
                      AND cak_status = 'Menunggu Upload SK';
                    
                    -- Also update mahasiswa status
                    UPDATE sia_msmahasiswa 
                    SET mhs_status_kuliah = 'Cuti' 
                    WHERE mhs_id = (SELECT mhs_id FROM sia_mscutiakademik WHERE cak_id = @id);", conn);

                directCmd.Parameters.AddWithValue("@id", dto.Id);
                directCmd.Parameters.AddWithValue("@fileName", fileName);
                directCmd.Parameters.AddWithValue("@uploadBy", dto.UploadBy);

                var directRows = await directCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[UploadSKAsync] Direct SQL rows affected: {directRows}");

                var success = directRows > 0;
                Console.WriteLine($"[UploadSKAsync] Final result: {success}");
                
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UploadSKAsync] ERROR: {ex.Message}");
                Console.WriteLine($"[UploadSKAsync] Stack trace: {ex.StackTrace}");
                throw; // Re-throw to let controller handle it
            }
        }
            
    }
}

    
