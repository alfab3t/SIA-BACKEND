using astratech_apps_backend.DTOs.MeninggalDunia;
using astratech_apps_backend.Models;
using astratech_apps_backend.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace astratech_apps_backend.Repositories.Implementations
{
    public class MeninggalDuniaRepository(IConfiguration config) : IMeninggalDuniaRepository
    {
        private readonly string _conn = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(config.GetConnectionString("DefaultConnection")!, Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING"));

        //CREATE DRAFT
        public async Task<string> CreateAsync(CreateMeninggalDuniaRequest dto, string createdBy)
        {
            // This method is kept for backward compatibility
            // Use CreateWithMahasiswaDataAsync for new implementation
            throw new NotImplementedException("Use CreateWithMahasiswaDataAsync instead");
        }

        //CREATE WITH MAHASISWA DATA
        public async Task<string> CreateWithMahasiswaDataAsync(string mhsId, string lampiranFileName, MahasiswaDetailDto mahasiswaData, string createdBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // STEP1: Create Draft dengan temporary numeric ID
            cmd.Parameters.AddWithValue("@p1", "STEP1");

            // @p2 = Lampiran (filename)
            cmd.Parameters.AddWithValue("@p2", lampiranFileName ?? "");

            // @p3 = mhs_id
            cmd.Parameters.AddWithValue("@p3", mhsId ?? "");

            // @p4 = createdBy
            cmd.Parameters.AddWithValue("@p4", createdBy ?? "");

            // @p5 - @p50 harus tetap dikirim
            for (int i = 5; i <= 50; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", "");
            }

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            // Get the created draft ID (temporary numeric ID)
            var getDraftIdCmd = new SqlCommand(
                "SELECT TOP 1 mdu_id FROM sia_msmeninggaldunia WHERE mdu_id NOT LIKE '%MD%' ORDER BY mdu_created_date DESC", 
                conn);
            var draftId = await getDraftIdCmd.ExecuteScalarAsync();

            return draftId?.ToString() ?? "DRAFT_CREATED";
        }

        //GET MAHASISWA DETAIL
        public async Task<MahasiswaDetailDto?> GetMahasiswaDetailAsync(string mhsId)
        {
            await using var conn = new SqlConnection(_conn);
            
            var sql = @"
                SELECT 
                    m.mhs_id,
                    m.mhs_nama,
                    m.mhs_angkatan,
                    p.pro_nama as program_studi,
                    p.pro_singkatan as program_studi_singkatan,
                    k.kon_nama as konsentrasi,
                    k.kon_id as konsentrasi_id
                FROM sia_msmahasiswa m
                LEFT JOIN sia_mskonsentrasi k ON m.kon_id = k.kon_id
                LEFT JOIN sia_msprodi p ON k.pro_id = p.pro_id
                WHERE m.mhs_id = @mhsId";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@mhsId", mhsId);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new MahasiswaDetailDto
                {
                    MhsId = reader["mhs_id"].ToString() ?? "",
                    MhsNama = reader["mhs_nama"].ToString() ?? "",
                    MhsAngkatan = reader["mhs_angkatan"].ToString() ?? "",
                    ProgramStudi = reader["program_studi"].ToString() ?? "",
                    ProgramStudiSingkatan = reader["program_studi_singkatan"].ToString() ?? "",
                    Konsentrasi = reader["konsentrasi"].ToString() ?? "",
                    KonsentrasiId = reader["konsentrasi_id"].ToString() ?? ""
                };
            }

            return null;
        }

        // ========= STEP 2: FINALIZE DRAFT TO OFFICIAL ID =========
        public async Task<string> FinalizeAsync(string draftId, string updatedBy)
        {
            try
            {
                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                // First, check if draft exists and is still in draft status
                var checkSql = "SELECT mdu_status FROM sia_msmeninggaldunia WHERE mdu_id = @draftId";
                await using var checkCmd = new SqlCommand(checkSql, conn);
                checkCmd.Parameters.AddWithValue("@draftId", draftId);
                
                var currentStatus = (await checkCmd.ExecuteScalarAsync())?.ToString();
                if (string.IsNullOrEmpty(currentStatus))
                {
                    Console.WriteLine($"[FinalizeAsync] Draft ID {draftId} not found");
                    return "";
                }

                if (currentStatus != "Draft")
                {
                    Console.WriteLine($"[FinalizeAsync] Draft ID {draftId} already processed with status: {currentStatus}");
                    
                    // If already finalized, try to get the existing official ID
                    if (draftId.Contains("PA/MD"))
                    {
                        return draftId; // Already an official ID
                    }
                    
                    return "";
                }

                // Generate new official ID manually
                var officialId = await GenerateOfficialIdAsync(conn);
                if (string.IsNullOrEmpty(officialId))
                {
                    Console.WriteLine($"[FinalizeAsync] Failed to generate official ID");
                    return "";
                }

                // Update the record with new official ID and status
                var updateSql = @"
                    UPDATE sia_msmeninggaldunia 
                    SET mdu_id = @officialId,
                        mdu_status = 'Belum Disetujui Wadir 1',
                        mdu_modif_by = @updatedBy,
                        mdu_modif_date = GETDATE()
                    WHERE mdu_id = @draftId";

                await using var updateCmd = new SqlCommand(updateSql, conn);
                updateCmd.Parameters.AddWithValue("@officialId", officialId);
                updateCmd.Parameters.AddWithValue("@draftId", draftId);
                updateCmd.Parameters.AddWithValue("@updatedBy", updatedBy);

                var rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    Console.WriteLine($"[FinalizeAsync] Successfully finalized Draft ID: {draftId} -> Official ID: {officialId}");
                    return officialId;
                }
                else
                {
                    Console.WriteLine($"[FinalizeAsync] Failed to update draft {draftId}");
                    return "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FinalizeAsync] Error: {ex.Message}");
                throw;
            }
        }

        private async Task<string> GenerateOfficialIdAsync(SqlConnection conn)
        {
            try
            {
                // Get current year and month
                var now = DateTime.Now;
                var year = now.Year;
                var month = now.Month;
                var romanMonth = GetRomanNumeral(month);

                // Get the highest existing sequence number for this month/year
                var maxSql = @"
                    SELECT ISNULL(MAX(
                        CASE 
                            WHEN mdu_id LIKE '[0-9][0-9][0-9]/PA/MD/' + @romanMonth + '/' + @year
                            THEN CAST(LEFT(mdu_id, 3) AS INT)
                            ELSE 0
                        END
                    ), 0) as MaxSequence
                    FROM sia_msmeninggaldunia 
                    WHERE mdu_id LIKE '%/PA/MD/' + @romanMonth + '/' + @year + '%'";

                await using var maxCmd = new SqlCommand(maxSql, conn);
                maxCmd.Parameters.AddWithValue("@romanMonth", romanMonth);
                maxCmd.Parameters.AddWithValue("@year", year.ToString());

                var maxSequence = (int)await maxCmd.ExecuteScalarAsync();
                Console.WriteLine($"[GenerateOfficialIdAsync] Found max sequence: {maxSequence} for {romanMonth}/{year}");

                // Start from next number
                var nextNumber = maxSequence + 1;

                // Generate ID and check for uniqueness (double-check)
                string officialId;
                int attempts = 0;
                const int maxAttempts = 100;

                do
                {
                    officialId = $"{nextNumber:D3}/PA/MD/{romanMonth}/{year}";
                    
                    // Check if this ID already exists
                    var existsSql = "SELECT COUNT(*) FROM sia_msmeninggaldunia WHERE mdu_id = @officialId";
                    await using var existsCmd = new SqlCommand(existsSql, conn);
                    existsCmd.Parameters.AddWithValue("@officialId", officialId);

                    var exists = (int)await existsCmd.ExecuteScalarAsync();
                    
                    if (exists == 0)
                    {
                        Console.WriteLine($"[GenerateOfficialIdAsync] Generated unique ID: {officialId}");
                        return officialId;
                    }

                    Console.WriteLine($"[GenerateOfficialIdAsync] ID {officialId} already exists, trying next number...");
                    nextNumber++;
                    attempts++;
                    
                } while (attempts < maxAttempts);

                Console.WriteLine($"[GenerateOfficialIdAsync] Could not generate unique ID after {maxAttempts} attempts for {romanMonth}/{year}");
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GenerateOfficialIdAsync] Error: {ex.Message}");
                return "";
            }
        }

        private string GetRomanNumeral(int month)
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

        // ========= DROPDOWN DATA =========
        public async Task<IEnumerable<MahasiswaDropdownDto>> GetMahasiswaListAsync(string? search = null)
        {
            await using var conn = new SqlConnection(_conn);
            
            var sql = @"
                SELECT 
                    m.mhs_id,
                    m.mhs_nama,
                    m.mhs_angkatan,
                    p.pro_nama as program_studi,
                    k.kon_nama as konsentrasi
                FROM sia_msmahasiswa m
                LEFT JOIN sia_mskonsentrasi k ON m.kon_id = k.kon_id
                LEFT JOIN sia_msprodi p ON k.pro_id = p.pro_id
                WHERE m.mhs_status = 'Aktif'";

            if (!string.IsNullOrEmpty(search))
            {
                sql += " AND (UPPER(m.mhs_nama) LIKE '%' + UPPER(@search) + '%' OR UPPER(m.mhs_id) LIKE '%' + UPPER(@search) + '%')";
            }

            sql += " ORDER BY m.mhs_nama";

            await using var cmd = new SqlCommand(sql, conn);
            
            if (!string.IsNullOrEmpty(search))
                cmd.Parameters.AddWithValue("@search", search);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            var list = new List<MahasiswaDropdownDto>();
            while (await reader.ReadAsync())
            {
                list.Add(new MahasiswaDropdownDto
                {
                    MhsId = reader["mhs_id"].ToString() ?? "",
                    MhsNama = reader["mhs_nama"].ToString() ?? "",
                    MhsAngkatan = reader["mhs_angkatan"].ToString() ?? "",
                    ProgramStudi = reader["program_studi"].ToString() ?? "",
                    Konsentrasi = reader["konsentrasi"].ToString() ?? ""
                });
            }

            return list;
        }

        public async Task<IEnumerable<ProgramStudiDropdownDto>> GetProgramStudiListAsync()
        {
            await using var conn = new SqlConnection(_conn);
            
            var sql = @"
                SELECT 
                    pro_id,
                    pro_nama,
                    pro_singkatan
                FROM sia_msprodi
                WHERE pro_status = 'Aktif'
                ORDER BY pro_nama";

            await using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            var list = new List<ProgramStudiDropdownDto>();
            while (await reader.ReadAsync())
            {
                list.Add(new ProgramStudiDropdownDto
                {
                    ProId = reader["pro_id"].ToString() ?? "",
                    ProNama = reader["pro_nama"].ToString() ?? "",
                    ProSingkatan = reader["pro_singkatan"].ToString() ?? ""
                });
            }

            return list;
        }

        //UPDATE SK
        public async Task<bool> UpdateSKAsync(string id, string sk, string spkb, string updatedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createSKMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // SP menerima 50 parameter tapi hanya @p1 - @p4 yang dipakai
            cmd.Parameters.AddWithValue("@p1", id);        // mdu_id
            cmd.Parameters.AddWithValue("@p2", sk);        // mdu_sk
            cmd.Parameters.AddWithValue("@p3", spkb);      // mdu_spkb
            cmd.Parameters.AddWithValue("@p4", updatedBy); // mdu_modif_by

            for (int i = 5; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", DBNull.Value);

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }


        public async Task<IEnumerable<MeninggalDuniaListResponse>> GetAllAsync(string status, string roleId)
        {
            var list = new List<MeninggalDuniaListResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Parameter sesuai dengan SP yang sudah di-ALTER (tidak disingkat)
            cmd.Parameters.AddWithValue("@Status", status ?? "");
            cmd.Parameters.AddWithValue("@RoleId", roleId ?? "");

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new MeninggalDuniaListResponse
                {
                    Id = reader["mdu_id"]?.ToString() ?? "",
                    IdAlternative = reader["mdu_id_alternative"]?.ToString() ?? "",
                    MhsId = reader["mhs_id"]?.ToString() ?? "",
                    ApproveDir1By = reader["mdu_approve_dir1_by"]?.ToString() ?? "",
                    CreatedDate = reader["mdu_created_date"]?.ToString() ?? "",   // varchar(11)
                    TanggalBuat = reader["tanggal_buat"] as DateTime? ?? DateTime.MinValue,
                    SuratNo = reader["srt_no"]?.ToString() ?? "",
                    Status = reader["mdu_status"]?.ToString() ?? ""
                    // Field tambahan (mhs_nama, nim, pro_nama) tersedia di SP tapi tidak di-map ke DTO ini
                    // karena DTO tidak memiliki property tersebut
                });
            }

            return list;
        }


        public async Task<(IEnumerable<MeninggalDuniaListDto> Data, int TotalData)>
        GetAllAsync(GetAllMeninggalDuniaRequest req)
        {
            await using var conn = new SqlConnection(_conn);
            
            // Gunakan query langsung untuk memastikan data bisa diambil
            var sql = @"
                SELECT 
                    a.mdu_id,
                    (case when CHARINDEX('PA',a.mdu_id) > 0 then a.mdu_id else 'Draft' end) as mdu_id_alternative,
                    a.mhs_id,
                    a.mdu_approve_dir1_by,
                    CONVERT(VARCHAR(11),a.mdu_created_date,106) AS mdu_created_date,
                    a.mdu_created_date as tanggal_buat,
                    a.srt_no,
                    a.mdu_status,
                    ISNULL(b.mhs_nama, '') as mhs_nama,
                    ISNULL(b.mhs_id, '') as nim,
                    ISNULL(d.pro_nama, '') as pro_nama
                FROM sia_msmeninggaldunia a
                LEFT JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
                LEFT JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
                LEFT JOIN sia_msprodi d ON c.pro_id = d.pro_id
                WHERE a.mdu_status != 'Dihapus'";

            // Add status filter if provided
            if (!string.IsNullOrEmpty(req.Status))
            {
                sql += " AND a.mdu_status = @Status";
            }

            // Add role filter if provided
            if (!string.IsNullOrEmpty(req.RoleId))
            {
                sql += " AND c.kon_npk = @RoleId";
            }

            sql += " ORDER BY a.mdu_created_date DESC";

            await using var cmd = new SqlCommand(sql, conn);
            
            if (!string.IsNullOrEmpty(req.Status))
                cmd.Parameters.AddWithValue("@Status", req.Status);
            
            if (!string.IsNullOrEmpty(req.RoleId))
                cmd.Parameters.AddWithValue("@RoleId", req.RoleId);

            await conn.OpenAsync();

            var reader = await cmd.ExecuteReaderAsync();
            var list = new List<MeninggalDuniaListDto>();

            while (await reader.ReadAsync())
            {
                var recordStatus = reader["mdu_status"]?.ToString() ?? "";
                var createdDate = reader["tanggal_buat"] as DateTime?;
                
                // Baca nomor SK dari database (srt_no) yang sudah di-generate oleh SP
                string nomorSK = reader["srt_no"]?.ToString() ?? "";
                
                // Jika srt_no kosong dan status "Disetujui", generate dinamis sebagai fallback
                if (string.IsNullOrEmpty(nomorSK) && recordStatus == "Disetujui" && createdDate.HasValue)
                {
                    var month = createdDate.Value.Month;
                    var year = createdDate.Value.Year;
                    var romanMonth = ConvertToRoman(month);
                    
                    // Generate nomor SK berdasarkan ID atau timestamp
                    var mduId = reader["mdu_id"].ToString();
                    var sequence = GenerateSequenceFromMeninggalDuniaId(mduId);
                    nomorSK = $"{sequence:D3}/PA-WADIR-I/SKM/{romanMonth}/{year}";
                }
                
                list.Add(new MeninggalDuniaListDto
                {
                    Id = reader["mdu_id"].ToString(),
                    NoPengajuan = reader["mdu_id_alternative"].ToString(),
                    TanggalPengajuan = reader["mdu_created_date"]?.ToString() ?? "",
                    NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? "",
                    Nim = reader["nim"]?.ToString() ?? "",
                    Prodi = reader["pro_nama"]?.ToString() ?? "",
                    NomorSK = nomorSK, // Baca dari database atau generate dinamis
                    Status = reader["mdu_status"]?.ToString() ?? ""
                });
            }

            // Searching
            if (!string.IsNullOrEmpty(req.SearchKeyword))
            {
                var q = req.SearchKeyword.ToLower();

                list = list.Where(x =>
                       x.NoPengajuan.ToLower().Contains(q) ||
                       x.NamaMahasiswa.ToLower().Contains(q))
                    .ToList();
            }

            // Sorting
            list = req.Sort switch
            {
                "mdu_created_date asc" => list.OrderBy(x => DateTime.Parse(x.TanggalPengajuan)).ToList(),
                "mdu_created_date desc" => list.OrderByDescending(x => DateTime.Parse(x.TanggalPengajuan)).ToList(),
                _ => list
            };

            // Paging
            int total = list.Count;

            list = list
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();

            return (list, total);
        }

        public async Task<MeninggalDuniaReportResponse?> GetReportAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_reportMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new MeninggalDuniaReportResponse
            {
                MhsId = reader["mhs_id"].ToString(),
                MhsNama = reader["mhs_nama"].ToString(),
                Konsentrasi = reader["kon_nama"].ToString(),
                TahunAjaran = reader["srt_tahun_ajaran"].ToString(),
                SuratNo = reader["srt_no"].ToString(),
                Kaprodi = reader["pro_kaprodi"].ToString(),
                Wadir = reader["wadir"].ToString(),
                Direktur = reader["dir"].ToString()
            };
        }




        //READ SEARCH BY ID
        public async Task<MeninggalDunia?> GetByIdAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_detailMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@mdu_id", id);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new MeninggalDunia
            {
                Id = reader["mdu_id"].ToString(),
                MhsId = reader["mhs_id"].ToString(),
                Lampiran = reader["mdu_lampiran"].ToString(),
                ApproveDir1By = reader["mdu_approve_dir1_by"].ToString(),
                ApproveDir1Date = reader["mdu_approve_dir1_date"] as DateTime?,
                SrtNo = reader["srt_no"].ToString(),
                NoSpkb = reader["mdu_no_spkb"].ToString(),
                Sk = reader["mdu_sk"].ToString(),
                Spkb = reader["mdu_spkb"].ToString(),
                Status = reader["mdu_status"].ToString(),
                CreatedBy = reader["mdu_created_by"].ToString(),
                CreatedDate = reader["mdu_created_date"] as DateTime?,
                ModifiedBy = reader["mdu_modif_by"].ToString(),
                ModifiedDate = reader["mdu_modif_date"] as DateTime?
            };
        }


        ////UPDATE
        public async Task<bool> UpdateAsync(string id, UpdateMeninggalDuniaRequest dto, string updatedBy)
        {
            try
            {
                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                // Handle file upload jika ada
                string? fileName = null;
                if (dto.LampiranFile != null)
                {
                    var folder = Path.Combine("uploads", "meninggal", "lampiran");
                    Directory.CreateDirectory(folder);

                    fileName = $"{Guid.NewGuid()}_{dto.LampiranFile.FileName}";
                    var filePath = Path.Combine(folder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await dto.LampiranFile.CopyToAsync(stream);
                    
                    Console.WriteLine($"[UpdateAsync] File uploaded: {fileName}");
                }

                // Tentukan nilai lampiran yang akan diupdate
                var lampiranValue = fileName ?? dto.Lampiran ?? "";

                // Update menggunakan direct SQL untuk fleksibilitas
                var sql = @"
                    UPDATE sia_msmeninggaldunia 
                    SET mdu_lampiran = @lampiran,
                        mdu_modif_by = @updatedBy,
                        mdu_modif_date = GETDATE()";

                // Tambahkan update mhs_id jika disediakan
                if (!string.IsNullOrEmpty(dto.MhsId))
                {
                    sql += ", mhs_id = @mhsId";
                }

                sql += " WHERE mdu_id = @id";

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@lampiran", lampiranValue);
                cmd.Parameters.AddWithValue("@updatedBy", updatedBy);
                
                if (!string.IsNullOrEmpty(dto.MhsId))
                {
                    cmd.Parameters.AddWithValue("@mhsId", dto.MhsId);
                }

                var rows = await cmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[UpdateAsync] Direct SQL - ID: {id}, Rows affected: {rows}, File: {fileName ?? "none"}");

                if (rows > 0)
                {
                    return true;
                }

                // Fallback ke stored procedure jika direct SQL gagal
                await using var spCmd = new SqlCommand("sia_editMeninggalDunia", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Parameter sesuai dengan SP yang sudah di-ALTER (tidak disingkat)
                spCmd.Parameters.AddWithValue("@MeninggalDuniaId", id);
                spCmd.Parameters.AddWithValue("@Lampiran", lampiranValue);
                spCmd.Parameters.AddWithValue("@ModifiedBy", updatedBy);

                var spRows = await spCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[UpdateAsync] SP - ID: {id}, Rows affected: {spRows}");

                return spRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateAsync] Error: {ex.Message}");
                Console.WriteLine($"[UpdateAsync] Stack trace: {ex.StackTrace}");
                throw;
            }
        }




        //DELETE
        public async Task<bool> SoftDeleteAsync(string id, string updatedBy)
        {
            try
            {
                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                
                // Coba dengan query langsung dulu untuk debugging
                var directSql = @"
                    UPDATE sia_msmeninggaldunia 
                    SET mdu_status = 'Dihapus',
                        mdu_modif_by = @updatedBy,
                        mdu_modif_date = GETDATE()
                    WHERE mdu_id = @id";

                await using var directCmd = new SqlCommand(directSql, conn);
                directCmd.Parameters.AddWithValue("@id", id);
                directCmd.Parameters.AddWithValue("@updatedBy", updatedBy);

                var directRows = await directCmd.ExecuteNonQueryAsync();

                Console.WriteLine($"[SoftDeleteAsync] Direct SQL - ID: {id}, UpdatedBy: {updatedBy}, Rows affected: {directRows}");

                if (directRows > 0)
                {
                    return true;
                }

                // Jika direct SQL gagal, coba stored procedure
                await using var spCmd = new SqlCommand("sia_deleteMeninggalDunia", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Parameter sesuai dengan SP yang sudah di-ALTER (tidak disingkat)
                spCmd.Parameters.AddWithValue("@MeninggalDuniaId", id);
                spCmd.Parameters.AddWithValue("@ModifiedBy", updatedBy);

                var spRows = await spCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[SoftDeleteAsync] SP - ID: {id}, UpdatedBy: {updatedBy}, Rows affected: {spRows}");

                return spRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SoftDeleteAsync] Error: {ex.Message}");
                Console.WriteLine($"[SoftDeleteAsync] Stack trace: {ex.StackTrace}");
                throw;
            }
        }


        //UPLOAD SK - Modified to bypass foreign key constraint like CutiAkademik
        public async Task<bool> UploadSKAsync(string id, string sk, string spkb, string updatedBy)
        {
            try
            {
                Console.WriteLine($"[MeninggalDunia UploadSKAsync] Starting SK upload for ID: {id}");
                Console.WriteLine($"[MeninggalDunia UploadSKAsync] SK: {sk}, SPKB: {spkb}, UpdatedBy: {updatedBy}");

                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                // First, check if record exists and get current status
                var checkCmd = new SqlCommand(
                    "SELECT mdu_id, mdu_status FROM sia_msmeninggaldunia WHERE mdu_id = @id", conn);
                checkCmd.Parameters.AddWithValue("@id", id);

                var reader = await checkCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    reader.Close();
                    Console.WriteLine($"[MeninggalDunia UploadSKAsync] ERROR: Record not found for ID: {id}");
                    return false;
                }

                var currentStatus = reader["mdu_status"].ToString();
                reader.Close();
                
                Console.WriteLine($"[MeninggalDunia UploadSKAsync] Current status: {currentStatus}");

                // Allow upload if status is "Menunggu Upload SK" OR "Disetujui" (untuk re-upload)
                if (currentStatus != "Menunggu Upload SK" && currentStatus != "Disetujui")
                {
                    Console.WriteLine($"[MeninggalDunia UploadSKAsync] ERROR: Invalid status for SK upload: {currentStatus}");
                    return false;
                }

                // Generate SK number untuk keperluan internal/logging (tidak disimpan ke DB)
                var skNumber = await GenerateMeninggalDuniaSKNumberAsync(conn);
                Console.WriteLine($"[MeninggalDunia UploadSKAsync] Generated SK number (for reference): {skNumber}");

                // Update record WITHOUT srt_no field (bypass foreign key constraint)
                var updateCmd = new SqlCommand(@"
                    UPDATE sia_msmeninggaldunia 
                    SET mdu_sk = @sk,
                        mdu_spkb = @spkb,
                        mdu_status = 'Disetujui',
                        mdu_modif_by = @updatedBy,
                        mdu_modif_date = GETDATE()
                    WHERE mdu_id = @id;
                    
                    -- Also update mahasiswa status
                    UPDATE sia_msmahasiswa 
                    SET mhs_status_kuliah = 'Meninggal Dunia' 
                    WHERE mhs_id = (SELECT mhs_id FROM sia_msmeninggaldunia WHERE mdu_id = @id);", conn);

                updateCmd.Parameters.AddWithValue("@id", id);
                updateCmd.Parameters.AddWithValue("@sk", sk);
                updateCmd.Parameters.AddWithValue("@spkb", spkb);
                updateCmd.Parameters.AddWithValue("@updatedBy", updatedBy);

                var rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[MeninggalDunia UploadSKAsync] Update rows affected: {rowsAffected}");

                if (rowsAffected > 0)
                {
                    Console.WriteLine($"[MeninggalDunia UploadSKAsync] ✓ SUCCESS! SK uploaded (Generated SK for reference: {skNumber})");
                    
                    // Verify the update worked
                    var verifyCmd = new SqlCommand(
                        "SELECT mdu_sk, mdu_spkb, mdu_status FROM sia_msmeninggaldunia WHERE mdu_id = @id", conn);
                    verifyCmd.Parameters.AddWithValue("@id", id);
                    
                    var verifyReader = await verifyCmd.ExecuteReaderAsync();
                    if (await verifyReader.ReadAsync())
                    {
                        var finalSk = verifyReader["mdu_sk"]?.ToString() ?? "";
                        var finalSpkb = verifyReader["mdu_spkb"]?.ToString() ?? "";
                        var finalStatus = verifyReader["mdu_status"]?.ToString() ?? "";
                        Console.WriteLine($"[MeninggalDunia UploadSKAsync] Verification - SK: '{finalSk}', SPKB: '{finalSpkb}', Status: '{finalStatus}'");
                    }
                    verifyReader.Close();
                    
                    return true;
                }
                else
                {
                    Console.WriteLine($"[MeninggalDunia UploadSKAsync] ✗ FAILED: No rows affected during update");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MeninggalDunia UploadSKAsync] ERROR: {ex.Message}");
                Console.WriteLine($"[MeninggalDunia UploadSKAsync] Stack trace: {ex.StackTrace}");
                throw; // Re-throw to let controller handle it
            }
        }

        /// <summary>
        /// Generate SK number for Meninggal Dunia with format: 010/PA-WADIR-I/SKM/IX/2026
        /// Logika murni backend untuk generate nomor SK otomatis
        /// </summary>
        private async Task<string> GenerateMeninggalDuniaSKNumberAsync(SqlConnection conn)
        {
            try
            {
                var now = DateTime.Now;
                var month = now.Month;
                var year = now.Year;
                
                // Convert month to Roman numerals
                string romanMonth = ConvertToRoman(month);
                string skFormat = $"/PA-WADIR-I/SKM/{romanMonth}/{year}"; // SKM = SK Meninggal
                
                Console.WriteLine($"[GenerateMeninggalDuniaSKNumberAsync] Generating SK number for {romanMonth}/{year}");
                
                // Get the highest sequence number for current year
                var getLastSkCmd = new SqlCommand(@"
                    SELECT srt_no 
                    FROM sia_msmeninggaldunia 
                    WHERE srt_no LIKE '%/PA-WADIR-I/SKM/%/' + CAST(@year AS VARCHAR(4))
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
                                    Console.WriteLine($"[GenerateMeninggalDuniaSKNumberAsync] Found sequence: {sequence} from SK: {srtNo}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[GenerateMeninggalDuniaSKNumberAsync] Error parsing SK: {srtNo}, Error: {ex.Message}");
                        }
                    }
                }
                reader.Close();
                
                int nextSequence = maxSequence + 1;
                Console.WriteLine($"[GenerateMeninggalDuniaSKNumberAsync] Max sequence found: {maxSequence}, Next will be: {nextSequence}");
                
                // Generate new SK number with collision protection
                for (int attempt = 0; attempt < 100; attempt++)
                {
                    // Format sequence number with leading zeros (always 3 digits)
                    string candidateSkNumber = $"{nextSequence:D3}{skFormat}";
                    
                    Console.WriteLine($"[GenerateMeninggalDuniaSKNumberAsync] Attempt {attempt + 1}: Trying SK number: {candidateSkNumber}");
                    
                    // Check if this SK number already exists
                    var checkExistCmd = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM sia_msmeninggaldunia 
                        WHERE srt_no = @candidateSkNumber", conn);
                    checkExistCmd.Parameters.AddWithValue("@candidateSkNumber", candidateSkNumber);
                    
                    var count = (int)await checkExistCmd.ExecuteScalarAsync();
                    if (count == 0)
                    {
                        Console.WriteLine($"[GenerateMeninggalDuniaSKNumberAsync] ✓ SK number is unique: {candidateSkNumber}");
                        return candidateSkNumber;
                    }
                    
                    Console.WriteLine($"[GenerateMeninggalDuniaSKNumberAsync] ✗ SK number collision detected, trying next sequence");
                    nextSequence++;
                }
                
                // If all attempts fail, use timestamp-based fallback
                var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds() % 999;
                var fallbackSkNumber = $"{timestamp + 500:D3}{skFormat}"; // Add 500 to avoid low numbers
                Console.WriteLine($"[GenerateMeninggalDuniaSKNumberAsync] ⚠️ Using fallback SK number: {fallbackSkNumber}");
                return fallbackSkNumber;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GenerateMeninggalDuniaSKNumberAsync] ERROR: {ex.Message}");
                Console.WriteLine($"[GenerateMeninggalDuniaSKNumberAsync] Stack trace: {ex.StackTrace}");
                
                // Emergency fallback
                var now = DateTime.Now;
                var romanMonth = ConvertToRoman(now.Month);
                var emergencySkNumber = $"999/PA-WADIR-I/SKM/{romanMonth}/{now.Year}";
                Console.WriteLine($"[GenerateMeninggalDuniaSKNumberAsync] 🚨 Using emergency fallback: {emergencySkNumber}");
                return emergencySkNumber;
            }
        }

        /// <summary>
        /// Generate sequence number from mdu_id for display purposes
        /// </summary>
        private int GenerateSequenceFromMeninggalDuniaId(string mduId)
        {
            try
            {
                if (string.IsNullOrEmpty(mduId))
                    return 1;
                
                // Extract numeric part from ID like "031/PA-MD/I/2026"
                if (mduId.Contains("/PA-MD/"))
                {
                    var parts = mduId.Split('/');
                    if (parts.Length > 0 && int.TryParse(parts[0], out int sequence))
                    {
                        return sequence;
                    }
                }
                
                // Fallback: generate from hash of ID
                var hash = Math.Abs(mduId.GetHashCode()) % 999;
                return hash == 0 ? 1 : hash;
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// Convert Month to Roman (Same as CutiAkademik)
        /// </summary>
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

        public async Task<IEnumerable<RiwayatMeninggalDuniaListDto>> GetRiwayatAsync(
            string keyword,
            string sort,
            string konsentrasi,
            string roleId
        )
        {
            var list = new List<RiwayatMeninggalDuniaListDto>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataRiwayatMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Parameter sesuai dengan SP yang sudah di-ALTER (tidak disingkat)
            cmd.Parameters.AddWithValue("@Keyword", keyword ?? "");
            cmd.Parameters.AddWithValue("@Sort", sort ?? "mdu_created_date desc");
            cmd.Parameters.AddWithValue("@Konsentrasi", konsentrasi ?? "");
            cmd.Parameters.AddWithValue("@RoleId", roleId ?? "");

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new RiwayatMeninggalDuniaListDto
                {
                    Id = reader["mdu_id"]?.ToString() ?? "",
                    NoPengajuan = reader["mdu_id"]?.ToString() ?? "",
                    TanggalPengajuan = reader["tanggal_buat"]?.ToString() ?? "",
                    NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? "",
                    Prodi = reader["pro_nama"]?.ToString() ?? "",
                    NomorSK = reader["srt_no"]?.ToString() ?? "",
                    Status = reader["mdu_status"]?.ToString() ?? ""
                });
            }

            return list;
        }

        public async Task<(IEnumerable<RiwayatMeninggalDuniaListDto> Data, int TotalData)>
        GetRiwayatAsync(GetRiwayatMeninggalDuniaRequest req)
        {
            var list = new List<RiwayatMeninggalDuniaListDto>();

            await using var conn = new SqlConnection(_conn);
            
            // Gunakan query langsung untuk memastikan data bisa diambil
            var sql = @"
                SELECT 
                    a.mdu_id,
                    a.mhs_id,
                    CONVERT(VARCHAR(11),a.mdu_created_date,106) AS tanggal_buat,
                    a.srt_no,
                    b.mhs_nama,
                    a.mdu_status,
                    ISNULL(d.pro_nama, '') as pro_nama
                FROM sia_msmeninggaldunia a
                LEFT JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
                LEFT JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
                LEFT JOIN sia_msprodi d ON d.pro_id = c.pro_id
                WHERE a.mdu_status NOT IN ('Draft', 'Dihapus')";

            // Add keyword filter if provided
            if (!string.IsNullOrEmpty(req.Keyword))
            {
                sql += @" AND (
                    UPPER(a.mhs_id) LIKE '%' + UPPER(@Keyword) + '%' OR
                    UPPER(b.mhs_nama) LIKE '%' + UPPER(@Keyword) + '%' OR
                    UPPER(a.srt_no) LIKE '%' + UPPER(@Keyword) + '%'
                )";
            }

            // Add konsentrasi filter if provided
            if (!string.IsNullOrEmpty(req.Konsentrasi))
            {
                sql += " AND b.kon_id = @Konsentrasi";
            }

            // Add roleId filter if provided
            if (!string.IsNullOrEmpty(req.RoleId))
            {
                sql += " AND c.kon_npk = @RoleId";
            }

            // Add sorting
            var sort = req.Sort ?? "mdu_created_date desc";
            sql += sort switch
            {
                "mhs_id asc" => " ORDER BY a.mhs_id ASC",
                "mhs_id desc" => " ORDER BY a.mhs_id DESC",
                "mdu_created_date asc" => " ORDER BY a.mdu_created_date ASC",
                "mdu_created_date desc" => " ORDER BY a.mdu_created_date DESC",
                _ => " ORDER BY a.mdu_created_date DESC"
            };

            await using var cmd = new SqlCommand(sql, conn);
            
            if (!string.IsNullOrEmpty(req.Keyword))
                cmd.Parameters.AddWithValue("@Keyword", req.Keyword);
            
            if (!string.IsNullOrEmpty(req.Konsentrasi))
                cmd.Parameters.AddWithValue("@Konsentrasi", req.Konsentrasi);
            
            if (!string.IsNullOrEmpty(req.RoleId))
                cmd.Parameters.AddWithValue("@RoleId", req.RoleId);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new RiwayatMeninggalDuniaListDto
                {
                    Id = reader["mdu_id"].ToString(),
                    NoPengajuan = reader["mdu_id"].ToString(),
                    TanggalPengajuan = reader["tanggal_buat"]?.ToString() ?? "",
                    NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? "",
                    Prodi = reader["pro_nama"]?.ToString() ?? "",
                    NomorSK = reader["srt_no"]?.ToString() ?? "",
                    Status = reader["mdu_status"]?.ToString() ?? ""
                });
            }

            // Total data sebelum paging
            int total = list.Count;

            // Paging FE-style
            list = list
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();

            return (list, total);
        }


        public async Task<IEnumerable<RiwayatMeninggalDuniaExcelResponse>> GetRiwayatExcelAsync(
        string sort,
        string konsentrasi
        )
        {
            var list = new List<RiwayatMeninggalDuniaExcelResponse>();

            await using var conn = new SqlConnection(_conn);
            
            // Gunakan stored procedure dengan parameter yang sudah di-ALTER
            await using var cmd = new SqlCommand("sia_getDataRiwayatMeninggalDuniaExcel", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Parameter sesuai dengan SP yang sudah di-ALTER (tidak disingkat)
            cmd.Parameters.AddWithValue("@Sort", sort ?? "");
            cmd.Parameters.AddWithValue("@Konsentrasi", konsentrasi ?? "");

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new RiwayatMeninggalDuniaExcelResponse
                {
                    NIM = reader["NIM"]?.ToString() ?? "",
                    NamaMahasiswa = reader["Nama Mahasiswa"]?.ToString() ?? "",
                    Konsentrasi = reader["Konsentrasi"]?.ToString() ?? "",
                    TanggalPengajuan = reader["Tanggal Pengajuan"]?.ToString() ?? "",
                    NoSK = reader["No SK"]?.ToString() ?? "",
                    NoPengajuan = reader["No Pengajuan"]?.ToString() ?? ""
                });
            }

            return list;
        }

        // Method ini sudah tidak diperlukan karena kita menggunakan UploadSKAsync yang lebih baik
        // yang sudah support file upload dan bypass foreign key constraint
        /*
        public async Task<bool> UploadSKMeninggalAsync(UploadSKMeninggalRequest request)
        {
            // Method lama - sudah diganti dengan UploadSKAsync
        }
        */

        public async Task<MeninggalDuniaDetailResponse?> GetDetailAsync(string id)
        {
            try
            {
                await using var conn = new SqlConnection(_conn);
                
                // Gunakan stored procedure dengan parameter yang sudah di-ALTER
                await using var cmd = new SqlCommand("sia_getDetailMeninggalDunia", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Parameter sesuai dengan SP yang sudah di-ALTER (tidak disingkat)
                cmd.Parameters.AddWithValue("@MeninggalDuniaId", id);

                await conn.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync()) return null;

                return new MeninggalDuniaDetailResponse
                {
                    MhsId = reader["mhs_id"]?.ToString() ?? "",
                    MhsNama = reader["mhs_nama"]?.ToString() ?? "",
                    KonNama = reader["kon_nama"]?.ToString() ?? "",
                    MhsAngkatan = reader["mhs_angkatan"]?.ToString() ?? "",
                    KonSingkatan = reader["kon_singkatan"]?.ToString() ?? "",
                    Lampiran = reader["mdu_lampiran"]?.ToString() ?? "",
                    Status = reader["mdu_status"]?.ToString() ?? "",
                    CreatedBy = reader["mdu_created_by"]?.ToString() ?? "",
                    ApproveDir1Date = reader["mdu_approve_dir1_date"]?.ToString() ?? "",
                    ApproveDir1By = reader["mdu_approve_dir1_by"]?.ToString() ?? "",
                    SuratNo = reader["srt_no"]?.ToString() ?? "-",
                    NoSpkb = reader["mdu_no_spkb"]?.ToString() ?? "",
                    SK = reader["mdu_sk"]?.ToString() ?? "",
                    SPKB = reader["mdu_spkb"]?.ToString() ?? ""
                };
            }
            catch (Exception ex)
            {
                // Log error jika perlu
                Console.WriteLine($"Error in GetDetailAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ApproveAsync(string id, ApproveMeninggalDuniaRequest dto)
        {
            try
            {
                Console.WriteLine($"[ApproveAsync] Starting approval for ID: '{id}', Role: '{dto.Role}', Username: '{dto.Username}'");
                
                await using var conn = new SqlConnection(_conn);
                
                // Get current status before approval
                var getStatusSql = "SELECT mdu_status FROM sia_msmeninggaldunia WHERE mdu_id = @id";
                await using var getStatusCmd = new SqlCommand(getStatusSql, conn);
                getStatusCmd.Parameters.AddWithValue("@id", id);
                
                await conn.OpenAsync();
                var currentStatus = (await getStatusCmd.ExecuteScalarAsync())?.ToString();
                Console.WriteLine($"[ApproveAsync] Current status before approval: '{currentStatus}'");
                
                if (string.IsNullOrEmpty(currentStatus))
                {
                    Console.WriteLine($"[ApproveAsync] Record not found for ID: '{id}'");
                    return false;
                }
                
                // Execute approval stored procedure
                await using var cmd = new SqlCommand("sia_setujuiMeninggalDunia", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Parameter sesuai dengan SP yang sudah di-ALTER (tidak disingkat)
                cmd.Parameters.AddWithValue("@MeninggalDuniaId", id);
                cmd.Parameters.AddWithValue("@Role", dto.Role);
                cmd.Parameters.AddWithValue("@Username", dto.Username);
                
                Console.WriteLine($"[ApproveAsync] Parameters - @MeninggalDuniaId: '{id}', @Role: '{dto.Role}', @Username: '{dto.Username}'");

                Console.WriteLine($"[ApproveAsync] Executing stored procedure...");
                
                var rows = await cmd.ExecuteNonQueryAsync();
                
                Console.WriteLine($"[ApproveAsync] Stored procedure executed, rows affected: {rows}");
                
                // Check status after approval to verify if it actually changed
                var newStatus = (await getStatusCmd.ExecuteScalarAsync())?.ToString();
                Console.WriteLine($"[ApproveAsync] Status after approval: '{newStatus}'");
                
                // Consider approval successful if status changed from the original status
                bool statusChanged = !string.Equals(currentStatus, newStatus, StringComparison.OrdinalIgnoreCase);
                
                if (statusChanged)
                {
                    Console.WriteLine($"[ApproveAsync] Approval successful - status changed from '{currentStatus}' to '{newStatus}' for ID: '{id}'");
                    return true;
                }
                else if (rows > 0)
                {
                    Console.WriteLine($"[ApproveAsync] Approval successful based on rows affected for ID: '{id}'");
                    return true;
                }
                else
                {
                    Console.WriteLine($"[ApproveAsync] Approval failed - no status change and no rows affected for ID: '{id}'");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApproveAsync] Error: {ex.Message}");
                Console.WriteLine($"[ApproveAsync] Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> RejectAsync(string id, RejectMeninggalDuniaRequest dto)
        {
            try
            {
                Console.WriteLine($"[RejectAsync] Starting rejection for ID: '{id}', Role: '{dto.Role}', Username: '{dto.Username}'");
                
                await using var conn = new SqlConnection(_conn);
                
                // Get current status before rejection
                var getStatusSql = "SELECT mdu_status FROM sia_msmeninggaldunia WHERE mdu_id = @id";
                await using var getStatusCmd = new SqlCommand(getStatusSql, conn);
                getStatusCmd.Parameters.AddWithValue("@id", id);
                
                await conn.OpenAsync();
                var currentStatus = (await getStatusCmd.ExecuteScalarAsync())?.ToString();
                Console.WriteLine($"[RejectAsync] Current status before rejection: '{currentStatus}'");
                
                if (string.IsNullOrEmpty(currentStatus))
                {
                    Console.WriteLine($"[RejectAsync] Record not found for ID: '{id}'");
                    return false;
                }
                
                // Execute rejection stored procedure
                await using var cmd = new SqlCommand("sia_tolakMeninggalDunia", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Parameter sesuai dengan SP yang sudah di-ALTER (tidak disingkat)
                cmd.Parameters.AddWithValue("@MeninggalDuniaId", id);
                cmd.Parameters.AddWithValue("@Role", dto.Role ?? "");
                cmd.Parameters.AddWithValue("@Username", dto.Username ?? "");
                
                Console.WriteLine($"[RejectAsync] Parameters - @MeninggalDuniaId: '{id}', @Role: '{dto.Role}', @Username: '{dto.Username}'");

                Console.WriteLine($"[RejectAsync] Executing stored procedure...");
                
                var result = await cmd.ExecuteNonQueryAsync();
                
                Console.WriteLine($"[RejectAsync] Stored procedure executed, rows affected: {result}");
                
                // Check status after rejection to verify if it actually changed
                var newStatus = (await getStatusCmd.ExecuteScalarAsync())?.ToString();
                Console.WriteLine($"[RejectAsync] Status after rejection: '{newStatus}'");
                
                // Consider rejection successful if status changed from the original status
                bool statusChanged = !string.Equals(currentStatus, newStatus, StringComparison.OrdinalIgnoreCase);
                
                if (statusChanged)
                {
                    Console.WriteLine($"[RejectAsync] Rejection successful - status changed from '{currentStatus}' to '{newStatus}' for ID: '{id}'");
                    return true;
                }
                else if (result > 0)
                {
                    Console.WriteLine($"[RejectAsync] Rejection successful based on rows affected for ID: '{id}'");
                    return true;
                }
                else
                {
                    Console.WriteLine($"[RejectAsync] Rejection failed - no status change and no rows affected for ID: '{id}'");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RejectAsync] Error: {ex.Message}");
                Console.WriteLine($"[RejectAsync] Stack trace: {ex.StackTrace}");
                return false;
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
                    
                    // Role detection based on str_main_id as mentioned by user
                    var role = strMainId switch
                    {
                        "27" or "23" or "28" => "finance",
                        _ => "wadir1"
                    };
                    
                    Console.WriteLine($"[DetectUserRoleAsync] Detected role: '{role}' for str_main_id: '{strMainId}'");
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
                    var role = strMainId switch
                    {
                        "27" or "23" or "28" => "finance",
                        _ => "wadir1"
                    };
                    
                    Console.WriteLine($"[DetectUserRoleAsync] Direct query - Detected role: '{role}' for str_main_id: '{strMainId}'");
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

        // ========= STORED PROCEDURE METHODS =========
        public async Task<IEnumerable<MahasiswaDropdownSPDto>> GetMahasiswaDropdownSPAsync()
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("lpm_getListMahasiswa", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // SP memerlukan 50 parameter tapi tidak digunakan
            for (int i = 1; i <= 50; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", "");
            }

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            var list = new List<MahasiswaDropdownSPDto>();
            while (await reader.ReadAsync())
            {
                list.Add(new MahasiswaDropdownSPDto
                {
                    Value = reader["Value"].ToString() ?? "",
                    Text = reader["Text"].ToString() ?? "",
                    NimNama = reader["NimNama"].ToString() ?? ""
                });
            }

            return list;
        }

        public async Task<MahasiswaProdiDto?> GetMahasiswaProdiSPAsync(string mhsId)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("lpm_getListMahasiswaByProdi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // @p1 = mhs_id
            cmd.Parameters.AddWithValue("@p1", mhsId);

            // @p2 - @p50 harus tetap dikirim kosong
            for (int i = 2; i <= 50; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", "");
            }

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new MahasiswaProdiDto
                {
                    KonId = reader["kon_id"].ToString() ?? "",
                    ProId = reader["pro_id"].ToString() ?? "",
                    ProNama = reader["pro_nama"].ToString() ?? ""
                };
            }

            return null;
        }



    }
}
