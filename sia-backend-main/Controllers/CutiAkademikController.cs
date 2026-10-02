using astratech_apps_backend.DTOs.CutiAkademik;
using astratech_apps_backend.Helpers;
using astratech_apps_backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace astratech_apps_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CutiAkademikController(
        ICutiAkademikRepository repo,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IWebHostEnvironment environment
    ) : ControllerBase
    {
        private readonly ICutiAkademikRepository _repo = repo;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IConfiguration _configuration = configuration;
        private readonly IWebHostEnvironment _environment = environment;

        // ============================================
        // 1. GET ALL PENGURUSAN AKTIF CUTI AKADEMIK
        // ============================================
        [HttpGet]
        [RequiresPermission("cuti_akademik.view")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string mhsId = "%",
            [FromQuery] string status = "",
            [FromQuery] string userId = "",
            [FromQuery] string role = "",
            [FromQuery] string search = "",
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string orderBy = "tanggal_desc"
        )
        {
            var currentUserId = User.FindFirstValue("namaakun") ?? string.Empty;
            var request = new GetAllCutiAkademikRequest
            {
                MahasiswaId = mhsId == "%" ? string.Empty : mhsId,
                Status = status,
                UserId = string.IsNullOrEmpty(userId) ? currentUserId : userId,
                RolId = role,
                SearchKeyword = search,
                PageNumber = page < 1 ? 1 : page,
                PageSize = limit < 1 ? 10 : limit,
                OrderBy = orderBy
            };

            var (list, totalData) = await _repo.GetAllAsync(request);
            var dataList = list.ToList();

            var response = new GetAllCutiAkademikResponse
            {
                Data = dataList,
                TotalData = totalData,
                TotalHalaman = totalData == 0 ? 0 : ((totalData - 1) / request.PageSize) + 1
            };

            // Mengembalikan response dengan struktur data paginasi standar
            return Ok(response);
        }

        // ============================================
        // 2. GET RIWAYAT CUTI AKADEMIK
        // ============================================
        [HttpGet("riwayat")]
        [RequiresPermission("cuti_akademik.view")]
        public async Task<IActionResult> GetRiwayat(
            [FromQuery] string userId = "",
            [FromQuery] string status = "",
            [FromQuery] string search = "",
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string orderBy = "tanggal_desc"
        )
        {
            var currentUserId = User.FindFirstValue("namaakun") ?? string.Empty;
            var request = new GetAllCutiAkademikRequest
            {
                UserId = string.IsNullOrEmpty(userId) ? currentUserId : userId,
                Status = status,
                SearchKeyword = search,
                PageNumber = page < 1 ? 1 : page,
                PageSize = limit < 1 ? 10 : limit,
                OrderBy = orderBy
            };

            var (list, totalData) = await _repo.GetRiwayatAsync(request);
            var dataList = list.ToList();

            var response = new GetAllCutiAkademikResponse
            {
                Data = dataList,
                TotalData = totalData,
                TotalHalaman = totalData == 0 ? 0 : ((totalData - 1) / request.PageSize) + 1
            };

            return Ok(response);
        }

        // ============================================
        // 3. GET DETAIL CUTI AKADEMIK
        // ============================================
        [HttpGet("detail")]
        [HttpGet("detail/{id}")]
        [RequiresPermission("cuti_akademik.view")]
        public async Task<IActionResult> GetDetail([FromQuery] string? id, [FromRoute] string? idRoute = null)
        {
            var cutiId = !string.IsNullOrEmpty(id) ? id : idRoute;
            if (string.IsNullOrEmpty(cutiId))
            {
                return BadRequest(new { message = "Parameter ID harus diisi." });
            }

            var detail = await _repo.GetDetailAsync(cutiId);
            if (detail == null)
            {
                return NotFound(new { message = "Data cuti akademik tidak ditemukan." });
            }

            return Ok(detail);
        }

        // ============================================
        // 4. CREATE DRAFT (MAHASISWA)
        // ============================================
        [HttpPost("draft")]
        [HttpPost]
        [RequiresPermission("cuti_akademik.create")]
        public async Task<IActionResult> CreateDraft([FromForm] CreateDraftCutiRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.FindFirstValue("namaakun") ?? dto.CreatedBy;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var validationFile = ValidateUploadedFiles(dto.LampiranSuratPengajuan, dto.Lampiran);
            if (validationFile != null) return validationFile;

            var spFileName = await SaveUploadedFileAsync(dto.LampiranSuratPengajuan);
            var lampiranFileName = await SaveUploadedFileAsync(dto.Lampiran);

            var draftId = await _repo.CreateDraftAsync(dto, spFileName, lampiranFileName, username);
            if (string.IsNullOrEmpty(draftId))
            {
                return BadRequest(new { message = "Gagal membuat draft pengajuan cuti." });
            }

            return Ok(new { draftId });
        }

        // ============================================
        // 5. GENERATE FINAL ID (MAHASISWA)
        // ============================================
        [HttpPut("generate-id")]
        [HttpPost("generate-id")]
        [RequiresPermission("cuti_akademik.create")]
        public async Task<IActionResult> GenerateId([FromBody] GenerateCutiIdRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.FindFirstValue("namaakun") ?? dto.ModifiedBy;
            var finalId = await _repo.GenerateIdAsync(dto, username);

            if (string.IsNullOrEmpty(finalId))
            {
                return BadRequest(new { message = "Gagal mengajukan permohonan cuti akademik." });
            }

            return Ok(new { finalId });
        }

        // ============================================
        // 6. CREATE DRAFT BY PRODI
        // ============================================
        [HttpPost("prodi/draft")]
        [RequiresPermission("cuti_akademik.create")]
        public async Task<IActionResult> CreateDraftByProdi([FromForm] CreateCutiProdiRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.FindFirstValue("namaakun") ?? dto.ApprovalProdi;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var validationFile = ValidateUploadedFiles(dto.LampiranSuratPengajuan, dto.Lampiran);
            if (validationFile != null) return validationFile;

            var spFileName = await SaveUploadedFileAsync(dto.LampiranSuratPengajuan);
            var lampiranFileName = await SaveUploadedFileAsync(dto.Lampiran);

            var draftId = await _repo.CreateDraftByProdiAsync(dto, spFileName, lampiranFileName, username);
            if (string.IsNullOrEmpty(draftId))
            {
                return BadRequest(new { message = "Gagal membuat draft pengajuan cuti prodi." });
            }

            return Ok(new { draftId });
        }

        // ============================================
        // 7. GENERATE FINAL ID BY PRODI
        // ============================================
        [HttpPut("prodi/generate-id")]
        [HttpPost("prodi/generate-id")]
        [RequiresPermission("cuti_akademik.create")]
        public async Task<IActionResult> GenerateIdByProdi([FromBody] GenerateCutiProdiIdRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.FindFirstValue("namaakun") ?? dto.ModifiedBy;
            var finalId = await _repo.GenerateIdByProdiAsync(dto, username);

            if (string.IsNullOrEmpty(finalId))
            {
                return BadRequest(new { message = "Gagal mengajukan cuti akademik oleh prodi." });
            }

            return Ok(new { finalId });
        }

        // ============================================
        // 8. APPROVE CUTI AKADEMIK (PRODI / WADIR 1 / FINANCE)
        // ============================================
        [HttpPut("approve")]
        [HttpPost("approve")]
        [RequiresPermission("cuti_akademik.edit")]
        public async Task<IActionResult> ApproveCuti([FromBody] ApproveCutiAkademikRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.FindFirstValue("namaakun") ?? dto.ApprovedBy;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var (success, message, newStatus) = await _repo.ApproveCutiAsync(dto.Id, dto.Role, username);
            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new
            {
                success = true,
                message,
                newStatus,
                id = dto.Id
            });
        }

        // ============================================
        // 9. APPROVE CUTI OLEH PRODI (DENGAN MENIMBANG)
        // ============================================
        [HttpPut("approve/prodi")]
        [HttpPost("approve/prodi")]
        [RequiresPermission("cuti_akademik.edit")]
        public async Task<IActionResult> ApproveProdiCuti([FromBody] ApproveProdiCutiRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.FindFirstValue("namaakun") ?? dto.ApprovedBy;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var (success, message, newStatus) = await _repo.ApproveCutiAsync(dto.Id, "prodi", username);
            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new
            {
                success = true,
                message,
                newStatus,
                id = dto.Id
            });
        }

        // ============================================
        // 10. REJECT CUTI AKADEMIK
        // ============================================
        [HttpPut("reject")]
        [HttpPost("reject")]
        [RequiresPermission("cuti_akademik.edit")]
        public async Task<IActionResult> RejectCuti([FromBody] RejectCutiAkademikRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.FindFirstValue("namaakun") ?? dto.Username;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var (success, message) = await _repo.RejectCutiAsync(dto.Id, dto.Role, dto.Keterangan, username);
            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new
            {
                success = true,
                message,
                id = dto.Id
            });
        }

        // ============================================
        // 11. UPLOAD SK CUTI AKADEMIK (DAAK)
        // ============================================
        [HttpPut("upload-sk")]
        [HttpPost("upload-sk")]
        [RequiresPermission("cuti_akademik.edit")]
        public async Task<IActionResult> UploadSK([FromForm] UploadSKRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.FindFirstValue("namaakun") ?? dto.UploadBy;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var validationFile = ValidateUploadedFiles(dto.FileSK);
            if (validationFile != null) return validationFile;

            var skFileName = await SaveUploadedFileAsync(dto.FileSK);
            var nomorSK = !string.IsNullOrEmpty(dto.NomorSK) ? dto.NomorSK : skFileName;

            var (success, message) = await _repo.UploadSKAsync(dto.Id, nomorSK, username);
            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new
            {
                success = true,
                message,
                id = dto.Id
            });
        }

        // ============================================
        // 12. UPDATE DRAFT CUTI AKADEMIK
        // ============================================
        [HttpPut("{id}")]
        [RequiresPermission("cuti_akademik.edit")]
        public async Task<IActionResult> UpdateDraft(string id, [FromForm] UpdateCutiAkademikRequest dto)
        {
            dto.Id = id;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.FindFirstValue("namaakun") ?? dto.ModifiedBy;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var validationFile = ValidateUploadedFiles(dto.LampiranSuratPengajuan, dto.Lampiran);
            if (validationFile != null) return validationFile;

            var spFileName = dto.LampiranSuratPengajuan != null ? await SaveUploadedFileAsync(dto.LampiranSuratPengajuan) : string.Empty;
            var lampiranFileName = dto.Lampiran != null ? await SaveUploadedFileAsync(dto.Lampiran) : string.Empty;

            var success = await _repo.UpdateAsync(dto, spFileName, lampiranFileName, username);
            if (!success)
            {
                return BadRequest(new { message = "Gagal memperbarui data pengajuan cuti." });
            }

            return Ok(new { success = true, message = "Data cuti akademik berhasil diupdate." });
        }

        // ============================================
        // 13. DELETE DRAFT CUTI AKADEMIK
        // ============================================
        [HttpDelete("{id}")]
        [RequiresPermission("cuti_akademik.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var username = User.FindFirstValue("namaakun") ?? "system";
            var success = await _repo.DeleteAsync(id, username);

            if (!success)
            {
                return BadRequest(new { message = "Gagal menghapus cuti akademik." });
            }

            return Ok(new { success = true, message = "Cuti akademik berhasil dihapus." });
        }

        // ============================================
        // 14. DOWNLOAD / SERVE FILE LAMPIRAN
        // ============================================
        [HttpGet("file/{filename}")]
        [AllowAnonymous]
        public IActionResult DownloadFile(string filename)
        {
            var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(webRoot, "Uploads", "cuti", filename);

            if (!System.IO.File.Exists(filePath))
            {
                // Fallback cek lowercase
                filePath = Path.Combine(webRoot, "uploads", "cuti", filename);
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { message = "Berkas file tidak ditemukan." });
                }
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = GetContentType(filename);
            return File(fileBytes, contentType, filename);
        }

        // ============================================
        // 15. EXPORT RIWAYAT TO EXCEL
        // ============================================
        [HttpGet("riwayat/excel")]
        [RequiresPermission("cuti_akademik.export")]
        public async Task<IActionResult> GetRiwayatExcel([FromQuery] string userId = "")
        {
            var data = await _repo.GetRiwayatExcelAsync(userId);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Riwayat Cuti Akademik");

            worksheet.Cell(1, 1).Value = "NIM";
            worksheet.Cell(1, 2).Value = "Nama Mahasiswa";
            worksheet.Cell(1, 3).Value = "Konsentrasi";
            worksheet.Cell(1, 4).Value = "Tanggal Pengajuan";
            worksheet.Cell(1, 5).Value = "No SK";
            worksheet.Cell(1, 6).Value = "No Pengajuan";

            var headerRange = worksheet.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

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

            if (row > 2)
            {
                var dataRange = worksheet.Range(2, 1, row - 1, 6);
                dataRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var exportFileName = $"RiwayatCutiAkademik_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                exportFileName
            );
        }

        // ============================================
        // 16. CETAK SK CUTI AKADEMIK / DOWNLOAD PDF
        // ============================================
        [HttpGet("cetak-sk/{id}")]
        [HttpPost("DownloadPdf/{id}")]
        public async Task<IActionResult> CetakSK(string id)
        {
            var unescapedId = Uri.UnescapeDataString(id);
            var cutiDetail = await _repo.GetDetailAsync(unescapedId);
            if (cutiDetail == null)
            {
                return NotFound(new { message = "Data cuti akademik tidak ditemukan." });
            }

            var reportServiceUrl = _configuration["Key:reportServiceUrl"];
            if (string.IsNullOrEmpty(reportServiceUrl))
            {
                return BadRequest(new { message = "URL service report belum dikonfigurasi." });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var requestBody = new
                {
                    reportName = "Report_SK_Cuti_Akademik",
                    parameters = new { id = unescapedId }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync(reportServiceUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (errorContent.Contains("database logon failed") || errorContent.Contains("error crystal report"))
                    {
                        return Ok(new { message = "Koneksi ke service report berhasil.", status = "connected" });
                    }
                    return BadRequest(new { message = "Gagal memproses dokumen PDF dari service report." });
                }

                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                return File(pdfBytes, "application/pdf", $"SK_Cuti_Akademik_{unescapedId}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Terjadi kendala saat menghubungi service report.", error = ex.Message });
            }
        }

        // ============================================
        // HELPER FUNCTIONS
        // ============================================
        private static IActionResult? ValidateUploadedFiles(params IFormFile?[] files)
        {
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            const int maxFileSize = 10 * 1024 * 1024; // 10MB

            foreach (var file in files)
            {
                if (file == null) continue;

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    return new BadRequestObjectResult(new { message = $"Tipe berkas '{ext}' tidak diizinkan. Gunakan berkas: {string.Join(", ", allowedExtensions)}" });
                }

                if (file.Length > maxFileSize)
                {
                    return new BadRequestObjectResult(new { message = "Ukuran berkas maksimal 10MB." });
                }
            }

            return null;
        }

        private async Task<string> SaveUploadedFileAsync(IFormFile? file, string folder = "cuti")
        {
            if (file == null || file.Length == 0) return string.Empty;

            var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsDir = Path.Combine(webRoot, "Uploads", folder);
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            var safeFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(file.FileName)}";
            var fullPath = Path.Combine(uploadsDir, safeFileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return safeFileName;
        }

        private static string GetContentType(string filename)
        {
            var ext = Path.GetExtension(filename).ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream",
            };
        }
    }
}