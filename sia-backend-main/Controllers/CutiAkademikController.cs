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
       
        [HttpGet("riwayat/excel")]
        [ProducesResponseType(typeof(IEnumerable<CutiAkademikRiwayatExcelResponse>), 200)]
        public async Task<IActionResult> GetRiwayatExcel([FromQuery] string userId = "")
        {
            var data = await _service.GetRiwayatExcelAsync(userId);
            return Ok(data);
        }

        
        [HttpGet("debug/check-record/{id}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DebugCheckRecord(string id)
        {
            try
            {
                Console.WriteLine($"[DEBUG] === RECORD CHECK ===");
                Console.WriteLine($"[DEBUG] ID: '{id}'");
                
                
                var connString = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
                    HttpContext.RequestServices.GetRequiredService<IConfiguration>()
                        .GetConnectionString("DefaultConnection")!,
                    Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
                );
                
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connString);
                await conn.OpenAsync();
                
                
                var checkCmd = new Microsoft.Data.SqlClient.SqlCommand(@"
                    SELECT cak_id, cak_status, mhs_id, cak_menimbang, cak_approval_prodi, 
                           cak_app_prodi_date, cak_created_date, cak_created_by,
                           CONVERT(VARCHAR(50), cak_created_date, 120) as created_date_raw,
                           CONVERT(VARCHAR(50), cak_app_prodi_date, 120) as app_prodi_date_raw
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
                    cak_created_by = reader["cak_created_by"].ToString(),
                    created_date_raw = reader["created_date_raw"].ToString(),
                    app_prodi_date_raw = reader["app_prodi_date_raw"].ToString()
                };
                reader.Close();
                
                Console.WriteLine($"[DEBUG] Record found - Status: {record.cak_status}");
                Console.WriteLine($"[DEBUG] Created Date Raw: {record.created_date_raw}");
                Console.WriteLine($"[DEBUG] Created By: {record.cak_created_by}");
                
                return Ok(new { 
                    exists = true,
                    record = record,
                    message = $"Record ditemukan dengan status: {record.cak_status}",
                    analysis = new {
                        created_date_looks_like_user_id = record.created_date_raw == record.cak_created_by,
                        created_date_is_valid = DateTime.TryParse(record.created_date_raw, out _),
                        app_prodi_date_is_valid = DateTime.TryParse(record.app_prodi_date_raw, out _),
                        problem_description = record.created_date_raw == record.cak_created_by ? 
                            "PROBLEM: cak_created_date contains user ID instead of actual date!" : 
                            "Date field looks normal"
                    }
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
                
                
                var connString = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
                    HttpContext.RequestServices.GetRequiredService<IConfiguration>()
                        .GetConnectionString("DefaultConnection")!,
                    Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
                );
                
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connString);
                await conn.OpenAsync();
                
                
                var cmd = new Microsoft.Data.SqlClient.SqlCommand("sia_setujuiCutiAkademikProdi", conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@p1", dto.Id);
                cmd.Parameters.AddWithValue("@p2", dto.Menimbang ?? "");
                cmd.Parameters.AddWithValue("@p3", dto.ApprovedBy);

               
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

        
        [HttpPost("debug/test-finance-approval")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DebugTestFinanceApproval([FromBody] ApproveCutiAkademikRequest dto)
        {
            try
            {
                Console.WriteLine($"[DEBUG] === FINANCE APPROVAL TEST ===");
                Console.WriteLine($"[DEBUG] ID: '{dto.Id}'");
                Console.WriteLine($"[DEBUG] Role: '{dto.Role}'");
                Console.WriteLine($"[DEBUG] ApprovedBy: '{dto.ApprovedBy}'");
                
                // Map KARYAWAN role to finance for approval logic
                if (dto.Role.ToUpper() == "KARYAWAN")
                {
                    Console.WriteLine("[DEBUG] Mapping KARYAWAN role to finance");
                    dto.Role = "finance";
                }
                
                Console.WriteLine($"[DEBUG] Mapped Role: '{dto.Role}'");
                
                var success = await _service.ApproveCutiAsync(dto);
                
                return Ok(new { 
                    success = success,
                    message = success ? "Finance approval berhasil" : "Finance approval gagal",
                    originalRole = dto.Role,
                    parameters = new {
                        id = dto.Id,
                        role = dto.Role,
                        approvedBy = dto.ApprovedBy
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Finance Approval Exception: {ex.Message}");
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

        // ============================================
        // SK MANAGEMENT ENDPOINTS
        // ============================================
        
        /// <summary>
        /// Create SK Cuti Akademik - Generate nomor SK dan siapkan untuk upload
        /// </summary>
        [HttpPost("create-sk")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateSK([FromBody] CreateSKRequest dto)
        {
            try
            {
                Console.WriteLine($"[Controller] Create SK - ID: {dto.Id}, NoSK: {dto.NoSK}, CreatedBy: {dto.CreatedBy}");
                
                // Validate input
                if (string.IsNullOrEmpty(dto.Id))
                {
                    Console.WriteLine("[Controller] ERROR: ID is required");
                    return BadRequest(new { message = "ID cuti akademik harus diisi." });
                }
                
                if (string.IsNullOrEmpty(dto.CreatedBy))
                {
                    Console.WriteLine("[Controller] ERROR: CreatedBy is required");
                    return BadRequest(new { message = "CreatedBy harus diisi." });
                }
                
                Console.WriteLine("[Controller] Calling service...");
                var noSK = await _service.CreateSKAsync(dto);
                Console.WriteLine($"[Controller] Service returned: {noSK}");
                
                if (!string.IsNullOrEmpty(noSK))
                {
                    Console.WriteLine("[Controller] SK creation successful");
                    return Ok(new { 
                        message = "SK berhasil dibuat dan siap untuk diupload.", 
                        noSK = noSK,
                        status = "Disetujui"
                    });
                }
                
                Console.WriteLine("[Controller] SK creation failed - service returned null");
                return BadRequest(new { message = "Gagal membuat SK. Periksa apakah ID valid dan status cuti sudah disetujui finance." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Controller] ERROR in CreateSK: {ex.Message}");
                Console.WriteLine($"[Controller] Stack trace: {ex.StackTrace}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat membuat SK.", 
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Upload SK Cuti Akademik (untuk admin)
        /// </summary>
        [HttpPut("upload-sk")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UploadSK([FromForm] UploadSKRequest dto)
        {
            try
            {
                Console.WriteLine($"[Controller] Upload SK - ID: {dto.Id}, File: {dto.FileSK?.FileName}, UploadBy: {dto.UploadBy}");
                
                // Validate input
                if (string.IsNullOrEmpty(dto.Id))
                {
                    Console.WriteLine("[Controller] ERROR: ID is required");
                    return BadRequest(new { message = "ID cuti akademik harus diisi." });
                }
                
                if (dto.FileSK == null || dto.FileSK.Length == 0)
                {
                    Console.WriteLine("[Controller] ERROR: File SK is required");
                    return BadRequest(new { message = "File SK harus diupload." });
                }
                
                if (string.IsNullOrEmpty(dto.UploadBy))
                {
                    Console.WriteLine("[Controller] ERROR: UploadBy is required");
                    return BadRequest(new { message = "UploadBy harus diisi." });
                }

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(dto.FileSK.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    Console.WriteLine($"[Controller] ERROR: Invalid file type: {fileExtension}");
                    return BadRequest(new { message = $"Tipe file tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" });
                }

                // Validate file size (max 10MB)
                if (dto.FileSK.Length > 10 * 1024 * 1024)
                {
                    Console.WriteLine($"[Controller] ERROR: File too large: {dto.FileSK.Length} bytes");
                    return BadRequest(new { message = "Ukuran file maksimal 10MB." });
                }
                
                Console.WriteLine("[Controller] Calling service...");
                var success = await _service.UploadSKAsync(dto);
                Console.WriteLine($"[Controller] Service returned: {success}");
                
                if (success)
                {
                    Console.WriteLine("[Controller] SK upload successful");
                    return Ok(new { message = "SK berhasil diupload. Status cuti akademik telah diubah menjadi 'Disetujui'." });
                }
                
                Console.WriteLine("[Controller] SK upload failed - service returned false");
                return BadRequest(new { message = "Gagal mengupload SK. Periksa apakah ID valid dan status cuti adalah 'Menunggu Upload SK'." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Controller] ERROR in UploadSK: {ex.Message}");
                Console.WriteLine($"[Controller] Stack trace: {ex.StackTrace}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengupload SK.", 
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }
    }
}