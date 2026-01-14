using astratech_apps_backend.DTOs.Mahasiswa;
using astratech_apps_backend.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace astratech_apps_backend.Repositories.Implementations
{
    public class MahasiswaRepository : IMahasiswaRepository
    {
        private readonly string _conn;

        public MahasiswaRepository(IConfiguration config)
        {
            _conn = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
                config.GetConnectionString("DefaultConnection")!,
                Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
            );
        }

        /// <summary>
        /// Mendapatkan detail profil mahasiswa berdasarkan mhs_id menggunakan stored procedure sia_detailMahasiswa
        /// </summary>
        /// <param name="mhsId">ID Mahasiswa</param>
        /// <returns>Detail profil mahasiswa lengkap</returns>
        public async Task<MahasiswaDetailResponse?> GetDetailAsync(string mhsId)
        {
            try
            {
                Console.WriteLine($"[GetDetailAsync] Starting to fetch detail for mhsId: {mhsId}");

                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("sia_detailMahasiswa", conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                
                cmd.Parameters.AddWithValue("@MahasiswaId", mhsId);

                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var result = new MahasiswaDetailResponse
                    {
                        MhsId = reader["mhs_id"]?.ToString() ?? "",
                        DulNoPendaftaran = reader["dul_no_pendaftaran"]?.ToString() ?? "",
                        MhsNama = reader["mhs_nama"]?.ToString() ?? "",
                        KonNama = reader["kon_nama"]?.ToString() ?? "",
                        MhsTempatLahir = reader["mhs_tempat_lahir"]?.ToString() ?? "",
                        MhsTglLahir = reader["mhs_tgl_lahir"]?.ToString() ?? "",
                        MhsJenisKelamin = reader["mhs_jenis_kelamin"]?.ToString() ?? "",
                        MhsAlamat = reader["mhs_alamat"]?.ToString() ?? "",
                        MhsKodepos = reader["mhs_kodepos"]?.ToString() ?? "",
                        MhsHp = reader["mhs_hp"]?.ToString() ?? "",
                        MhsEmail = reader["mhs_email"]?.ToString() ?? "",
                        MhsTglMasuk = reader["mhs_tgl_masuk"]?.ToString() ?? "",
                        MhsTglLulus = reader["mhs_tgl_lulus"]?.ToString() ?? "",
                        MhsAngkatan = Convert.ToInt32(reader["mhs_angkatan"] ?? 0),
                        MhsStatusKuliah = reader["mhs_status_kuliah"]?.ToString() ?? "",
                        DulNamaAyah = reader["dul_nama_ayah"]?.ToString() ?? "",
                        DulHpAyah = reader["dul_hp_ayah"]?.ToString() ?? "",
                        DulStatusAyah = reader["dul_status_ayah"]?.ToString() ?? "",
                        DulAlamatAyah = reader["dul_alamat_ayah"]?.ToString() ?? "",
                        DulKodeposAyah = reader["dul_kodepos_ayah"]?.ToString() ?? "",
                        DulNamaIbu = reader["dul_nama_ibu"]?.ToString() ?? "",
                        DulHpIbu = reader["dul_hp_ibu"]?.ToString() ?? "",
                        DulStatusIbu = reader["dul_status_ibu"]?.ToString() ?? "",
                        DulAlamatIbu = reader["dul_alamat_ibu"]?.ToString() ?? "",
                        DulKodeposIbu = reader["dul_kodepos_ibu"]?.ToString() ?? "",
                        DulNamaWali = reader["dul_nama_wali"]?.ToString() ?? "",
                        DulHpWali = reader["dul_hp_wali"]?.ToString() ?? "",
                        DulStatusWali = reader["dul_status_wali"]?.ToString() ?? "",
                        DulAlamatWali = reader["dul_alamat_wali"]?.ToString() ?? "",
                        DulKodeposWali = reader["dul_kodepos_wali"]?.ToString() ?? "",
                        KonId = Convert.ToInt32(reader["kon_id"] ?? 0),
                        MhsJenis = reader["mhs_jenis"]?.ToString() ?? "",
                        DulJalur = reader["dul_jalur"]?.ToString() ?? "",
                        AtasNama = reader["atasnama"]?.ToString() ?? "",
                        NoRek = reader["norek"]?.ToString() ?? "",
                        NamaBank = reader["namabank"]?.ToString() ?? "",
                        DulNisn = reader["dul_nisn"]?.ToString() ?? "",
                        KelId = reader["kel_id"]?.ToString() ?? "",
                        DulNik = reader["dul_nik"]?.ToString() ?? "",
                        RfidAktif = reader["rfid_aktif"]?.ToString() ?? ""
                    };

                    Console.WriteLine($"[GetDetailAsync] Successfully found detail for mhsId: {mhsId}");
                    return result;
                }

                Console.WriteLine($"[GetDetailAsync] No data found for mhsId: {mhsId}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetDetailAsync] ERROR: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Mendapatkan daftar konsentrasi berdasarkan username sekprodi menggunakan direct SQL query
        /// </summary>
        /// <param name="username">Username sekprodi</param>
        /// <returns>List konsentrasi dengan id dan nama</returns>
        public async Task<List<KonsentrasiListResponse>> GetKonsentrasiListBySekprodiAsync(string username)
        {
            try
            {
                Console.WriteLine($"[GetKonsentrasiListBySekprodiAsync] Starting to fetch konsentrasi list for username: {username}");

                var result = new List<KonsentrasiListResponse>();

                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                // Direct SQL query based on the stored procedure logic
                var sql = @"
                    SELECT 
                        a.kon_id as id,
                        b.pro_nama + ' (' + RTRIM(a.kon_singkatan) + ')' as nama
                    FROM sia_mskonsentrasi a
                    INNER JOIN sia_msprodi b ON a.pro_id = b.pro_id
                    INNER JOIN ess_mskaryawan c ON a.kon_npk = c.kry_id
                    WHERE a.kon_status = 'Aktif' 
                      AND c.kry_username = @username
                    ORDER BY b.pro_nama";

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", username);

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new KonsentrasiListResponse
                    {
                        Id = reader["id"]?.ToString() ?? "",
                        Nama = reader["nama"]?.ToString() ?? ""
                    });
                }

                Console.WriteLine($"[GetKonsentrasiListBySekprodiAsync] Found {result.Count} konsentrasi for username: {username}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetKonsentrasiListBySekprodiAsync] ERROR: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Mendapatkan daftar mahasiswa berdasarkan konsentrasi menggunakan direct SQL query
        /// </summary>
        /// <param name="konId">ID Konsentrasi</param>
        /// <returns>List mahasiswa dengan id dan nama</returns>
        public async Task<List<MahasiswaByKonsentrasiResponse>> GetMahasiswaByKonsentrasiAsync(string konId)
        {
            try
            {
                Console.WriteLine($"[GetMahasiswaByKonsentrasiAsync] Starting to fetch mahasiswa list for konId: {konId}");

                var result = new List<MahasiswaByKonsentrasiResponse>();

                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                // Direct SQL query based on the stored procedure logic
                var sql = @"
                    SELECT 
                        mhs_id, 
                        mhs_nama
                    FROM sia_msmahasiswa
                    WHERE mhs_status = 'Aktif' 
                      AND (mhs_status_kuliah = 'Aktif' OR mhs_status_kuliah = 'Menunggu Yudisium') 
                      AND kon_id = @konId
                    ORDER BY mhs_id ASC";

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@konId", konId);

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new MahasiswaByKonsentrasiResponse
                    {
                        MhsId = reader["mhs_id"]?.ToString() ?? "",
                        MhsNama = reader["mhs_nama"]?.ToString() ?? ""
                    });
                }

                Console.WriteLine($"[GetMahasiswaByKonsentrasiAsync] Found {result.Count} mahasiswa for konId: {konId}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetMahasiswaByKonsentrasiAsync] ERROR: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Mendapatkan data mahasiswa berdasarkan NIM menggunakan direct SQL query
        /// </summary>
        /// <param name="nim">NIM Mahasiswa</param>
        /// <returns>Data mahasiswa dengan nama, konsentrasi, angkatan, dan kelas</returns>
        public async Task<MahasiswaByNIMResponse?> GetMahasiswaByNIMAsync(string nim)
        {
            try
            {
                Console.WriteLine($"[GetMahasiswaByNIMAsync] Starting to fetch mahasiswa data for NIM: {nim}");

                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                // Direct SQL query based on the stored procedure logic
                var sql = @"
                    SELECT TOP 1 
                        a.mhs_nama,
                        d.pro_nama + ' (' + b.kon_singkatan + ')' as kon_nama,
                        a.mhs_angkatan,
                        c.kel_id as kelas
                    FROM sia_msmahasiswa a
                    INNER JOIN sia_mskonsentrasi b ON a.kon_id = b.kon_id
                    INNER JOIN sia_mskelas c ON a.kel_id = c.kel_id
                    INNER JOIN sia_msprodi d ON d.pro_id = b.pro_id
                    WHERE a.mhs_id = @nim 
                      AND a.mhs_status = 'Aktif'";

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@nim", nim);

                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var result = new MahasiswaByNIMResponse
                    {
                        MhsNama = reader["mhs_nama"]?.ToString() ?? "",
                        KonNama = reader["kon_nama"]?.ToString() ?? "",
                        MhsAngkatan = Convert.ToInt32(reader["mhs_angkatan"] ?? 0),
                        Kelas = reader["kelas"]?.ToString() ?? ""
                    };

                    Console.WriteLine($"[GetMahasiswaByNIMAsync] Successfully found mahasiswa data for NIM: {nim}");
                    return result;
                }

                Console.WriteLine($"[GetMahasiswaByNIMAsync] No data found for NIM: {nim}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetMahasiswaByNIMAsync] ERROR: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check bebas tanggungan mahasiswa menggunakan stored procedure sia_checkBebasTanggungan
        /// </summary>
        /// <param name="userId">User ID / NIM Mahasiswa</param>
        /// <returns>Status bebas tanggungan: "OK" atau "NOK"</returns>
        public async Task<string> CheckBebasTanggunganAsync(string userId)
        {
            try
            {
                Console.WriteLine($"[CheckBebasTanggunganAsync] Starting to check bebas tanggungan for userId: {userId}");

                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("sia_checkBebasTanggungan", conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                
                cmd.Parameters.AddWithValue("@UserId", userId);

                var result = await cmd.ExecuteScalarAsync();
                var status = result?.ToString() ?? "NOK";

                Console.WriteLine($"[CheckBebasTanggunganAsync] Status for userId {userId}: {status}");
                return status;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CheckBebasTanggunganAsync] ERROR: {ex.Message}");
                throw;
            }
        }
    }
}