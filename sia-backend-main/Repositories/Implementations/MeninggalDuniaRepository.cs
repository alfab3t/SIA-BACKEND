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
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // STEP2: Convert Draft ID to Official ID format (xxx/PA/MD/Roman/Year)
            cmd.Parameters.AddWithValue("@p1", "STEP2");

            // @p2 = draft ID (temporary numeric ID)
            cmd.Parameters.AddWithValue("@p2", draftId);

            // @p3 = updatedBy
            cmd.Parameters.AddWithValue("@p3", updatedBy);

            // @p4 - @p50 sisanya tetap dikirim kosong
            for (int i = 4; i <= 50; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", "");
            }

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString() ?? "";
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


        //public async Task<IEnumerable<MeninggalDuniaListResponse>> GetAllAsync(string status, string roleId)
        //{
        //    var list = new List<MeninggalDuniaListResponse>();

        //    await using var conn = new SqlConnection(_conn);
        //    await using var cmd = new SqlCommand("sia_getDataMeninggalDunia", conn)
        //    {
        //        CommandType = CommandType.StoredProcedure
        //    };

        //    // SP punya 50 parameter, tapi hanya p2 & p3 yang dipakai
        //    cmd.Parameters.AddWithValue("@p1", "");
        //    cmd.Parameters.AddWithValue("@p2", status ?? "");
        //    cmd.Parameters.AddWithValue("@p3", roleId ?? "");

        //    // sisanya dummy agar SP tidak error
        //    for (int i = 4; i <= 50; i++)
        //        cmd.Parameters.AddWithValue($"@p{i}", "");

        //    await conn.OpenAsync();
        //    await using var reader = await cmd.ExecuteReaderAsync();

        //    while (await reader.ReadAsync())
        //    {
        //        list.Add(new MeninggalDuniaListResponse
        //        {
        //            Id = reader["mdu_id"].ToString(),
        //            IdAlternative = reader["mdu_id_alternative"].ToString(),
        //            MhsId = reader["mhs_id"].ToString(),
        //            ApproveDir1By = reader["mdu_approve_dir1_by"].ToString(),
        //            CreatedDate = reader["mdu_created_date"].ToString(),   // varchar(11)
        //            TanggalBuat = reader["tanggal_buat"] as DateTime? ?? DateTime.MinValue,
        //            SuratNo = reader["srt_no"].ToString(),
        //            Status = reader["mdu_status"].ToString(),
        //        });
        //    }

        //    return list;
        //}


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
                    ISNULL(c.kon_singkatan, '') as kon_singkatan
                FROM sia_msmeninggaldunia a
                LEFT JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
                LEFT JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
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
                list.Add(new MeninggalDuniaListDto
                {
                    Id = reader["mdu_id"].ToString(),
                    NoPengajuan = reader["mdu_id_alternative"].ToString(),
                    TanggalPengajuan = reader["mdu_created_date"]?.ToString() ?? "",
                    NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? "",
                    Prodi = reader["kon_singkatan"]?.ToString() ?? "",
                    NomorSK = reader["srt_no"]?.ToString() ?? "-",
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
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_editMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // wajib isi p1 - p50
            for (int i = 1; i <= 50; i++)
            {
                if (i == 1)
                    cmd.Parameters.AddWithValue("@p1", id);
                else if (i == 2)
                    cmd.Parameters.AddWithValue("@p2", dto.Lampiran ?? "");
                else if (i == 3)
                    cmd.Parameters.AddWithValue("@p3", updatedBy);
                else
                    cmd.Parameters.AddWithValue($"@p{i}", ""); // dummy
            }

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }




        //DELETE
        public async Task<bool> SoftDeleteAsync(string id, string updatedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_deleteMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // wajib sesuai SP
            cmd.Parameters.AddWithValue("@p1", id);         // mdu_id
            cmd.Parameters.AddWithValue("@p2", updatedBy);   // mdu_modif_by

            // @p3 - @p50 harus tetap dikirim
            for (int i = 3; i <= 50; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", "");
            }

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }


        //UPLOAD SK
        public async Task<bool> UploadSKAsync(string id, string sk, string spkb, string updatedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createSKMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", id);
            cmd.Parameters.AddWithValue("@p2", sk);
            cmd.Parameters.AddWithValue("@p3", spkb);
            cmd.Parameters.AddWithValue("@p4", updatedBy);

            // 46 parameters lain tetap diisi kosong
            for (int i = 5; i <= 50; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", "");
            }

            await conn.OpenAsync();
            var result = await cmd.ExecuteNonQueryAsync();

            return result > 0;
        }

        //        public async Task<IEnumerable<GetRiwayatMeninggalDuniaResponse>> GetRiwayatAsync(
        //    string keyword,
        //    string sort,
        //    string konsentrasi,
        //    string roleId
        //)
        //        {
        //            var list = new List<GetRiwayatMeninggalDuniaResponse>();

        //            await using var conn = new SqlConnection(_conn);
        //            await using var cmd = new SqlCommand("sia_getDataRiwayatMeninggalDunia", conn)
        //            {
        //                CommandType = CommandType.StoredProcedure
        //            };

        //            // p1 tidak dipakai
        //            cmd.Parameters.AddWithValue("@p1", "");

        //            // p2 = "" → MODE PRODI (sesuai SP lama)
        //            cmd.Parameters.AddWithValue("@p2", "");

        //            // p3 = kon_npk / roleId
        //            cmd.Parameters.AddWithValue("@p3", roleId ?? "");

        //            // p4 = keyword
        //            cmd.Parameters.AddWithValue("@p4", keyword ?? "");

        //            // p5 = sort
        //            cmd.Parameters.AddWithValue("@p5", sort ?? "mdu_created_date desc");

        //            // p6 = konsentrasi
        //            cmd.Parameters.AddWithValue("@p6", konsentrasi ?? "");

        //            // p7 – p50 = dummy
        //            for (int i = 7; i <= 50; i++)
        //                cmd.Parameters.AddWithValue($"@p{i}", "");

        //            await conn.OpenAsync();
        //            using var reader = await cmd.ExecuteReaderAsync();

        //            while (await reader.ReadAsync())
        //            {
        //                list.Add(new GetRiwayatMeninggalDuniaResponse
        //                {
        //                    mdu_id = reader["mdu_id"].ToString(),
        //                    mhs_id = reader["mhs_id"].ToString(),
        //                    tanggal_buat = reader["tanggal_buat"].ToString(),
        //                    srt_no = reader["srt_no"].ToString(),
        //                    mhs_nama = reader["mhs_nama"].ToString(),
        //                    mdu_status = reader["mdu_status"].ToString(),
        //                    kon_singkatan = reader["kon_singkatan"].ToString()
        //                });
        //            }

        //            return list;
        //        }

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
                    d.pro_singkatan + ' (' + c.kon_singkatan + ')' as kon_singkatan
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
                    Prodi = reader["kon_singkatan"]?.ToString() ?? "",
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
            await using var cmd = new SqlCommand("sia_getDataRiwayatMeninggalDuniaExcel", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // p1-p4 dan p7-p50 dummy
            for (int i = 1; i <= 50; i++)
            {
                if (i == 5)
                    cmd.Parameters.AddWithValue("@p5", sort ?? "");
                else if (i == 6)
                    cmd.Parameters.AddWithValue("@p6", konsentrasi ?? "");
                else
                    cmd.Parameters.AddWithValue($"@p{i}", "");
            }

            await conn.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new RiwayatMeninggalDuniaExcelResponse
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

        public async Task<bool> UploadSKMeninggalAsync(UploadSKMeninggalRequest request)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createSKMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", request.MduId);
            cmd.Parameters.AddWithValue("@p2", request.SK);
            cmd.Parameters.AddWithValue("@p3", request.SKPB);
            cmd.Parameters.AddWithValue("@p4", request.ModifiedBy);

            // Kosongkan p5–p50
            for (int i = 5; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }

        public async Task<MeninggalDuniaDetailResponse?> GetDetailAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDetailMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // P1 → ID
            cmd.Parameters.AddWithValue("@p1", id);

            // p2–p50 → kosong
            for (int i = 2; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", "");

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync()) return null;

            return new MeninggalDuniaDetailResponse
            {
                MhsId = reader["mhs_id"].ToString(),
                MhsNama = reader["mhs_nama"].ToString(),
                KonNama = reader["kon_nama"].ToString(),
                MhsAngkatan = reader["mhs_angkatan"].ToString(),
                KonSingkatan = reader["kon_singkatan"].ToString(),
                Lampiran = reader["mdu_lampiran"].ToString(),
                Status = reader["mdu_status"].ToString(),
                CreatedBy = reader["mdu_created_by"].ToString(),
                ApproveDir1Date = reader["mdu_approve_dir1_date"].ToString(),
                ApproveDir1By = reader["mdu_approve_dir1_by"].ToString(),
                SuratNo = reader["srt_no"].ToString(),
                NoSpkb = reader["mdu_no_spkb"].ToString(),
                SK = reader["mdu_sk"].ToString(),
                SPKB = reader["mdu_spkb"].ToString()
            };
        }

        public async Task<bool> ApproveAsync(string id, ApproveMeninggalDuniaRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_setujuiMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", id);          // mdu_id
            cmd.Parameters.AddWithValue("@p2", dto.Role);    // 'wadir1'
            cmd.Parameters.AddWithValue("@p3", dto.Username); // approver
                                                              // p4–p50 tidak dipakai → kirim NULL
            for (int i = 4; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", DBNull.Value);

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }

        public async Task<bool> RejectAsync(string id, RejectMeninggalDuniaRequest dto)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_tolakMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p1", id);               // mdu_id
            cmd.Parameters.AddWithValue("@p2", dto.Role ?? "");   // jenis penolak
            cmd.Parameters.AddWithValue("@p3", dto.Username ?? ""); // username

            // sisa parameter p4 - p50 = NULL
            for (int i = 4; i <= 50; i++)
                cmd.Parameters.AddWithValue($"@p{i}", DBNull.Value);

            await conn.OpenAsync();
            var result = await cmd.ExecuteNonQueryAsync();

            return result > 0;
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
