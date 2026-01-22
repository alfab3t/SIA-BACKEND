using astratech_apps_backend.DTOs.CutiAkademik;
using astratech_apps_backend.Services.Interfaces;
using astratech_apps_backend.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace astratech_apps_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CutiAkademikController : ControllerBase
    {
        private readonly ICutiAkademikService _service;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CutiAkademikController(ICutiAkademikService service, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _service = service;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // ============================================
        // CREATE DRAFT (Mahasiswa)
        // ============================================
        [HttpPost("CreateDraftCutiAkademik")]
        [RequiresPermission("cuti_akademik.create")]
        public async Task<IActionResult> CreateDraftCutiAkademik([FromForm] CreateDraftCutiRequest dto)
        {
            // File validation constants
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            const int maxFileSize = 10 * 1024 * 1024; // 10MB

            // Validate file types - MS Word documents not allowed
            if (dto.LampiranSuratPengajuan != null)
            {
                var fileExtension = Path.GetExtension(dto.LampiranSuratPengajuan.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { message = $"Tipe file lampiran surat pengajuan tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" });
                }

                // Validate file size (max 10MB)
                if (dto.LampiranSuratPengajuan.Length > maxFileSize)
                {
                    return BadRequest(new { message = "Ukuran file lampiran surat pengajuan maksimal 10MB." });
                }
            }

            if (dto.Lampiran != null)
            {
                var fileExtension = Path.GetExtension(dto.Lampiran.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { message = $"Tipe file lampiran tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" });
                }

                // Validate file size (max 10MB)
                if (dto.Lampiran.Length > maxFileSize)
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
        [HttpPut("GenerateFinalIdCutiAkademik")]
        [RequiresPermission("cuti_akademik.create")]
        public async Task<IActionResult> GenerateFinalIdCutiAkademik([FromBody] GenerateCutiIdRequest dto)
        {
            var id = await _service.GenerateIdAsync(dto);

            if (id == null)
                return BadRequest(new { message = "Gagal generate ID final." });

            return Ok(new { finalId = id });
        }

        // ============================================
        // CREATE DRAFT (Prodi)
        // ============================================
        [HttpPost("CreateDraftCutiAkademikByProdi")]
        [RequiresPermission("cuti_akademik.create")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateDraftCutiAkademikByProdi([FromForm] CreateCutiProdiRequest dto)
        {
            try
            {
                // File validation constants
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                const int maxFileSize = 10 * 1024 * 1024; // 10MB

                // Validate file types - MS Word documents not allowed
                if (dto.LampiranSuratPengajuan != null)
                {
                    var fileExtension = Path.GetExtension(dto.LampiranSuratPengajuan.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = $"Tipe file lampiran surat pengajuan tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" });
                    }

                    // Validate file size (max 10MB)
                    if (dto.LampiranSuratPengajuan.Length > maxFileSize)
                    {
                        return BadRequest(new { message = "Ukuran file lampiran surat pengajuan maksimal 10MB." });
                    }
                }

                if (dto.Lampiran != null)
                {
                    var fileExtension = Path.GetExtension(dto.Lampiran.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = $"Tipe file lampiran tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" });
                    }

                    // Validate file size (max 10MB)
                    if (dto.Lampiran.Length > maxFileSize)
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
                return BadRequest(new
                {
                    message = "Terjadi kesalahan saat membuat draft.",
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = "Operasi tidak valid saat membuat draft.",
                    error = ex.Message
                });
            }
        }

        // ============================================
        // GENERATE FINAL ID (Prodi)
        // ============================================
        [HttpPut("GenerateFinalIdCutiAkademikByProdi")]
        [RequiresPermission("cuti_akademik.create")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GenerateFinalIdCutiAkademikByProdi([FromBody] GenerateCutiProdiIdRequest dto)
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
                return BadRequest(new
                {
                    message = "Terjadi kesalahan saat generate final ID.",
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = "Operasi tidak valid saat generate final ID.",
                    error = ex.Message
                });
            }
        }

        // ============================================
        // GET ALL CUTI
        // ============================================
        [HttpGet("GetAllCutiAkademik")]
        [RequiresPermission("cuti_akademik.view")]
        [ProducesResponseType(typeof(IEnumerable<CutiAkademikListResponse>), 200)]
        public async Task<IActionResult> GetAllCutiAkademik(
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
        [HttpGet("GetDetailCutiAkademik")]
        [RequiresPermission("cuti_akademik.view")]
        public async Task<IActionResult> GetDetailCutiAkademik([FromQuery] string id)
        {
            var data = await _service.GetDetailAsync(id);

            if (data == null)
                return NotFound(new { message = "Detail Cuti Akademik tidak ditemukan." });

            return Ok(data);
        }

        // ============================================
        // UPDATE CUTI (WITH FILE UPLOAD)
        // ============================================
        [HttpPut("UpdateCutiAkademik/{id}")]
        [RequiresPermission("cuti_akademik.edit")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateCutiAkademik(string id, [FromForm] UpdateCutiAkademikRequest dto)
        {
            try
            {
                // File validation constants
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                const int maxFileSize = 10 * 1024 * 1024; // 10MB

                // Validate file types - MS Word documents not allowed
                if (dto.LampiranSuratPengajuan != null)
                {
                    var fileExtension = Path.GetExtension(dto.LampiranSuratPengajuan.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = $"Tipe file lampiran surat pengajuan tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" });
                    }

                    // Validate file size (max 10MB)
                    if (dto.LampiranSuratPengajuan.Length > maxFileSize)
                    {
                        return BadRequest(new { message = "Ukuran file lampiran surat pengajuan maksimal 10MB." });
                    }
                }

                if (dto.Lampiran != null)
                {
                    var fileExtension = Path.GetExtension(dto.Lampiran.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = $"Tipe file lampiran tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" });
                    }

                    // Validate file size (max 10MB)
                    if (dto.Lampiran.Length > maxFileSize)
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
                return BadRequest(new
                {
                    message = "Terjadi kesalahan saat mengupdate data.",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = "Operasi tidak valid saat mengupdate data.",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        // ============================================
        // SOFT DELETE
        // ============================================
        [HttpDelete("DeleteCutiAkademik/{id}")]
        [RequiresPermission("cuti_akademik.delete")]
        public async Task<IActionResult> DeleteCutiAkademik(string id)
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
        [HttpGet("GetRiwayatCutiAkademik")]
        [RequiresPermission("cuti_akademik.view")]
        [ProducesResponseType(typeof(IEnumerable<CutiAkademikListResponse>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRiwayatCutiAkademik(
            [FromQuery] string userId = "",
            [FromQuery] string status = "",
            [FromQuery] string search = "")
        {
            var result = await _service.GetRiwayatAsync(userId, status, search);
            return Ok(result);
        }

        [HttpGet("ExportRiwayatCutiAkademikToExcel")]
        [RequiresPermission("cuti_akademik.export")]
        [ProducesResponseType(typeof(FileResult), 200)]
        public async Task<IActionResult> ExportRiwayatCutiAkademikToExcel([FromQuery] string userId = "")
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
                return BadRequest(new
                {
                    message = "Terjadi kesalahan saat membuat file Excel.",
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = "Operasi tidak valid saat membuat file Excel.",
                    error = ex.Message
                });
            }
        }

        // ============================================
        // DOWNLOAD FILE
        // ============================================
        [HttpGet("DownloadFileCutiAkademik/{filename}")]
        [RequiresPermission("cuti_akademik.print")]
        public IActionResult DownloadFileCutiAkademik(string filename)
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
        [HttpPut("ApproveCutiAkademik")]
        [RequiresPermission("cuti_akademik.approve_reject")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ApproveCutiAkademik([FromBody] ApproveCutiAkademikRequest dto)
        {
            try
            {
                // Auto-detect role based on username using stored procedure
                var detectedRole = await _service.DetectUserRoleAsync(dto.ApprovedBy);
                if (string.IsNullOrEmpty(detectedRole))
                {
                    return BadRequest(new
                    {
                        message = "Tidak dapat mendeteksi role pengguna. Pastikan username valid.",
                        username = dto.ApprovedBy
                    });
                }
                
                // Override role dengan hasil deteksi
                dto.Role = detectedRole;
                
                var success = await _service.ApproveCutiAsync(dto);
                
                if (success)
                {
                    return Ok(new
                    {
                        approved = true,
                        id = dto.Id,
                        approvedBy = dto.ApprovedBy,
                        role = detectedRole,
                        message = $"Cuti akademik berhasil disetujui oleh {detectedRole}"
                    });
                }
                
                return BadRequest(new
                {
                    message = "Gagal menyetujui cuti akademik. Data mungkin tidak ditemukan atau sudah diproses.",
                    id = dto.Id,
                    detectedRole = detectedRole,
                    username = dto.ApprovedBy
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = "Terjadi kesalahan saat menyetujui cuti akademik.",
                    error = ex.Message,
                    id = dto.Id
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = "Operasi tidak valid saat menyetujui cuti akademik.",
                    error = ex.Message,
                    id = dto.Id
                });
            }
        }

        [HttpPut("ApproveCutiAkademikByProdi")]
        [RequiresPermission("cuti_akademik.approve_reject")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ApproveCutiAkademikByProdi([FromBody] ApproveProdiCutiRequest dto)
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
                return BadRequest(new
                {
                    message = "Terjadi kesalahan saat menyetujui cuti akademik.",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = "Operasi tidak valid saat menyetujui cuti akademik.",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        [HttpPut("RejectCutiAkademik")]
        [RequiresPermission("cuti_akademik.approve_reject")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RejectCutiAkademik([FromBody] RejectCutiAkademikRequest dto)
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
                    return BadRequest(new
                    {
                        message = "Tidak dapat mendeteksi role pengguna. Pastikan username valid.",
                        username = dto.Username
                    });
                }
                
                // Override role dengan hasil deteksi
                dto.Role = detectedRole;
                
                var success = await _service.RejectCutiAsync(dto);
                
                if (success)
                {
                    return Ok(new
                    {
                        rejected = true,
                        id = dto.Id,
                        rejectedBy = dto.Username,
                        role = detectedRole,
                        message = $"Cuti akademik berhasil ditolak oleh {detectedRole}"
                    });
                }
                
                return BadRequest(new
                {
                    message = "Gagal menolak cuti akademik. Data mungkin tidak ditemukan atau sudah diproses.",
                    id = dto.Id,
                    detectedRole = detectedRole,
                    username = dto.Username
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = "Terjadi kesalahan saat menolak cuti akademik.",
                    error = ex.Message,
                    id = dto.Id
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = "Operasi tidak valid saat menolak cuti akademik.",
                    error = ex.Message,
                    id = dto.Id
                });
            }
        }

        // ============================================
        // SK MANAGEMENT ENDPOINTS
        // ============================================
        [HttpPut("UploadSKCutiAkademik")]
        [RequiresPermission("cuti_akademik.import")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UploadSKCutiAkademik([FromForm] UploadSKRequest dto)
        {
            try
            {
                // File validation constants
                var allowedFileExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                const int maxFileSize = 10 * 1024 * 1024; // 10MB
                const string idRequiredMessage = "ID cuti akademik harus diisi.";
                const string fileSizeErrorMessage = "Ukuran file maksimal 10MB.";

                // Validate input
                if (string.IsNullOrEmpty(dto.Id))
                {
                    return BadRequest(new { message = idRequiredMessage });
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
                var fileExtension = Path.GetExtension(dto.FileSK.FileName).ToLowerInvariant();
                
                if (!allowedFileExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { message = $"Tipe file tidak diizinkan. Gunakan: {string.Join(", ", allowedFileExtensions)}" });
                }

                // Validate file size (max 10MB)
                if (dto.FileSK.Length > maxFileSize)
                {
                    return BadRequest(new { message = fileSizeErrorMessage });
                }
                
                var success = await _service.UploadSKAsync(dto);
                
                if (success)
                {
                    return Ok(new
                    {
                        message = "SK berhasil diupload. Status cuti akademik telah diubah menjadi 'Disetujui'. Nomor SK akan ditampilkan otomatis di daftar.",
                        success = true,
                        id = dto.Id
                    });
                }
                
                return BadRequest(new { message = "Gagal mengupload SK. Periksa apakah ID valid dan status cuti adalah 'Menunggu Upload SK'." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = "Terjadi kesalahan saat mengupload SK.",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = "Operasi tidak valid saat mengupload SK.",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        // ============================================
        // CETAK SK CUTI AKADEMIK ENDPOINT  
        // ============================================
        [HttpPost("DownloadPdfSKCutiAkademik/{id}")]
        [RequiresPermission("cuti_akademik.print")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DownloadPdfSKCutiAkademik(string id, [FromQuery] string username, [FromQuery] string role)
        {
            try
            {
                // Decode URL jika perlu
                id = Uri.UnescapeDataString(id);

                // Validasi input parameters
                var validationResult = ValidateDownloadPdfParameters(username, role);
                if (validationResult != null) return validationResult;

                // Ambil detail cuti akademik untuk validasi status
                var cutiDetail = await _service.GetDetailAsync(id);
                if (cutiDetail == null)
                {
                    return NotFound(new
                    {
                        message = "Data cuti akademik tidak ditemukan",
                        id = id,
                        username = username,
                        role = role
                    });
                }

                // Validasi berdasarkan role dan status
                var roleValidationResult = ValidateRoleAndStatus(role, cutiDetail.Status);
                if (roleValidationResult != null) return roleValidationResult;

                // Call service report
                return await CallReportService(id, username, role);
            }
            catch (HttpRequestException ex)
            {
                return BadRequest(new
                {
                    message = "Terjadi kesalahan saat download PDF SK Cuti Akademik.",
                    error = ex.Message,
                    id = id,
                    username = username,
                    role = role
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Terjadi kesalahan saat download PDF SK Cuti Akademik.",
                    error = ex.Message,
                    id = id,
                    username = username,
                    role = role
                });
            }
        }

        private IActionResult? ValidateDownloadPdfParameters(string username, string role)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new
                {
                    message = "Parameter username harus diisi",
                    username = username
                });
            }

            if (string.IsNullOrEmpty(role))
            {
                return BadRequest(new
                {
                    message = "Parameter role harus diisi",
                    username = username,
                    role = role
                });
            }

            return null;
        }

        private IActionResult? ValidateRoleAndStatus(string role, string? status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return BadRequest(new
                {
                    message = "Status tidak ditemukan",
                    role = role
                });
            }

            return role switch
            {
                "ROL23" when status != "Disetujui" => StatusCode(403, new
                {
                    message = "Mahasiswa hanya dapat cetak SK saat status 'Disetujui'",
                    currentStatus = status,
                    requiredStatus = "Disetujui",
                    role = role
                }),
                "ROL21" when status != "Menunggu Upload SK" => StatusCode(403, new
                {
                    message = "Admin Akademik hanya dapat cetak SK saat status 'Menunggu Upload SK'",
                    currentStatus = status,
                    requiredStatus = "Menunggu Upload SK",
                    role = role
                }),
                "ROL23" or "ROL21" => null,
                _ => StatusCode(403, new
                {
                    message = "Role tidak memiliki akses untuk cetak SK",
                    role = role,
                    allowedRoles = new[] { "ROL23", "ROL21" }
                })
            };
        }

        private async Task<IActionResult> CallReportService(string id, string username, string role)
        {
            var client = _httpClientFactory.CreateClient();
            var url = _configuration["Key:reportServiceUrl"];

            if (string.IsNullOrEmpty(url))
            {
                return BadRequest(new
                {
                    message = "URL service report tidak dikonfigurasi",
                    id = id,
                    username = username,
                    role = role
                });
            }

            var requestBody = new
            {
                reportName = "Report_SK_Cuti_Akademik",
                parameters = new { id }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
                    // Check for specific database or Crystal Report errors
                    if (errorContent.Contains("database logon failed") ||
                        errorContent.Contains("error crystal report"))
                    {
                        return BadRequest(new
                        {
                            message = "Service report berhasil terhubung namun terjadi error database/crystal report",
                            error = errorContent,
                            connectionStatus = "Connected - Database/Crystal Report Error",
                            id = id,
                            username = username,
                            role = role
                        });
                    }
                    
                    return BadRequest(new
                    {
                        message = "Gagal mengambil file PDF dari service report",
                        error = errorContent,
                        statusCode = (int)response.StatusCode,
                        id = id,
                        username = username,
                        role = role
                    });
                }

                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                return File(pdfBytes, "application/pdf", $"SK_Cuti_Akademik_{id.Replace("/", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (HttpRequestException ex)
            {
                return BadRequest(new
                {
                    message = "Terjadi kesalahan saat download PDF SK Cuti Akademik.",
                    error = ex.Message,
                    id = id,
                    username = username,
                    role = role
                });
            }
        }
    }
}