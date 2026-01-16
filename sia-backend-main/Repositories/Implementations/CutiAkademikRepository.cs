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
        // STEP 1 — Create Draft (Menggunakan SP sia_createCutiAkademik)
        // ============================================================
        public async Task<string?> CreateDraftAsync(CreateDraftCutiRequest dto)
        {
            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            // Simpan file terlebih dahulu
            var fileSP = SaveFile(dto.LampiranSuratPengajuan);
            var fileLampiran = SaveFile(dto.Lampiran);

            // Gunakan stored procedure dengan parameter yang sudah di-ALTER
            await using var cmd = new SqlCommand("sia_createCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Step", "STEP1");
            cmd.Parameters.AddWithValue("@TahunAjaran", dto.TahunAjaran ?? "");
            cmd.Parameters.AddWithValue("@Semester", dto.Semester ?? "");
            cmd.Parameters.AddWithValue("@LampiranSuratPengajuan", fileSP ?? "");
            cmd.Parameters.AddWithValue("@Lampiran", fileLampiran ?? "");
            cmd.Parameters.AddWithValue("@MahasiswaId", dto.MhsId);
            cmd.Parameters.AddWithValue("@DraftId", ""); // Tidak digunakan di STEP1
            cmd.Parameters.AddWithValue("@ModifiedBy", ""); // Tidak digunakan di STEP1

            await cmd.ExecuteNonQueryAsync();

            // Ambil draft ID yang baru dibuat
            var getDraftIdCmd = new SqlCommand(@"
                SELECT TOP 1 cak_id 
                FROM sia_mscutiakademik 
                WHERE cak_created_by = @mhs_id 
                  AND cak_status = 'Draft'
                  AND cak_id NOT LIKE '%CA%'
                ORDER BY cak_created_date DESC", conn);
            getDraftIdCmd.Parameters.AddWithValue("@mhs_id", dto.MhsId);

            var draftId = await getDraftIdCmd.ExecuteScalarAsync();
            return draftId?.ToString();
        }




        // ============================================================
        // STEP 2 — Generate Final ID (Menggunakan SP sia_createCutiAkademik)
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
                reader.Close();
                
                if (status != "Draft")
                {
                    throw new Exception($"Record dengan ID '{dto.DraftId}' bukan dalam status Draft (status: {status}).");
                }
                
                // Gunakan stored procedure dengan parameter yang sudah di-ALTER
                await using var cmd = new SqlCommand("sia_createCutiAkademik", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Step", "STEP2");
                cmd.Parameters.AddWithValue("@TahunAjaran", ""); // Tidak digunakan di STEP2
                cmd.Parameters.AddWithValue("@Semester", ""); // Tidak digunakan di STEP2
                cmd.Parameters.AddWithValue("@LampiranSuratPengajuan", ""); // Tidak digunakan di STEP2
                cmd.Parameters.AddWithValue("@Lampiran", ""); // Tidak digunakan di STEP2
                cmd.Parameters.AddWithValue("@MahasiswaId", ""); // Tidak digunakan di STEP2
                cmd.Parameters.AddWithValue("@DraftId", dto.DraftId);
                cmd.Parameters.AddWithValue("@ModifiedBy", dto.ModifiedBy);

                await cmd.ExecuteNonQueryAsync();
                
                // Ambil final ID yang baru dibuat
                var getFinalIdCmd = new SqlCommand(@"
                    SELECT cak_id 
                    FROM sia_mscutiakademik 
                    WHERE cak_id LIKE '%/PMA/CA/%'
                      AND cak_modif_by = @modifiedBy
                    ORDER BY cak_modif_date DESC", conn);
                getFinalIdCmd.Parameters.AddWithValue("@modifiedBy", dto.ModifiedBy);

                var finalId = await getFinalIdCmd.ExecuteScalarAsync();
                
                if (finalId == null)
                {
                    throw new Exception("Gagal mengambil final ID setelah generate.");
                }
                
                Console.WriteLine($"[Repository] Successfully generated final ID: {finalId}");
                return finalId.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Repository] ERROR in GenerateIdAsync: {ex.Message}");
                throw;
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
            await conn.OpenAsync();
            
            // Gunakan stored procedure dengan parameter yang sudah di-ALTER (tidak disingkat)
            await using var cmd = new SqlCommand("sia_getDataCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Parameter sesuai dengan SP yang sudah di-ALTER
            cmd.Parameters.AddWithValue("@MahasiswaId", mhsId ?? "");
            cmd.Parameters.AddWithValue("@Status", status ?? "");
            cmd.Parameters.AddWithValue("@UserId", userId ?? "");
            cmd.Parameters.AddWithValue("@Search", search ?? "");

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new CutiAkademikListResponse
                {
                    Id = reader["cak_id"].ToString(),
                    IdDisplay = reader["id"].ToString(),
                    MhsId = reader["mhs_id"].ToString(),
                    NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? "",
                    Prodi = reader["kon_nama"]?.ToString() ?? "",
                    TahunAjaran = reader["cak_tahunajaran"].ToString(),
                    Semester = reader["cak_semester"].ToString(),
                    ApproveProdi = reader["approve_prodi"]?.ToString() ?? "",
                    ApproveDir1 = reader["approve_dir1"]?.ToString() ?? "",
                    Tanggal = reader["tanggal"].ToString(),
                    SuratNo = reader["srt_no"]?.ToString() ?? "",
                    Status = reader["status"].ToString(),
                });
            }

            return result;
        }

        // ============================================================
        // GET DETAIL - Menggunakan Stored Procedure
        // ============================================================
        public async Task<CutiAkademikDetailResponse?> GetDetailAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            // Gunakan stored procedure dengan parameter yang sudah di-ALTER
            await using var cmd = new SqlCommand("sia_detailCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CutiAkademikId", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new CutiAkademikDetailResponse
            {
                Id = reader["cak_id"].ToString(),
                MhsId = reader["mhs_id"].ToString(),
                Mahasiswa = reader["mhs_nama"]?.ToString() ?? "",
                Konsentrasi = reader["kon_nama"]?.ToString() ?? "",
                Angkatan = reader["mhs_angkatan"]?.ToString() ?? "",
                KonsentrasiSingkatan = reader["kon_singkatan"]?.ToString() ?? "",
                TahunAjaran = reader["cak_tahunajaran"]?.ToString() ?? "",
                Semester = reader["cak_semester"]?.ToString() ?? "",
                LampiranSP = reader["cak_lampiran_suratpengajuan"]?.ToString() ?? "",
                Lampiran = reader["cak_lampiran"]?.ToString() ?? "",
                Status = reader["cak_status"]?.ToString() ?? "",
                CreatedBy = reader["cak_created_by"]?.ToString() ?? "",
                TglPengajuan = reader["tgl"]?.ToString() ?? "",
                Sk = reader["cak_sk"]?.ToString() ?? "",
                SrtNo = reader["srt_no"]?.ToString() ?? "",
                ProdiNama = reader["pro_nama"]?.ToString() ?? "",
                Kaprodi = reader["kaprod"]?.ToString() ?? "",
                AppProdiDate = reader["cak_app_prodi_date"]?.ToString() ?? "",
                ApprovalProdi = reader["cak_approval_prodi"]?.ToString() ?? "",
                AppDir1Date = reader["cak_app_dir1_date"]?.ToString() ?? "",
                ApprovalDir1 = reader["cak_approval_dir1"]?.ToString() ?? "",
                Alamat = reader["mhs_alamat"]?.ToString() ?? "",
                Menimbang = reader["cak_menimbang"]?.ToString() ?? "",
                BulanCuti = reader["BulanCuti"]?.ToString() ?? "",
                Direktur = reader["direktur"]?.ToString() ?? "",
                Wadir1 = reader["wadir1"]?.ToString() ?? "",
                Wadir2 = reader["wadir2"]?.ToString() ?? "",
                Wadir3 = reader["wadir3"]?.ToString() ?? "",
                KodePos = reader["mhs_kodepos"]?.ToString() ?? "",
            };
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
                // Untuk final ID, gunakan stored procedure dengan parameter yang sudah di-ALTER
                var cmd = new SqlCommand("sia_editCutiAkademik", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CutiAkademikId", id);
                cmd.Parameters.AddWithValue("@TahunAjaran", dto.TahunAjaran ?? "");
                cmd.Parameters.AddWithValue("@Semester", dto.Semester ?? "");
                cmd.Parameters.AddWithValue("@LampiranSuratPengajuan", fileSP ?? "");
                cmd.Parameters.AddWithValue("@Lampiran", fileLampiran ?? "");
                cmd.Parameters.AddWithValue("@ModifiedBy", dto.ModifiedBy ?? "");

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

                // Gunakan parameter yang sudah di-ALTER (tidak disingkat)
                cmd.Parameters.AddWithValue("@CutiAkademikId", id);
                cmd.Parameters.AddWithValue("@ModifiedBy", modifiedBy);

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
            await conn.OpenAsync();
            
            // Gunakan stored procedure dengan parameter yang sudah di-ALTER (tidak disingkat)
            await using var cmd = new SqlCommand("sia_getDataRiwayatCutiAkademik", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Parameter sesuai dengan SP yang sudah di-ALTER
            cmd.Parameters.AddWithValue("@UserId", userId ?? "");
            cmd.Parameters.AddWithValue("@Status", status ?? "");
            cmd.Parameters.AddWithValue("@Search", search ?? "");

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new CutiAkademikListResponse
                {
                    Id = reader["cak_id"].ToString(),
                    IdDisplay = reader["id"].ToString(),
                    MhsId = reader["mhs_id"].ToString(),
                    NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? "",
                    Prodi = reader["kon_nama"]?.ToString() ?? "",
                    TahunAjaran = reader["cak_tahunajaran"].ToString(),
                    Semester = reader["cak_semester"].ToString(),
                    ApproveProdi = reader["approve_prodi"].ToString(),
                    ApproveDir1 = reader["approve_dir1"].ToString(),
                    Tanggal = reader["tanggal"].ToString(),
                    SuratNo = reader["srt_no"]?.ToString() ?? "",
                    Status = reader["status"].ToString()
                });
            }

            return result;
        }

        public async Task<IEnumerable<CutiAkademikRiwayatExcelResponse>> GetRiwayatExcelAsync(string userId)
        {
            var result = new List<CutiAkademikRiwayatExcelResponse>();

            await using var conn = new SqlConnection(_conn);
            
            // Use direct SQL query to filter only "Disetujui" status
            var sql = @"
                SELECT 
                    b.mhs_id as NIM,
                    b.mhs_nama as [Nama Mahasiswa],
                    c.kon_nama as Konsentrasi,
                    FORMAT(a.cak_created_date, 'dd MMMM yyyy', 'id-ID') as [Tanggal Pengajuan],
                    ISNULL(a.srt_no, '') as [No SK],
                    a.cak_id as [No Pengajuan]
                FROM sia_mscutiakademik a
                LEFT JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
                LEFT JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
                WHERE a.cak_status = 'Disetujui'
                  AND (@userId = '' OR a.mhs_id = @userId)
                ORDER BY a.cak_created_date DESC";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId ?? "");

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new CutiAkademikRiwayatExcelResponse
                {
                    NIM = reader["NIM"]?.ToString() ?? "",
                    NamaMahasiswa = reader["Nama Mahasiswa"]?.ToString() ?? "",
                    Konsentrasi = reader["Konsentrasi"]?.ToString() ?? "",
                    TanggalPengajuan = reader["Tanggal Pengajuan"]?.ToString() ?? "",
                    NoSK = reader["No SK"]?.ToString() ?? "",
                    NoPengajuan = reader["No Pengajuan"]?.ToString() ?? ""
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
            try
            {
                Console.WriteLine($"[ApproveCutiAsync] === STARTING APPROVAL ===");
                Console.WriteLine($"[ApproveCutiAsync] ID: '{dto.Id}'");
                Console.WriteLine($"[ApproveCutiAsync] Role: '{dto.Role}'");
                Console.WriteLine($"[ApproveCutiAsync] ApprovedBy: '{dto.ApprovedBy}'");

                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                Console.WriteLine($"[ApproveCutiAsync] Database connection opened successfully");

                // First, check if record exists and get current status
                var checkCmd = new SqlCommand(
                    "SELECT cak_id, cak_status, mhs_id FROM sia_mscutiakademik WHERE cak_id = @id", conn);
                checkCmd.Parameters.AddWithValue("@id", dto.Id);

                Console.WriteLine($"[ApproveCutiAsync] Checking if record exists...");
                var reader = await checkCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    reader.Close();
                    Console.WriteLine($"[ApproveCutiAsync] ERROR: Record not found for ID: '{dto.Id}'");
                    return false;
                }

                var currentStatus = reader["cak_status"].ToString();
                var mhsId = reader["mhs_id"].ToString();
                reader.Close();
                
                Console.WriteLine($"[ApproveCutiAsync] Record found!");
                Console.WriteLine($"[ApproveCutiAsync] Current status: '{currentStatus}'");
                Console.WriteLine($"[ApproveCutiAsync] MHS ID: '{mhsId}'");

                // Handle finance approval specifically
                if (dto.Role.ToLower() == "finance" || dto.Role.ToLower() == "karyawan")
                {
                    Console.WriteLine($"[ApproveCutiAsync] Processing finance approval...");
                    
                    // Finance approval should change status from "Belum Disetujui Finance" to "Menunggu Upload SK"
                    if (currentStatus != "Belum Disetujui Finance")
                    {
                        Console.WriteLine($"[ApproveCutiAsync] ERROR: Invalid status for finance approval");
                        Console.WriteLine($"[ApproveCutiAsync] Current status: '{currentStatus}'");
                        Console.WriteLine($"[ApproveCutiAsync] Expected status: 'Belum Disetujui Finance'");
                        return false;
                    }

                    Console.WriteLine($"[ApproveCutiAsync] Status validation passed, executing finance approval update...");

                    var financeCmd = new SqlCommand(@"
                        UPDATE sia_mscutiakademik 
                        SET cak_approval_dakap = @approvedBy,
                            cak_status = 'Menunggu Upload SK',
                            cak_app_dakap_date = GETDATE(),
                            cak_modif_date = GETDATE(),
                            cak_modif_by = @approvedBy
                        WHERE cak_id = @id", conn);

                    financeCmd.Parameters.AddWithValue("@id", dto.Id);
                    financeCmd.Parameters.AddWithValue("@approvedBy", dto.ApprovedBy);

                    Console.WriteLine($"[ApproveCutiAsync] Executing SQL update...");
                    Console.WriteLine($"[ApproveCutiAsync] Parameters: @id='{dto.Id}', @approvedBy='{dto.ApprovedBy}'");

                    var financeRows = await financeCmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"[ApproveCutiAsync] Finance approval rows affected: {financeRows}");
                    
                    if (financeRows > 0)
                    {
                        Console.WriteLine($"[ApproveCutiAsync] Finance approval successful!");
                        
                        // Verify the update by checking the new status
                        var verifyCmd = new SqlCommand(
                            "SELECT cak_status, cak_approval_dakap FROM sia_mscutiakademik WHERE cak_id = @id", conn);
                        verifyCmd.Parameters.AddWithValue("@id", dto.Id);
                        
                        var verifyReader = await verifyCmd.ExecuteReaderAsync();
                        if (await verifyReader.ReadAsync())
                        {
                            var updatedStatus = verifyReader["cak_status"].ToString();
                            var updatedApproval = verifyReader["cak_approval_dakap"].ToString();
                            Console.WriteLine($"[ApproveCutiAsync] Verification - New status: '{updatedStatus}'");
                            Console.WriteLine($"[ApproveCutiAsync] Verification - New approval: '{updatedApproval}'");
                        }
                        verifyReader.Close();
                        
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"[ApproveCutiAsync] Finance approval failed - no rows affected");
                        Console.WriteLine($"[ApproveCutiAsync] This might indicate the WHERE condition didn't match any records");
                        return false;
                    }
                }

                // For other roles, try stored procedure first
                Console.WriteLine($"[ApproveCutiAsync] Processing non-finance approval for role: {dto.Role}");
                
                var spCmd = new SqlCommand("sia_setujuiCutiAkademik", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                spCmd.Parameters.AddWithValue("@p1", dto.Id);
                spCmd.Parameters.AddWithValue("@p2", dto.Role.ToLower());
                spCmd.Parameters.AddWithValue("@p3", dto.ApprovedBy);

                // p4-p50 kosong
                for (int i = 4; i <= 50; i++)
                    spCmd.Parameters.AddWithValue($"@p{i}", "");

                Console.WriteLine($"[ApproveCutiAsync] Trying stored procedure for role: {dto.Role}");
                var spRows = await spCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[ApproveCutiAsync] SP rows affected: {spRows}");

                // Check status after stored procedure execution to verify if it actually changed
                var newStatusCmd = new SqlCommand("SELECT cak_status FROM sia_mscutiakademik WHERE cak_id = @id", conn);
                newStatusCmd.Parameters.AddWithValue("@id", dto.Id);
                var newStatus = (await newStatusCmd.ExecuteScalarAsync())?.ToString();
                Console.WriteLine($"[ApproveCutiAsync] Status after SP: '{newStatus}'");
                
                // Consider approval successful if status changed from the original status
                bool statusChanged = !string.Equals(currentStatus, newStatus, StringComparison.OrdinalIgnoreCase);
                
                if (statusChanged)
                {
                    Console.WriteLine($"[ApproveCutiAsync] SP success - status changed from '{currentStatus}' to '{newStatus}'!");
                    return true;
                }
                else if (spRows > 0)
                {
                    Console.WriteLine($"[ApproveCutiAsync] SP success based on rows affected!");
                    return true;
                }

                // If SP failed, try role-specific direct SQL update as fallback
                Console.WriteLine($"[ApproveCutiAsync] SP failed, trying direct SQL update...");
                
                string targetStatus = "";
                string approvalField = "";
                string dateField = "";

                switch (dto.Role.ToLower())
                {
                    case "prodi":
                        targetStatus = "Belum Disetujui Wadir 1";
                        approvalField = "cak_approval_prodi";
                        dateField = "cak_app_prodi_date";
                        break;
                    case "wadir1":
                    case "wadir 1":
                        targetStatus = "Belum Disetujui Finance";
                        approvalField = "cak_approval_dir1";
                        dateField = "cak_app_dir1_date";
                        break;
                    default:
                        Console.WriteLine($"[ApproveCutiAsync] ERROR: Unknown role for fallback: {dto.Role}");
                        return false;
                }

                var directCmd = new SqlCommand($@"
                    UPDATE sia_mscutiakademik 
                    SET {approvalField} = @approvedBy,
                        cak_status = @newStatus,
                        {dateField} = GETDATE(),
                        cak_modif_date = GETDATE(),
                        cak_modif_by = @approvedBy
                    WHERE cak_id = @id", conn);

                directCmd.Parameters.AddWithValue("@id", dto.Id);
                directCmd.Parameters.AddWithValue("@approvedBy", dto.ApprovedBy);
                directCmd.Parameters.AddWithValue("@newStatus", targetStatus);

                var directRows = await directCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[ApproveCutiAsync] Direct SQL rows affected: {directRows}");

                var success = directRows > 0;
                Console.WriteLine($"[ApproveCutiAsync] Final result: {success}");
                
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApproveCutiAsync] EXCEPTION: {ex.Message}");
                Console.WriteLine($"[ApproveCutiAsync] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ApproveCutiAsync] Inner exception: {ex.InnerException.Message}");
                }
                throw; // Re-throw to let controller handle it
            }
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

                // Check status after stored procedure execution to verify if it actually changed
                var newStatusCmd = new SqlCommand("SELECT cak_status FROM sia_mscutiakademik WHERE cak_id = @id", conn);
                newStatusCmd.Parameters.AddWithValue("@id", dto.Id);
                var newStatus = (await newStatusCmd.ExecuteScalarAsync())?.ToString();
                Console.WriteLine($"[ApproveProdiCutiAsync] Status after SP: '{newStatus}'");
                
                // Consider approval successful if status changed from the original status
                bool statusChanged = !string.Equals(currentStatus, newStatus, StringComparison.OrdinalIgnoreCase);
                
                if (statusChanged)
                {
                    Console.WriteLine($"[ApproveProdiCutiAsync] SP success - status changed from '{currentStatus}' to '{newStatus}'!");
                    return true;
                }
                else if (spRows > 0)
                {
                    Console.WriteLine($"[ApproveProdiCutiAsync] SP success based on rows affected!");
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
                spCmd.Parameters.AddWithValue("@p3", ""); // Empty keterangan

                // p4-p50 kosong
                for (int i = 4; i <= 50; i++)
                    spCmd.Parameters.AddWithValue($"@p{i}", "");

                Console.WriteLine($"[RejectCutiAsync] Trying stored procedure first...");
                var spRows = await spCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[RejectCutiAsync] SP rows affected: {spRows}");

                // Check status after stored procedure execution to verify if it actually changed
                var newStatusCmd = new SqlCommand("SELECT cak_status FROM sia_mscutiakademik WHERE cak_id = @id", conn);
                newStatusCmd.Parameters.AddWithValue("@id", dto.Id);
                var newStatus = (await newStatusCmd.ExecuteScalarAsync())?.ToString();
                Console.WriteLine($"[RejectCutiAsync] Status after SP: '{newStatus}'");
                
                // Consider rejection successful if status changed from the original status
                bool statusChanged = !string.Equals(currentStatus, newStatus, StringComparison.OrdinalIgnoreCase);
                
                if (statusChanged)
                {
                    Console.WriteLine($"[RejectCutiAsync] SP success - status changed from '{currentStatus}' to '{newStatus}'!");
                    return true;
                }
                else if (spRows > 0)
                {
                    Console.WriteLine($"[RejectCutiAsync] SP success based on rows affected!");
                    return true;
                }

                // If SP failed, try direct SQL update as fallback
                Console.WriteLine($"[RejectCutiAsync] SP failed, trying direct SQL update...");
                
                var directCmd = new SqlCommand(@"
                    UPDATE sia_mscutiakademik 
                    SET cak_keterangan = '',
                        cak_status = @newStatus
                    WHERE cak_id = @id", conn);

                directCmd.Parameters.AddWithValue("@id", dto.Id);
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
        /// Upload SK Cuti Akademik (untuk admin) - Generate SK number automatically and upload file
        /// Logika murni backend: Generate nomor SK tanpa simpan ke database (bypass foreign key)
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

                // First, check if record exists and get current status
                var checkCmd = new SqlCommand(
                    "SELECT cak_id, cak_status, srt_no FROM sia_mscutiakademik WHERE cak_id = @id", conn);
                checkCmd.Parameters.AddWithValue("@id", dto.Id);

                var reader = await checkCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    reader.Close();
                    Console.WriteLine($"[UploadSKAsync] ERROR: Record not found for ID: {dto.Id}");
                    return false;
                }

                var currentStatus = reader["cak_status"].ToString();
                var existingSrtNo = reader["srt_no"]?.ToString() ?? "";
                reader.Close();
                
                Console.WriteLine($"[UploadSKAsync] Current status: {currentStatus}");
                Console.WriteLine($"[UploadSKAsync] Existing srt_no: '{existingSrtNo}'");

                // Allow upload if status is "Menunggu Upload SK" OR "Disetujui" (untuk re-upload)
                if (currentStatus != "Menunggu Upload SK" && currentStatus != "Disetujui")
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

                // Generate SK number untuk keperluan internal/logging (tidak disimpan ke DB)
                var skNumber = await GenerateSKNumberAsync(conn);
                Console.WriteLine($"[UploadSKAsync] Generated SK number (for reference): {skNumber}");

                // Update record WITHOUT srt_no field (bypass foreign key constraint)
                var updateCmd = new SqlCommand(@"
                    UPDATE sia_mscutiakademik 
                    SET cak_sk = @fileName,
                        cak_status = 'Disetujui',
                        cak_status_cuti = 'Cuti',
                        cak_approval_dakap = GETDATE(),
                        cak_modif_date = GETDATE(),
                        cak_modif_by = @uploadBy
                    WHERE cak_id = @id;
                    
                    -- Also update mahasiswa status
                    UPDATE sia_msmahasiswa 
                    SET mhs_status_kuliah = 'Cuti' 
                    WHERE mhs_id = (SELECT mhs_id FROM sia_mscutiakademik WHERE cak_id = @id);", conn);

                updateCmd.Parameters.AddWithValue("@id", dto.Id);
                updateCmd.Parameters.AddWithValue("@fileName", fileName);
                updateCmd.Parameters.AddWithValue("@uploadBy", dto.UploadBy);

                var rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[UploadSKAsync] Update rows affected: {rowsAffected}");

                if (rowsAffected > 0)
                {
                    Console.WriteLine($"[UploadSKAsync] ✓ SUCCESS! SK uploaded (Generated SK for reference: {skNumber})");
                    
                    // Verify the update worked
                    var verifyCmd = new SqlCommand(
                        "SELECT cak_sk, cak_status FROM sia_mscutiakademik WHERE cak_id = @id", conn);
                    verifyCmd.Parameters.AddWithValue("@id", dto.Id);
                    
                    var verifyReader = await verifyCmd.ExecuteReaderAsync();
                    if (await verifyReader.ReadAsync())
                    {
                        var finalSk = verifyReader["cak_sk"]?.ToString() ?? "";
                        var finalStatus = verifyReader["cak_status"]?.ToString() ?? "";
                        Console.WriteLine($"[UploadSKAsync] Verification - cak_sk: '{finalSk}', status: '{finalStatus}'");
                    }
                    verifyReader.Close();
                    
                    return true;
                }
                else
                {
                    Console.WriteLine($"[UploadSKAsync] ✗ FAILED: No rows affected during update");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UploadSKAsync] ERROR: {ex.Message}");
                Console.WriteLine($"[UploadSKAsync] Stack trace: {ex.StackTrace}");
                throw; // Re-throw to let controller handle it
            }
        }

        /// <summary>
        /// Generate sequence number from cak_id for display purposes
        /// </summary>
        private int GenerateSequenceFromId(string cakId)
        {
            try
            {
                if (string.IsNullOrEmpty(cakId))
                    return 1;
                
                // Extract numeric part from ID like "031/PMA/CA/I/2026"
                if (cakId.Contains("/PMA/CA/"))
                {
                    var parts = cakId.Split('/');
                    if (parts.Length > 0 && int.TryParse(parts[0], out int sequence))
                    {
                        return sequence;
                    }
                }
                
                // Fallback: generate from hash of ID
                var hash = Math.Abs(cakId.GetHashCode()) % 999;
                return hash == 0 ? 1 : hash;
            }
            catch
            {
                return 1;
            }
        }
        private async Task<string> GenerateSKNumberAsync(SqlConnection conn)
        {
            try
            {
                var now = DateTime.Now;
                var month = now.Month;
                var year = now.Year;
                
                // Convert month to Roman numerals
                string romanMonth = ConvertToRoman(month);
                string skFormat = $"/PA-WADIR-I/SKC/{romanMonth}/{year}";
                
                Console.WriteLine($"[GenerateSKNumberAsync] Generating SK number for {romanMonth}/{year}");
                
                // Get the highest sequence number for current year
                // Use simpler query to avoid parsing issues
                var getLastSkCmd = new SqlCommand(@"
                    SELECT srt_no 
                    FROM sia_mscutiakademik 
                    WHERE srt_no LIKE '%/PA-WADIR-I/SKC/%/' + CAST(@year AS VARCHAR(4))
                      AND srt_no IS NOT NULL 
                      AND srt_no != ''
                      AND LEN(srt_no) > 10
                    ORDER BY srt_no DESC", conn);
                
                getLastSkCmd.Parameters.AddWithValue("@year", year);
                
                var reader = await getLastSkCmd.ExecuteReaderAsync();
                
                int maxSequence = 0;
                while (await reader.ReadAsync())
                {
                    var srtNo = reader["srt_no"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(srtNo) && srtNo.Length >= 3)
                    {
                        try
                        {
                            // Extract first 3 characters as sequence
                            var sequenceStr = srtNo.Substring(0, 3);
                            if (int.TryParse(sequenceStr, out int sequence))
                            {
                                if (sequence > maxSequence)
                                {
                                    maxSequence = sequence;
                                    Console.WriteLine($"[GenerateSKNumberAsync] Found sequence: {sequence} from SK: {srtNo}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[GenerateSKNumberAsync] Error parsing SK: {srtNo}, Error: {ex.Message}");
                        }
                    }
                }
                reader.Close();
                
                int nextSequence = maxSequence + 1;
                Console.WriteLine($"[GenerateSKNumberAsync] Max sequence found: {maxSequence}, Next will be: {nextSequence}");
                
                // Generate new SK number with collision protection
                for (int attempt = 0; attempt < 100; attempt++)
                {
                    // Format sequence number with leading zeros (always 3 digits)
                    string candidateSkNumber = $"{nextSequence:D3}{skFormat}";
                    
                    Console.WriteLine($"[GenerateSKNumberAsync] Attempt {attempt + 1}: Trying SK number: {candidateSkNumber}");
                    
                    // Check if this SK number already exists
                    var checkExistCmd = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM sia_mscutiakademik 
                        WHERE srt_no = @candidateSkNumber", conn);
                    checkExistCmd.Parameters.AddWithValue("@candidateSkNumber", candidateSkNumber);
                    
                    var count = (int)await checkExistCmd.ExecuteScalarAsync();
                    if (count == 0)
                    {
                        Console.WriteLine($"[GenerateSKNumberAsync] ✓ SK number is unique: {candidateSkNumber}");
                        return candidateSkNumber;
                    }
                    
                    Console.WriteLine($"[GenerateSKNumberAsync] ✗ SK number collision detected, trying next sequence");
                    nextSequence++;
                }
                
                // If all attempts fail, use timestamp-based fallback
                var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds() % 999;
                var fallbackSkNumber = $"{timestamp + 500:D3}{skFormat}"; // Add 500 to avoid low numbers
                Console.WriteLine($"[GenerateSKNumberAsync] ⚠️ Using fallback SK number: {fallbackSkNumber}");
                return fallbackSkNumber;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GenerateSKNumberAsync] ERROR: {ex.Message}");
                Console.WriteLine($"[GenerateSKNumberAsync] Stack trace: {ex.StackTrace}");
                
                // Emergency fallback
                var now = DateTime.Now;
                var romanMonth = ConvertToRoman(now.Month);
                var emergencySkNumber = $"999/PA-WADIR-I/SKC/{romanMonth}/{now.Year}";
                Console.WriteLine($"[GenerateSKNumberAsync] 🚨 Using emergency fallback: {emergencySkNumber}");
                return emergencySkNumber;
            }
        }

        public async Task<string> DetectUserRoleAsync(string username)  
        {
            try
            {
                Console.WriteLine($"[DetectUserRoleAsync] Starting role detection for username: '{username}'");
                
                await using var conn = new SqlConnection(_conn);
                await using var cmd = new SqlCommand("all_getIdentityByUser", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Based on the error message, the SP expects @UsernameToFind parameter
                cmd.Parameters.AddWithValue("@UsernameToFind", username);
                Console.WriteLine($"[DetectUserRoleAsync] Set @UsernameToFind parameter to: '{username}'");

                await conn.OpenAsync();
                Console.WriteLine($"[DetectUserRoleAsync] Connection opened, executing stored procedure...");
                
                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    // Log all available columns for debugging
                    Console.WriteLine($"[DetectUserRoleAsync] Found data! Available columns:");
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var columnName = reader.GetName(i);
                        var columnValue = reader[i]?.ToString() ?? "NULL";
                        Console.WriteLine($"[DetectUserRoleAsync]   {columnName}: '{columnValue}'");
                    }
                    
                    var strMainId = reader["str_main_id"]?.ToString() ?? "";
                    var kryUsername = reader["kry_username"]?.ToString() ?? "";
                    var jabMainId = reader["jab_main_id"]?.ToString() ?? "";
                    var rolId = reader["rol_id"]?.ToString() ?? "";
                    
                    Console.WriteLine($"[DetectUserRoleAsync] Key fields - Username: '{username}', kry_username: '{kryUsername}', str_main_id: '{strMainId}', jab_main_id: '{jabMainId}', rol_id: '{rolId}'");
                    
                    // Updated role detection logic based on jabMainId
                    var role = jabMainId switch
                    {
                        "4" => "wadir1",                    // Wadir position
                        "6" => "prodi",                     // Prodi position  
                        "1" when username.ToLower().Contains("finance") => "finance", // Finance user
                        _ => "other"                        // Default for other positions
                    };
                    
                    // Special case for specific finance users
                    if (username.ToLower().Equals("user_finance") && jabMainId == "1" && strMainId == "27")
                    {
                        role = "finance";
                    }
                    
                    Console.WriteLine($"[DetectUserRoleAsync] Detected role: '{role}' for jabMainId: '{jabMainId}', username: '{username}'");
                    return role;
                }
                
                Console.WriteLine($"[DetectUserRoleAsync] No data found for username: '{username}' - stored procedure returned no rows");
                
                // Let's also try a direct query to see if the user exists in the tables
                await using var directCmd = new SqlCommand(@"
                    SELECT a.kry_username, a.jab_main_id, a.str_main_id, b.rol_id, a.kry_id
                    FROM ess_mskaryawan a 
                    RIGHT JOIN sso_msuser b ON a.kry_username = b.usr_id 
                    WHERE b.usr_id = @username", conn);
                directCmd.Parameters.AddWithValue("@username", username);
                
                await using var directReader = await directCmd.ExecuteReaderAsync();
                if (await directReader.ReadAsync())
                {
                    Console.WriteLine($"[DetectUserRoleAsync] Direct query found data:");
                    for (int i = 0; i < directReader.FieldCount; i++)
                    {
                        var columnName = directReader.GetName(i);
                        var columnValue = directReader[i]?.ToString() ?? "NULL";
                        Console.WriteLine($"[DetectUserRoleAsync]   {columnName}: '{columnValue}'");
                    }
                    
                    var strMainId = directReader["str_main_id"]?.ToString() ?? "";
                    var jabMainId = directReader["jab_main_id"]?.ToString() ?? "";
                    
                    // Updated role detection logic based on jabMainId
                    var role = jabMainId switch
                    {
                        "4" => "wadir1",                    // Wadir position
                        "6" => "prodi",                     // Prodi position  
                        "1" when username.ToLower().Contains("finance") => "finance", // Finance user
                        _ => "other"                        // Default for other positions
                    };
                    
                    // Special case for specific finance users
                    if (username.ToLower().Equals("user_finance") && jabMainId == "1" && strMainId == "27")
                    {
                        role = "finance";
                    }
                    
                    Console.WriteLine($"[DetectUserRoleAsync] Direct query - Detected role: '{role}' for jabMainId: '{jabMainId}', username: '{username}'");
                    return role;
                }
                else
                {
                    Console.WriteLine($"[DetectUserRoleAsync] Direct query also found no data for username: '{username}'");
                }
                
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DetectUserRoleAsync] Error: {ex.Message}");
                Console.WriteLine($"[DetectUserRoleAsync] Stack trace: {ex.StackTrace}");
                return "";
            }
        }
            
    }
}