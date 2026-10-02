using astratech_apps_backend.DTOs.MeninggalDunia;
using astratech_apps_backend.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace astratech_apps_backend.Repositories.Implementations
{
    public class MeninggalDuniaRepository(IConfiguration config) : IMeninggalDuniaRepository
    {
        private readonly string _conn = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
            config.GetConnectionString("DefaultConnection")!,
            Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
        );

        public async Task<(IEnumerable<MeninggalDuniaListDto> Data, int TotalData)> GetAllAsync(GetAllMeninggalDuniaRequest req)
        {
            var list = new List<MeninggalDuniaListDto>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Status", req.Status ?? string.Empty);
            cmd.Parameters.AddWithValue("@RoleId", req.RoleId ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new MeninggalDuniaListDto
                {
                    Id = reader["mdu_id"]?.ToString() ?? string.Empty,
                    NoPengajuan = reader["mdu_id_alternative"]?.ToString() ?? reader["mdu_id"]?.ToString() ?? string.Empty,
                    TanggalPengajuan = reader["mdu_created_date"]?.ToString() ?? string.Empty,
                    NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? string.Empty,
                    Nim = reader["nim"]?.ToString() ?? reader["mhs_id"]?.ToString() ?? string.Empty,
                    Prodi = reader["pro_nama"]?.ToString() ?? string.Empty,
                    NomorSK = reader["srt_no"]?.ToString() ?? "-",
                    Status = reader["mdu_status"]?.ToString() ?? string.Empty
                });
            }

            var totalData = list.Count;

            // Jika ada kata kunci pencarian
            if (!string.IsNullOrEmpty(req.SearchKeyword))
            {
                var kw = req.SearchKeyword.Trim().ToLower();
                list = list.Where(x =>
                    x.NamaMahasiswa.ToLower().Contains(kw) ||
                    x.Nim.ToLower().Contains(kw) ||
                    x.NoPengajuan.ToLower().Contains(kw) ||
                    x.NomorSK.ToLower().Contains(kw)
                ).ToList();
                totalData = list.Count;
            }

            // Paginasi di memori jika requested
            if (req.PageSize > 0)
            {
                var skip = (req.PageNumber < 1 ? 0 : req.PageNumber - 1) * req.PageSize;
                list = list.Skip(skip).Take(req.PageSize).ToList();
            }

            return (list, totalData);
        }

        public async Task<(IEnumerable<RiwayatMeninggalDuniaListDto> Data, int TotalData)> GetRiwayatAsync(GetRiwayatMeninggalDuniaRequest req)
        {
            var list = new List<RiwayatMeninggalDuniaListDto>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataRiwayatMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Keyword", req.Keyword ?? string.Empty);
            cmd.Parameters.AddWithValue("@Sort", req.Sort ?? string.Empty);
            cmd.Parameters.AddWithValue("@Konsentrasi", req.Konsentrasi ?? string.Empty);
            cmd.Parameters.AddWithValue("@RoleId", req.RoleId ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new RiwayatMeninggalDuniaListDto
                {
                    Id = reader["mdu_id"]?.ToString() ?? string.Empty,
                    NoPengajuan = reader["mdu_id"]?.ToString() ?? string.Empty,
                    TanggalPengajuan = reader["tanggal_buat"]?.ToString() ?? string.Empty,
                    NamaMahasiswa = reader["mhs_nama"]?.ToString() ?? string.Empty,
                    Nim = reader["mhs_id"]?.ToString() ?? string.Empty,
                    Prodi = reader["pro_nama"]?.ToString() ?? string.Empty,
                    NomorSK = reader["srt_no"]?.ToString() ?? "-",
                    Status = reader["mdu_status"]?.ToString() ?? string.Empty
                });
            }

            var totalData = list.Count;

            if (req.PageSize > 0)
            {
                var skip = (req.PageNumber < 1 ? 0 : req.PageNumber - 1) * req.PageSize;
                list = list.Skip(skip).Take(req.PageSize).ToList();
            }

            return (list, totalData);
        }

        public async Task<MeninggalDuniaDetailResponse?> GetDetailAsync(string id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDetailMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MeninggalDuniaId", id);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new MeninggalDuniaDetailResponse
                {
                    Id = reader["mdu_id"]?.ToString() ?? string.Empty,
                    MhsId = reader["mhs_id"]?.ToString() ?? string.Empty,
                    MhsNama = reader["mhs_nama"]?.ToString() ?? string.Empty,
                    KonNama = reader["kon_nama"]?.ToString() ?? string.Empty,
                    MhsAngkatan = reader["mhs_angkatan"]?.ToString() ?? string.Empty,
                    KonSingkatan = reader["kon_singkatan"]?.ToString() ?? string.Empty,
                    Lampiran = reader["mdu_lampiran"]?.ToString() ?? string.Empty,
                    Status = reader["mdu_status"]?.ToString() ?? string.Empty,
                    CreatedBy = reader["mdu_created_by"]?.ToString() ?? string.Empty,
                    ApproveDir1Date = reader["mdu_approve_dir1_date"]?.ToString() ?? string.Empty,
                    ApproveDir1By = reader["mdu_approve_dir1_by"]?.ToString() ?? string.Empty,
                    SuratNo = reader["srt_no"]?.ToString() ?? "-",
                    NoSpkb = reader["mdu_no_spkb"]?.ToString() ?? "-",
                    SK = reader["mdu_sk"]?.ToString() ?? string.Empty,
                    SPKB = reader["mdu_spkb"]?.ToString() ?? string.Empty,
                    TanggalPengajuan = reader["tanggal_pengajuan"]?.ToString() ?? string.Empty
                };
            }

            return null;
        }

        public async Task<string?> CreateAsync(CreateMeninggalDuniaRequest dto, string lampiranFileName, string createdBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Step", "STEP1");
            cmd.Parameters.AddWithValue("@Lampiran", lampiranFileName);
            cmd.Parameters.AddWithValue("@MahasiswaId", dto.MhsId);
            cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString();
        }

        public async Task<string?> FinalizeAsync(string draftId, string updatedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Step", "STEP2");
            cmd.Parameters.AddWithValue("@Lampiran", draftId);
            cmd.Parameters.AddWithValue("@MahasiswaId", updatedBy);
            cmd.Parameters.AddWithValue("@CreatedBy", updatedBy);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString();
        }

        public async Task<bool> UpdateAsync(string id, string lampiranFileName, string updatedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_editMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MeninggalDuniaId", id);
            cmd.Parameters.AddWithValue("@Lampiran", lampiranFileName ?? string.Empty);
            cmd.Parameters.AddWithValue("@ModifiedBy", updatedBy);

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows >= 0;
        }

        public async Task<bool> SoftDeleteAsync(string id, string updatedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_deleteMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MeninggalDuniaId", id);
            cmd.Parameters.AddWithValue("@ModifiedBy", updatedBy);

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows >= 0;
        }

        public async Task<bool> ApproveAsync(string id, string role, string username)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_setujuiMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MeninggalDuniaId", id);
            cmd.Parameters.AddWithValue("@Role", role);
            cmd.Parameters.AddWithValue("@Username", username);

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows >= 0;
        }

        public async Task<bool> RejectAsync(string id, string role, string username)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_tolakMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MeninggalDuniaId", id);
            cmd.Parameters.AddWithValue("@Role", role);
            cmd.Parameters.AddWithValue("@Username", username);

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows >= 0;
        }

        public async Task<bool> UploadSKAsync(string id, string skFileName, string spkbFileName, string updatedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createSKMeninggalDunia", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MeninggalDuniaId", id);
            cmd.Parameters.AddWithValue("@SuratKeteranganMeninggalDunia", skFileName);
            cmd.Parameters.AddWithValue("@SuratKeteranganPernahBerkuliah", spkbFileName);
            cmd.Parameters.AddWithValue("@ModifiedBy", updatedBy);

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows >= 0;
        }

        public async Task<IEnumerable<RiwayatMeninggalDuniaExcelResponse>> GetRiwayatExcelAsync(string sort, string konsentrasi)
        {
            var list = new List<RiwayatMeninggalDuniaExcelResponse>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataRiwayatMeninggalDuniaExcel", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Sort", sort ?? string.Empty);
            cmd.Parameters.AddWithValue("@Konsentrasi", konsentrasi ?? string.Empty);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new RiwayatMeninggalDuniaExcelResponse
                {
                    NIM = reader["NIM"]?.ToString() ?? string.Empty,
                    NamaMahasiswa = reader["Nama Mahasiswa"]?.ToString() ?? string.Empty,
                    Konsentrasi = reader["Konsentrasi"]?.ToString() ?? string.Empty,
                    TanggalPengajuan = reader["Tanggal Pengajuan"]?.ToString() ?? string.Empty,
                    NoSK = reader["No SK"]?.ToString() ?? "-",
                    NoPengajuan = reader["No Pengajuan"]?.ToString() ?? string.Empty
                });
            }

            return list;
        }

        // ============================================
        // HELPER DROPDOWN DATA MAHASISWA
        // ============================================
        public async Task<IEnumerable<MahasiswaDropdownDto>> GetMahasiswaListAsync(string? search = null)
        {
            var list = new List<MahasiswaDropdownDto>();

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataMahasiswa", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Keyword", search ?? string.Empty);
            cmd.Parameters.AddWithValue("@Status", "Aktif");
            cmd.Parameters.AddWithValue("@Urut", "mhs_id asc");
            cmd.Parameters.AddWithValue("@Halaman", 1);
            cmd.Parameters.AddWithValue("@Limit", 100);

            try
            {
                await conn.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new MahasiswaDropdownDto
                    {
                        MhsId = reader["mhs_id"]?.ToString() ?? string.Empty,
                        MhsNama = reader["mhs_nama"]?.ToString() ?? string.Empty,
                        MhsAngkatan = reader["mhs_angkatan"]?.ToString() ?? string.Empty,
                        ProgramStudi = reader["pro_nama"]?.ToString() ?? string.Empty,
                        Konsentrasi = reader["kon_nama"]?.ToString() ?? string.Empty
                    });
                }
            }
            catch
            {
                // Fallback graceful
            }

            return list;
        }

        public async Task<MahasiswaDetailDto?> GetMahasiswaDetailAsync(string mhsId)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_detailMahasiswa", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MahasiswaId", mhsId);

            try
            {
                await conn.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new MahasiswaDetailDto
                    {
                        MhsId = reader["mhs_id"]?.ToString() ?? string.Empty,
                        MhsNama = reader["mhs_nama"]?.ToString() ?? string.Empty,
                        MhsAngkatan = reader["mhs_angkatan"]?.ToString() ?? string.Empty,
                        Konsentrasi = reader["kon_nama"]?.ToString() ?? string.Empty,
                        KonsentrasiId = reader["kon_id"]?.ToString() ?? string.Empty,
                        ProgramStudi = reader["pro_nama"]?.ToString() ?? string.Empty,
                        ProgramStudiSingkatan = reader["pro_singkatan"]?.ToString() ?? string.Empty
                    };
                }
            }
            catch
            {
                // Fallback graceful
            }

            return null;
        }

        public async Task<MahasiswaProdiDto?> GetMahasiswaProdiAsync(string mhsId)
        {
            var detail = await GetMahasiswaDetailAsync(mhsId);
            if (detail != null)
            {
                return new MahasiswaProdiDto
                {
                    KonId = detail.KonsentrasiId,
                    ProId = string.Empty,
                    ProNama = detail.ProgramStudi
                };
            }

            return null;
        }
    }
}
