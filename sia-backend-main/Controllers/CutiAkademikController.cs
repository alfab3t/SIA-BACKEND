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
            // Validate file types - MS Word documents not allowed
            if (dto.LampiranSuratPengajuan != null)
            {
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(dto.LampiranSuratPengajuan.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { message = $"Tipe file lampiran surat pengajuan tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" });
                }

                // Validate file size (max 10MB)
                if (dto.LampiranSuratPengajuan.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Ukuran file lampiran surat pengajuan maksimal 10MB." });
                }
            }

            if (dto.Lampiran != null)
            {
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(dto.Lampiran.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { message = $"Tipe file lampiran tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" });
                }

                // Validate file size (max 10MB)
                if (dto.Lampiran.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Ukuran file lampiran maksimal 10MB." });
                }
            }

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
                // Validate file types - MS Word documents not allowed
                if (dto.LampiranSuratPengajuan != null)
                {
                    var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(dto.LampiranSuratPengajuan.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = $"Tipe file lampiran surat pengajuan tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" });
                    }

                    // Validate file size (max 10MB)
                    if (dto.LampiranSuratPengajuan.Length > 10 * 1024 * 1024)
                    {
                        return BadRequest(new { message = "Ukuran file lampiran surat pengajuan maksimal 10MB." });
                    }
                }

                if (dto.Lampiran != null)
                {
                    var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(dto.Lampiran.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = $"Tipe file lampiran tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" });
                    }

                    // Validate file size (max 10MB)
                    if (dto.Lampiran.Length > 10 * 1024 * 1024)
                    {
                        return BadRequest(new { message = "Ukuran file lampiran maksimal 10MB." });
                    }
                }

                var id = await _service.CreateDraftByProdiAsync(dto);
                
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { message = "Gagal membuat draft cuti akademik." });
                }
                
                return Ok(new { draftId = id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat membuat draft.", 
                    error = ex.Message 
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { 
                    message = "Operasi tidak valid saat membuat draft.", 
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
                var id = await _service.GenerateIdByProdiAsync(dto);
                
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { message = "Gagal generate id final (prodi)." });
                }
                
                return Ok(new { finalId = id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat generate final ID.", 
                    error = ex.Message 
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { 
                    message = "Operasi tidak valid saat generate final ID.", 
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
                // Validate file types - MS Word documents not allowed
                if (dto.LampiranSuratPengajuan != null)
                {
                    var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(dto.LampiranSuratPengajuan.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = $"Tipe file lampiran surat pengajuan tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" });
                    }

                    // Validate file size (max 10MB)
                    if (dto.LampiranSuratPengajuan.Length > 10 * 1024 * 1024)
                    {
                        return BadRequest(new { message = "Ukuran file lampiran surat pengajuan maksimal 10MB." });
                    }
                }

                if (dto.Lampiran != null)
                {
                    var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(dto.Lampiran.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = $"Tipe file lampiran tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" });
                    }

                    // Validate file size (max 10MB)
                    if (dto.Lampiran.Length > 10 * 1024 * 1024)
                    {
                        return BadRequest(new { message = "Ukuran file lampiran maksimal 10MB." });
                    }
                }

                // Set ModifiedBy dari context jika tidak ada
                if (string.IsNullOrEmpty(dto.ModifiedBy))
                {
                    dto.ModifiedBy = HttpContext.Items["UserId"]?.ToString() ?? "system";
                }

                var success = await _service.UpdateAsync(id, dto);

                if (success)
                {
                    return Ok(new { message = "Cuti Akademik berhasil diupdate." });
                }
                
                return BadRequest(new { message = "Gagal mengupdate Cuti Akademik. Data mungkin tidak ditemukan." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengupdate data.", 
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { 
                    message = "Operasi tidak valid saat mengupdate data.", 
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
        /// Get riwayat cuti akademik data as Excel file (only approved status)
        /// </summary>
        [HttpGet("riwayat/excel")]
        [ProducesResponseType(typeof(FileResult), 200)]
        public async Task<IActionResult> GetRiwayatExcel([FromQuery] string userId = "")
        {
            try
            {
                var data = await _service.GetRiwayatExcelAsync(userId);
                
                // Create Excel file using ClosedXML
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Riwayat Cuti Akademik");

                // Add headers
                worksheet.Cell(1, 1).Value = "NIM";
                worksheet.Cell(1, 2).Value = "Nama Mahasiswa";
                worksheet.Cell(1, 3).Value = "Konsentrasi";
                worksheet.Cell(1, 4).Value = "Tanggal Pengajuan";
                worksheet.Cell(1, 5).Value = "No SK";
                worksheet.Cell(1, 6).Value = "No Pengajuan";

                // Style headers
                var headerRange = worksheet.Range(1, 1, 1, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                // Add data rows
                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.NIM;
                    worksheet.Cell(row, 2).Value = item.NamaMahasiswa;
                    worksheet.Cell(row, 3).Value = item.Konsentrasi;
                    worksheet.Cell(row, 4).Value = item.TanggalPengajuan;
                    worksheet.Cell(row, 5).Value = item.NoSK;
                    worksheet.Cell(row, 6).Value = item.NoPengajuan;
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Add borders to data
                if (row > 2)
                {
                    var dataRange = worksheet.Range(2, 1, row - 1, 6);
                    dataRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                }

                // Generate file
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"RiwayatCutiAkademik_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat membuat file Excel.", 
                    error = ex.Message 
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { 
                    message = "Operasi tidak valid saat membuat file Excel.", 
                    error = ex.Message 
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
                // Auto-detect role based on username using stored procedure
                var detectedRole = await _service.DetectUserRoleAsync(dto.ApprovedBy);
                if (string.IsNullOrEmpty(detectedRole))
                {
                    return BadRequest(new { 
                        message = "Tidak dapat mendeteksi role pengguna. Pastikan username valid.",
                        username = dto.ApprovedBy
                    });
                }
                
                // Override role dengan hasil deteksi
                dto.Role = detectedRole;
                
                var success = await _service.ApproveCutiAsync(dto);
                
                if (success)
                {
                    return Ok(new { 
                        approved = true,
                        id = dto.Id,
                        approvedBy = dto.ApprovedBy,
                        role = detectedRole,
                        message = $"Cuti akademik berhasil disetujui oleh {detectedRole}"
                    });
                }
                
                return BadRequest(new { 
                    message = "Gagal menyetujui cuti akademik. Data mungkin tidak ditemukan atau sudah diproses.",
                    id = dto.Id,
                    detectedRole = detectedRole,
                    username = dto.ApprovedBy
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat menyetujui cuti akademik.", 
                    error = ex.Message,
                    id = dto.Id
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { 
                    message = "Operasi tidak valid saat menyetujui cuti akademik.", 
                    error = ex.Message,
                    id = dto.Id
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
                // Validate input
                if (string.IsNullOrEmpty(dto.Id))
                {
                    return BadRequest(new { message = "ID cuti akademik harus diisi." });
                }
                
                if (string.IsNullOrEmpty(dto.ApprovedBy))
                {
                    return BadRequest(new { message = "ApprovedBy harus diisi." });
                }
                
                if (string.IsNullOrWhiteSpace(dto.Menimbang))
                {
                    return BadRequest(new { message = "Menimbang/pertimbangan harus diisi dan tidak boleh kosong." });
                }
                
                var success = await _service.ApproveProdiCutiAsync(dto);
                
                if (success)
                {
                    return Ok(new { message = "Cuti akademik berhasil disetujui oleh prodi." });
                }
                
                return BadRequest(new { message = "Gagal menyetujui cuti akademik. Periksa apakah ID valid dan data dapat diupdate." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat menyetujui cuti akademik.", 
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { 
                    message = "Operasi tidak valid saat menyetujui cuti akademik.", 
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
                // Validate input
                if (string.IsNullOrEmpty(dto.Id))
                {
                    return BadRequest(new { message = "ID cuti akademik harus diisi." });
                }
                
                if (string.IsNullOrEmpty(dto.Username))
                {
                    return BadRequest(new { message = "Username harus diisi." });
                }
                
                // Auto-detect role based on username using stored procedure
                var detectedRole = await _service.DetectUserRoleAsync(dto.Username);
                if (string.IsNullOrEmpty(detectedRole))
                {
                    return BadRequest(new { 
                        message = "Tidak dapat mendeteksi role pengguna. Pastikan username valid.",
                        username = dto.Username
                    });
                }
                
                // Override role dengan hasil deteksi
                dto.Role = detectedRole;
                
                var success = await _service.RejectCutiAsync(dto);
                
                if (success)    
                {
                    return Ok(new { 
                        rejected = true,
                        id = dto.Id,
                        rejectedBy = dto.Username,
                        role = detectedRole,
                        message = $"Cuti akademik berhasil ditolak oleh {detectedRole}"
                    });
                }
                
                return BadRequest(new { 
                    message = "Gagal menolak cuti akademik. Data mungkin tidak ditemukan atau sudah diproses.",
                    id = dto.Id,
                    detectedRole = detectedRole,
                    username = dto.Username
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat menolak cuti akademik.", 
                    error = ex.Message,
                    id = dto.Id
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { 
                    message = "Operasi tidak valid saat menolak cuti akademik.", 
                    error = ex.Message,
                    id = dto.Id
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
                // Validate input
                if (string.IsNullOrEmpty(dto.Id))
                {
                    return BadRequest(new { message = "ID cuti akademik harus diisi." });
                }
                
                if (string.IsNullOrEmpty(dto.CreatedBy))
                {
                    return BadRequest(new { message = "CreatedBy harus diisi." });
                }
                
                var noSK = await _service.CreateSKAsync(dto);
                
                if (!string.IsNullOrEmpty(noSK))
                {
                    return Ok(new { 
                        message = "SK berhasil dibuat dan siap untuk diupload.", 
                        noSK = noSK,
                        status = "Disetujui"
                    });
                }
                
                return BadRequest(new { message = "Gagal membuat SK. Periksa apakah ID valid dan status cuti sudah disetujui finance." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat membuat SK.", 
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { 
                    message = "Operasi tidak valid saat membuat SK.", 
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
                // Validate input
                if (string.IsNullOrEmpty(dto.Id))
                {
                    return BadRequest(new { message = "ID cuti akademik harus diisi." });
                }
                
                if (dto.FileSK == null || dto.FileSK.Length == 0)
                {
                    return BadRequest(new { message = "File SK harus diupload." });
                }
                
                if (string.IsNullOrEmpty(dto.UploadBy))
                {
                    return BadRequest(new { message = "UploadBy harus diisi." });
                }

                // Validate file type - MS Word documents not allowed
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(dto.FileSK.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { message = $"Tipe file tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" });
                }

                // Validate file size (max 10MB)
                if (dto.FileSK.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Ukuran file maksimal 10MB." });
                }
                
                var success = await _service.UploadSKAsync(dto);
                
                if (success)
                {
                    return Ok(new { 
                        message = "SK berhasil diupload. Status cuti akademik telah diubah menjadi 'Disetujui'. Nomor SK akan ditampilkan otomatis di daftar.",
                        success = true,
                        id = dto.Id
                    });
                }
                
                return BadRequest(new { message = "Gagal mengupload SK. Periksa apakah ID valid dan status cuti adalah 'Menunggu Upload SK'." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengupload SK.", 
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { 
                    message = "Operasi tidak valid saat mengupload SK.", 
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        // ============================================
        // CETAK SK CUTI AKADEMIK ENDPOINT
        // ============================================
        
        /// <summary>
        /// Cetak SK Cuti Akademik - Single endpoint untuk cetak SK
        /// Role ROL21 (Admin Akademik): Can print when status = "Menunggu Upload SK"
        /// Role ROL23 (Mahasiswa): Can print when status = "Disetujui"
        /// Query parameter 'format' menentukan output: 'json' (default) atau 'pdf'
        /// </summary>
        [HttpGet("cetak-sk/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CetakSK(string id, [FromQuery] string username, [FromQuery] string role = "", [FromQuery] string format = "json")
        {
            try
            {
                // 1. Validate input
                var validationResult = ValidateCetakSKInput(id, username);
                if (validationResult != null) return validationResult;

                id = Uri.UnescapeDataString(id);

                // 2. Detect role if not provided
                role = await DetectUserRoleIfNeeded(role, username);
                if (string.IsNullOrEmpty(role))
                {
                    return BadRequest(new { 
                        message = "Tidak dapat mendeteksi role pengguna. Pastikan username valid.",
                        username = username
                    });
                }

                // 3. Get cuti detail
                var cutiDetail = await _service.GetDetailAsync(id);
                if (cutiDetail == null)
                {
                    return NotFound(new { message = "Data cuti akademik tidak ditemukan." });
                }

                // 4. Check permissions
                var permissionResult = CheckPrintPermission(role, cutiDetail.Status ?? "");
                if (!permissionResult.canPrint)
                {
                    return StatusCode(403, new { 
                        message = "Tidak memiliki akses untuk cetak SK.", 
                        reason = permissionResult.reason,
                        currentStatus = cutiDetail.Status,
                        allowedStatus = permissionResult.allowedStatus,
                        userRole = role,
                        canPrint = false
                    });
                }

                // 5. Generate response based on format
                return await GenerateCetakSKResponse(id, username, role, format, cutiDetail, permissionResult.reason);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat cetak SK.", 
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { 
                    message = "Operasi tidak valid saat cetak SK.", 
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        private IActionResult? ValidateCetakSKInput(string id, string username)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "ID cuti akademik harus diisi." });
            }
            
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { message = "Username harus diisi." });
            }

            return null;
        }

        private async Task<string> DetectUserRoleIfNeeded(string role, string username)
        {
            if (string.IsNullOrEmpty(role))
            {
                role = await _service.DetectUserRoleAsync(username);
            }
            return role ?? "";
        }

        private (bool canPrint, string reason, string allowedStatus) CheckPrintPermission(string role, string currentStatus)
        {
            var roleUpper = role.ToUpper();
            
            if (roleUpper == "ROL21" || roleUpper == "ADMIN")
            {
                var allowedStatus = "Menunggu Upload SK";
                var canPrint = currentStatus == allowedStatus;
                var reason = canPrint 
                    ? "Admin Akademik dapat cetak SK saat status 'Menunggu Upload SK'" 
                    : $"Admin Akademik hanya dapat cetak SK saat status 'Menunggu Upload SK', status saat ini: '{currentStatus}'";
                return (canPrint, reason, allowedStatus);
            }
            
            if (roleUpper == "ROL23" || roleUpper == "MAHASISWA")
            {
                var allowedStatus = "Disetujui";
                var canPrint = currentStatus == allowedStatus;
                var reason = canPrint 
                    ? "Mahasiswa dapat cetak SK saat status 'Disetujui'" 
                    : $"Mahasiswa hanya dapat cetak SK saat status 'Disetujui', status saat ini: '{currentStatus}'";
                return (canPrint, reason, allowedStatus);
            }

            return (false, $"Role '{role}' tidak memiliki akses untuk cetak SK Cuti Akademik", "");
        }

        private async Task<IActionResult> GenerateCetakSKResponse(string id, string username, string role, string format, dynamic cutiDetail, string reason)
        {
            var tokenData = $"{id}#{DateTime.Now}";
            var encryptedToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(tokenData));

            var skData = CreateSKData(cutiDetail);

            if (format.ToLower() == "pdf")
            {
                return GeneratePDFResponse(id, skData);
            }

            return GenerateJSONResponse(id, username, role, skData, cutiDetail, reason, encryptedToken);
        }

        private object CreateSKData(dynamic cutiDetail)
        {
            return new {
                id = cutiDetail.Id,
                noPengajuan = cutiDetail.Id,
                nim = cutiDetail.MhsId,
                namaMahasiswa = cutiDetail.Mahasiswa,
                konsentrasi = cutiDetail.Konsentrasi,
                angkatan = cutiDetail.Angkatan,
                tahunAjaran = cutiDetail.TahunAjaran,
                semester = cutiDetail.Semester,
                status = cutiDetail.Status,
                nomorSK = cutiDetail.SrtNo ?? "",
                tanggalSK = cutiDetail.TglPengajuan ?? "",
                approvalProdi = cutiDetail.ApprovalProdi ?? "",
                tanggalApprovalProdi = cutiDetail.AppProdiDate ?? "",
                approvalWadir1 = cutiDetail.ApprovalDir1 ?? "",
                tanggalApprovalWadir1 = cutiDetail.AppDir1Date ?? "",
                menimbang = cutiDetail.Menimbang ?? "",
                prodiNama = cutiDetail.ProdiNama ?? "",
                kaprodi = cutiDetail.Kaprodi ?? "",
                direktur = cutiDetail.Direktur ?? "",
                wadir1 = cutiDetail.Wadir1 ?? "",
                alamat = cutiDetail.Alamat ?? "",
                kodePos = cutiDetail.KodePos ?? "",
                createdBy = cutiDetail.CreatedBy,
                tglPengajuan = cutiDetail.TglPengajuan
            };
        }

        private IActionResult GeneratePDFResponse(string id, object skData)
        {
            var fileName = $"SK_Cuti_Akademik_{id.Replace("/", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var pdfContent = GeneratePDFContent(skData);
            var pdfBytes = System.Text.Encoding.UTF8.GetBytes(pdfContent);
            return File(pdfBytes, "application/pdf", fileName);
        }

        private IActionResult GenerateJSONResponse(string id, string username, string role, object skData, dynamic cutiDetail, string reason, string encryptedToken)
        {
            return Ok(new { 
                success = true,
                canPrint = true,
                message = "Data SK berhasil diambil dan siap untuk dicetak",
                data = skData,
                printInfo = new {
                    userRole = role,
                    username = username,
                    currentStatus = cutiDetail.Status,
                    reason = reason,
                    printTime = DateTime.Now
                },
                reportUrl = $"/Reports/SK_Cuti_Akademik.aspx?token={encryptedToken}",
                pdfUrl = $"/api/cutiakademik/cetak-sk/{id}?username={username}&role={role}&format=pdf",
                token = encryptedToken
            });
        }

        private string GeneratePDFContent(object skData)
        {
            // Use skData for future PDF generation enhancement
            var dataInfo = skData?.ToString() ?? "No data";
            
            return $@"%PDF-1.4
1 0 obj
<<
/Type /Catalog
/Pages 2 0 R
>>
endobj

2 0 obj
<<
/Type /Pages
/Kids [3 0 R]
/Count 1
>>
endobj

3 0 obj
<<
/Type /Page
/Parent 2 0 R
/MediaBox [0 0 612 792]
/Contents 4 0 R
/Resources <<
/Font <<
/F1 5 0 R
>>
>>
>>
endobj

4 0 obj
<<
/Length 500
>>
stream
BT
/F1 12 Tf
50 750 Td
(SURAT KETERANGAN CUTI AKADEMIK) Tj
0 -20 Td
(Nomor: SK_CUTI_AKADEMIK) Tj
0 -40 Td
(Yang bertanda tangan di bawah ini:) Tj
0 -20 Td
(Direktur Politeknik Astra) Tj
0 -40 Td
(Dengan ini menerangkan bahwa:) Tj
0 -20 Td
(Nama        : MAHASISWA) Tj
0 -20 Td
(NIM         : NIM_MAHASISWA) Tj
0 -20 Td
(Konsentrasi : KONSENTRASI) Tj
0 -20 Td
(Angkatan    : ANGKATAN) Tj
0 -40 Td
(Telah mengajukan cuti akademik) Tj
0 -40 Td
(Demikian surat keterangan ini dibuat untuk dapat dipergunakan sebagaimana mestinya.) Tj
0 -40 Td
(Diterbitkan pada: {DateTime.Now.ToString("dd MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture)}) Tj
0 -40 Td
(Direktur Politeknik Astra) Tj
0 -40 Td
([Tanda Tangan Digital]) Tj
ET
endstream
endobj

5 0 obj
<<
/Type /Font
/Subtype /Type1
/BaseFont /Helvetica
>>
endobj

xref
0 6
0000000000 65535 f 
0000000010 00000 n 
0000000079 00000 n 
0000000173 00000 n 
0000000301 00000 n 
0000000856 00000 n 
trailer
<<
/Size 6
/Root 1 0 R
>>
startxref
955
%%EOF";
        }
    }
}