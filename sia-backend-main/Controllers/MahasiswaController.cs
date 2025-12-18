using astratech_apps_backend.DTOs.Mahasiswa;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace astratech_apps_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MahasiswaController : ControllerBase
    {
        private readonly string _conn;

        public MahasiswaController(IConfiguration config)
        {
            _conn = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
                config.GetConnectionString("DefaultConnection")!,
                Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
            );
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
    }
}