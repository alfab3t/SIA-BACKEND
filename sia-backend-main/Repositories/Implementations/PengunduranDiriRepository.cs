#nullable disable
using astratech_apps_backend.DTOs.PengunduranDiri;
using astratech_apps_backend.DTOs.Common;
using astratech_apps_backend.Models;
using astratech_apps_backend.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;

namespace astratech_apps_backend.Repositories.Implementations
{
    public class PengunduranDiriRepository : IPengunduranDiriRepository
    {
        private readonly string _conn = string.Empty;

        public PengunduranDiriRepository(IConfiguration config)
        {
            try
            {
                var encryptedConn = config.GetConnectionString("DefaultConnection")!;
                var decryptKey = Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING");
                
                Console.WriteLine($"DEBUG: Encrypted connection string exists: {!string.IsNullOrEmpty(encryptedConn)}");
                Console.WriteLine($"DEBUG: Decrypt key exists: {!string.IsNullOrEmpty(decryptKey)}");
                
                if (string.IsNullOrEmpty(decryptKey))
                {
                    Console.WriteLine("WARNING: DECRYPT_KEY_CONNECTION_STRING environment variable not found. Using fallback connection.");
                    _conn = "Server=.\\SQLEXPRESS;Database=ERP_PolmanAstra_NDA;Integrated Security=true;TrustServerCertificate=true;";
                }
                else
                {
                    _conn = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(encryptedConn, decryptKey);
                    Console.WriteLine($"DEBUG: Decrypted connection string length: {_conn?.Length}");
                    Console.WriteLine($"DEBUG: Connection contains 'Server=': {_conn?.Contains("Server=", StringComparison.OrdinalIgnoreCase)}");
                    Console.WriteLine($"DEBUG: Connection contains 'Database=': {_conn?.Contains("Database=", StringComparison.OrdinalIgnoreCase)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in decryption: {ex.Message}");
                Console.WriteLine($"ERROR Stack: {ex.StackTrace}");
                // Fallback connection string
                _conn = "Server=.\\SQLEXPRESS;Database=ERP_PolmanAstra_NDA;Integrated Security=true;TrustServerCertificate=true;";
                Console.WriteLine("Using fallback connection string");
            }
        }

        // STEP 1 - Buat Draft dengan lampiran
        public async Task<string> CreateStep1Async(string mhsId, string createdBy, string? lampiranSuratPengajuan = "", string? lampiran = "")
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_createPengunduranDiri", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@step", "STEP1");
            cmd.Parameters.AddWithValue("@pdi_lampiran_surat_pengajuan", lampiranSuratPengajuan ?? "");
            cmd.Parameters.AddWithValue("@pdi_lampiran", lampiran ?? "");
            cmd.Parameters.AddWithValue("@mhs_id", mhsId);
            cmd.Parameters.AddWithValue("@pdi_id_draft", DBNull.Value);
            cmd.Parameters.AddWithValue("@pdi_modif_by", DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            // Query untuk ambil draft_id yang baru dibuat
            using var cmd2 = new SqlCommand(@"
                SELECT TOP 1 pdi_id 
                FROM sia_mspengundurandiri 
                WHERE pdi_id NOT LIKE '%PD%' AND mhs_id = @mhsId
                ORDER BY pdi_modif_date DESC", conn);
            cmd2.Parameters.AddWithValue("@mhsId", mhsId);
            
            var draftId = await cmd2.ExecuteScalarAsync();
            return draftId?.ToString() ?? "";
        }

        // STEP 2
        public async Task<CreatePengunduranDiriResponse?> CreateStep2Async(string draftId, string createdBy)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_createPengunduranDiri", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@step", "STEP2");
            cmd.Parameters.AddWithValue("@pdi_lampiran_surat_pengajuan", "");
            cmd.Parameters.AddWithValue("@pdi_lampiran", "");
            cmd.Parameters.AddWithValue("@mhs_id", "");
            cmd.Parameters.AddWithValue("@pdi_id_draft", draftId);
            cmd.Parameters.AddWithValue("@pdi_modif_by", createdBy);

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new CreatePengunduranDiriResponse
            {
                PdiId = reader["pdi_id"].ToString(),
                MhsId = reader["mhs_id"].ToString(),
                MhsNama = reader["mhs_nama"].ToString(),
                Konsentrasi = reader["kon_nama"].ToString(),
                Angkatan = reader["mhs_angkatan"].ToString(),
                CreatedBy = reader["pdi_created_by"].ToString()
            };
        }

        // SP baru: Adopsi pola dari sia_getDataPendingDO
        // @username = NIM mahasiswa atau username karyawan
        // @keyword = search keyword
        // @sort_by = sorting
        // @kon_id = filter konsentrasi/prodi
        // @status = multiple status (comma-separated)
        public async Task<IEnumerable<PengunduranDiriListResponse>> GetAllAsync(
            string p1, 
            string keyword, 
            string sortBy, 
            string konId, 
            string status, 
            string userId)
        {
            var list = new List<PengunduranDiriListResponse>();

            try
            {
                Console.WriteLine($"DEBUG GetAllAsync - username: '{p1}', keyword: '{keyword}', konId: '{konId}', status: '{status}', userId: '{userId}'");
                
                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                
                await using var cmd = new SqlCommand("sia_getDataPengunduranDiri", conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 30
                };

                // Parameter sesuai SP baru
                cmd.Parameters.AddWithValue("@username", p1 ?? "");
                cmd.Parameters.AddWithValue("@keyword", keyword ?? "");
                cmd.Parameters.AddWithValue("@sort_by", sortBy ?? "");
                cmd.Parameters.AddWithValue("@kon_id", konId ?? "");
                cmd.Parameters.AddWithValue("@pdi_status", "");            // Legacy parameter (not used)
                cmd.Parameters.AddWithValue("@kry_id", userId ?? "");      // Legacy parameter (not used)
                cmd.Parameters.AddWithValue("@status", status ?? "");      // Multiple status (comma-separated)

                using var reader = await cmd.ExecuteReaderAsync();
                Console.WriteLine($"DEBUG GetAllAsync - HasRows: {reader.HasRows}");

                while (await reader.ReadAsync())
                {
                    // SP baru menggunakan ROW_NUMBER, ada kolom rownum dan Count
                    int totalCount = 0;
                    try
                    {
                        totalCount = Convert.ToInt32(reader["Count"]);
                    }
                    catch { }

                    list.Add(new PengunduranDiriListResponse
                    {
                        PdiId = reader["pdi_id"]?.ToString() ?? "",
                        IdAlternative = reader["pdi_id"]?.ToString() ?? "",
                        MhsId = reader["mhs_id"]?.ToString() ?? "",
                        NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? "",
                        ApproveProdi = "",
                        ApproveDir1 = "",
                        Tanggal = reader["pdi_created_date"]?.ToString() ?? "",
                        TanggalDisetujui = "",
                        SuratNo = reader["srt_no"]?.ToString() ?? "",
                        Status = reader["pdi_status"]?.ToString() ?? "",
                        CreatedBy = reader["pdi_created_by"]?.ToString() ?? "",
                        ProdiNama = reader["kon_nama"]?.ToString() ?? "",
                        Konsentrasi = reader["kon_nama"]?.ToString() ?? ""
                    });
                }

                Console.WriteLine($"DEBUG GetAllAsync - Total: {list.Count}");
                return list;
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"SQL ERROR GetAllAsync: {sqlEx.Message}");
                throw new Exception($"Database error: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR GetAllAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<PaginatedResponse<PengunduranDiriListResponse>> GetAllPaginatedAsync(
            string p1, 
            string keyword, 
            string sortBy, 
            string konId, 
            string status, 
            string userId,
            int page, 
            int pageSize)
        {
            var list = new List<PengunduranDiriListResponse>();
            int totalRecords = 0;

            try
            {
                Console.WriteLine($"DEBUG GetAllPaginatedAsync - username: '{p1}', keyword: '{keyword}', konId: '{konId}', status: '{status}', userId: '{userId}'");
                Console.WriteLine($"DEBUG GetAllPaginatedAsync - page: {page}, pageSize: {pageSize}");
                
                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                
                await using var cmd = new SqlCommand("sia_getDataPengunduranDiri", conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 30
                };

                // Parameter sesuai SP baru
                cmd.Parameters.AddWithValue("@username", p1 ?? "");
                cmd.Parameters.AddWithValue("@keyword", keyword ?? "");
                cmd.Parameters.AddWithValue("@sort_by", sortBy ?? "");
                cmd.Parameters.AddWithValue("@kon_id", konId ?? "");
                cmd.Parameters.AddWithValue("@pdi_status", "");            // Legacy parameter (not used)
                cmd.Parameters.AddWithValue("@kry_id", userId ?? "");      // Legacy parameter (not used)
                cmd.Parameters.AddWithValue("@status", status ?? "");      // Multiple status (comma-separated)
                
                // Pagination parameters
                cmd.Parameters.AddWithValue("@Page", page);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using var reader = await cmd.ExecuteReaderAsync();
                Console.WriteLine($"DEBUG GetAllPaginatedAsync - HasRows: {reader.HasRows}");

                while (await reader.ReadAsync())
                {
                    // SP baru menggunakan ROW_NUMBER, ada kolom Count untuk total records
                    if (totalRecords == 0)
                    {
                        try
                        {
                            totalRecords = Convert.ToInt32(reader["Count"]);
                        }
                        catch { }
                    }

                    list.Add(new PengunduranDiriListResponse
                    {
                        PdiId = reader["pdi_id"]?.ToString() ?? "",
                        IdAlternative = reader["pdi_id"]?.ToString() ?? "",
                        MhsId = reader["mhs_id"]?.ToString() ?? "",
                        NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? "",
                        ApproveProdi = "",
                        ApproveDir1 = "",
                        Tanggal = reader["pdi_created_date"]?.ToString() ?? "",
                        TanggalDisetujui = "",
                        SuratNo = reader["srt_no"]?.ToString() ?? "",
                        Status = reader["pdi_status"]?.ToString() ?? "",
                        CreatedBy = reader["pdi_created_by"]?.ToString() ?? "",
                        ProdiNama = reader["kon_nama"]?.ToString() ?? "",
                        Konsentrasi = reader["kon_nama"]?.ToString() ?? ""
                    });
                }

                Console.WriteLine($"DEBUG GetAllPaginatedAsync - Total: {totalRecords}, Current: {list.Count}");

                return new PaginatedResponse<PengunduranDiriListResponse>
                {
                    Data = list,
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalRecords = totalRecords,
                        TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
                    }
                };
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"SQL ERROR GetAllPaginatedAsync: {sqlEx.Message}");
                throw new Exception($"Database error: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR GetAllPaginatedAsync: {ex.Message}");
                throw;
            }
        }


        public async Task<PengunduranDiri?> GetByIdAsync(string id)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_detailPengunduranDIri", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@pdi_id", id);

            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();

            if (!await r.ReadAsync())
                return null;

            return new PengunduranDiri
            {
                Id = r["pdi_id"].ToString(),
                MhsId = r["mhs_id"].ToString(),
                LampiranSuratPengajuan = r["pdi_lampiransuratpengajuan"].ToString(),
                Lampiran = r["pdi_lampiran"].ToString(),
                Keterangan = r["pdi_keterangan"].ToString(),
                ApprovalProdiBy = r["pdi_approval_prodi_by"].ToString(),
                AppProdiDate = r["pdi_app_prodi_date"] as DateTime?,
                ApprovalDir1By = r["pdi_approval_dir1_by"].ToString(),
                AppDir1Date = r["pdi_app_dir1_date"] as DateTime?,
                SrtNo = r["srt_no"].ToString(),
                NoSkpb = r["pdi_no_skpb"].ToString(),
                Sk = r["pdi_sk"].ToString(),
                Skpb = r["pdi_skpb"].ToString(),
                Status = r["pdi_status"].ToString(),
                CreatedBy = r["pdi_created_by"].ToString(),
                CreatedDate = r["pdi_created_date"] as DateTime?,
                ModifiedBy = r["pdi_modif_by"].ToString(),
                ModifiedDate = r["pdi_modif_date"] as DateTime?
            };
        }

        public async Task<bool> UpdateAsync(string id, UpdatePengunduranDiriRequest dto, string updatedBy)
        {
            try
            {
                await using var conn = new SqlConnection(_conn);
                await using var cmd = new SqlCommand("sia_editPengunduranDiri", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@pdi_id", id);
                cmd.Parameters.AddWithValue("@pdi_lampiran_surat_pengajuan", dto.LampiranSuratPengajuan ?? "");
                cmd.Parameters.AddWithValue("@pdi_lampiran", dto.Lampiran ?? "");
                cmd.Parameters.AddWithValue("@pdi_modif_by", updatedBy);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return true; // SP legacy dengan SET NOCOUNT ON tidak return rows
            }
            catch (Exception)
            {
                return false;
            }
        }


        public async Task<bool> SoftDeleteAsync(string id, string updatedBy)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                using var cmd = new SqlCommand("sia_deletePengunduranDiri", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@pdi_id", id);
                cmd.Parameters.AddWithValue("@pdi_modif_by", updatedBy);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return true; // SP legacy dengan SET NOCOUNT ON tidak return rows
            }
            catch (Exception)
            {
                return false;
            }
        }


        public async Task<string?> CheckReportAsync(string pdiId)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_checkReportPengunduranDIri", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@pdi_id", pdiId);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString();
        }

        // STEP 1 - Buat Draft by Prodi
        public async Task<string> CreateByProdiStep1Async(string mhsId, string createdBy, string? lampiranSuratPengajuan = "", string? lampiran = "")
        {
            Console.WriteLine($"DEBUG Repository CreateByProdiStep1 - mhsId: '{mhsId}', createdBy: '{createdBy}'");
            
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_createPengunduranDiriByProdi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@step", "STEP1");
            cmd.Parameters.AddWithValue("@pdi_lampiran_surat_pengajuan", lampiranSuratPengajuan ?? "");
            cmd.Parameters.AddWithValue("@pdi_lampiran", lampiran ?? "");
            cmd.Parameters.AddWithValue("@mhs_id", mhsId);
            cmd.Parameters.AddWithValue("@pdi_created_by", createdBy);
            cmd.Parameters.AddWithValue("@pdi_id_draft", DBNull.Value);
            cmd.Parameters.AddWithValue("@pdi_modif_by", DBNull.Value);

            Console.WriteLine($"DEBUG Repository - @mhs_id: '{mhsId}', @pdi_created_by: '{createdBy}'");

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            // Get draft ID yang baru dibuat
            using var cmd2 = new SqlCommand(@"
                SELECT TOP 1 pdi_id 
                FROM sia_mspengundurandiri 
                WHERE mhs_id = @mhsId AND pdi_id NOT LIKE '%PD%' 
                ORDER BY pdi_created_date DESC", conn);
            cmd2.Parameters.AddWithValue("@mhsId", mhsId);
            
            var draftId = await cmd2.ExecuteScalarAsync();
            var draftIdStr = draftId?.ToString() ?? "";
            
            Console.WriteLine($"DEBUG Repository - draftId: '{draftIdStr}'");

            // Fix: Update created_by karena SP tidak set dengan benar
            if (!string.IsNullOrEmpty(draftIdStr) && !string.IsNullOrEmpty(createdBy))
            {
                Console.WriteLine($"DEBUG Repository - Updating pdi_created_by to '{createdBy}' for draftId '{draftIdStr}'");
                using var cmdFix = new SqlCommand(@"
                    UPDATE sia_mspengundurandiri 
                    SET pdi_created_by = @createdBy 
                    WHERE pdi_id = @draftId", conn);
                cmdFix.Parameters.AddWithValue("@createdBy", createdBy);
                cmdFix.Parameters.AddWithValue("@draftId", draftIdStr);
                var rowsAffected = await cmdFix.ExecuteNonQueryAsync();
                Console.WriteLine($"DEBUG Repository - UPDATE rows affected: {rowsAffected}");
            }

            return draftIdStr;
        }

        // STEP 2 - Submit Draft by Prodi
        public async Task<CreatePengunduranDiriByProdiResponse?> CreateByProdiStep2Async(string draftId, string modifiedBy)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_createPengunduranDiriByProdi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@step", "STEP2");
            cmd.Parameters.AddWithValue("@pdi_lampiran_surat_pengajuan", "");
            cmd.Parameters.AddWithValue("@pdi_lampiran", "");
            cmd.Parameters.AddWithValue("@mhs_id", "");
            cmd.Parameters.AddWithValue("@pdi_created_by", "");
            cmd.Parameters.AddWithValue("@pdi_id_draft", draftId);
            cmd.Parameters.AddWithValue("@pdi_modif_by", modifiedBy);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new CreatePengunduranDiriByProdiResponse
            {
                Id = reader["pdi_id"].ToString() ?? "",
                MhsId = reader["mhs_id"].ToString() ?? "",
                Nama = reader["mhs_nama"].ToString() ?? "",
                Konsentrasi = reader["kon_nama"].ToString() ?? "",
                Angkatan = reader["mhs_angkatan"].ToString() ?? "",
                CreatedBy = reader["pdi_created_by"].ToString() ?? ""
            };
        }

        // Legacy method - gabungan STEP1 & STEP2 (untuk backward compatibility)
        public async Task<CreatePengunduranDiriByProdiResponse> CreateByProdiAsync(CreatePengunduranDiriByProdiRequest dto)
        {
            var draftId = await CreateByProdiStep1Async(dto.MhsId, dto.CreatedBy, dto.LampiranSuratPengajuan, dto.Lampiran);
            var result = await CreateByProdiStep2Async(draftId, dto.CreatedBy);
            return result ?? new CreatePengunduranDiriByProdiResponse();
        }

        public async Task<bool> CreateSKAsync(string id, UploadSKPengunduranDiriRequest dto, string updatedBy)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                using var cmd = new SqlCommand("sia_createSKPengunduranDiri", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@pdi_id", id);
                cmd.Parameters.AddWithValue("@pdi_sk", dto.Sk ?? "");
                cmd.Parameters.AddWithValue("@pdi_skpb", dto.Skpb ?? "");
                cmd.Parameters.AddWithValue("@pdi_modif_by", updatedBy);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                
                return true; // SP legacy dengan SET NOCOUNT ON tidak return rows
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<PengunduranDiriDetailResponse?> GetDetailAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDetailPengunduranDiri", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@pdi_id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new PengunduranDiriDetailResponse
            {
                Id = reader["pdi_id"]?.ToString() ?? "",
                MhsId = reader["mhs_id"]?.ToString() ?? "",
                NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? "",
                KonsentrasiNama = reader["kon_nama"]?.ToString() ?? "",
                Angkatan = reader["mhs_angkatan"]?.ToString() ?? "",
                KonsentrasiSingkatan = reader["kon_singkatan"]?.ToString() ?? "",
                LampiranSuratPengajuan = reader["pdi_lampiransuratpengajuan"]?.ToString() ?? "",
                Lampiran = reader["pdi_lampiran"]?.ToString() ?? "",
                Status = reader["pdi_status"]?.ToString() ?? "",
                CreatedBy = reader["pdi_created_by"]?.ToString() ?? "",
                TanggalSekarang = reader["tgl"]?.ToString() ?? "",
                SK = reader["pdi_sk"]?.ToString() ?? "",
                SuratNo = reader["srt_no"]?.ToString() ?? "",
                ProdiNama = reader["pro_nama"]?.ToString() ?? "",
                Kaprodi = reader["kaprod"]?.ToString() ?? "",
                AppProdiDate = reader["pdi_app_prodi_date"]?.ToString() ?? "",
                ApprovalProdiBy = reader["pdi_approval_prodi_by"]?.ToString() ?? "",
                AppDir1Date = reader["pdi_app_dir1_date"]?.ToString() ?? "",
                ApprovalDir1By = reader["pdi_approval_dir1_by"]?.ToString() ?? "",
                Direktur = reader["direktur"]?.ToString() ?? "",
                Wadir1 = reader["wadir1"]?.ToString() ?? "",
                Wadir2 = reader["wadir2"]?.ToString() ?? "",
                Wadir3 = reader["wadir3"]?.ToString() ?? "",
                NoSK = reader["noSK"]?.ToString() ?? "",
                NoSkpb = reader["pdi_no_skpb"]?.ToString() ?? "",
                Skpb = reader["pdi_skpb"]?.ToString() ?? ""
            };
        }

        public async Task<PengunduranDiriNotifResponse?> GetNotifAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_detailPengunduranDiriNotif", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new PengunduranDiriNotifResponse
            {
                PdiId = reader["pdi_id"].ToString(),
                MhsId = reader["mhs_id"].ToString(),
                NamaMahasiswa = reader["mhs_nama"].ToString(),
                Konsentrasi = reader["kon_nama"].ToString(),
                Angkatan = reader["mhs_angkatan"].ToString(),
                CreatedBy = reader["pdi_created_by"].ToString()
            };
        }

        public async Task<IEnumerable<PengunduranDiriRiwayatResponse>> GetRiwayatAsync(
        string username,
        string status,
        string keyword,
        string orderBy,
        string konsentrasi)
        {
            var list = new List<PengunduranDiriRiwayatResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataRiwayatPengunduranDiri", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@username", username ?? "");
            cmd.Parameters.AddWithValue("@pdi_status", status ?? "");
            cmd.Parameters.AddWithValue("@unused", "");
            cmd.Parameters.AddWithValue("@keyword", keyword ?? "");
            cmd.Parameters.AddWithValue("@order_by", orderBy ?? "");
            cmd.Parameters.AddWithValue("@kon_id", konsentrasi ?? "");
            cmd.Parameters.AddWithValue("@status", status ?? ""); // Support multiple status (comma-separated)
            // No pagination parameters for non-paginated version

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new PengunduranDiriRiwayatResponse
                {
                    PdiId = reader["pdi_id"]?.ToString() ?? "",
                    MhsId = reader["mhs_id"]?.ToString() ?? "",
                    ApproveProdi = reader["approve_prodi"]?.ToString() ?? "",
                    ApproveDir1 = reader["approve_dir1"]?.ToString() ?? "",
                    Tanggal = reader["tanggal"]?.ToString() ?? "",
                    TanggalDisetujui = reader["tanggal_disetujui"]?.ToString() ?? "",
                    SuratNo = reader["srt_no"]?.ToString() ?? "",
                    NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? "",
                    ProdiNama = reader["prodi_nama"]?.ToString() ?? "", // Format: "Teknik Informatika" (tanpa jenjang)
                    Konsentrasi = reader["konsentrasi"]?.ToString() ?? "", // Format: "SE", "DS", dll
                    Status = reader["status"]?.ToString() ?? ""
                });
            }

            return list;
        }

        public async Task<PaginatedResponse<PengunduranDiriRiwayatResponse>> GetRiwayatPaginatedAsync(
            string username,
            string status,
            string keyword,
            string orderBy,
            string konsentrasi,
            int page,
            int pageSize)
        {
            var list = new List<PengunduranDiriRiwayatResponse>();
            int totalRecords = 0;

            Console.WriteLine($"DEBUG GetRiwayatPaginatedAsync - page: {page}, pageSize: {pageSize}, status: '{status}'");

            // Validate and fix orderBy parameter
            var validOrderBy = ValidateOrderBy(orderBy);
            Console.WriteLine($"DEBUG GetRiwayatPaginatedAsync - orderBy: '{validOrderBy}'");

            try
            {
                await using var conn = new SqlConnection(_conn);
                await using var cmd = new SqlCommand("sia_getDataRiwayatPengunduranDiri", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@pdi_status", status);
                cmd.Parameters.AddWithValue("@unused", ""); // SP asli punya parameter @unused
                cmd.Parameters.AddWithValue("@keyword", keyword);
                cmd.Parameters.AddWithValue("@order_by", validOrderBy);
                cmd.Parameters.AddWithValue("@kon_id", konsentrasi);
                cmd.Parameters.AddWithValue("@status", status ?? "");  // Support multiple status (comma-separated)
                
                // Pagination parameters (SP sudah support)
                cmd.Parameters.AddWithValue("@Page", page);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                // Output parameter for total records
                var totalParam = new SqlParameter("@TotalRecords", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(totalParam);

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new PengunduranDiriRiwayatResponse
                    {
                        PdiId = reader["pdi_id"].ToString(),
                        MhsId = reader["mhs_id"].ToString(),
                        ApproveProdi = reader["approve_prodi"]?.ToString() ?? "",
                        ApproveDir1 = reader["approve_dir1"]?.ToString() ?? "",
                        Tanggal = reader["tanggal"]?.ToString() ?? "",
                        TanggalDisetujui = reader["tanggal_disetujui"]?.ToString() ?? "",
                        SuratNo = reader["srt_no"]?.ToString() ?? "",
                        NamaMahasiswa = reader["mhs_nama"].ToString(),
                        ProdiNama = reader["prodi_nama"]?.ToString() ?? "",
                        Konsentrasi = reader["konsentrasi"]?.ToString() ?? "",
                        Status = reader["status"].ToString()
                    });
                }

                reader.Close();
                totalRecords = (int)totalParam.Value;

                Console.WriteLine($"DEBUG GetRiwayatPaginatedAsync - Total: {totalRecords}, Current: {list.Count}");

                return new PaginatedResponse<PengunduranDiriRiwayatResponse>
                {
                    Data = list,
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalRecords = totalRecords,
                        TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR GetRiwayatPaginatedAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<PengunduranDiriRiwayatExcelResponse>> GetRiwayatExcelAsync(
        string orderBy,
        string konsentrasi
        )
        {
            var list = new List<PengunduranDiriRiwayatExcelResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataRiwayatPengunduranDiriExcel", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Fill parameters
            cmd.Parameters.AddWithValue("@username", "");
            cmd.Parameters.AddWithValue("@pdi_status", "");
            cmd.Parameters.AddWithValue("@unused", "");
            cmd.Parameters.AddWithValue("@keyword", "");
            cmd.Parameters.AddWithValue("@order_by", orderBy);
            cmd.Parameters.AddWithValue("@kon_id", konsentrasi);

            await conn.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new PengunduranDiriRiwayatExcelResponse
                {
                    NIM = reader["NIM"].ToString(),
                    NamaMahasiswa = reader["Nama Mahasiswa"].ToString(),
                    Konsentrasi = reader["Konsentrasi"].ToString(),
                    TanggalPengajuan = reader["Tanggal Pengajuan"].ToString(),
                    NoSk = reader["No SK"].ToString(),
                    NoPengajuan = reader["No Pengajuan"].ToString()
                });
            }

            return list;
        }

        public async Task<bool> ApproveAsync(string id, ApprovePengunduranDiriRequest dto)
        {
            try
            {
                Console.WriteLine($"DEBUG ApproveAsync - START");
                Console.WriteLine($"DEBUG ApproveAsync - id: '{id}', role: '{dto.Role}', approvedBy: '{dto.ApprovedBy}'");
                
                // Normalize role to match SP expectations (capitalize first letter)
                var normalizedRole = dto.Role.ToLower() switch
                {
                    "prodi" => "Prodi",
                    "wadir1" => "Wadir1",
                    "wadir 1" => "Wadir1",
                    _ => dto.Role // fallback to original if not recognized
                };
                
                Console.WriteLine($"DEBUG ApproveAsync - normalizedRole: '{normalizedRole}'");
                
                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                
                // Check if data exists before approve
                var checkCmd = new SqlCommand(@"
                    SELECT pdi_id, pdi_status, mhs_id 
                    FROM sia_mspengundurandiri 
                    WHERE pdi_id = @pdi_id", conn);
                checkCmd.Parameters.AddWithValue("@pdi_id", id);
                
                using var reader = await checkCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    Console.WriteLine($"DEBUG ApproveAsync - Data found: pdi_id={reader["pdi_id"]}, status={reader["pdi_status"]}, mhs_id={reader["mhs_id"]}");
                }
                else
                {
                    Console.WriteLine($"ERROR ApproveAsync - Data NOT FOUND for pdi_id: '{id}'");
                    return false;
                }
                reader.Close();
                
                // Execute approve SP
                await using var cmd = new SqlCommand("sia_setujuiPengunduranDiri", conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 60 // Add timeout
                };

                // Required parameters
                cmd.Parameters.AddWithValue("@pdi_id", id);
                cmd.Parameters.AddWithValue("@role", normalizedRole);
                cmd.Parameters.AddWithValue("@approved_by", dto.ApprovedBy);

                Console.WriteLine($"DEBUG ApproveAsync - Executing SP with params: @pdi_id='{id}', @role='{normalizedRole}', @approved_by='{dto.ApprovedBy}'");
                
                await cmd.ExecuteNonQueryAsync();
                
                Console.WriteLine($"DEBUG ApproveAsync - SP executed successfully");
                
                // Verify update
                var verifyCmd = new SqlCommand(@"
                    SELECT pdi_status, pdi_approval_prodi_by, pdi_approval_dir1_by 
                    FROM sia_mspengundurandiri 
                    WHERE pdi_id = @pdi_id", conn);
                verifyCmd.Parameters.AddWithValue("@pdi_id", id);
                
                using var verifyReader = await verifyCmd.ExecuteReaderAsync();
                if (await verifyReader.ReadAsync())
                {
                    Console.WriteLine($"DEBUG ApproveAsync - After update: status={verifyReader["pdi_status"]}, prodi_by={verifyReader["pdi_approval_prodi_by"]}, dir1_by={verifyReader["pdi_approval_dir1_by"]}");
                }
                
                Console.WriteLine($"DEBUG ApproveAsync - END SUCCESS");
                return true;
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"SQL ERROR ApproveAsync: {sqlEx.Message}");
                Console.WriteLine($"SQL ERROR Number: {sqlEx.Number}");
                Console.WriteLine($"SQL ERROR State: {sqlEx.State}");
                Console.WriteLine($"SQL ERROR StackTrace: {sqlEx.StackTrace}");
                if (sqlEx.InnerException != null)
                {
                    Console.WriteLine($"SQL ERROR InnerException: {sqlEx.InnerException.Message}");
                }
                throw new Exception($"Database error during approve: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR ApproveAsync: {ex.Message}");
                Console.WriteLine($"ERROR ApproveAsync StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"ERROR ApproveAsync InnerException: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<bool> RejectAsync(string id, RejectPengunduranDiriRequest dto)
        {
            try
            {
                await using var conn = new SqlConnection(_conn);
                await using var cmd = new SqlCommand("sia_tolakPengunduranDiri", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@pdi_id", id);
                cmd.Parameters.AddWithValue("@role", dto.Role);
                cmd.Parameters.AddWithValue("@alasan_tolak", dto.Reason);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return true; // SP legacy dengan SET NOCOUNT ON tidak return rows
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<MahasiswaListResponse>> GetMahasiswaListAsync()
        {
            var list = new List<MahasiswaListResponse>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("lpm_getListMahasiswa", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            // Tambahkan semua parameter p1-p50 dengan nilai kosong
            for (int i = 1; i <= 50; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", "");
            }

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new MahasiswaListResponse
                {
                    Value = reader["Value"].ToString() ?? "",
                    Text = reader["Text"].ToString() ?? "",
                    NimNama = reader["NimNama"].ToString() ?? ""
                });
            }

            return list;
        }

        public async Task<IEnumerable<MahasiswaByProdiResponse>> GetMahasiswaByProdiAsync(string userId)
        {
            var list = new List<MahasiswaByProdiResponse>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("lpm_getListMahasiswaByProdi", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            // Parameter p1 = userId untuk mencari prodi user
            cmd.Parameters.AddWithValue("@p1", userId);
            
            // Tambahkan parameter p2-p50 dengan nilai kosong
            for (int i = 2; i <= 50; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", "");
            }

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new MahasiswaByProdiResponse
                {
                    // SP lpm_getListMahasiswaByProdi hanya mengembalikan kon_id, pro_id, pro_nama
                    // Tidak ada mhs_id, mhs_nama, NimNama
                    KonsentrasiId = reader["kon_id"].ToString() ?? "",
                    ProdiId = reader["pro_id"].ToString() ?? "",
                    ProdiNama = reader["pro_nama"].ToString() ?? "",
                    // Set default values untuk field yang tidak ada di SP
                    Value = "",
                    Text = "",
                    NimNama = ""
                });
            }

            return list;
        }

        public async Task<IEnumerable<ProdiOptionResponse>> GetListProdiAsync()
        {
            var list = new List<ProdiOptionResponse>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_getListProdi", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new ProdiOptionResponse
                {
                    Value = reader["pro_id"].ToString() ?? "",
                    Text = reader["pro_nama"].ToString() ?? ""
                });
            }

            return list;
        }

        public async Task<MahasiswaProdiResponse?> GetMahasiswaProdiAsync(string mhsId)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("lpm_getListMahasiswaByProdi", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            // Parameter p1 = mhsId
            cmd.Parameters.AddWithValue("@p1", mhsId);
            
            // Tambahkan parameter p2-p50 dengan nilai kosong
            for (int i = 2; i <= 50; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", "");
            }

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new MahasiswaProdiResponse
                {
                    KonId = reader["kon_id"].ToString() ?? "",
                    ProId = reader["pro_id"].ToString() ?? "",
                    ProNama = reader["pro_nama"].ToString() ?? ""
                };
            }

            return null;
        }

        public async Task<MahasiswaAngkatanResponse?> GetMahasiswaAngkatanAsync(string mhsId)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_getListAngkatanByMahasiswa", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            // Parameter p1 = mhsId
            cmd.Parameters.AddWithValue("@p1", mhsId);
            
            // Tambahkan parameter p2-p50 dengan nilai kosong
            for (int i = 2; i <= 50; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", "");
            }

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new MahasiswaAngkatanResponse
                {
                    DulAngkatan = reader["dul_angkatan"].ToString() ?? ""
                };
            }

            return null;
        }

        public async Task<IEnumerable<MahasiswaListResponse>> GetMahasiswaByKonsentrasiAsync(string username)
        {
            var list = new List<MahasiswaListResponse>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_getListMahasiswaByKonsentrasi", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Id", username);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new MahasiswaListResponse
                {
                    Value = reader["mhs_id"].ToString() ?? "",
                    Text = reader["mhs_nama"].ToString() ?? "",
                    NimNama = reader["mhs_nama"].ToString() ?? ""
                });
            }

            return list;
        }

        public async Task<IEnumerable<ProdiOptionResponse>> GetProdiByUserAsync(string username)
        {
            var list = new List<ProdiOptionResponse>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_getListProdibySekprod", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Username", username);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new ProdiOptionResponse
                {
                    Value = reader["pro_id"].ToString() ?? "",
                    Text = reader["pro_nama"].ToString() ?? ""
                });
            }

            return list;
        }

        public async Task<BebasTanggunganResponse?> CekBebasTanggunganAsync(string mhsId)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_checkBebasTanggungan", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UserId", mhsId);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            
            var status = result?.ToString() ?? "";
            var isBebasTanggungan = status == "OK";
            
            string message;
            if (isBebasTanggungan)
            {
                message = "Mahasiswa bebas tanggungan dan dapat melanjutkan proses pengunduran diri.";
            }
            else
            {
                message = "Mahasiswa masih memiliki tanggungan. Silakan selesaikan tanggungan terlebih dahulu.";
            }

            return new BebasTanggunganResponse
            {
                MhsId = mhsId,
                Status = status,
                IsBebasTanggungan = isBebasTanggungan,
                Message = message,
                StatusKeuangan = "",
                StatusJam = "",
                StatusPeminjamanAlat = ""
            };
        }

        public async Task<MahasiswaProfilDetailResponse?> GetProfilMahasiswaAsync(string mhsId)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_getProfilMahasiswa", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@NIM", mhsId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new MahasiswaProfilDetailResponse
            {
                MhsId = reader["mhs_id"]?.ToString() ?? "",
                MhsNama = reader["mhs_nama"]?.ToString() ?? "",
                JenisKelamin = reader["mhs_jenis_kelamin"]?.ToString() ?? "",
                Ttl = reader["ttl"]?.ToString() ?? "",
                Prodi = reader["prodi"]?.ToString() ?? "",
                Angkatan = reader["awal"]?.ToString() ?? "",
                JalurMasuk = reader["dul_jalur"]?.ToString() ?? "",
                StatusKuliah = reader["mhs_status_kuliah"]?.ToString() ?? "",
                StatusBeasiswa = reader["statusBeasiswa"]?.ToString() ?? "",
                DosenWali = reader["mhs_dosen_akademik"]?.ToString() ?? "",
                Email = reader["dul_email"]?.ToString() ?? "",
                VaWisuda = reader["mhs_va_wisuda"]?.ToString() ?? "",
                VaCuti = reader["mhs_va_cuti"]?.ToString() ?? "",
                VaIdcard = reader["mhs_va_idcard"]?.ToString() ?? "",
                VaLainnya = reader["mhs_va_lainnya"]?.ToString() ?? "",
                Nik = reader["dul_nik"]?.ToString() ?? "",
                Nisn = reader["dul_nisn"]?.ToString() ?? "",
                Agama = reader["dul_agama"]?.ToString() ?? "",
                Kewarganegaraan = reader["dul_kewarganegaraan"]?.ToString() ?? "",
                GolonganDarah = reader["dul_golongan_darah"]?.ToString() ?? "",
                Alamat = reader["dul_alamat"]?.ToString() ?? "",
                Kodepos = reader["dul_kodepos"]?.ToString() ?? "",
                Hp = reader["dul_hp"]?.ToString() ?? "",
                Sd = reader["dul_sd"]?.ToString() ?? "",
                SdTahunLulus = reader["dul_sd_tahun_lulus"]?.ToString() ?? "",
                Smp = reader["dul_smp"]?.ToString() ?? "",
                SmpTahunLulus = reader["dul_smp_tahun_lulus"]?.ToString() ?? "",
                Sma = reader["dul_sma"]?.ToString() ?? "",
                SmaTahunLulus = reader["dul_sma_tahun_lulus"]?.ToString() ?? "",
                Pt = reader["dul_pt"]?.ToString() ?? "",
                PtTahunLulus = reader["dul_pt_tahun_lulus"]?.ToString() ?? "",
                Kursus = reader["dul_kursus"]?.ToString() ?? "",
                Hobby = reader["dul_hobby"]?.ToString() ?? "",
                PengalamanKerja = reader["dul_pengalaman_kerja"]?.ToString() ?? "",
                Organisasi = reader["dul_organisasi"]?.ToString() ?? "",
                StatusKawin = reader["dul_status_kawin"]?.ToString() ?? "",
                UkuranSepatu = reader["dul_ukuran_sepatu"]?.ToString() ?? "",
                UkuranKemeja = reader["dul_ukuran_kemeja"]?.ToString() ?? "",
                TinggiBadan = reader["dul_tinggi_badan"]?.ToString() ?? "",
                BeratBadan = reader["dul_berat_badan"]?.ToString() ?? "",
                NamaAyah = reader["dul_nama_ayah"]?.ToString() ?? "",
                NikAyah = reader["dul_nik_ayah"]?.ToString() ?? "",
                StatusAyah = reader["dul_status_ayah"]?.ToString() ?? "",
                KewarganegaraanAyah = reader["dul_kewarganegaraan_ayah"]?.ToString() ?? "",
                AgamaAyah = reader["dul_agama_ayah"]?.ToString() ?? "",
                AlamatAyah = reader["dul_alamat_ayah"]?.ToString() ?? "",
                KodeposAyah = reader["dul_kodepos_ayah"]?.ToString() ?? "",
                HpAyah = reader["dul_hp_ayah"]?.ToString() ?? "",
                PendidikanAyah = reader["dul_pendidikan_ayah"]?.ToString() ?? "",
                PekerjaanAyah = reader["dul_pekerjaan_ayah"]?.ToString() ?? "",
                PerusahaanAyah = reader["dul_perusahaan_ayah"]?.ToString() ?? "",
                AlamatPerusahaanAyah = reader["dul_alamat_perusahaan_ayah"]?.ToString() ?? "",
                PenghasilanAyah = reader["dul_penghasilan_ayah"]?.ToString() ?? "",
                NamaIbu = reader["dul_nama_ibu"]?.ToString() ?? "",
                NikIbu = reader["dul_nik_ibu"]?.ToString() ?? "",
                StatusIbu = reader["dul_status_ibu"]?.ToString() ?? "",
                KewarganegaraanIbu = reader["dul_kewarganegaraan_ibu"]?.ToString() ?? "",
                AgamaIbu = reader["dul_agama_ibu"]?.ToString() ?? "",
                AlamatIbu = reader["dul_alamat_ibu"]?.ToString() ?? "",
                KodeposIbu = reader["dul_kodepos_ibu"]?.ToString() ?? "",
                HpIbu = reader["dul_hp_ibu"]?.ToString() ?? "",
                PendidikanIbu = reader["dul_pendidikan_ibu"]?.ToString() ?? "",
                PekerjaanIbu = reader["dul_pekerjaan_ibu"]?.ToString() ?? "",
                PerusahaanIbu = reader["dul_perusahaan_ibu"]?.ToString() ?? "",
                AlamatPerusahaanIbu = reader["dul_alamat_perusahaan_ibu"]?.ToString() ?? "",
                PenghasilanIbu = reader["dul_penghasilan_ibu"]?.ToString() ?? "",
                NamaWali = reader["dul_nama_wali"]?.ToString() ?? "",
                NikWali = reader["dul_nik_wali"]?.ToString() ?? "",
                StatusWali = reader["dul_status_wali"]?.ToString() ?? "",
                KewarganegaraanWali = reader["dul_kewarganegaraan_wali"]?.ToString() ?? "",
                AgamaWali = reader["dul_agama_wali"]?.ToString() ?? "",
                AlamatWali = reader["dul_alamat_wali"]?.ToString() ?? "",
                KodeposWali = reader["dul_kodepos_wali"]?.ToString() ?? "",
                HpWali = reader["dul_hp_wali"]?.ToString() ?? "",
                PendidikanWali = reader["dul_pendidikan_wali"]?.ToString() ?? "",
                PekerjaanWali = reader["dul_pekerjaan_wali"]?.ToString() ?? "",
                PerusahaanWali = reader["dul_perusahaan_wali"]?.ToString() ?? "",
                AlamatPerusahaanWali = reader["dul_alamat_perusahaan_wali"]?.ToString() ?? "",
                PenghasilanWali = reader["dul_penghasilan_wali"]?.ToString() ?? "",
                JumlahSaudara = reader["dul_jumlah_saudara"]?.ToString() ?? "",
                JumlahKakak = reader["dul_jumlah_kakak"]?.ToString() ?? "",
                JumlahAdik = reader["dul_jumlah_adik"]?.ToString() ?? "",
                SaudaraSekolah = reader["dul_saudara_sekolah"]?.ToString() ?? "",
                SaudaraBekerja = reader["dul_saudara_bekerja"]?.ToString() ?? "",
                AstraGrup = reader["dul_astra_grup"]?.ToString() ?? "",
                AstraHubungan = reader["dul_astra_hubungan"]?.ToString() ?? "",
                AstraPerusahaan = reader["dul_astra_perusahaan"]?.ToString() ?? "",
                PasFoto = reader["dul_pas_foto"]?.ToString() ?? "",
                KtpSim = reader["dul_ktp_sim"]?.ToString() ?? "",
                AktaKelahiran = reader["dul_akta_kelahiran"]?.ToString() ?? "",
                KartuKeluarga = reader["dul_kartu_keluarga"]?.ToString() ?? "",
                Ijazah = reader["dul_ijazah"]?.ToString() ?? "",
                Skhun = reader["dul_skhun"]?.ToString() ?? "",
                BebasNarkoba = reader["dul_bebas_narkoba"]?.ToString() ?? "",
                SanggupBayar = reader["dul_sanggup_bayar"]?.ToString() ?? "",
                BuktiBayar = reader["dul_bukti_bayar"]?.ToString() ?? "",
                AtasNama = reader["atasnama"]?.ToString() ?? "",
                NoRek = reader["norek"]?.ToString() ?? "",
                NamaBank = reader["namabank"]?.ToString() ?? "",
                VaSumbangan = reader["dul_va_sumbangan"]?.ToString() ?? "",
                VaSpp = reader["dul_va_spp"]?.ToString() ?? "",
                Status = reader["dul_status"]?.ToString() ?? ""
            };
        }

        private string ValidateOrderBy(string orderBy)
        {
            if (string.IsNullOrEmpty(orderBy))
                return "pdi_created_date desc";

            // Remove alias 'a.' if present
            orderBy = orderBy.Replace("a.pdi_created_date", "pdi_created_date");
            orderBy = orderBy.Trim();
            
            // Valid orderBy options based on stored procedure
            var validOptions = new[]
            {
                "no asc", "no desc",
                "nim asc", "nim desc", 
                "pdi_created_date asc", "pdi_created_date desc"
            };

            // Check if orderBy is valid
            if (validOptions.Contains(orderBy.ToLower()))
                return orderBy;

            // If not valid, return default
            Console.WriteLine($"WARN: Invalid orderBy '{orderBy}', using default 'pdi_created_date desc'");
            return "pdi_created_date desc";
        }

    }
}
