using astratech_apps_backend.DTOs.Mahasiswa;
using astratech_apps_backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace astratech_apps_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MahasiswaController : ControllerBase
    {
        private readonly string _conn;
        private readonly IMahasiswaRepository _mahasiswaRepository;

        public MahasiswaController(IConfiguration config, IMahasiswaRepository mahasiswaRepository)
        {
            _conn = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
                config.GetConnectionString("DefaultConnection")!,
                Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
            );
            _mahasiswaRepository = mahasiswaRepository;
        }

        /// <summary>
        /// Mendapatkan daftar Program Studi yang unik
        /// </summary>
        /// <returns>List Program Studi dengan kon_id dan kon_nama</returns>
        /// <response code="200">Berhasil mendapatkan daftar prodi</response>
        /// <remarks>
        /// Endpoint ini membaca data unik kon_id dan kon_nama dari sia_msmahasiswa
        /// untuk menampilkan daftar Program Studi yang tersedia.
        /// 
        /// Contoh response:
        /// [
        ///   {
        ///     "konId": "001",
        ///     "konNama": "Teknik Informatika"
        ///   },
        ///   {
        ///     "konId": "002", 
        ///     "konNama": "Sistem Informasi"
        ///   }
        /// ]
        /// </remarks>
        [HttpGet("GetProdiList")]
        [ProducesResponseType(typeof(IEnumerable<ProdiListResponse>), 200)]
        public async Task<IActionResult> GetProdiList()
        {
            try
            {
                Console.WriteLine("[GetProdiList] Starting to fetch prodi list");
                
                var result = new List<ProdiListResponse>();

                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                // Query untuk mendapatkan daftar prodi yang unik dari mahasiswa
                var sql = @"
                    SELECT DISTINCT k.kon_id, k.kon_nama
                    FROM sia_msmahasiswa m
                    INNER JOIN sia_mskonsentrasi k ON m.kon_id = k.kon_id
                    WHERE m.kon_id IS NOT NULL 
                      AND k.kon_nama IS NOT NULL
                    ORDER BY k.kon_nama";

                await using var cmd = new SqlCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new ProdiListResponse
                    {
                        KonId = reader["kon_id"].ToString() ?? "",
                        KonNama = reader["kon_nama"].ToString() ?? ""
                    });
                }

                Console.WriteLine($"[GetProdiList] Found {result.Count} prodi");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetProdiList] ERROR: {ex.Message}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengambil daftar prodi.", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Mendapatkan daftar Mahasiswa berdasarkan Program Studi
        /// </summary>
        /// <param name="konId">ID Konsentrasi/Program Studi</param>
        /// <returns>List Mahasiswa dengan mhs_id dan mhs_nama</returns>
        /// <response code="200">Berhasil mendapatkan daftar mahasiswa</response>
        /// <response code="400">Parameter konId tidak valid</response>
        /// <remarks>
        /// Endpoint ini membaca data mhs_id dan mhs_nama dari sia_msmahasiswa 
        /// berdasarkan kon_id untuk menampilkan mahasiswa yang sesuai dengan prodi yang dipilih.
        /// 
        /// Contoh penggunaan:
        /// GET /api/Mahasiswa/GetByProdi?konId=001
        /// 
        /// Contoh response:
        /// [
        ///   {
        ///     "mhsId": "0420240001",
        ///     "mhsNama": "John Doe",
        ///     "angkatan": "2024",
        ///     "konNama": "Teknik Informatika"
        ///   }
        /// ]
        /// </remarks>
        [HttpGet("GetByProdi")]
        [ProducesResponseType(typeof(IEnumerable<MahasiswaListResponse>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetByProdi([FromQuery] string konId)
        {
            try
            {
                Console.WriteLine($"[GetByProdi] Starting to fetch mahasiswa for konId: {konId}");
                
                if (string.IsNullOrEmpty(konId))
                {
                    return BadRequest(new { message = "Parameter konId harus diisi." });
                }

                var result = new List<MahasiswaListResponse>();

                await using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                // Query untuk mendapatkan mahasiswa berdasarkan kon_id
                var sql = @"
                    SELECT m.mhs_id, m.mhs_nama, m.mhs_angkatan, k.kon_nama
                    FROM sia_msmahasiswa m
                    INNER JOIN sia_mskonsentrasi k ON m.kon_id = k.kon_id
                    WHERE m.kon_id = @konId
                      AND m.mhs_nama IS NOT NULL
                    ORDER BY m.mhs_nama";

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@konId", konId);
                
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new MahasiswaListResponse
                    {
                        MhsId = reader["mhs_id"].ToString() ?? "",
                        MhsNama = reader["mhs_nama"].ToString() ?? "",
                        Angkatan = reader["mhs_angkatan"].ToString(),
                        KonNama = reader["kon_nama"].ToString()
                    });
                }

                Console.WriteLine($"[GetByProdi] Found {result.Count} mahasiswa for konId: {konId}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetByProdi] ERROR: {ex.Message}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengambil daftar mahasiswa.", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Mendapatkan detail profil mahasiswa berdasarkan mhs_id
        /// </summary>
        /// <param name="mhsId">ID Mahasiswa</param>
        /// <returns>Detail profil mahasiswa lengkap</returns>
        /// <response code="200">Berhasil mendapatkan detail mahasiswa</response>
        /// <response code="400">Parameter mhsId tidak valid</response>
        /// <response code="404">Mahasiswa tidak ditemukan</response>
        /// <remarks>
        /// Endpoint ini menggunakan stored procedure sia_detailMahasiswa untuk mendapatkan
        /// informasi lengkap profil mahasiswa termasuk data pribadi, orang tua/wali, 
        /// informasi akademik, dan data rekening.
        /// 
        /// Contoh penggunaan:
        /// GET /api/Mahasiswa/GetDetail?mhsId=0320210077
        /// 
        /// Contoh response:
        /// {
        ///   "dulNoPendaftaran": "2021001",
        ///   "mhsNama": "John Doe",
        ///   "konNama": "Teknik Informatika (TI)",
        ///   "mhsTempatLahir": "Jakarta",
        ///   "mhsTglLahir": "2000-01-01",
        ///   "mhsJenisKelamin": "L",
        ///   "mhsAlamat": "Jl. Contoh No. 123",
        ///   "mhsAngkatan": 2021,
        ///   "mhsStatusKuliah": "Aktif",
        ///   "mhsJenis": "Reguler"
        /// }
        /// </remarks>
        [HttpGet("GetDetail")]
        [ProducesResponseType(typeof(MahasiswaDetailResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetDetail([FromQuery] string mhsId)
        {
            try
            {
                Console.WriteLine($"[GetDetail] Starting to fetch detail for mhsId: {mhsId}");
                
                if (string.IsNullOrEmpty(mhsId))
                {
                    return BadRequest(new { message = "Parameter mhsId harus diisi." });
                }

                var result = await _mahasiswaRepository.GetDetailAsync(mhsId);
                
                if (result == null)
                {
                    return NotFound(new { message = $"Mahasiswa dengan ID {mhsId} tidak ditemukan." });
                }

                Console.WriteLine($"[GetDetail] Successfully retrieved detail for mhsId: {mhsId}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetDetail] ERROR: {ex.Message}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengambil detail mahasiswa.", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Mendapatkan daftar konsentrasi berdasarkan username sekprodi
        /// </summary>
        /// <param name="username">Username sekprodi</param>
        /// <returns>List konsentrasi dengan id dan nama</returns>
        /// <response code="200">Berhasil mendapatkan daftar konsentrasi</response>
        /// <response code="400">Parameter username tidak valid</response>
        /// <remarks>
        /// Endpoint ini menggunakan direct SQL query untuk mendapatkan daftar konsentrasi 
        /// yang dapat diakses oleh sekprodi berdasarkan username. Query menggabungkan tabel
        /// sia_mskonsentrasi, sia_msprodi, dan ess_mskaryawan untuk filter berdasarkan username.
        /// 
        /// Contoh penggunaan:
        /// GET /api/Mahasiswa/GetKonsentrasiList?username=sekprodi_user
        /// 
        /// Contoh response:
        /// [
        ///   {
        ///     "id": "001",
        ///     "nama": "Teknik Informatika (TI)"
        ///   },
        ///   {
        ///     "id": "002", 
        ///     "nama": "Sistem Informasi (SI)"
        ///   }
        /// ]
        /// </remarks>
        [HttpGet("GetKonsentrasiList")]
        [ProducesResponseType(typeof(IEnumerable<KonsentrasiListResponse>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetKonsentrasiList([FromQuery] string username)
        {
            try
            {
                Console.WriteLine($"[GetKonsentrasiList] Starting to fetch konsentrasi list for username: {username}");
                
                if (string.IsNullOrEmpty(username))
                {
                    return BadRequest(new { message = "Parameter username harus diisi." });
                }

                var result = await _mahasiswaRepository.GetKonsentrasiListBySekprodiAsync(username);

                Console.WriteLine($"[GetKonsentrasiList] Successfully retrieved {result.Count} konsentrasi for username: {username}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetKonsentrasiList] ERROR: {ex.Message}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengambil daftar konsentrasi.", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Mendapatkan daftar mahasiswa berdasarkan konsentrasi
        /// </summary>
        /// <param name="konId">ID Konsentrasi</param>
        /// <returns>List mahasiswa dengan id dan nama</returns>
        /// <response code="200">Berhasil mendapatkan daftar mahasiswa</response>
        /// <response code="400">Parameter konId tidak valid</response>
        /// <remarks>
        /// Endpoint ini menggunakan direct SQL query untuk mendapatkan daftar mahasiswa 
        /// berdasarkan konsentrasi dengan status aktif dan status kuliah aktif atau menunggu yudisium.
        /// 
        /// Contoh penggunaan:
        /// GET /api/Mahasiswa/GetByKonsentrasi?konId=3
        /// 
        /// Contoh response:
        /// [
        ///   {
        ///     "mhsId": "0320240001",
        ///     "mhsNama": "0320240001 - John Doe"
        ///   },
        ///   {
        ///     "mhsId": "0320240002", 
        ///     "mhsNama": "0320240002 - Jane Smith"
        ///   }
        /// ]
        /// </remarks>
        [HttpGet("GetByKonsentrasi")]
        [ProducesResponseType(typeof(IEnumerable<MahasiswaByKonsentrasiResponse>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetByKonsentrasi([FromQuery] string konId)
        {
            try
            {
                Console.WriteLine($"[GetByKonsentrasi] Starting to fetch mahasiswa list for konId: {konId}");
                
                if (string.IsNullOrEmpty(konId))
                {
                    return BadRequest(new { message = "Parameter konId harus diisi." });
                }

                var result = await _mahasiswaRepository.GetMahasiswaByKonsentrasiAsync(konId);

                Console.WriteLine($"[GetByKonsentrasi] Successfully retrieved {result.Count} mahasiswa for konId: {konId}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetByKonsentrasi] ERROR: {ex.Message}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengambil daftar mahasiswa.", 
                    error = ex.Message 
                });
            }
        }
    }
}