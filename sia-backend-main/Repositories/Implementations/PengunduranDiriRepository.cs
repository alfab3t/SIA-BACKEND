using astratech_apps_backend.DTOs.PengunduranDiri;
using astratech_apps_backend.Models;
using astratech_apps_backend.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace astratech_apps_backend.Repositories.Implementations
{
    public class PengunduranDiriRepository : IPengunduranDiriRepository
    {
        private readonly string _conn;

        public PengunduranDiriRepository(IConfiguration config)
        {
            _conn = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
                config.GetConnectionString("DefaultConnection")!,
                Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
            );
        }

        // STEP 1
        public async Task<string> CreateStep1Async(string mhsId, string createdBy)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_createPengunduranDiri", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p1", "STEP1");
            cmd.Parameters.AddWithValue("@p2", "");
            cmd.Parameters.AddWithValue("@p3", createdBy);
            cmd.Parameters.AddWithValue("@p4", mhsId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            // RETURN draft_id (p2 = draft number)
            return "DRAFT_GENERATED";
        }

        // STEP 2
        public async Task<CreatePengunduranDiriResponse?> CreateStep2Async(string draftId, string createdBy)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_createPengunduranDiri", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p1", "STEP2");
            cmd.Parameters.AddWithValue("@p2", draftId);
            cmd.Parameters.AddWithValue("@p3", createdBy);
            cmd.Parameters.AddWithValue("@p4", "");

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

        public async Task<IEnumerable<PengunduranDiriListResponse>> GetAllAsync(string p1, string status, string userId)
        {
            var list = new List<PengunduranDiriListResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataPengunduranDiri", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // SP memiliki 50 parameter (p1 - p50)
            // Tapi yang dipakai hanya p1, p2, p3.
            // Sisanya harus dikirim string kosong
            cmd.Parameters.AddWithValue("@p1", p1);
            cmd.Parameters.AddWithValue("@p2", status);
            cmd.Parameters.AddWithValue("@p3", userId);

            for (int i = 4; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new PengunduranDiriListResponse
                {
                    PdiId = reader["pdi_id"].ToString(),
                    IdAlternative = reader["id"].ToString(),
                    MhsId = reader["mhs_id"].ToString(),
                    ApproveProdi = reader["approve_prodi"].ToString(),
                    ApproveDir1 = reader["approve_dir1"].ToString(),
                    Tanggal = reader["tanggal"].ToString(),
                    TanggalDisetujui = reader["tanggal_disetujui"]?.ToString(),
                    SuratNo = reader["srt_no"].ToString(),
                    Status = reader["status"].ToString()
                });
            }

            return list;
        }


        public async Task<PengunduranDiri?> GetByIdAsync(string id)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_detailPengunduranDiri", conn)
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
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_editPengunduranDiri", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", id);
            cmd.Parameters.AddWithValue("@p2", dto.LampiranSuratPengajuan ?? "");
            cmd.Parameters.AddWithValue("@p3", dto.Lampiran ?? "");
            cmd.Parameters.AddWithValue("@p4", updatedBy);

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }


        public async Task<bool> SoftDeleteAsync(string id, string updatedBy)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_deletePengunduranDiri", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // P1 & P2 → dipakai
            cmd.Parameters.AddWithValue("@p1", id);
            cmd.Parameters.AddWithValue("@p2", updatedBy);

            // P3–P50 → wajib ada
            for (int i = 3; i <= 50; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", "");
            }

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }


        public async Task<string?> CheckReportAsync(string pdiId)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_checkReportPengunduranDIri", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Isi p1 - p50
            for (int i = 1; i <= 50; i++)
            {
                if (i == 1)
                    cmd.Parameters.AddWithValue("@p1", pdiId);
                else
                    cmd.Parameters.AddWithValue($"@p{i}", "");
            }

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString();
        }

        public async Task<CreatePengunduranDiriByProdiResponse> CreateByProdiAsync(CreatePengunduranDiriByProdiRequest dto)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_createPengunduranDiriByProdi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // STEP 1
            cmd.Parameters.AddWithValue("@p1", "STEP1");
            cmd.Parameters.AddWithValue("@p2", dto.Alasan);
            cmd.Parameters.AddWithValue("@p3", dto.Catatan);
            cmd.Parameters.AddWithValue("@p4", dto.MhsId);
            cmd.Parameters.AddWithValue("@p5", dto.ProdiNpk);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            // STEP 2
            cmd.Parameters["@p1"].Value = "STEP2";

            var reader = await cmd.ExecuteReaderAsync();

            CreatePengunduranDiriByProdiResponse result = new();

            if (await reader.ReadAsync())
            {
                result.Id = reader["pdi_id"].ToString() ?? "";
                result.MhsId = reader["mhs_id"].ToString() ?? "";
                result.Nama = reader["mhs_nama"].ToString() ?? "";
                result.Konsentrasi = reader["kon_nama"].ToString() ?? "";
                result.Angkatan = reader["mhs_angkatan"].ToString() ?? "";
                result.CreatedBy = reader["pdi_created_by"].ToString() ?? "";
            }

            return result;
        }

        public async Task<bool> CreateSKAsync(string id, UploadSKPengunduranDiriRequest dto, string updatedBy)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sia_createSKPengunduranDiri", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", id);            // pdi_id
            cmd.Parameters.AddWithValue("@p2", dto.Sk ?? "");  // pdi_sk
            cmd.Parameters.AddWithValue("@p3", dto.Skpb ?? ""); // pdi_skpb
            cmd.Parameters.AddWithValue("@p4", updatedBy);      // updated by

            // 46 remaining parameters diisi kosong, SP butuh 50 parameter.
            for (int i = 5; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<PengunduranDiriDetailResponse?> GetDetailAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDetailPengunduranDiri", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", id);
            for (int i = 2; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

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

            cmd.Parameters.AddWithValue("@p1", username);
            cmd.Parameters.AddWithValue("@p2", status);
            cmd.Parameters.AddWithValue("@p3", "");
            cmd.Parameters.AddWithValue("@p4", keyword);
            cmd.Parameters.AddWithValue("@p5", orderBy);
            cmd.Parameters.AddWithValue("@p6", konsentrasi);

            // parameters p7 - p50
            for (int i = 7; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new PengunduranDiriRiwayatResponse
                {
                    PdiId = reader["pdi_id"].ToString(),
                    MhsId = reader["mhs_id"].ToString(),
                    ApproveProdi = reader["approve_prodi"].ToString(),
                    ApproveDir1 = reader["approve_dir1"].ToString(),
                    Tanggal = reader["tanggal"].ToString(),
                    TanggalDisetujui = reader["tanggal_disetujui"].ToString(),
                    SuratNo = reader["srt_no"].ToString(),
                    NamaMahasiswa = reader["mhs_nama"].ToString(),
                    Konsentrasi = reader["kon_singkatan"].ToString(),
                    Status = reader["status"].ToString()
                });
            }

            return list;
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
            cmd.Parameters.AddWithValue("@p1", "");
            cmd.Parameters.AddWithValue("@p2", "");
            cmd.Parameters.AddWithValue("@p3", "");
            cmd.Parameters.AddWithValue("@p4", "");
            cmd.Parameters.AddWithValue("@p5", orderBy);
            cmd.Parameters.AddWithValue("@p6", konsentrasi);

            for (int i = 7; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

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
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_setujuiPengunduranDiri", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Required parameters
            cmd.Parameters.AddWithValue("@p1", id);
            cmd.Parameters.AddWithValue("@p2", dto.Role);
            cmd.Parameters.AddWithValue("@p3", dto.ApprovedBy);

            // The rest @p4 .. @p50 must still be filled
            for (int i = 4; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }

        public async Task<bool> RejectAsync(string id, RejectPengunduranDiriRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_tolakPengunduranDiri", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", id);
            cmd.Parameters.AddWithValue("@p2", dto.Role);
            cmd.Parameters.AddWithValue("@p3", dto.Reason);

            // Tambahkan parameter kosong @p4 ... @p50
            for (int i = 4; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }



    }
}
