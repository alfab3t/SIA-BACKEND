using astratech_apps_backend.DTOs.MeninggalDunia;
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
    public class MeninggalDuniaController(
        IMeninggalDuniaRepository repo,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IWebHostEnvironment environment
    ) : ControllerBase
    {
        private readonly IMeninggalDuniaRepository _repo = repo;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IConfiguration _configuration = configuration;
        private readonly IWebHostEnvironment _environment = environment;

        // ============================================
        // 1. GET ALL MENINGGAL DUNIA (PENGAJUAN AKTIF)
        // ============================================
        [HttpGet("GetAllMeninggalDunia")]
        [RequiresPermission("meninggal_dunia.view")]
        public async Task<IActionResult> GetAllMeninggalDunia([FromQuery] GetAllMeninggalDuniaRequest req)
        {
            if (string.IsNullOrEmpty(req.UserId))
            {
                req.UserId = User.FindFirstValue("namaakun") ?? string.Empty;
            }

            var (list, totalData) = await _repo.GetAllAsync(req);
            var dataList = list.ToList();

            var response = new GetAllMeninggalDuniaResponse
            {
                Data = dataList,
                TotalData = totalData,
                TotalHalaman = req.PageSize <= 0 ? 1 : ((totalData - 1) / req.PageSize) + 1
            };

            return Ok(response);
        }

        // ============================================
        // 2. GET RIWAYAT MENINGGAL DUNIA
        // ============================================
        [HttpGet("RiwayatMeninggalDunia")]
        [RequiresPermission("meninggal_dunia.view")]
        public async Task<IActionResult> GetRiwayatMeninggalDunia([FromQuery] GetRiwayatMeninggalDuniaRequest req)
        {
            var (list, totalData) = await _repo.GetRiwayatAsync(req);
            var dataList = list.ToList();

            var response = new GetRiwayatMeninggalDuniaResponse
            {
                Data = dataList,
                TotalData = totalData,
                TotalHalaman = req.PageSize <= 0 ? 1 : ((totalData - 1) / req.PageSize) + 1
            };

            return Ok(response);
        }

        // ============================================
        // 3. GET DETAIL MENINGGAL DUNIA
        // ============================================
        [HttpGet("DetailMeninggalDunia/{id}")]
        [RequiresPermission("meninggal_dunia.view")]
        public async Task<IActionResult> GetDetailMeninggalDunia(string id)
        {
            var unescapedId = Uri.UnescapeDataString(id);
            var data = await _repo.GetDetailAsync(unescapedId);

            if (data == null)
            {
                return NotFound(new { message = $"Data dengan ID '{unescapedId}' tidak ditemukan." });
            }

            return Ok(data);
        }

        // ============================================
        // 4. CREATE DRAFT (PENGAJUAN MENINGGAL DUNIA)
        // ============================================
        [HttpPost("CreateDraftMeninggalDunia")]
        [RequiresPermission("meninggal_dunia.create")]
        public async Task<IActionResult> CreateMeninggalDunia([FromForm] CreateMeninggalDuniaRequest dto)
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

            var validationFile = ValidateUploadedFiles(dto.LampiranFile);
            if (validationFile != null) return validationFile;

            var safeFileName = await SaveUploadedFileAsync(dto.LampiranFile);
            var id = await _repo.CreateAsync(dto, safeFileName, username);

            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "Gagal membuat draft pengajuan meninggal dunia." });
            }

            return Ok(new { id });
        }

        // ============================================
        // 5. FINALIZE DRAFT -> PENGAJUAN RESMI
        // ============================================
        [HttpPost("FinalizeMeninggalDunia/{draftId}")]
        [RequiresPermission("meninggal_dunia.create")]
        public async Task<IActionResult> FinalizeMeninggalDunia(string draftId)
        {
            var username = User.FindFirstValue("namaakun") ?? "system";
            var officialId = await _repo.FinalizeAsync(draftId, username);

            if (string.IsNullOrEmpty(officialId))
            {
                return BadRequest(new
                {
                    message = "Gagal memfinalisasi draft pengajuan meninggal dunia.",
                    draftId
                });
            }

            return Ok(new
            {
                message = "Draft berhasil difinalisasi menjadi pengajuan resmi.",
                draftId,
                officialId,
                updatedBy = username
            });
        }

        // ============================================
        // 6. UPDATE MENINGGAL DUNIA
        // ============================================
        [HttpPut("EditMeninggalDunia/{id}")]
        [RequiresPermission("meninggal_dunia.edit")]
        public async Task<IActionResult> UpdateMeninggalDunia(string id, [FromForm] UpdateMeninggalDuniaRequest dto)
        {
            dto.Id = id;
            var username = User.FindFirstValue("namaakun") ?? dto.ModifiedBy;

            var validationFile = ValidateUploadedFiles(dto.LampiranFile);
            if (validationFile != null) return validationFile;

            var safeFileName = dto.LampiranFile != null ? await SaveUploadedFileAsync(dto.LampiranFile) : dto.Lampiran;
            var success = await _repo.UpdateAsync(id, safeFileName, username);

            if (!success)
            {
                return BadRequest(new { message = "Gagal memperbarui data pengajuan meninggal dunia.", id });
            }

            return Ok(new
            {
                message = "Data berhasil diperbarui.",
                id,
                updatedBy = username
            });
        }

        // ============================================
        // 7. SOFT DELETE MENINGGAL DUNIA
        // ============================================
        [HttpDelete("DeleteMeninggalDunia/{id}")]
        [RequiresPermission("meninggal_dunia.delete")]
        public async Task<IActionResult> DeleteMeninggalDunia(string id)
        {
            var username = User.FindFirstValue("namaakun") ?? "system";
            var result = await _repo.SoftDeleteAsync(id, username);

            if (!result)
            {
                return BadRequest(new { message = "Gagal menghapus data pengajuan meninggal dunia." });
            }

            return Ok(new { message = "Data meninggal dunia berhasil dihapus (soft delete)." });
        }

        // ============================================
        // 8. APPROVE MENINGGAL DUNIA (WADIR 1)
        // ============================================
        [HttpPut("ApproveMeninggalDunia/{id}")]
        [RequiresPermission("meninggal_dunia.approve_reject")]
        public async Task<IActionResult> ApproveMeninggalDunia(string id, [FromBody] ApproveMeninggalDuniaRequest dto)
        {
            var unescapedId = Uri.UnescapeDataString(id);
            var username = User.FindFirstValue("namaakun") ?? dto.Username;

            var role = !string.IsNullOrEmpty(dto.Role) ? dto.Role : "wadir1";
            var result = await _repo.ApproveAsync(unescapedId, role, username);

            if (!result)
            {
                return BadRequest(new { message = "Gagal menyetujui pengajuan meninggal dunia.", id = unescapedId });
            }

            return Ok(new
            {
                approved = true,
                id = unescapedId,
                approvedBy = username,
                message = "Pengajuan meninggal dunia berhasil disetujui."
            });
        }

        // ============================================
        // 9. REJECT MENINGGAL DUNIA
        // ============================================
        [HttpPut("RejectMeninggalDunia/{id}")]
        [RequiresPermission("meninggal_dunia.approve_reject")]
        public async Task<IActionResult> RejectMeninggalDunia(string id, [FromBody] RejectMeninggalDuniaRequest dto)
        {
            var unescapedId = Uri.UnescapeDataString(id);
            var username = User.FindFirstValue("namaakun") ?? dto.Username;

            var role = !string.IsNullOrEmpty(dto.Role) ? dto.Role : "Wadir 1";
            var result = await _repo.RejectAsync(unescapedId, role, username);

            if (!result)
            {
                return BadRequest(new { message = "Gagal menolak pengajuan meninggal dunia.", id = unescapedId });
            }

            return Ok(new
            {
                rejected = true,
                id = unescapedId,
                rejectedBy = username,
                message = "Pengajuan meninggal dunia berhasil ditolak."
            });
        }

        // ============================================
        // 10. UPLOAD SK & SPKB MENINGGAL DUNIA (DAAK)
        // ============================================
        [HttpPut("UploadSKMeninggalDunia")]
        [RequiresPermission("meninggal_dunia.import")]
        public async Task<IActionResult> UploadSKMeninggalDunia([FromForm] UploadSKMeninggalRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.FindFirstValue("namaakun") ?? request.ModifiedBy;
            var validationFile = ValidateUploadedFiles(request.SK, request.SKPB);
            if (validationFile != null) return validationFile;

            var skFileName = await SaveUploadedFileAsync(request.SK);
            var spkbFileName = await SaveUploadedFileAsync(request.SKPB);

            var result = await _repo.UploadSKAsync(request.MduId, skFileName, spkbFileName, username);
            if (!result)
            {
                return BadRequest(new { message = "Gagal mengunggah SK Meninggal Dunia." });
            }

            return Ok(new
            {
                message = "Upload SK berhasil. Status meninggal dunia telah diubah menjadi 'Disetujui'.",
                success = true,
                mduId = request.MduId,
                modifiedBy = username
            });
        }

        // ============================================
        // 11. EXPORT RIWAYAT TO EXCEL
        // ============================================
        [HttpGet("ExportExcelRiwayatMeninggalDunia")]
        [RequiresPermission("meninggal_dunia.export")]
        public async Task<IActionResult> ExportRiwayatMeninggalDuniaToExcel(
            [FromQuery] string sort = "",
            [FromQuery] string konsentrasi = "")
        {
            var data = await _repo.GetRiwayatExcelAsync(sort, konsentrasi);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Riwayat Meninggal Dunia");

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

            var fileName = $"RiwayatMeninggalDunia_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        // ============================================
        // 12. DOWNLOAD FILE LAMPIRAN
        // ============================================
        [HttpGet("DownloadFileMeninggalDunia/{filename}")]
        [RequiresPermission("meninggal_dunia.export")]
        public IActionResult DownloadFileMeninggalDunia(string filename)
        {
            var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var possiblePaths = new[]
            {
                Path.Combine(webRoot, "Uploads", "meninggal", filename),
                Path.Combine(webRoot, "uploads", "meninggal", filename),
                Path.Combine(webRoot, "uploads", "meninggal", "lampiran", filename)
            };

            var foundPath = possiblePaths.FirstOrDefault(System.IO.File.Exists);
            if (foundPath == null)
            {
                return NotFound(new { message = "Berkas file tidak ditemukan.", filename });
            }

            var fileBytes = System.IO.File.ReadAllBytes(foundPath);
            var contentType = GetContentType(filename);
            return File(fileBytes, contentType, filename);
        }

        // ============================================
        // 13. CETAK SK / DOWNLOAD PDF SK
        // ============================================
        [HttpGet("CetakSKMeninggalDunia/{id}")]
        [RequiresPermission("meninggal_dunia.print")]
        public async Task<IActionResult> CetakSK(string id)
        {
            var unescapedId = Uri.UnescapeDataString(id);
            var detail = await _repo.GetDetailAsync(unescapedId);
            if (detail == null)
            {
                return NotFound(new { message = "Data meninggal dunia tidak ditemukan." });
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
                    reportName = "Report_SK_Meninggal_Dunia",
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
                return File(pdfBytes, "application/pdf", $"SK_Meninggal_Dunia_{unescapedId.Replace("/", "_")}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Terjadi kendala saat menghubungi service report.", error = ex.Message });
            }
        }

        // ============================================
        // 14. HELPER DROPDOWN MAHASISWA
        // ============================================
        [HttpGet("GetListMahasiswa")]
        [RequiresPermission("meninggal_dunia.view")]
        public async Task<IActionResult> GetMahasiswaList([FromQuery] string? search = null)
        {
            var data = await _repo.GetMahasiswaListAsync(search);
            return Ok(data);
        }

        [HttpGet("GetDetailMahasiswa/{mhsId}")]
        [RequiresPermission("meninggal_dunia.view")]
        public async Task<IActionResult> GetMahasiswaDetail(string mhsId)
        {
            var data = await _repo.GetMahasiswaDetailAsync(mhsId);
            if (data == null)
            {
                return NotFound(new { message = "Data mahasiswa tidak ditemukan." });
            }
            return Ok(data);
        }

        [HttpGet("GetProdiMahasiswa/{mhsId}")]
        [RequiresPermission("meninggal_dunia.view")]
        public async Task<IActionResult> GetMahasiswaProdi(string mhsId)
        {
            var data = await _repo.GetMahasiswaProdiAsync(mhsId);
            if (data == null)
            {
                return NotFound(new { message = "Data prodi mahasiswa tidak ditemukan." });
            }
            return Ok(data);
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

        private async Task<string> SaveUploadedFileAsync(IFormFile? file, string folder = "meninggal")
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