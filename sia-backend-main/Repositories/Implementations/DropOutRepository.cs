using astratech_apps_backend.DTOs.DropOut;
using astratech_apps_backend.Models;
using astratech_apps_backend.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace astratech_apps_backend.Repositories.Implementations
{
    public class DropOutRepository : IDropOutRepository
    {
        private readonly string _conn;

        public DropOutRepository(IConfiguration config)
        {
            _conn = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
                config.GetConnectionString("DefaultConnection")!,
                Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
            );
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

            // SP expects 50 params → isi p1–p50
            cmd.Parameters.AddWithValue("@p1", dto.MhsId);
            cmd.Parameters.AddWithValue("@p2", dto.Lampiran ?? "");
            cmd.Parameters.AddWithValue("@p3", dto.LampiranSuratPengajuan ?? "");
            cmd.Parameters.AddWithValue("@p4", createdBy);

            for (int i = 5; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

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

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_getDataDropOut", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Keyword", keyword ?? "");
            cmd.Parameters.AddWithValue("@Halaman", page);
            cmd.Parameters.AddWithValue("@Limit", limit);

            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();

            while (await r.ReadAsync())
            {
                list.Add(new DropOut
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
                });
            }

            return list;
        }

        public async Task<DropOutDetailResponse?> GetDetailAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_detailDO", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", id);
            for (int i = 2; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

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
                ApproveWadir1Date = reader[""]?.ToString() ?? "", // dari CONVERT di SP
                ApproveWadir1By = reader["dro_appr_wadir1"]?.ToString() ?? "",
                ApproveDirDate = reader[""]?.ToString() ?? "",    // dari CONVERT di SP
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
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_editDO", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", id);
            cmd.Parameters.AddWithValue("@p2", dto.Menimbang ?? "");
            cmd.Parameters.AddWithValue("@p3", dto.Mengingat ?? "");
            cmd.Parameters.AddWithValue("@p4", updatedBy);

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
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

            deleteCmd.Parameters.AddWithValue("@p1", id);
            for (int i = 2; i <= 50; i++)
                deleteCmd.Parameters.AddWithValue($"@p{i}", "");

            await deleteCmd.ExecuteNonQueryAsync();

            return true; // 💥 TANPA peduli rows
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

                cmd.Parameters.AddWithValue("@p1", id);
                cmd.Parameters.AddWithValue("@p2", dto.Username);

                for (int i = 3; i <= 50; i++)
                    cmd.Parameters.AddWithValue($"@p{i}", "");

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

            // SP membutuhkan 50 parameter → kita isi semua @p1–@p50
            cmd.Parameters.AddWithValue("@p1", id);
            for (int i = 2; i <= 50; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", "");
            }

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

            cmd.Parameters.AddWithValue("@p1", droId);

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

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataRiwayatDO", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // PARAMETER SESUAI SP LEGACY
            cmd.Parameters.AddWithValue("@p1", username ?? "");
            cmd.Parameters.AddWithValue("@p2", keyword ?? "");
            cmd.Parameters.AddWithValue("@p3", sortBy ?? "a.dro_created_date desc");
            cmd.Parameters.AddWithValue("@p4", konsentrasi ?? "");
            cmd.Parameters.AddWithValue("@p5", role ?? "");
            cmd.Parameters.AddWithValue("@p6", displayName ?? "");

            // WAJIB: p7–p50
            for (int i = 7; i <= 50; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", "");
            }

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new DropOutRiwayatResponse
                {
                    DroId = reader["dro_id"]?.ToString() ?? "",
                    TanggalPengajuan = reader["dro_created_date"]?.ToString() ?? "",
                    DibuatOleh = reader["dro_created_by"]?.ToString() ?? "",
                    NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? "",
                    Prodi = reader["kon_nama"]?.ToString() ?? "",
                    NoSkDo = reader["srt_no"]?.ToString() ?? "",
                    Status = reader["dro_status"]?.ToString() ?? ""
                });
            }

            return result;
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

            cmd.Parameters.AddWithValue("@p1", username);
            cmd.Parameters.AddWithValue("@p2", keyword);
            cmd.Parameters.AddWithValue("@p3", sortBy);
            cmd.Parameters.AddWithValue("@p4", konsentrasi);
            cmd.Parameters.AddWithValue("@p5", role);
            cmd.Parameters.AddWithValue("@p6", sekprodi);

            // sisanya p7–p50 kosong
            for (int i = 7; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

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
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getIdDOByDraft", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Parameter p1 = dro_id draft
            cmd.Parameters.AddWithValue("@p1", id);

            // sisa p2–p50 kosong
            for (int i = 2; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new DropOutGetIdByDraftResponse
                {
                    Id = reader[0].ToString()
                };
            }

            return null;
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

                cmd.Parameters.AddWithValue("@p1", id);
                cmd.Parameters.AddWithValue("@p2", dto.Username);
                cmd.Parameters.AddWithValue("@p3", dto.Reason);

                for (int i = 4; i <= 50; i++)
                    cmd.Parameters.AddWithValue($"@p{i}", "");

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

            cmd.Parameters.AddWithValue("@p1", id);

            // sisanya isi string kosong seperti SP lainnya
            for (int i = 2; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

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

            cmd.Parameters.AddWithValue("@p1", id);

            // parameter 2–50 kosong
            for (int i = 2; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

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

                cmd.Parameters.AddWithValue("@p1", request.DroId);
                cmd.Parameters.AddWithValue("@p2", request.SK);
                cmd.Parameters.AddWithValue("@p3", request.SKPB);
                cmd.Parameters.AddWithValue("@p4", request.ModifiedBy);

                // parameter 5–50 kosong
                for (int i = 5; i <= 50; i++)
                    cmd.Parameters.AddWithValue($"@p{i}", "");

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

            // SP expects @p1–@p50
            cmd.Parameters.AddWithValue("@p1", username ?? "");
            cmd.Parameters.AddWithValue("@p2", keyword ?? "");
            cmd.Parameters.AddWithValue("@p3", sortBy ?? "a.dro_created_date desc");
            cmd.Parameters.AddWithValue("@p4", konsentrasi ?? "");
            cmd.Parameters.AddWithValue("@p5", role ?? "");        // 🔴 WAJIB
            cmd.Parameters.AddWithValue("@p6", displayName ?? ""); // 🔴 WAJIB

            for (int i = 7; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");


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

            // SP pakai @p1 = kon_id
            cmd.Parameters.AddWithValue("@p1", konsentrasiId);

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

        public async Task<IEnumerable<DropOutProdiOptionResponse>> GetProdiAsync()
        {
            var result = new List<DropOutProdiOptionResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getListProdi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            for (int i = 1; i <= 50; i++)
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

            // SP pakai @p1 & @p2
            cmd.Parameters.AddWithValue("@p1", prodiId);
            cmd.Parameters.AddWithValue("@p2", sekprodiUsername ?? "");

            // p3 – p50 WAJIB ada (signature legacy)
            for (int i = 3; i <= 50; i++)
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



    }
}
