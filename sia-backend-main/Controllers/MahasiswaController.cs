using astratech_apps_backend.DTOs.Mahasiswa;
using astratech_apps_backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace astratech_apps_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SuppressMessage("SonarQube", "S6960", Justification = "Controller consolidates related mahasiswa operations for system consistency")]
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

        
        [HttpGet("GetProdiList")]
        [ProducesResponseType(typeof(IEnumerable<ProdiListResponse>), 200)]
        public async Task<IActionResult> GetProdiList()
        {
            try
            {
                
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

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengambil daftar prodi.", 
                    error = ex.Message 
                });
            }
        }

       
        [HttpGet("GetByProdi")]
        [ProducesResponseType(typeof(IEnumerable<MahasiswaListResponse>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetByProdi([FromQuery] string konId)
        {
            try
            {
                
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

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengambil daftar mahasiswa.", 
                    error = ex.Message 
                });
            }
        }

       
        [HttpGet("GetDetail")]
        [ProducesResponseType(typeof(MahasiswaDetailResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetDetail([FromQuery] string mhsId)
        {
            try
            {
                
                if (string.IsNullOrEmpty(mhsId))
                {
                    return BadRequest(new { message = "Parameter mhsId harus diisi." });
                }

                var result = await _mahasiswaRepository.GetDetailAsync(mhsId);
                
                if (result == null)
                {
                    return NotFound(new { message = $"Mahasiswa dengan ID {mhsId} tidak ditemukan." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengambil detail mahasiswa.", 
                    error = ex.Message 
                });
            }
        }

       
        [HttpGet("GetKonsentrasiList")]
        [ProducesResponseType(typeof(IEnumerable<KonsentrasiListResponse>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetKonsentrasiList([FromQuery] string username)
        {
            try
            {
                
                if (string.IsNullOrEmpty(username))
                {
                    return BadRequest(new { message = "Parameter username harus diisi." });
                }

                var result = await _mahasiswaRepository.GetKonsentrasiListBySekprodiAsync(username);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengambil daftar konsentrasi.", 
                    error = ex.Message 
                });
            }
        }

       
        [HttpGet("GetByKonsentrasi")]
        [ProducesResponseType(typeof(IEnumerable<MahasiswaByKonsentrasiResponse>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetByKonsentrasi([FromQuery] string konId)
        {
            try
            {
                
                if (string.IsNullOrEmpty(konId))
                {
                    return BadRequest(new { message = "Parameter konId harus diisi." });
                }

                var result = await _mahasiswaRepository.GetMahasiswaByKonsentrasiAsync(konId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengambil daftar mahasiswa.", 
                    error = ex.Message 
                });
            }
        }

      
        [HttpGet("GetByNIM")]
        [ProducesResponseType(typeof(MahasiswaByNIMResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByNIM([FromQuery] string nim)
        {
            try
            {
                
                if (string.IsNullOrEmpty(nim))
                {
                    return BadRequest(new { message = "Parameter nim harus diisi." });
                }

                var result = await _mahasiswaRepository.GetMahasiswaByNIMAsync(nim);

                if (result == null)
                {
                    return NotFound(new { message = $"Mahasiswa dengan NIM {nim} tidak ditemukan atau tidak aktif." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengambil data mahasiswa.", 
                    error = ex.Message 
                });
            }
        }

        
        [HttpGet("CheckBebasTanggungan")]
        [ProducesResponseType(typeof(BebasTanggunganResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CheckBebasTanggungan([FromQuery] string userId)
        {
            try
            {
                
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { message = "Parameter userId harus diisi." });
                }

                var status = await _mahasiswaRepository.CheckBebasTanggunganAsync(userId);

                return Ok(new BebasTanggunganResponse { Status = status });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengecek bebas tanggungan.", 
                    error = ex.Message 
                });
            }
        }

        
            [HttpGet("GetProfil")]
            [ProducesResponseType(typeof(ProfilMahasiswaResponse), 200)]
            [ProducesResponseType(400)]
            [ProducesResponseType(404)]
            public async Task<IActionResult> GetProfil([FromQuery] string nim)
            {
                try
                {
                
                    if (string.IsNullOrEmpty(nim))
                    {
                        return BadRequest(new { message = "Parameter nim harus diisi." });
                    }

                    var result = await _mahasiswaRepository.GetProfilMahasiswaAsync(nim);

                    if (result == null)
                    {
                        return NotFound(new { message = $"Profil mahasiswa dengan NIM {nim} tidak ditemukan." });
                    }

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { 
                        message = "Terjadi kesalahan saat mengambil profil mahasiswa.", 
                        error = ex.Message 
                    });
                }
            }
    }
}
