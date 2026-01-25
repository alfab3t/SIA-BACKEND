#nullable disable
using astratech_apps_backend.DTOs.DropOut;
using astratech_apps_backend.DTOs.PengunduranDiri;
using astratech_apps_backend.Models;
using astratech_apps_backend.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace astratech_apps_backend.Repositories.Implementations
{
    public class DropOutRepository : IDropOutRepository
    {
        private readonly string _conn = string.Empty;

        public DropOutRepository(IConfiguration config)
        {
            try
            {
                var encryptedConn = config.GetConnectionString("DefaultConnection")!;
                var decryptKey = Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING");
                
                Console.WriteLine($"DEBUG DropOut: Encrypted connection string exists: {!string.IsNullOrEmpty(encryptedConn)}");
                Console.WriteLine($"DEBUG DropOut: Decrypt key exists: {!string.IsNullOrEmpty(decryptKey)}");
                
                if (string.IsNullOrEmpty(decryptKey))
                {
                    Console.WriteLine("WARNING DropOut: DECRYPT_KEY_CONNECTION_STRING environment variable not found. Using fallback connection.");
                    _conn = "Server=.\\SQLEXPRESS;Database=ERP_PolmanAstra_NDA;Integrated Security=true;TrustServerCertificate=true;";
                }
                else
                {
                    _conn = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(encryptedConn, decryptKey);
                    Console.WriteLine($"DEBUG DropOut: Decrypted connection string: {_conn?.Substring(0, Math.Min(50, _conn?.Length ?? 0))}...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR DropOut in decryption: {ex.Message}");
                Console.WriteLine($"ERROR DropOut Stack: {ex.StackTrace}");
                // Fallback connection string
                _conn = "Server=.\\SQLEXPRESS;Database=ERP_PolmanAstra_NDA;Integrated Security=true;TrustServerCertificate=true;";
                Console.WriteLine("Using fallback connection string for DropOut");
            }
        }

        //public async Task<string> CreateAsync(CreateDropOutRequest dto, string createdBy)
        //{
        //    using var conn = new SqlConnection(_conn);
        //    using var cmd = new SqlCommand("USE [ERP_PolmanAstra_NDA]\r\nGO\r\n/****** Object:  StoredProcedure [dbo].[sia_createPengajuanDO]    Script Date: 12/30/2025 10:17:52 AM ******/\r\nSET ANSI_NULLS ON\r\nGO\r\nSET QUOTED_IDENTIFIER ON\r\nGO\r\nALTER PROCEDURE [dbo].[sia_createPengajuanDO]\r\n\t@p1 varchar(max), @p2 varchar(max), @p3 varchar(max), @p4 varchar(max), @p5 varchar(max),\r\n\t@p6 varchar(max), @p7 varchar(max), @p8 varchar(max), @p9 varchar(max), @p10 varchar(max),\r\n\t@p11 varchar(max), @p12 varchar(max), @p13 varchar(max), @p14 varchar(max), @p15 varchar(max),\r\n\t@p16 varchar(max), @p17 varchar(max), @p18 varchar(max), @p19 varchar(max), @p20 varchar(max),\r\n\t@p21 varchar(max), @p22 varchar(max), @p23 varchar(max), @p24 varchar(max), @p25 varchar(max),\r\n\t@p26 varchar(max), @p27 varchar(max), @p28 varchar(max), @p29 varchar(max), @p30 varchar(max),\r\n\t@p31 varchar(max), @p32 varchar(max), @p33 varchar(max), @p34 varchar(max), @p35 varchar(max),\r\n\t@p36 varchar(max), @p37 varchar(max), @p38 varchar(max), @p39 varchar(max), @p40 varchar(max),\r\n\t@p41 varchar(max), @p42 varchar(max), @p43 varchar(max), @p44 varchar(max), @p45 varchar(max),\r\n\t@p46 varchar(max), @p47 varchar(max), @p48 varchar(max), @p49 varchar(max), @p50 varchar(max)\r\nAS\r\nBEGIN\r\n\tSET NOCOUNT ON;\r\n\t\r\n\tdeclare @tempIdDraft int;\r\n\t\r\n\tselect @tempIdDraft = (select top 1 dro_id from sia_msdropout where dro_id not like '%DO%' order by dro_created_date desc);\r\n\r\n\tif @tempIdDraft is null\r\n\t\tselect @tempIdDraft = 0;\r\n\tselect @tempIdDraft = @tempIdDraft + 1;\r\n\tinsert into sia_msdropout values (CAST(@tempIdDraft as varchar), @p1, @p2, @p3, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Draft', @p4, GETDATE(), NULL, NULL);\r\n\t\r\nEND\r\n\r\n\r\n\r\n", conn)
        //    {
        //        CommandType = CommandType.StoredProcedure
        //    };

        //    cmd.Parameters.AddWithValue("@mhs_id", dto.MhsId);
        //    cmd.Parameters.AddWithValue("@dro_menimbang", dto.Menimbang ?? "");
        //    cmd.Parameters.AddWithValue("@dro_mengingat", dto.Mengingat ?? "");
        //    cmd.Parameters.AddWithValue("@createdBy", createdBy);

        //    await conn.OpenAsync();
        //    var id = await cmd.ExecuteScalarAsync();
        //    return id?.ToString() ?? "";
        //}

        public async Task<string?> CreatePengajuanDOAsync(CreatePengajuanDORequest dto, string createdBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createPengajuanDO", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // SP dengan parameter deskriptif
            cmd.Parameters.AddWithValue("@mhs_id", dto.MhsId);
            cmd.Parameters.AddWithValue("@dro_lampiran", dto.Lampiran ?? "");
            cmd.Parameters.AddWithValue("@dro_lampiran_surat_pengajuan", dto.LampiranSuratPengajuan ?? "");
            cmd.Parameters.AddWithValue("@dro_created_by", createdBy);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            // SP INSERT using new draft ID → must fetch the ID
            // We must query latest draft ID
            var cmd2 = new SqlCommand(@"
        SELECT TOP 1 dro_id 
        FROM sia_msdropout 
        WHERE dro_created_by = @createdBy 
        ORDER BY dro_created_date DESC",
                conn
            );

            cmd2.Parameters.AddWithValue("@createdBy", createdBy);

            var newId = (string?)await cmd2.ExecuteScalarAsync();

            return newId;
        }


        public async Task<IEnumerable<DropOut>> GetAllAsync(string keyword, int page, int limit)
        {
            var list = new List<DropOut>();

            try
            {
                Console.WriteLine($"DEBUG DropOut GetAllAsync - keyword: '{keyword}', page: {page}, limit: {limit}");
                Console.WriteLine($"DEBUG DropOut GetAllAsync - Testing connection: {_conn?.Substring(0, Math.Min(50, _conn?.Length ?? 0))}...");

                using var conn = new SqlConnection(_conn);
                // Menggunakan sia_getDataRiwayatDO karena sia_getDataDropOut tidak ada
                using var cmd = new SqlCommand("sia_getDataRiwayatDO", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Parameter sesuai dengan SP sia_getDataRiwayatDO:
                // @p1 = username, @p2 = keyword, @p3 = sortBy, @p4 = konsentrasi, @p5 = role, @p6 = displayName
                cmd.Parameters.AddWithValue("@p1", ""); // username - kosong untuk get all
                cmd.Parameters.AddWithValue("@p2", keyword ?? ""); // keyword pencarian
                cmd.Parameters.AddWithValue("@p3", "a.dro_created_date desc"); // sortBy
                cmd.Parameters.AddWithValue("@p4", ""); // konsentrasi
                cmd.Parameters.AddWithValue("@p5", ""); // role
                cmd.Parameters.AddWithValue("@p6", ""); // displayName
                
                // Parameter p7-p50 wajib ada
                for (int i = 7; i <= 50; i++)
                    cmd.Parameters.AddWithValue($"@p{i}", "");

                Console.WriteLine($"DEBUG DropOut GetAllAsync - Parameters: @p1='', @p2='{keyword}', @p3='a.dro_created_date desc'");

                await conn.OpenAsync();
                Console.WriteLine("DEBUG DropOut GetAllAsync - Connection opened successfully");
                
                using var r = await cmd.ExecuteReaderAsync();
                Console.WriteLine($"DEBUG DropOut GetAllAsync - Command executed, HasRows: {r.HasRows}");

                int rowCount = 0;
                while (await r.ReadAsync())
                {
                    rowCount++;
                    if (rowCount <= 3)
                        Console.WriteLine($"DEBUG DropOut GetAllAsync - Reading row {rowCount}");
                    
                    list.Add(new DropOut
                    {
                        Id = r["dro_id"]?.ToString() ?? "",
                        MhsId = r["mhs_id"]?.ToString() ?? "",
                        SrtNo = r["srt_no"]?.ToString() ?? "",
                        Status = r["dro_status"]?.ToString() ?? "",
                        CreatedBy = r["dro_created_by"]?.ToString() ?? ""
                    });
                }

                Console.WriteLine($"DEBUG DropOut GetAllAsync - Total rows processed: {rowCount}, List count: {list.Count}");
                return list;
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"SQL ERROR DropOut GetAllAsync: {sqlEx.Message}");
                Console.WriteLine($"SQL ERROR Number: {sqlEx.Number}");
                Console.WriteLine($"SQL ERROR State: {sqlEx.State}");
                throw new Exception($"Database error in DropOut GetAllAsync: {sqlEx.Message} (Error Number: {sqlEx.Number})", sqlEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR DropOut GetAllAsync: {ex.Message}");
                Console.WriteLine($"ERROR Stack: {ex.StackTrace}");
                throw new Exception($"Error in DropOut GetAllAsync: {ex.Message}", ex);
            }
        }

        public async Task<DropOutDetailResponse?> GetDetailAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_detailDO", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@dro_id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.Read())
                return null;

            return new DropOutDetailResponse
            {
                Id = reader["dro_id"]?.ToString() ?? "",
                MhsId = reader["mhs_id"]?.ToString() ?? "",
                MhsText = reader["mhstext"]?.ToString() ?? "",
                Konsentrasi = reader["kon_nama"]?.ToString() ?? "",
                Angkatan = reader["mhs_angkatan"]?.ToString() ?? "",
                Menimbang = reader["dro_menimbang"]?.ToString() ?? "",
                Mengingat = reader["dro_mengingat"]?.ToString() ?? "",
                Status = reader["dro_status"]?.ToString() ?? "",
                CreatedBy = reader["dro_created_by"]?.ToString() ?? "",
                Sk = reader["dro_sk"]?.ToString() ?? "",
                ApproveWadir1Date = reader["dro_appr_wadir1_date"]?.ToString() ?? "",
                ApproveWadir1By = reader["dro_appr_wadir1"]?.ToString() ?? "",
                ApproveDirDate = reader["dro_appr_dir_date"]?.ToString() ?? "",
                ApproveDirBy = reader["dro_appr_dir"]?.ToString() ?? "",
                AlasanTolak = reader["dro_alasan_tolak"]?.ToString() ?? "",
                Konsentrasi2 = reader["kon_nama2"]?.ToString() ?? "",
                Prodi = reader["pro_nama"]?.ToString() ?? "",
                SuratKeteranganNo = reader["dro_srt_ket_no"]?.ToString() ?? ""
            };
        }



        public async Task<DropOut?> GetByIdAsync(string id)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_detailDropOut", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@dro_id", id);

            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();

            if (!await r.ReadAsync())
                return null;

            return new DropOut
            {
                Id = r["dro_id"].ToString(),
                MhsId = r["mhs_id"].ToString(),
                Menimbang = r["dro_menimbang"].ToString(),
                Mengingat = r["dro_mengingat"].ToString(),
                ApproveWadir1 = r["dro_appr_wadir1"].ToString(),
                ApproveWadir1Date = r["dro_appr_wadir1_date"] as DateTime?,
                ApproveDir = r["dro_appr_dir"].ToString(),
                ApproveDirDate = r["dro_appr_dir_date"] as DateTime?,
                SrtNo = r["srt_no"].ToString(),
                SrtKetNo = r["dro_srt_ket_no"].ToString(),
                Sk = r["dro_sk"].ToString(),
                Skpb = r["dro_skpb"].ToString(),
                AlasanTolak = r["dro_alasan_tolak"].ToString(),
                Status = r["dro_status"].ToString(),
                CreatedBy = r["dro_created_by"].ToString(),
                CreatedDate = r["dro_created_date"] as DateTime?,
                ModifiedBy = r["dro_modif_by"].ToString(),
                ModifiedDate = r["dro_modif_date"] as DateTime?
            };
        }

        public async Task<bool> UpdateAsync(string id, UpdateDropOutRequest dto, string updatedBy)
        {
            try
            {
                await using var conn = new SqlConnection(_conn);
                await using var cmd = new SqlCommand("sia_editDO", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@dro_id", id);
                cmd.Parameters.AddWithValue("@dro_menimbang", dto.Menimbang ?? "");
                cmd.Parameters.AddWithValue("@dro_mengingat", dto.Mengingat ?? "");
                cmd.Parameters.AddWithValue("@dro_modif_by", updatedBy);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return true; // SP legacy dengan SET NOCOUNT ON tidak return rows
            }
            catch (Exception)
            {
                return false;
            }
        }



        public async Task<bool> DeleteAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            // 1️⃣ Cek dulu data ada atau tidak
            var checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM sia_msdropout WHERE dro_id = @id",
                conn
            );
            checkCmd.Parameters.AddWithValue("@id", id);

            var exists = (int)await checkCmd.ExecuteScalarAsync();
            if (exists == 0)
                return false;

            // 2️⃣ Baru delete
            await using var deleteCmd = new SqlCommand("sia_deleteDropOut", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            deleteCmd.Parameters.AddWithValue("@dro_id", id);

            await deleteCmd.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<bool> ApproveByWadirAsync(string id, ApproveDropOutRequest dto)
        {
            try
            {
                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                // Jalankan SP approve langsung
                await using var cmd = new SqlCommand("sia_approveDropOut", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@dro_id", id);
                cmd.Parameters.AddWithValue("@username", dto.Username);

                await cmd.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public async Task<string?> CheckReportAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_checkReportDropOut", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@dro_id", id);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString();
        }

        public async Task<DropOutReportSuketResponse?> GetReportSuketAsync(string suratNo)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_detailReportDOSuket", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", suratNo);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new DropOutReportSuketResponse
            {
                Nama = reader.GetString(0),
                Konsentrasi = reader.GetString(1),
                Angkatan = reader.GetString(2),
                SuratNo = reader.GetString(3),
                TanggalLahir = reader.GetString(4),
                TanggalLahirID = reader.GetString(5),
                Alamat = reader.GetString(6),
                KodePos = reader.GetString(7),
                Prodi = reader.GetString(8),
                Kaprodi = reader.GetString(9),
                Direktur = reader.GetString(10),
                Wadir1 = reader.GetString(11),
                Wadir2 = reader.GetString(12),
                Wadir3 = reader.GetString(13),
                Tingkat = reader.GetString(14),
                Semester = reader.GetInt32(15),
                TahunAjaran = reader.GetString(16),
                SemesterText = reader.GetString(17),
                StatusKuliah = reader.GetString(18),
                TahunLulus = reader.IsDBNull(19) ? "" : reader.GetString(19),
                TempatLahir = reader.GetString(20),
                MhsId = reader.GetString(21)
            };
        }

        public async Task<DropOutDownloadSkResponse?> DownloadSKAsync(string droId)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_downloadSKDO", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@dro_id", droId);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new DropOutDownloadSkResponse
            {
                Sk = reader.IsDBNull(0) ? "" : reader.GetString(0),
                Skpb = reader.IsDBNull(1) ? "" : reader.GetString(1)
            };
        }

        //    public async Task<IEnumerable<DropOutRiwayatResponse>> GetRiwayatAsync(
        //string username, string keyword, string sortBy, string konsentrasi, string role, string sekprodi)
        //    {
        //        var result = new List<DropOutRiwayatResponse>();

        //        await using var conn = new SqlConnection(_conn);
        //        await using var cmd = new SqlCommand("sia_getDataRiwayatDO", conn)
        //        {
        //            CommandType = CommandType.StoredProcedure
        //        };

        //        cmd.Parameters.AddWithValue("@p1", username);
        //        cmd.Parameters.AddWithValue("@p2", keyword);
        //        cmd.Parameters.AddWithValue("@p3", sortBy);
        //        cmd.Parameters.AddWithValue("@p4", konsentrasi);
        //        cmd.Parameters.AddWithValue("@p5", role);
        //        cmd.Parameters.AddWithValue("@p6", sekprodi);

        //        // sisanya p7 - p50 = "" (kosong)
        //        for (int i = 7; i <= 50; i++)
        //        {
        //            cmd.Parameters.AddWithValue($"@p{i}", "");
        //        }

        //        await conn.OpenAsync();
        //        using var reader = await cmd.ExecuteReaderAsync();

        //        while (await reader.ReadAsync())
        //        {
        //            result.Add(new DropOutRiwayatResponse
        //            {
        //                Id = reader["dro_id"].ToString(),
        //                MhsId = reader["mhs_id"].ToString(),
        //                Mahasiswa = reader["mhs_nama"].ToString(),
        //                Konsentrasi = reader["kon_nama"].ToString(),
        //                Tanggal = reader["dro_created_date"].ToString(),
        //                CreatedBy = reader["dro_created_by"].ToString(),
        //                SuratNo = reader["srt_no"].ToString(),
        //                Status = reader["dro_status"].ToString()
        //            });
        //        }

        //        return result;
        //    }


        public async Task<IEnumerable<DropOutRiwayatResponse>> GetRiwayatAsync(
     string username,
     string keyword,
     string sortBy,
     string konsentrasi,
     string role,
     string displayName)
        {
            var result = new List<DropOutRiwayatResponse>();

            Console.WriteLine($"DEBUG GetRiwayatAsync - username: '{username}', keyword: '{keyword}', sortBy: '{sortBy}'");
            Console.WriteLine($"DEBUG GetRiwayatAsync - konsentrasi: '{konsentrasi}', role: '{role}', displayName: '{displayName}'");

            try
            {
                await using var conn = new SqlConnection(_conn);
                await using var cmd = new SqlCommand("sia_getDataRiwayatDO", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Parameter dengan nama deskriptif
                cmd.Parameters.AddWithValue("@username", username ?? "");
                cmd.Parameters.AddWithValue("@keyword", keyword ?? "");
                cmd.Parameters.AddWithValue("@sort_by", sortBy ?? "a.dro_created_date desc");
                cmd.Parameters.AddWithValue("@kon_id", konsentrasi ?? "");
                cmd.Parameters.AddWithValue("@role_id", role ?? "");
                cmd.Parameters.AddWithValue("@display_name", displayName ?? "");

                await conn.OpenAsync();
                Console.WriteLine("DEBUG GetRiwayatAsync - Connection opened, executing SP...");
                
                using var reader = await cmd.ExecuteReaderAsync();
                Console.WriteLine($"DEBUG GetRiwayatAsync - SP executed, HasRows: {reader.HasRows}");

                int rowCount = 0;
                while (await reader.ReadAsync())
                {
                    rowCount++;
                    
                    // SP return mhs_nama dalam format "mhs_id - nama_lengkap"
                    var mhsNamaFull = reader["mhs_nama"]?.ToString() ?? "";
                    var mhsId = reader["mhs_id"]?.ToString() ?? "";
                    
                    // Split untuk mendapatkan nama saja (tanpa mhs_id)
                    var namaSaja = mhsNamaFull;
                    if (mhsNamaFull.Contains(" - "))
                    {
                        var parts = mhsNamaFull.Split(new[] { " - " }, 2, StringSplitOptions.None);
                        if (parts.Length > 1)
                            namaSaja = parts[1];
                    }
                    
                    result.Add(new DropOutRiwayatResponse
                    {
                        DroId = reader["dro_id"]?.ToString() ?? "",
                        TanggalPengajuan = reader["dro_created_date"]?.ToString() ?? "",
                        DibuatOleh = reader["dro_created_by"]?.ToString() ?? "",
                        MhsId = mhsId,
                        NamaMahasiswa = namaSaja,
                        Prodi = reader["kon_nama"]?.ToString() ?? "",
                        NoSkDo = reader["srt_no"]?.ToString() ?? "",
                        Status = reader["dro_status"]?.ToString() ?? ""
                    });
                }

                Console.WriteLine($"DEBUG GetRiwayatAsync - Total rows: {rowCount}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR GetRiwayatAsync: {ex.Message}");
                Console.WriteLine($"ERROR Stack: {ex.StackTrace}");
                throw;
            }
        }


        public async Task<IEnumerable<DropOutRiwayatExcelResponse>> GetRiwayatExcelAsync(
        string username, string keyword, string sortBy, string konsentrasi, string role, string sekprodi)
        {
            var result = new List<DropOutRiwayatExcelResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataRiwayatDOExcel", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@keyword", keyword);
            cmd.Parameters.AddWithValue("@sort_by", sortBy);
            cmd.Parameters.AddWithValue("@kon_id", konsentrasi);
            cmd.Parameters.AddWithValue("@role_id", role);
            cmd.Parameters.AddWithValue("@display_name", sekprodi);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new DropOutRiwayatExcelResponse
                {
                    NIM = reader["NIM"].ToString(),
                    NamaMahasiswa = reader["Nama Mahasiswa"].ToString(),
                    Konsentrasi = reader["Konsenstrasi"].ToString(),
                    TanggalPengajuan = reader["Tanggal Pengajuan"].ToString(),
                    NoSK = reader["No SK"].ToString(),
                    NoPengajuan = reader["No Pengajuan"].ToString()
                });
            }

            return result;
        }

        public async Task<DropOutGetIdByDraftResponse?> GetIdByDraftAsync(string id)
        {
            try
            {
                Console.WriteLine($"DEBUG GetIdByDraftAsync - Input ID: '{id}'");
                
                await using var conn = new SqlConnection(_conn);
                await using var cmd = new SqlCommand("sia_getIdDOByDraft", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@dro_id_draft", id);

                await conn.OpenAsync();
                Console.WriteLine("DEBUG GetIdByDraftAsync - Connection opened, executing SP...");
                
                using var reader = await cmd.ExecuteReaderAsync();
                Console.WriteLine($"DEBUG GetIdByDraftAsync - SP executed, HasRows: {reader.HasRows}");

                if (await reader.ReadAsync())
                {
                    var newId = reader[0]?.ToString() ?? "";
                    Console.WriteLine($"DEBUG GetIdByDraftAsync - New ID generated: '{newId}'");
                    
                    return new DropOutGetIdByDraftResponse
                    {
                        Id = newId
                    };
                }

                Console.WriteLine("DEBUG GetIdByDraftAsync - No rows returned from SP");
                return null;
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"SQL ERROR GetIdByDraftAsync: {sqlEx.Message}");
                Console.WriteLine($"SQL ERROR Number: {sqlEx.Number}");
                Console.WriteLine($"SQL ERROR State: {sqlEx.State}");
                Console.WriteLine($"SQL ERROR Stack: {sqlEx.StackTrace}");
                throw new Exception($"Database error in GetIdByDraftAsync: {sqlEx.Message} (Error Number: {sqlEx.Number})", sqlEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR GetIdByDraftAsync: {ex.Message}");
                Console.WriteLine($"ERROR Stack: {ex.StackTrace}");
                throw new Exception($"Error in GetIdByDraftAsync: {ex.Message}", ex);
            }
        }

        public async Task<bool> RejectByWadirAsync(string id, RejectDropOutRequest dto)
        {
            try
            {
                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                // Jalankan SP reject langsung
                await using var cmd = new SqlCommand("sia_rejectDropOut", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@dro_id", id);
                cmd.Parameters.AddWithValue("@username", dto.Username);
                cmd.Parameters.AddWithValue("@alasan_tolak", dto.Reason);

                await cmd.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }




        public async Task<SKDOReportResponse?> GetReportSKDOAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_reportSKDO", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@dro_id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.Read())
                return null;

            return new SKDOReportResponse
            {
                DropOutId = reader["dro_id"].ToString(),
                SuratNo = reader["srt_no"].ToString(),
                Menimbang = reader["dro_menimbang"].ToString(),
                Mengingat = reader["dro_mengingat"].ToString(),
                MahasiswaNama = reader["mhs_nama"].ToString(),
                MahasiswaId = reader["mhs_id"].ToString(),
                ProdiNama = reader["pro_nama"].ToString(),
                KonsentrasiNama = reader["kon_nama"].ToString(),
                TahunAjaran = reader["srt_tahun_ajaran"].ToString(),
                Direktur = reader["direktur"].ToString(),
                Wadir1 = reader["wadir1"].ToString(),
                Kaprodi = reader["kaprod"].ToString()
            };
        }

        public async Task<List<SKDOReportSubResponse>> GetReportSKDOSubAsync(string id)
        {
            var result = new List<SKDOReportSubResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_reportSKDOsub", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@dro_id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new SKDOReportSubResponse
                {
                    DropOutId = reader["dro_id"].ToString(),
                    Jenis = reader["jenis"].ToString(),
                    Isi = reader["isi"].ToString()
                });
            }

            return result;
        }

        public async Task<bool> UploadSKDOAsync(UploadSKDORequest request)
        {
            try
            {
                await using var conn = new SqlConnection(_conn);
                await using var cmd = new SqlCommand("sia_uploadSKDO", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@dro_id", request.DroId);
                cmd.Parameters.AddWithValue("@dro_sk", request.SK);
                cmd.Parameters.AddWithValue("@dro_skpb", request.SKPB);
                cmd.Parameters.AddWithValue("@dro_modif_by", request.ModifiedBy);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return true; // SP legacy dengan SET NOCOUNT ON tidak return rows
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<DropOutPendingResponse>> GetPendingAsync(
    string username,
    string keyword,
    string sortBy,
    string konsentrasi,
    string role,
    string displayName)
        {
            var result = new List<DropOutPendingResponse>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_getDataPendingDO", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Parameter dengan nama deskriptif
            cmd.Parameters.AddWithValue("@username", username ?? "");
            cmd.Parameters.AddWithValue("@keyword", keyword ?? "");
            cmd.Parameters.AddWithValue("@sort_by", sortBy ?? "a.dro_created_date desc");
            cmd.Parameters.AddWithValue("@kon_id", konsentrasi ?? "");
            cmd.Parameters.AddWithValue("@role_id", role ?? "");
            cmd.Parameters.AddWithValue("@display_name", displayName ?? "");

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new DropOutPendingResponse
                {
                    Id = reader["dro_id"].ToString(),
                    MhsId = reader["mhs_id"].ToString(),
                    Mahasiswa = reader["mhs_nama"].ToString(),
                    Konsentrasi = reader["kon_nama"].ToString(),
                    CreatedDate = reader["dro_created_date"].ToString(),
                    CreatedBy = reader["dro_created_by"].ToString(),
                    SuratNo = reader["srt_no"].ToString(),
                    Status = reader["dro_status"].ToString()
                });
            }

            return result;
        }

        public async Task<IEnumerable<DropOutMahasiswaOptionResponse>>
    GetMahasiswaByKonsentrasiAsync(string konsentrasiId)
        {
            var result = new List<DropOutMahasiswaOptionResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand(
                "sia_getListMahasiswaByKonsentrasi2",
                conn
            )
            {
                CommandType = CommandType.StoredProcedure
            };

            // SP sia_getListMahasiswaByKonsentrasi2 menggunakan @p1 = kon_id
            cmd.Parameters.AddWithValue("@p1", konsentrasiId ?? "");

            // p2–p50 wajib diisi
            for (int i = 2; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new DropOutMahasiswaOptionResponse
                {
                    Value = reader["mhs_id"].ToString() ?? "",
                    Text = reader["mhs_nama"].ToString() ?? ""
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

            cmd.Parameters.AddWithValue("@p1", username);
            for (int i = 2; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new DropOutProdiOptionResponse
                {
                    Value = reader["pro_id"].ToString(),
                    Text = reader["pro_nama"].ToString()
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
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new DropOutProdiOptionResponse
                {
                    Value = reader["pro_id"].ToString() ?? "",
                    Text = reader["pro_nama"].ToString() ?? ""
                });
            }

            return result;
        }

        public async Task<IEnumerable<DropOutKonsentrasiOptionResponse>> GetKonsentrasiAsync(string username)
        {
            var result = new List<DropOutKonsentrasiOptionResponse>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_getListKonsentrasiByProdi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // SP HANYA TERIMA PARAMETER INI
            cmd.Parameters.AddWithValue("@SekprodiUsername", username);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new DropOutKonsentrasiOptionResponse
                {
                    Value = reader["kon_id"].ToString(),
                    Text = reader["kon_nama"].ToString()
                });
            }

            return result;
        }

        public async Task<IEnumerable<DropOutKonsentrasiOptionResponse>>
    GetKonsentrasiByProdiAsync(string prodiId, string sekprodiUsername)
        {
            var result = new List<DropOutKonsentrasiOptionResponse>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_getListKonsentrasiByProdi2", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // SP hanya pakai @p1 = pro_id
            cmd.Parameters.AddWithValue("@p1", prodiId);

            // p2 – p50 WAJIB ada (signature legacy)
            for (int i = 2; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new DropOutKonsentrasiOptionResponse
                {
                    Value = reader["kon_id"].ToString(),
                    Text = reader["kon_nama"].ToString()
                });
            }

            return result;
        }

        public async Task<string?> GetAngkatanByMahasiswaAsync(string mhsId)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand(
                "sia_getListAngkatanByMahasiswa",
                conn
            )
            {
                CommandType = CommandType.StoredProcedure
            };

            // SP hanya pakai @p1
            cmd.Parameters.AddWithValue("@p1", mhsId);

            // p2–p50 WAJIB ADA (SP legacy)
            for (int i = 2; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();

            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString();
        }

        public async Task<MahasiswaProfilResponse?> GetMahasiswaProfilAsync(string mhsId)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_getMahasiswaByNIM", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Id", mhsId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new MahasiswaProfilResponse
            {
                Nama = reader["mhs_nama"].ToString(),
                ProdiKonsentrasi = reader["kon_nama"].ToString(),
                Angkatan = reader["mhs_angkatan"].ToString(),
                Kelas = reader["kelas"].ToString()
            };
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
                message = "Mahasiswa bebas tanggungan dan dapat melanjutkan proses Drop Out.";
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

        public async Task<MahasiswaProfilDetailResponse?> GetProfilMahasiswaDetailAsync(string mhsId)
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



    }
}
