using astratech_apps_backend.DTOs.CutiAkademik;
using astratech_apps_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace astratech_apps_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class CutiAkademikController : ControllerBase
    {
        private readonly ICutiAkademikService _service;

        public CutiAkademikController(ICutiAkademikService service)
        {
            _service = service;
        }

        // ============================================
        // CREATE DRAFT (Mahasiswa)
        // ============================================
        [HttpPost("draft")]
        [HttpPost]
        public async Task<IActionResult> CreateDraft([FromForm] CreateDraftCutiRequest dto)

        {
            var id = await _service.CreateDraftAsync(dto);
            return Ok(new { draftId = id });
        }

        // ============================================
        // GENERATE FINAL ID (Mahasiswa)
        // ============================================
        [HttpPut("generate-id")]
        public async Task<IActionResult> GenerateId([FromBody] GenerateCutiIdRequest dto)
        {
            var id = await _service.GenerateIdAsync(dto);

            if (id == null)
                return BadRequest(new { message = "Gagal generate ID final." });

            return Ok(new { finalId = id });
        }

        // ============================================
        // CREATE DRAFT (Prodi)
        // ============================================
        /// <summary>
        /// Create draft cuti akademik oleh prodi
        /// </summary>
        /// <param name="dto">Data draft cuti akademik</param>
        /// <returns>Draft ID yang dibuat</returns>
        [HttpPost("prodi/draft")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateDraftByProdi([FromForm] CreateCutiProdiRequest dto)
        {
            try
            {
                Console.WriteLine($"CreateDraftByProdi - MhsId: {dto.MhsId}, ApprovalProdi: {dto.ApprovalProdi}");
                
                var id = await _service.CreateDraftByProdiAsync(dto);
                
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { message = "Gagal membuat draft cuti akademik." });
                }
                
                Console.WriteLine($"Draft created successfully with ID: {id}");
                return Ok(new { draftId = id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateDraftByProdi: {ex.Message}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat membuat draft.", 
                    error = ex.Message 
                });
            }
        }

        // ============================================
        // GENERATE FINAL ID (Prodi)
        // ============================================
        /// <summary>
        /// Generate final ID dari draft cuti akademik (prodi)
        /// </summary>
        /// <param name="dto">Data untuk generate final ID</param>
        /// <returns>Final ID yang di-generate</returns>
        [HttpPut("prodi/generate-id")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GenerateIdByProdi([FromBody] GenerateCutiProdiIdRequest dto)
        {
            try
            {
                Console.WriteLine($"GenerateIdByProdi - DraftId: {dto.DraftId}, ModifiedBy: {dto.ModifiedBy}");
                
                var id = await _service.GenerateIdByProdiAsync(dto);
                
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { message = "Gagal generate id final (prodi)." });
                }
                
                Console.WriteLine($"Final ID generated successfully: {id}");
                return Ok(new { finalId = id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GenerateIdByProdi: {ex.Message}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat generate final ID.", 
                    error = ex.Message 
                });
            }
        }

        // ============================================
        // GET ALL CUTI
        // ============================================
        /// <summary>
        /// Mendapatkan semua data cuti akademik dengan filter
        /// </summary>
        /// <param name="mhsId">ID Mahasiswa untuk filter (default: % untuk semua)</param>
        /// <param name="status">Status cuti untuk filter. Kosongkan untuk menampilkan semua data. Contoh: 'disetujui', 'belum disetujui prodi'</param>
        /// <param name="userId">ID User untuk filter berdasarkan role</param>
        /// <param name="role">Role user untuk menentukan akses data</param>
        /// <param name="search">Kata kunci pencarian</param>
        /// <returns>List data cuti akademik</returns>
        /// <response code="200">Berhasil mendapatkan data</response>
        /// <remarks>
        /// Endpoint ini akan menampilkan SEMUA data cuti akademik secara default jika tidak ada parameter yang diisi.
        /// 
        /// Contoh penggunaan:
        /// 
        ///     GET /api/CutiAkademik (menampilkan semua data)
        ///     GET /api/CutiAkademik?status=disetujui (filter status disetujui)
        ///     GET /api/CutiAkademik?search=0420240032 (cari berdasarkan NIM)
        /// 
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CutiAkademikListResponse>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string mhsId = "%", 
            [FromQuery] string status = "",
            [FromQuery] string userId = "", 
            [FromQuery] string role = "", 
            [FromQuery] string search = "")
        {
            var result = await _service.GetAllAsync(mhsId, status, userId, role, search);
            return Ok(result);
        }

        // ============================================
        // GET DETAIL CUTI
        // ============================================
        [HttpGet("detail")]
        public async Task<IActionResult> GetDetail([FromQuery] string id)
        {
            var data = await _service.GetDetailAsync(id);

            if (data == null)
                return NotFound(new { message = "Detail Cuti Akademik tidak ditemukan." });

            return Ok(data);
        }


        // ============================================
        // UPDATE CUTI (WITH FILE UPLOAD)
        // ============================================
        /// <summary>
        /// Update data cuti akademik (draft atau final)
        /// </summary>
        /// <param name="id">ID cuti akademik yang akan diupdate</param>
        /// <param name="dto">Data update cuti akademik</param>
        /// <returns>Status update</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateDraft(string id, [FromForm] UpdateCutiAkademikRequest dto)
        {
            try
            {
                // Debug logging
                Console.WriteLine($"Update request for ID: {id}");
                Console.WriteLine($"TahunAjaran: {dto.TahunAjaran}");
                Console.WriteLine($"Semester: {dto.Semester}");
                Console.WriteLine($"ModifiedBy: {dto.ModifiedBy}");

                // Set ModifiedBy dari context jika tidak ada
                if (string.IsNullOrEmpty(dto.ModifiedBy))
                {
                    dto.ModifiedBy = HttpContext.Items["UserId"]?.ToString() ?? "system";
                    Console.WriteLine($"Auto-set ModifiedBy to: {dto.ModifiedBy}");
                }

                var success = await _service.UpdateAsync(id, dto);

                if (success)
                {
                    Console.WriteLine("Update successful");
                    return Ok(new { message = "Cuti Akademik berhasil diupdate." });
                }
                
                Console.WriteLine("Update failed - no rows affected");
                return BadRequest(new { message = "Gagal mengupdate Cuti Akademik. Data mungkin tidak ditemukan." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengupdate data.", 
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        // ============================================
        // SOFT DELETE
        // ============================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var modifiedBy = HttpContext.Items["UserId"]?.ToString() ?? "system";

            var success = await _service.DeleteAsync(id, modifiedBy);

            if (!success)
                return BadRequest(new { message = "Gagal menghapus Cuti Akademik." });

            return Ok(new { message = "Cuti Akademik berhasil dihapus." });
        }

        // ============================================
        // RIWAYAT CUTI
        // ============================================
        /// <summary>
        /// Mendapatkan riwayat data cuti akademik
        /// </summary>
        /// <param name="userId">ID User untuk filter berdasarkan role (opsional)</param>
        /// <param name="status">Status cuti akademik untuk filter (opsional). Kosongkan untuk menampilkan semua data. Contoh: 'disetujui', 'belum disetujui prodi', 'menunggu upload sk'</param>
        /// <param name="search">Kata kunci pencarian berdasarkan NIM atau ID cuti (opsional)</param>
        /// <returns>List riwayat cuti akademik</returns>
        /// <response code="200">Berhasil mendapatkan data riwayat</response>
        /// <response code="500">Terjadi kesalahan server</response>
        /// <remarks>
        /// Endpoint ini akan menampilkan SEMUA riwayat cuti akademik secara default jika tidak ada parameter yang diisi.
        /// 
        /// Contoh penggunaan:
        /// 
        ///     GET /api/CutiAkademik/riwayat (menampilkan semua riwayat)
        ///     GET /api/CutiAkademik/riwayat?status=disetujui (filter status disetujui)
        ///     GET /api/CutiAkademik/riwayat?search=0420240032 (cari berdasarkan NIM)
        ///     GET /api/CutiAkademik/riwayat?status=disetujui&amp;search=042024 (kombinasi filter)
        /// 
        /// Status yang tersedia:
        /// - disetujui
        /// - belum disetujui prodi  
        /// - belum disetujui wadir 1
        /// - menunggu upload sk
        /// - belum disetujui finance
        /// - draft
        /// </remarks>
        [HttpGet("riwayat")]
        [ProducesResponseType(typeof(IEnumerable<CutiAkademikListResponse>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRiwayat(
            [FromQuery] string userId = "", 
            [FromQuery] string status = "", 
            [FromQuery] string search = "")
        {
            var result = await _service.GetRiwayatAsync(userId, status, search);
            return Ok(result);
        }

        /// <summary>
        /// Mendapatkan data riwayat cuti akademik dalam format Excel
        /// </summary>
        /// <param name="userId">ID User untuk filter data (opsional)</param>
        /// <returns>Data riwayat dalam format yang siap untuk export Excel</returns>
        /// <response code="200">Berhasil mendapatkan data untuk Excel</response>
        [HttpGet("riwayat/excel")]
        [ProducesResponseType(typeof(IEnumerable<CutiAkademikRiwayatExcelResponse>), 200)]
        public async Task<IActionResult> GetRiwayatExcel([FromQuery] string userId = "")
        {
            var data = await _service.GetRiwayatExcelAsync(userId);
            return Ok(data);
        }

        /// <summary>
        /// Debug endpoint - Check record status and test stored procedure
        /// </summary>
        [HttpGet("debug/check-record/{id}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DebugCheckRecord(string id)
        {
            try
            {
                Console.WriteLine($"[DEBUG] === RECORD CHECK ===");
                Console.WriteLine($"[DEBUG] ID: '{id}'");
                
                // Get connection string
                var connString = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
                    HttpContext.RequestServices.GetRequiredService<IConfiguration>()
                        .GetConnectionString("DefaultConnection")!,
                    Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
                );
                
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connString);
                await conn.OpenAsync();
                
                // Check if record exists and get all relevant fields
                var checkCmd = new Microsoft.Data.SqlClient.SqlCommand(@"
                    SELECT cak_id, cak_status, mhs_id, cak_menimbang, cak_approval_prodi, 
                           cak_app_prodi_date, cak_created_date, cak_created_by
                    FROM sia_mscutiakademik 
                    WHERE cak_id = @id", conn);
                checkCmd.Parameters.AddWithValue("@id", id);
                
                var reader = await checkCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    reader.Close();
                    return Ok(new { 
                        exists = false,
                        message = "Record tidak ditemukan di database",
                        id = id
                    });
                }
                
                var record = new {
                    cak_id = reader["cak_id"].ToString(),
                    cak_status = reader["cak_status"].ToString(),
                    mhs_id = reader["mhs_id"].ToString(),
                    cak_menimbang = reader["cak_menimbang"].ToString(),
                    cak_approval_prodi = reader["cak_approval_prodi"].ToString(),
                    cak_app_prodi_date = reader["cak_app_prodi_date"].ToString(),
                    cak_created_date = reader["cak_created_date"].ToString(),
                    cak_created_by = reader["cak_created_by"].ToString()
                };
                reader.Close();
                
                Console.WriteLine($"[DEBUG] Record found - Status: {record.cak_status}");
                
                return Ok(new { 
                    exists = true,
                    record = record,
                    message = $"Record ditemukan dengan status: {record.cak_status}",
                    canBeApproved = !string.IsNullOrEmpty(record.cak_status) && 
                                   record.cak_status != "Belum Disetujui Wadir 1" &&
                                   record.cak_status != "Disetujui"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Exception: {ex.Message}");
                return Ok(new { 
                    exists = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Debug endpoint - Test stored procedure execution
        /// </summary>
        [HttpPost("debug/test-sp-approval")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DebugTestSpApproval([FromBody] ApproveProdiCutiRequest dto)
        {
            try
            {
                Console.WriteLine($"[DEBUG] === SP APPROVAL TEST ===");
                Console.WriteLine($"[DEBUG] ID: '{dto.Id}'");
                Console.WriteLine($"[DEBUG] Menimbang: '{dto.Menimbang}' (Length: {dto.Menimbang?.Length ?? 0})");
                Console.WriteLine($"[DEBUG] ApprovedBy: '{dto.ApprovedBy}'");
                
                // Get connection string
                var connString = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
                    HttpContext.RequestServices.GetRequiredService<IConfiguration>()
                        .GetConnectionString("DefaultConnection")!,
                    Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
                );
                
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connString);
                await conn.OpenAsync();
                
                // Test the stored procedure directly
                var cmd = new Microsoft.Data.SqlClient.SqlCommand("sia_setujuiCutiAkademikProdi", conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@p1", dto.Id);
                cmd.Parameters.AddWithValue("@p2", dto.Menimbang ?? "");
                cmd.Parameters.AddWithValue("@p3", dto.ApprovedBy);

                // p4-p50 kosong
                for (int i = 4; i <= 50; i++)
                    cmd.Parameters.AddWithValue($"@p{i}", "");

                Console.WriteLine($"[DEBUG] Executing SP with params: @p1='{dto.Id}', @p2='{dto.Menimbang}', @p3='{dto.ApprovedBy}'");
                
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                
                Console.WriteLine($"[DEBUG] SP executed - Rows affected: {rowsAffected}");
                
                return Ok(new { 
                    success = rowsAffected > 0,
                    rowsAffected = rowsAffected,
                    message = rowsAffected > 0 ? "SP berhasil dieksekusi" : "SP tidak mengupdate record apapun",
                    parameters = new {
                        p1 = dto.Id,
                        p2 = dto.Menimbang,
                        p3 = dto.ApprovedBy
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] SP Exception: {ex.Message}");
                return Ok(new { 
                    success = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // ============================================
        // DOWNLOAD FILE
        // ============================================
        [HttpGet("file/{filename}")]
        public IActionResult DownloadFile(string filename)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/cuti", filename);

            if (!System.IO.File.Exists(path))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "application/octet-stream", filename);
        }

        // ============================================
        // APPROVAL & REJECTION ENDPOINTS
        // ============================================
        
        /// <summary>
        /// Menyetujui cuti akademik (untuk prodi/wadir1/finance)
        /// </summary>
        /// <param name="dto">Data approval</param>
        /// <returns>Status approval</returns>
        /// <remarks>
        /// Role yang valid:
        /// - "prodi" → Status menjadi "Belum Disetujui Wadir 1"
        /// - "wadir1" → Status menjadi "Belum Disetujui Finance"
        /// - "finance" → Status menjadi "Menunggu Upload SK" dan generate nomor surat
        /// </remarks>
        [HttpPut("approve")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ApproveCuti([FromBody] ApproveCutiAkademikRequest dto)
        {
            try
            {
                Console.WriteLine($"Approve request - ID: {dto.Id}, Role: {dto.Role}, ApprovedBy: {dto.ApprovedBy}");
                
                var success = await _service.ApproveCutiAsync(dto);
                
                if (success)
                {
                    Console.WriteLine("Approval successful");
                    return Ok(new { message = "Cuti akademik berhasil disetujui." });
                }
                
                return BadRequest(new { message = "Gagal menyetujui cuti akademik." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ApproveCuti: {ex.Message}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat menyetujui cuti akademik.", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Menyetujui cuti akademik oleh prodi
        /// </summary>
        /// <param name="dto">Data approval prodi (termasuk menimbang)</param>
        /// <returns>Status approval</returns>
        /// <remarks>
        /// Contoh request body:
        /// {
        ///   "id": "033/PMA/CA/XIII/2025",
        ///   "menimbang": "Mahasiswa memenuhi syarat untuk cuti akademik",
        ///   "approvedBy": "prodi_user"
        /// }
        /// </remarks>
        [HttpPut("approve/prodi")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ApproveProdiCuti([FromBody] ApproveProdiCutiRequest dto)
        {
            try
            {
                Console.WriteLine($"[Controller] Approve Prodi - ID: {dto.Id}, Menimbang: '{dto.Menimbang}', ApprovedBy: {dto.ApprovedBy}");
                
                // Validate input
                if (string.IsNullOrEmpty(dto.Id))
                {
                    Console.WriteLine("[Controller] ERROR: ID is required");
                    return BadRequest(new { message = "ID cuti akademik harus diisi." });
                }
                
                if (string.IsNullOrEmpty(dto.ApprovedBy))
                {
                    Console.WriteLine("[Controller] ERROR: ApprovedBy is required");
                    return BadRequest(new { message = "ApprovedBy harus diisi." });
                }
                
                if (string.IsNullOrWhiteSpace(dto.Menimbang))
                {
                    Console.WriteLine("[Controller] ERROR: Menimbang is required");
                    return BadRequest(new { message = "Menimbang/pertimbangan harus diisi dan tidak boleh kosong." });
                }
                
                Console.WriteLine("[Controller] Calling service...");
                var success = await _service.ApproveProdiCutiAsync(dto);
                Console.WriteLine($"[Controller] Service returned: {success}");
                
                if (success)
                {
                    Console.WriteLine("[Controller] Prodi approval successful");
                    return Ok(new { message = "Cuti akademik berhasil disetujui oleh prodi." });
                }
                
                Console.WriteLine("[Controller] Prodi approval failed - service returned false");
                return BadRequest(new { message = "Gagal menyetujui cuti akademik. Periksa apakah ID valid dan data dapat diupdate." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Controller] ERROR in ApproveProdiCuti: {ex.Message}");
                Console.WriteLine($"[Controller] Stack trace: {ex.StackTrace}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat menyetujui cuti akademik.", 
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Menolak cuti akademik dengan keterangan
        /// </summary>
        /// <param name="dto">Data penolakan</param>
        /// <returns>Status penolakan</returns>
        /// <remarks>
        /// Status akan menjadi "Ditolak {role}"
        /// Contoh: "Ditolak prodi", "Ditolak wadir1", "Ditolak finance"
        /// 
        /// Contoh request body:
        /// {
        ///   "id": "033/PMA/CA/XIII/2025",
        ///   "role": "prodi",
        ///   "keterangan": "Dokumen tidak lengkap"
        /// }
        /// </remarks>
        [HttpPut("reject")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RejectCuti([FromBody] RejectCutiAkademikRequest dto)
        {
            try
            {
                Console.WriteLine($"[Controller] Reject request - ID: {dto.Id}, Role: {dto.Role}, Keterangan: {dto.Keterangan}");
                
                // Validate input
                if (string.IsNullOrEmpty(dto.Id))
                {
                    Console.WriteLine("[Controller] ERROR: ID is required");
                    return BadRequest(new { message = "ID cuti akademik harus diisi." });
                }
                
                if (string.IsNullOrEmpty(dto.Role))
                {
                    Console.WriteLine("[Controller] ERROR: Role is required");
                    return BadRequest(new { message = "Role harus diisi." });
                }
                
                if (string.IsNullOrWhiteSpace(dto.Keterangan))
                {
                    Console.WriteLine("[Controller] ERROR: Keterangan is required");
                    return BadRequest(new { message = "Keterangan/alasan penolakan harus diisi dan tidak boleh kosong." });
                }
                
                Console.WriteLine("[Controller] Calling service...");
                var success = await _service.RejectCutiAsync(dto);
                Console.WriteLine($"[Controller] Service returned: {success}");
                
                if (success)
                {
                    Console.WriteLine("[Controller] Rejection successful");
                    return Ok(new { message = "Cuti akademik berhasil ditolak." });
                }
                
                Console.WriteLine("[Controller] Rejection failed - service returned false");
                return BadRequest(new { message = "Gagal menolak cuti akademik. Periksa apakah ID valid dan data dapat diupdate." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Controller] ERROR in RejectCuti: {ex.Message}");
                Console.WriteLine($"[Controller] Stack trace: {ex.StackTrace}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat menolak cuti akademik.", 
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }
    }
}