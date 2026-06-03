using astratech_apps_backend.DTOs.DropOut;
using astratech_apps_backend.DTOs.PengunduranDiri;
using astratech_apps_backend.Helpers;
using astratech_apps_backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace astratech_apps_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DropOutController : ControllerBase
    {
        private readonly IDropOutRepository _repo;
        private readonly IConfiguration Configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public DropOutController(IDropOutRepository repo, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _repo = repo;
            Configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }



        [Authorize]
        [RequiresPermission("drop_out.view")]
        [HttpGet("debug-claims")]
        public IActionResult DebugClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(new { 
                claims = claims,
                identityName = User.Identity?.Name,
                isAuthenticated = User.Identity?.IsAuthenticated
            });
        }

        [Authorize]
        [RequiresPermission("drop_out.view")]
        [HttpGet("debug/database-info")]
        public async Task<IActionResult> GetDatabaseInfo()
        {
            try
            {
                var connString = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
                    Configuration.GetConnectionString("DefaultConnection")!,
                    Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
                );
                
                await using var conn = new SqlConnection(connString);
                await conn.OpenAsync();
                
                var cmd = new SqlCommand(@"
                    SELECT 
                        DB_NAME() AS DatabaseName,
                        @@SERVERNAME AS ServerName,
                        SUSER_NAME() AS LoginName,
                        USER_NAME() AS UserName,
                        GETDATE() AS ServerTime
                ", conn);
                
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return Ok(new {
                        database = reader["DatabaseName"]?.ToString(),
                        server = reader["ServerName"]?.ToString(),
                        login = reader["LoginName"]?.ToString(),
                        user = reader["UserName"]?.ToString(),
                        serverTime = reader["ServerTime"]?.ToString(),
                        message = "Backend is connected to this database"
                    });
                }
                
                return Ok(new { message = "Unable to get database info" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Error getting database info",
                    error = ex.Message 
                });
            }
        }

        [Authorize]
        [RequiresPermission("drop_out.view")]
        [HttpGet("debug/check-status/{id}")]
        public async Task<IActionResult> DebugCheckStatus(string id)
        {
            try
            {
                var connString = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
                    Configuration.GetConnectionString("DefaultConnection")!,
                    Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
                );
                
                await using var conn = new SqlConnection(connString);
                await conn.OpenAsync();
                
                var cmd = new SqlCommand(@"
                    SELECT 
                        dro_id,
                        dro_status,
                        dro_created_date,
                        dro_updated_date,
                        dro_created_by,
                        dro_updated_by,
                        DB_NAME() as CurrentDatabase,
                        @@SERVERNAME as CurrentServer
                    FROM sia_msdropout 
                    WHERE dro_id = @id
                ", conn);
                cmd.Parameters.AddWithValue("@id", id);
                
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return Ok(new {
                        id = reader["dro_id"]?.ToString(),
                        status = reader["dro_status"]?.ToString(),
                        createdDate = reader["dro_created_date"]?.ToString(),
                        updatedDate = reader["dro_updated_date"]?.ToString(),
                        createdBy = reader["dro_created_by"]?.ToString(),
                        updatedBy = reader["dro_updated_by"]?.ToString(),
                        database = reader["CurrentDatabase"]?.ToString(),
                        server = reader["CurrentServer"]?.ToString(),
                        message = "Data found in backend database"
                    });
                }
                
                return NotFound(new { 
                    message = $"Data with ID '{id}' not found in backend database",
                    database = conn.Database,
                    server = conn.DataSource
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Error checking status",
                    error = ex.Message 
                });
            }
        }

        [Authorize]
        [RequiresPermission("drop_out.view")]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? page = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] string keyword = "",
            [FromQuery] string sortBy = "a.dro_created_date desc",
            [FromQuery] string konsentrasi = "",
            [FromQuery] string status = "")  // NEW: Support multiple status (comma-separated)
        {
            // Ambil dari JWT claims
            var username = User.FindFirst("namaakun")?.Value ?? "";
            var role = User.FindFirst("role")?.Value ?? "";
            var idrole = User.FindFirst("idrole")?.Value ?? "";
            var displayName = User.FindFirst("displayname")?.Value 
                           ?? User.FindFirst("name")?.Value 
                           ?? "";
            
            // Debug semua claims
            Console.WriteLine("=== DEBUG ALL JWT CLAIMS ===");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }
            Console.WriteLine("=== END CLAIMS ===");
            
            Console.WriteLine($"DEBUG GetAll DO - username: {username}, role: {role}, idrole: {idrole}, displayName: {displayName}");
            
            // Gunakan idrole bukan role
            var roleToUse = string.IsNullOrEmpty(idrole) ? role : idrole;
            
            // Jika page dan pageSize ada, gunakan pagination
            if (page.HasValue && pageSize.HasValue)
            {
                var pageVal = page.Value < 1 ? 1 : page.Value;
                var pageSizeVal = pageSize.Value < 1 ? 10 : (pageSize.Value > 100 ? 100 : pageSize.Value);
                
                Console.WriteLine($"DEBUG GetAll - Using pagination: page={pageVal}, pageSize={pageSizeVal}");
                
                var result = await _repo.GetPendingPaginatedAsync(
                    username, keyword, sortBy, konsentrasi, roleToUse, displayName, status, pageVal, pageSizeVal);
                
                return Ok(result);
            }
            
            // Jika tidak ada pagination, return semua data (backward compatibility)
            return Ok(await _repo.GetPendingAsync(username, keyword, sortBy, konsentrasi, roleToUse, displayName));
        }

        [RequiresPermission("drop_out.view")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var data = await _repo.GetByIdAsync(id);
            return data == null ? NotFound() : Ok(data);
        }

        [RequiresPermission("drop_out.view")]
        [HttpGet("detail")]
        public async Task<IActionResult> GetDetail([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "ID wajib diisi." });

            var data = await _repo.GetDetailAsync(id);

            if (data == null)
                return NotFound(new { message = "Data Drop Out tidak ditemukan." });

            return Ok(data);
        }



        [RequiresPermission("drop_out.view")]
        [HttpGet("{id}/check-report")]
        public async Task<IActionResult> CheckReport(string id)
        {
            var data = await _repo.CheckReportAsync(id);

            if (data == null)
                return NotFound(new { message = "Report suffix tidak ditemukan." });

            return Ok(new { suffix = data });
        }

        [RequiresPermission("drop_out.view")]
        [HttpGet("report-suket/{suratNo}")]
        public async Task<IActionResult> GetReportSuket(string suratNo)
        {
            var data = await _repo.GetReportSuketAsync(suratNo);

            if (data == null)
                return NotFound(new { message = "Data surat keterangan tidak ditemukan." });

            return Ok(data);
        }

        [RequiresPermission("drop_out.export")]
        [HttpGet("download-sk/{droId}")]
        public async Task<IActionResult> DownloadSK(string droId)
        {
            var result = await _repo.DownloadSKAsync(droId);

            if (result == null)
                return NotFound(new { message = "SK tidak ditemukan untuk DropOut ini." });

            return Ok(result);
        }

        [RequiresPermission("drop_out.export")]
        [HttpGet("download-sk-file/{droId}")]
        public async Task<IActionResult> DownloadSKFile(string droId)
        {
            var result = await _repo.DownloadSKAsync(droId);

            if (result == null)
                return NotFound(new { message = "SK tidak ditemukan untuk DropOut ini." });

            if (string.IsNullOrEmpty(result.Sk))
                return NotFound(new { message = "File SK belum diupload." });

            // Cek apakah path sudah lengkap atau perlu digabung dengan wwwroot
            string fullPath;
            if (System.IO.Path.IsPathRooted(result.Sk))
            {
                fullPath = result.Sk;
            }
            else
            {
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                fullPath = Path.Combine(webRootPath, result.Sk.TrimStart('/').Replace("/", "\\"));
            }

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { message = $"File tidak ditemukan di server: {result.Sk}" });

            var extension = Path.GetExtension(fullPath).ToLower();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            var fileName = Path.GetFileName(fullPath);
            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);

            return File(fileBytes, contentType, fileName);
        }

        //[HttpGet("riwayat")]
        //public async Task<IActionResult> GetRiwayat(
        //[FromQuery] string username,
        //[FromQuery] string keyword = "",
        //[FromQuery] string sortBy = "dro_id asc",
        //[FromQuery] string konsentrasi = "",
        //[FromQuery] string role = "",
        //[FromQuery] string sekprodi = ""
        //)
        //{
        //    var data = await _repo.GetRiwayatAsync(
        //        username, keyword, sortBy, konsentrasi, role, sekprodi
        //    );

        //    return Ok(data);
        //}

        [RequiresPermission("drop_out.view")]
        [HttpGet("riwayat")]
        public async Task<IActionResult> GetRiwayat(
           [FromQuery] string username,
           [FromQuery] int? page = 1,
           [FromQuery] int? pageSize = 10,
           [FromQuery] string keyword = "",
           [FromQuery] string sortBy = "",
           [FromQuery] string konsentrasi = "",
           [FromQuery] string role = "",
           [FromQuery] string displayName = "",
           [FromQuery] string status = "")  // NEW: Support multiple status (comma-separated)
        {
            // Default pagination: page=1, pageSize=10
            var pageVal = page.HasValue && page.Value >= 1 ? page.Value : 1;
            var pageSizeVal = pageSize.HasValue && pageSize.Value >= 1 ? 
                (pageSize.Value > 100 ? 100 : pageSize.Value) : 10;
            
            var result = await _repo.GetRiwayatPaginatedAsync(
                username, keyword, sortBy, konsentrasi, role, displayName, status, pageVal, pageSizeVal);
            
            return Ok(result);
        }


        [RequiresPermission("drop_out.view")]
        [HttpGet("riwayat/excel")]
        public async Task<IActionResult> GetRiwayatExcel(
        [FromQuery] string username,
        [FromQuery] string keyword = "",
        [FromQuery] string sortBy = "dro_id asc",
        [FromQuery] string konsentrasi = "",
        [FromQuery] string role = "",
        [FromQuery] string sekprodi = ""
        )
        {
            var data = await _repo.GetRiwayatExcelAsync(
                username, keyword, sortBy, konsentrasi, role, sekprodi
            );

            return Ok(data);
        }

        [RequiresPermission("drop_out.view")]
        [HttpGet("{id}/report-sk")]
        public async Task<IActionResult> GetReportSK(string id)
        {
            var data = await _repo.GetReportSKDOAsync(id);

            if (data == null)
                return NotFound(new { message = "Data laporan SK DO tidak ditemukan." });

            return Ok(data);
            }

        [RequiresPermission("drop_out.view")]
        [HttpGet("{id}/report-sk-sub")]
        public async Task<IActionResult> GetReportSKDOSub(string id)
        {
            var data = await _repo.GetReportSKDOSubAsync(id);

            if (data == null || data.Count == 0)
                return NotFound(new { message = "Subreport SK DO tidak ditemukan." });

            return Ok(data);
        }

        [RequiresPermission("drop_out.export")]
        [HttpGet("{id}/generate-pdf-sk")]
        public async Task<IActionResult> GeneratePdfSK(string id)
        {
            try
            {
                // TEMPORARY: Check if we should use mock/dummy response
                var useMock = Configuration["ReportService:UseMock"] == "true";
                
                if (useMock)
                {
                    // Return dummy PDF for testing
                    return Ok(new { 
                        message = "MOCK MODE: Service report sedang dalam development",
                        droId = id,
                        reportServiceUrl = Configuration["ReportService:Url"],
                        note = "Set ReportService:UseMock = false di appsettings.json untuk menggunakan service report asli"
                    });
                }

                // Langsung panggil service report tanpa ambil data dari database
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30); // Set timeout 30 detik
                
                var reportServiceUrl = Configuration["ReportService:Url"] ?? "http://10.5.0.94/api/Report/GetReport";

                Console.WriteLine($"=== Calling Report Service ===");
                Console.WriteLine($"URL: {reportServiceUrl}");
                Console.WriteLine($"DroId: {id}");

                // Siapkan request body untuk service report
                var requestBody = new
                {
                    reportName = "Report_SK_Drop_Out_2",
                    parameters = new
                    {
                        droId = id
                    }
                };

                // Serialize ke JSON
                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(requestBody),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                Console.WriteLine($"Request Body: {System.Text.Json.JsonSerializer.Serialize(requestBody)}");

                // POST ke service report
                var response = await client.PostAsync(reportServiceUrl, content);

                Console.WriteLine($"Response Status: {response.StatusCode}");

                // Jika gagal, return error dari service report (database logon failed, crystal report error, dll)
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Response: {errorContent}");
                    
                    return StatusCode((int)response.StatusCode, new { 
                        message = "Gagal generate PDF dari service report",
                        error = errorContent,
                        reportServiceUrl = reportServiceUrl
                    });
                }

                // Baca PDF sebagai byte array
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                Console.WriteLine($"PDF Size: {pdfBytes.Length} bytes");

                // Return file PDF ke client
                var fileName = $"SK_DO_{id.Replace("/", "-")}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (HttpRequestException ex)
            {
                // Error koneksi ke service report
                Console.WriteLine($"HttpRequestException: {ex.Message}");
                
                return StatusCode(503, new { 
                    message = "Service report tidak dapat diakses",
                    error = ex.Message,
                    reportServiceUrl = Configuration["ReportService:Url"],
                    troubleshooting = new
                    {
                        step1 = "Pastikan service report di 10.5.0.94 sudah running",
                        step2 = "Test koneksi: curl http://10.5.0.94/api/Report/GetReport",
                        step3 = "Cek firewall/network antara backend dan service report",
                        step4 = "Verifikasi URL dan port yang benar",
                        step5 = "Set ReportService:UseMock = true di appsettings.json untuk testing tanpa service report"
                    }
                });
            }
            catch (TaskCanceledException ex)
            {
                // Timeout
                Console.WriteLine($"TaskCanceledException (Timeout): {ex.Message}");
                
                return StatusCode(504, new { 
                    message = "Request ke service report timeout",
                    error = ex.Message,
                    hint = "Service report terlalu lama merespon atau tidak bisa diakses"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                return StatusCode(500, new { 
                    message = "Terjadi kesalahan saat generate PDF SK",
                    error = ex.Message 
                });
            }
        }






        [Authorize]
        [RequiresPermission("drop_out.create")]
        [HttpPost("create-pengajuan")]
        public async Task<IActionResult> CreatePengajuanDO([FromBody] CreatePengajuanDORequest dto)
        {
            // Prioritas: dari JWT token, fallback ke body, fallback ke "system"
            var createdBy = User.FindFirst("namaakun")?.Value 
                         ?? dto.CreatedBy 
                         ?? "system";

            var newId = await _repo.CreatePengajuanDOAsync(dto, createdBy);

            if (newId == null)
                return BadRequest(new { message = "Gagal membuat pengajuan Draft drop_out." });

            return Ok(new
            {
                message = "Pengajuan DO Draft berhasil dibuat.",
                id = newId,
                createdBy = createdBy
            });
        }

        [Authorize]
        [RequiresPermission("drop_out.edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateDropOutRequest dto)
        {
            var updatedBy = User.FindFirst("namaakun")?.Value ?? "system";
            var success = await _repo.UpdateAsync(id, dto, updatedBy);

            if (!success)
                return NotFound(new { message = "Gagal memperbarui data Drop Out." });

            return Ok(new { message = "Data Drop Out berhasil diperbarui." });
        }

        [Authorize]
        [RequiresPermission("drop_out.edit")]
        [HttpPut("draft/{id}/generate-id")]
        public async Task<IActionResult> GenerateIdFromDraft(string id)
        {
            try
            {
                // URL decode the ID to handle slashes properly
                var decodedId = System.Web.HttpUtility.UrlDecode(id);
                Console.WriteLine($"DEBUG GenerateIdFromDraft - Input ID: '{id}', Decoded: '{decodedId}'");
                
                if (string.IsNullOrEmpty(decodedId))
                    return BadRequest(new { message = "ID draft wajib diisi" });

                var data = await _repo.GetIdByDraftAsync(decodedId);

                if (data == null)
                {
                    Console.WriteLine($"DEBUG GenerateIdFromDraft - No data returned for ID: '{decodedId}'");
                    return NotFound(new { message = "Draft tidak ditemukan atau gagal generate ID" });
                }

                Console.WriteLine($"DEBUG GenerateIdFromDraft - Success, new ID: '{data.Id}'");
                return Ok(new { 
                    message = "ID DO berhasil di-generate",
                    oldId = decodedId,
                    newId = data.Id 
                });
            }
            catch (InvalidOperationException ex)
            {
                // Validation error from SP
                Console.WriteLine($"VALIDATION ERROR GenerateIdFromDraft: {ex.Message}");
                return BadRequest(new { 
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR GenerateIdFromDraft: {ex.Message}");
                return StatusCode(500, new { 
                    message = "Terjadi kesalahan saat generate ID DO",
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Endpoint alternatif untuk generate ID menggunakan query parameter.
        /// Berguna untuk ID yang mengandung karakter slash (/) seperti "1/PMA/DO/III/2026"
        /// </summary>
        [Authorize]
        [RequiresPermission("drop_out.edit")]
        [HttpPut("draft/generate-id")]
        public async Task<IActionResult> GenerateIdFromDraftByQuery([FromQuery] string id)
        {
            try
            {
                // URL decode the ID to handle slashes properly
                var decodedId = System.Web.HttpUtility.UrlDecode(id);
                Console.WriteLine($"DEBUG GenerateIdFromDraftByQuery - Input ID: '{id}', Decoded: '{decodedId}'");
                
                if (string.IsNullOrEmpty(decodedId))
                    return BadRequest(new { message = "ID draft wajib diisi" });

                var data = await _repo.GetIdByDraftAsync(decodedId);

                if (data == null)
                {
                    Console.WriteLine($"DEBUG GenerateIdFromDraftByQuery - No data returned for ID: '{decodedId}'");
                    return NotFound(new { message = "Draft tidak ditemukan atau gagal generate ID" });
                }

                Console.WriteLine($"DEBUG GenerateIdFromDraftByQuery - Success, new ID: '{data.Id}'");
                return Ok(new { 
                    message = "ID DO berhasil di-generate",
                    oldId = decodedId,
                    newId = data.Id 
                });
            }
            catch (InvalidOperationException ex)
            {
                // Validation error from SP
                Console.WriteLine($"VALIDATION ERROR GenerateIdFromDraftByQuery: {ex.Message}");
                return BadRequest(new { 
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR GenerateIdFromDraftByQuery: {ex.Message}");
                return StatusCode(500, new { 
                    message = "Terjadi kesalahan saat generate ID DO",
                    error = ex.Message 
                });
            }
        }


        [Authorize]
        [RequiresPermission("drop_out.approve_reject")]
        [HttpPut("wadir/approve")]
        public async Task<IActionResult> ApproveByWadir(
            [FromQuery] string id,
            [FromBody] ApproveDropOutRequest dto)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "ID wajib diisi." });

            var success = await _repo.ApproveByWadirAsync(id, dto);

            if (!success)
                return BadRequest(new { message = "Approve gagal." });

            return Ok(new { message = "Drop Out berhasil disetujui Wadir 1" });
        }

        [Authorize]
        [RequiresPermission("drop_out.approve_reject")]
        [HttpPut("wadir/reject")]
        public async Task<IActionResult> RejectByWadir(
            [FromQuery] string id,
            [FromBody] RejectDropOutRequest dto)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "ID wajib diisi." });

            var success = await _repo.RejectByWadirAsync(id, dto);

            if (!success)
                return BadRequest(new { message = "Reject gagal." });

            return Ok(new { message = "Drop Out berhasil ditolak" });
        }

        [RequiresPermission("drop_out.import")]
        [HttpPut("upload-sk")]
        public async Task<IActionResult> UploadSKDO([FromBody] UploadSKDORequest request)
        {
            var result = await _repo.UploadSKDOAsync(request);

            if (!result)
                return BadRequest(new { message = "Gagal upload SK DO" });

            return Ok(new { message = "Upload SK DO berhasil" });
        }

        [Authorize]
        // Upload file SK dan SKPB ke folder terpisah (tanpa rename)
        [Authorize]
        [RequiresPermission("drop_out.import")]
        [HttpPost("upload-sk-file")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadSKFile([FromForm] UploadSKFileRequest request)
        {
            if (string.IsNullOrEmpty(request.DroId))
                return BadRequest(new { message = "DroId wajib diisi" });

            if (request.SkFile == null && request.SkpbFile == null)
                return BadRequest(new { message = "Minimal satu file harus diupload (SK atau SKPB)" });

            var baseUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "dropout");
            var skFolderPath = Path.Combine(baseUploadPath, "sk");
            var skpbFolderPath = Path.Combine(baseUploadPath, "skpb");
            
            // Pastikan folder ada
            if (!Directory.Exists(skFolderPath))
                Directory.CreateDirectory(skFolderPath);
            if (!Directory.Exists(skpbFolderPath))
                Directory.CreateDirectory(skpbFolderPath);

            string skPath = "";
            string skpbPath = "";

            // Generate timestamp untuk nama file
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Upload SK file ke folder sk/ (tambahkan timestamp)
            if (request.SkFile != null && request.SkFile.Length > 0)
            {
                var originalFileName = Path.GetFileNameWithoutExtension(request.SkFile.FileName);
                var extension = Path.GetExtension(request.SkFile.FileName);
                var skFileName = $"{originalFileName}_{timestamp}{extension}";
                var skFullPath = Path.Combine(skFolderPath, skFileName);
                
                using (var stream = new FileStream(skFullPath, FileMode.Create))
                {
                    await request.SkFile.CopyToAsync(stream);
                }
                skPath = $"/uploads/dropout/sk/{skFileName}";
            }

            // Upload SKPB file ke folder skpb/ (tambahkan timestamp)
            if (request.SkpbFile != null && request.SkpbFile.Length > 0)
            {
                var originalFileName = Path.GetFileNameWithoutExtension(request.SkpbFile.FileName);
                var extension = Path.GetExtension(request.SkpbFile.FileName);
                var skpbFileName = $"{originalFileName}_{timestamp}{extension}";
                var skpbFullPath = Path.Combine(skpbFolderPath, skpbFileName);
                
                using (var stream = new FileStream(skpbFullPath, FileMode.Create))
                {
                    await request.SkpbFile.CopyToAsync(stream);
                }
                skpbPath = $"/uploads/dropout/skpb/{skpbFileName}";
            }

            // Return path file saja, TIDAK update database
            // Frontend akan menggunakan path ini untuk PUT request ke /upload-sk
            return Ok(new { 
                message = "File berhasil diupload ke server",
                droId = request.DroId,
                skPath = skPath,
                skpbPath = skpbPath
            });
        }

        // Method ini tidak dipakai lagi karena sekarang pakai nomor surat yang sudah ada di database
        // /// <summary>
        // /// Generate nomor SK untuk Drop Out
        // /// </summary>
        // private async Task<string> GenerateNoSKAsync(string droId)
        // {
        //     try
        //     {
        //         // Get data Drop Out untuk ambil konsentrasi
        //         var detail = await _repo.GetByIdAsync(droId);
        //         if (detail == null)
        //             return $"SK_DO_{DateTime.Now:yyyyMMddHHmmss}";

        //         // TODO: Sesuaikan jenisSuratId dengan ID di database
        //         // Contoh: "JS001" untuk jenis surat Drop Out
        //         string jenisSuratId = "JS_DROP_OUT"; // Ganti dengan ID yang sesuai
        //         
        //         var connString = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
        //             Configuration.GetConnectionString("DefaultConnection")!,
        //             Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
        //         );
        //         
        //         var generator = new Helpers.NoSuratGenerator(connString);
        //         
        //         // Generate nomor surat
        //         // Jika ada konsentrasi ID, bisa ditambahkan sebagai parameter kedua
        //         return await generator.GenerateNoSKForFileAsync(jenisSuratId);
        //     }
        //     catch
        //     {
        //         // Fallback jika gagal generate
        //         return $"SK_DO_{DateTime.Now:yyyyMMddHHmmss}";
        //     }
        // }



        [RequiresPermission("drop_out.delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _repo.DeleteAsync(id);

            if (!success)
                return NotFound(new { message = "Data DropOut tidak ditemukan atau gagal dihapus." });

            return Ok(new { message = "DropOut berhasil dihapus." });
        }

        [RequiresPermission("drop_out.view")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(
            [FromQuery] string username,
            [FromQuery] int? page = 1,
            [FromQuery] int? pageSize = 10,
            [FromQuery] string keyword = "",
            [FromQuery] string sortBy = "a.dro_created_date desc",
            [FromQuery] string konsentrasi = "",
            [FromQuery] string role = "",
            [FromQuery] string displayName = "",
            [FromQuery] string status = "")  // NEW: Support multiple status (comma-separated)
        {
            // Default pagination: page=1, pageSize=10
            var pageVal = page.HasValue && page.Value >= 1 ? page.Value : 1;
            var pageSizeVal = pageSize.HasValue && pageSize.Value >= 1 ? 
                (pageSize.Value > 100 ? 100 : pageSize.Value) : 10;
            
            var result = await _repo.GetPendingPaginatedAsync(
                username, keyword, sortBy, konsentrasi, role, displayName, status, pageVal, pageSizeVal);
            
            return Ok(result);
        }

        [HttpGet("mahasiswa-by-konsentrasi")]
        public async Task<IActionResult> GetMahasiswaByKonsentrasi(
    [FromQuery] string konsentrasiId)
        {
            if (string.IsNullOrEmpty(konsentrasiId))
                return BadRequest(new { message = "Konsentrasi wajib diisi." });

            var data = await _repo.GetMahasiswaByKonsentrasiAsync(konsentrasiId);

            return Ok(data);
        }

        [Authorize]
        [RequiresPermission("drop_out.view")]
        [HttpGet("prodi")]
        public async Task<IActionResult> GetProdi()
        {
            var username = User.FindFirst("namaakun")?.Value ?? "";
            return Ok(await _repo.GetProdiAsync(username));
        }

        [RequiresPermission("drop_out.view")]
        [HttpGet("prodi/list")]
        public async Task<IActionResult> GetListProdi()
        {
            return Ok(await _repo.GetListProdiAsync());
        }

        [Authorize]
        [RequiresPermission("drop_out.view")]
        [HttpGet("konsentrasi")]
        public async Task<IActionResult> GetKonsentrasiByProdi(
    [FromQuery] string prodiId
)
        {
            if (string.IsNullOrEmpty(prodiId))
                return BadRequest("prodiId wajib diisi");

            // username dipakai utk filter sekprodi (opsional)
            var username = User.FindFirst("namaakun")?.Value ?? "";

            var data = await _repo.GetKonsentrasiByProdiAsync(prodiId, username);
            return Ok(data);
        }




        [HttpGet("mahasiswa")]
        public async Task<IActionResult> GetMahasiswa([FromQuery] string konsentrasiId)
        {
            return Ok(await _repo.GetMahasiswaByKonsentrasiAsync(konsentrasiId));
        }


        [RequiresPermission("drop_out.view")]
        [HttpGet("angkatan-by-mahasiswa")]
        public async Task<IActionResult> GetAngkatanByMahasiswa(
    [FromQuery] string mhsId
)
        {
            if (string.IsNullOrEmpty(mhsId))
                return BadRequest(new { message = "Mahasiswa ID wajib diisi." });

            var angkatan = await _repo.GetAngkatanByMahasiswaAsync(mhsId);

            if (angkatan == null)
                return NotFound(new { message = "Angkatan mahasiswa tidak ditemukan." });

            return Ok(new { angkatan });
        }

        [RequiresPermission("drop_out.view")]
        [HttpGet("mahasiswa/{mhsId}/bebas-tanggungan")]
        public async Task<IActionResult> CekBebasTanggungan(string mhsId)
        {
            var result = await _repo.CekBebasTanggunganAsync(mhsId);
            
            if (result == null)
                return NotFound(new { message = "Data mahasiswa tidak ditemukan" });
                
            return Ok(result);
        }

        [RequiresPermission("drop_out.view")]
        [HttpGet("mahasiswa/{mhsId}/profil")]
        public async Task<IActionResult> GetProfilMahasiswaDetail(string mhsId)
        {
            var result = await _repo.GetProfilMahasiswaDetailAsync(mhsId);
            
            if (result == null)
                return NotFound(new { message = "Data profil mahasiswa tidak ditemukan" });
                
            return Ok(result);
        }

        [RequiresPermission("drop_out.view")]
        [HttpGet("template-sk")]
        public IActionResult DownloadTemplateSK([FromQuery] string? type = "rpt")
        {
            // Mapping file berdasarkan type
            var fileMapping = new Dictionary<string, (string Path, string ContentType, string FileName)>
            {
                { "rpt", ("wwwroot/uploads/dropout/templates/Report_SK_Drop_Out_2.rpt", "application/octet-stream", "Report_SK_Drop_Out_2.rpt") },
                { "cs", ("wwwroot/uploads/dropout/templates/Report_SK_Drop_Out_2.cs", "text/plain", "Report_SK_Drop_Out_2.cs") },
                { "aspx", ("wwwroot/uploads/dropout/templates/SK_Drop_Out.aspx", "text/plain", "SK_Drop_Out.aspx") },
                { "aspx-cs", ("wwwroot/uploads/dropout/templates/SK_Drop_Out.aspx.cs", "text/plain", "SK_Drop_Out.aspx.cs") },
                { "aspx-designer", ("wwwroot/uploads/dropout/templates/SK_Drop_Out.aspx.designer.cs", "text/plain", "SK_Drop_Out.aspx.designer.cs") }
            };

            if (!fileMapping.ContainsKey(type))
                return BadRequest(new { message = "Type tidak valid. Gunakan: rpt, cs, aspx, aspx-cs, aspx-designer" });

            var (path, contentType, fileName) = fileMapping[type];
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), path);
            
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = $"File template {fileName} tidak ditemukan" });

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType, fileName);
        }

        [RequiresPermission("drop_out.view")]
        [HttpGet("template-sk/list")]
        public IActionResult GetTemplateList()
        {
            return Ok(new
            {
                message = "Daftar file template SK Drop Out",
                files = new[]
                {
                    new { type = "rpt", name = "Report_SK_Drop_Out_2.rpt", description = "Template report Crystal Reports", url = "/api/dropout/template-sk?type=rpt" },
                    new { type = "cs", name = "Report_SK_Drop_Out_2.cs", description = "Wrapper class untuk .rpt", url = "/api/dropout/template-sk?type=cs" },
                    new { type = "aspx", name = "SK_Drop_Out.aspx", description = "Halaman web ASP.NET", url = "/api/dropout/template-sk?type=aspx" },
                    new { type = "aspx-cs", name = "SK_Drop_Out.aspx.cs", description = "Logic/controller ASP.NET", url = "/api/dropout/template-sk?type=aspx-cs" },
                    new { type = "aspx-designer", name = "SK_Drop_Out.aspx.designer.cs", description = "Designer code ASP.NET", url = "/api/dropout/template-sk?type=aspx-designer" },
                    new { type = "template-code", name = "Complete Template Code", description = "Semua kode template dalam format DTO", url = "/api/dropout/template-code" }
                },
                endpoints = new[]
                {
                    new { method = "POST", url = "/api/dropout/DownloadTemplateSK/{mahasiswaId}", description = "Download template SK untuk mahasiswa tertentu (sesuai pola dokumentasi)", input = "ID Mahasiswa", output = "PDF Template SK & SKPB" }
                }
            });
        }

        // Cek status file SK/SKPB (ada atau belum)
        [RequiresPermission("drop_out.view")]
        [HttpGet("check-sk-status")]
        public async Task<IActionResult> CheckSKStatus([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "Parameter id wajib diisi" });

            // Ambil data lengkap untuk dapat nomor surat
            var dropoutData = await _repo.GetByIdAsync(id);
            if (dropoutData == null)
                return NotFound(new { message = "Data Drop Out tidak ditemukan" });

            var result = await _repo.DownloadSKAsync(id);
            if (result == null)
                return NotFound(new { message = "Data Drop Out tidak ditemukan" });

            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            
            // Cek SK
            bool skExists = false;
            string skFullPath = "";
            long skFileSize = 0;
            
            if (!string.IsNullOrEmpty(result.Sk))
            {
                skFullPath = Path.IsPathRooted(result.Sk) 
                    ? result.Sk 
                    : Path.Combine(webRootPath, result.Sk.TrimStart('/').Replace("/", "\\"));
                
                if (System.IO.File.Exists(skFullPath))
                {
                    skExists = true;
                    skFileSize = new FileInfo(skFullPath).Length;
                }
            }
            
            // Cek SKPB
            bool skpbExists = false;
            string skpbFullPath = "";
            long skpbFileSize = 0;
            
            if (!string.IsNullOrEmpty(result.Skpb))
            {
                skpbFullPath = Path.IsPathRooted(result.Skpb) 
                    ? result.Skpb 
                    : Path.Combine(webRootPath, result.Skpb.TrimStart('/').Replace("/", "\\"));
                
                if (System.IO.File.Exists(skpbFullPath))
                {
                    skpbExists = true;
                    skpbFileSize = new FileInfo(skpbFullPath).Length;
                }
            }

            return Ok(new
            {
                droId = id,
                nomorSK = dropoutData.SrtNo,
                sk = new
                {
                    uploaded = !string.IsNullOrEmpty(result.Sk),
                    exists = skExists,
                    path = result.Sk,
                    fileName = skExists ? Path.GetFileName(skFullPath) : null,
                    fileSize = skExists ? $"{skFileSize / 1024.0:F2} KB" : null,
                    downloadUrl = skExists ? $"/api/DropOut/download-sk-file?id={Uri.EscapeDataString(id)}" : null
                },
                skpb = new
                {
                    uploaded = !string.IsNullOrEmpty(result.Skpb),
                    exists = skpbExists,
                    path = result.Skpb,
                    fileName = skpbExists ? Path.GetFileName(skpbFullPath) : null,
                    fileSize = skpbExists ? $"{skpbFileSize / 1024.0:F2} KB" : null,
                    downloadUrl = skpbExists ? $"/api/DropOut/download-sk-file?id={Uri.EscapeDataString(id)}" : null
                },
                message = (skExists || skpbExists) 
                    ? "File tersedia untuk didownload" 
                    : "File belum diupload"
            });
        }

        // Download file SK dengan query parameter (untuk ID yang ada slash)
        [RequiresPermission("drop_out.export")]
        [HttpGet("download-sk-file")]
        public async Task<IActionResult> DownloadSKFileByQuery([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "Parameter id wajib diisi" });

            var result = await _repo.DownloadSKAsync(id);

            if (result == null)
                return NotFound(new { message = "Data Drop Out tidak ditemukan" });

            if (string.IsNullOrEmpty(result.Sk))
                return NotFound(new { message = "File SK belum diupload" });

            string fullPath;
            if (Path.IsPathRooted(result.Sk))
            {
                fullPath = result.Sk;
            }
            else
            {
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                fullPath = Path.Combine(webRootPath, result.Sk.TrimStart('/').Replace("/", "\\"));
            }

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { message = $"File tidak ditemukan di server: {result.Sk}" });

            var extension = Path.GetExtension(fullPath).ToLower();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            var fileName = Path.GetFileName(fullPath);
            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);

            return File(fileBytes, contentType, fileName);
        }

        // Download file SK langsung dengan filename
        [RequiresPermission("drop_out.export")]
        [HttpGet("sk/{filename}")]
        public IActionResult DownloadSKByFilename(string filename)
        {
            var baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "dropout", "sk");
            var filePath = Path.Combine(baseFolder, filename);

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "File SK tidak ditemukan" });

            var extension = Path.GetExtension(filePath).ToLower();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType, filename);
        }

        // Download file SKPB langsung dengan filename
        [RequiresPermission("drop_out.export")]
        [HttpGet("skpb/{filename}")]
        public IActionResult DownloadSKPBByFilename(string filename)
        {
            var baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "dropout", "skpb");
            var filePath = Path.Combine(baseFolder, filename);

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "File SKPB tidak ditemukan" });

            var extension = Path.GetExtension(filePath).ToLower();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType, filename);
        }

        // Get info SK + SKPB untuk download terpisah
        [RequiresPermission("drop_out.export")]
        [HttpGet("download-all-sk")]
        public async Task<IActionResult> DownloadAllSK([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "Parameter id wajib diisi" });

            Console.WriteLine($"=== DEBUG DownloadAllSK ===");
            Console.WriteLine($"Input ID: '{id}'");

            var result = await _repo.DownloadSKAsync(id);
            if (result == null)
            {
                Console.WriteLine($"ERROR: DownloadSKAsync returned null for ID: '{id}'");
                return NotFound(new { message = "Data tidak ditemukan" });
            }

            Console.WriteLine($"SK from DB: '{result.Sk}'");
            Console.WriteLine($"SKPB from DB: '{result.Skpb}'");

            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var files = new List<object>();

            // Cek SK
            if (!string.IsNullOrEmpty(result.Sk))
            {
                Console.WriteLine($"Processing SK: '{result.Sk}'");
                
                // Handle path yang hanya nama file atau path lengkap
                string skFullPath;
                string actualFileName = "";
                
                if (result.Sk.StartsWith("/") || result.Sk.StartsWith("\\"))
                {
                    // Path lengkap dari database
                    skFullPath = Path.Combine(webRootPath, result.Sk.TrimStart('/').Replace("/", "\\"));
                }
                else
                {
                    // Hanya nama file, cari di folder sk/
                    var skFolder = Path.Combine(webRootPath, "uploads", "dropout", "sk");
                    skFullPath = Path.Combine(skFolder, result.Sk);
                    
                    // Jika file tidak ada, cari file dengan timestamp (pattern matching)
                    if (!System.IO.File.Exists(skFullPath))
                    {
                        Console.WriteLine($"SK file not found, searching with timestamp pattern...");
                        
                        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(result.Sk);
                        var extension = Path.GetExtension(result.Sk);
                        
                        // Cari file dengan pattern: originalname_timestamp.ext
                        var matchingFiles = Directory.GetFiles(skFolder, $"{fileNameWithoutExt}_*{extension}")
                                                   .OrderByDescending(f => new FileInfo(f).CreationTime)
                                                   .ToArray();
                        
                        if (matchingFiles.Length > 0)
                        {
                            skFullPath = matchingFiles[0]; // Ambil yang terbaru
                            actualFileName = Path.GetFileName(skFullPath);
                            Console.WriteLine($"Found SK file with timestamp: '{actualFileName}'");
                        }
                    }
                }

                Console.WriteLine($"SK Full Path: '{skFullPath}'");
                Console.WriteLine($"SK File Exists: {System.IO.File.Exists(skFullPath)}");

                if (System.IO.File.Exists(skFullPath))
                {
                    var fileName = string.IsNullOrEmpty(actualFileName) ? Path.GetFileName(skFullPath) : actualFileName;
                    files.Add(new
                    {
                        type = "SK",
                        fileName = fileName,
                        url = $"/uploads/dropout/sk/{fileName}",
                        downloadUrl = $"/api/DropOut/sk/{fileName}"  // Fixed: Use correct endpoint
                    });
                    Console.WriteLine($"Added SK file: '{fileName}'");
                }
                else
                {
                    Console.WriteLine($"SK file not found at: '{skFullPath}'");
                }
            }
            else
            {
                Console.WriteLine("SK is null or empty");
            }

            // Cek SKPB
            if (!string.IsNullOrEmpty(result.Skpb))
            {
                Console.WriteLine($"Processing SKPB: '{result.Skpb}'");
                
                // Handle path yang hanya nama file atau path lengkap
                string skpbFullPath;
                string actualFileName = "";
                
                if (result.Skpb.StartsWith("/") || result.Skpb.StartsWith("\\"))
                {
                    // Path lengkap dari database
                    skpbFullPath = Path.Combine(webRootPath, result.Skpb.TrimStart('/').Replace("/", "\\"));
                }
                else
                {
                    // Hanya nama file, cari di folder skpb/
                    var skpbFolder = Path.Combine(webRootPath, "uploads", "dropout", "skpb");
                    skpbFullPath = Path.Combine(skpbFolder, result.Skpb);
                    
                    // Jika file tidak ada, cari file dengan timestamp (pattern matching)
                    if (!System.IO.File.Exists(skpbFullPath))
                    {
                        Console.WriteLine($"SKPB file not found, searching with timestamp pattern...");
                        
                        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(result.Skpb);
                        var extension = Path.GetExtension(result.Skpb);
                        
                        // Cari file dengan pattern: originalname_timestamp.ext
                        var matchingFiles = Directory.GetFiles(skpbFolder, $"{fileNameWithoutExt}_*{extension}")
                                                   .OrderByDescending(f => new FileInfo(f).CreationTime)
                                                   .ToArray();
                        
                        if (matchingFiles.Length > 0)
                        {
                            skpbFullPath = matchingFiles[0]; // Ambil yang terbaru
                            actualFileName = Path.GetFileName(skpbFullPath);
                            Console.WriteLine($"Found SKPB file with timestamp: '{actualFileName}'");
                        }
                    }
                }

                Console.WriteLine($"SKPB Full Path: '{skpbFullPath}'");
                Console.WriteLine($"SKPB File Exists: {System.IO.File.Exists(skpbFullPath)}");

                if (System.IO.File.Exists(skpbFullPath))
                {
                    var fileName = string.IsNullOrEmpty(actualFileName) ? Path.GetFileName(skpbFullPath) : actualFileName;
                    files.Add(new
                    {
                        type = "SKPB",
                        fileName = fileName,
                        url = $"/uploads/dropout/skpb/{fileName}",
                        downloadUrl = $"/api/DropOut/skpb/{fileName}"  // Fixed: Use correct endpoint
                    });
                    Console.WriteLine($"Added SKPB file: '{fileName}'");
                }
                else
                {
                    Console.WriteLine($"SKPB file not found at: '{skpbFullPath}'");
                }
            }
            else
            {
                Console.WriteLine("SKPB is null or empty");
            }

            Console.WriteLine($"Total files found: {files.Count}");

            if (files.Count == 0)
                return NotFound(new { message = "Tidak ada file SK/SKPB yang tersedia" });

            return Ok(new
            {
                message = "File SK dan SKPB tersedia",
                doId = id,
                files = files,
                debug = new
                {
                    skFromDb = result.Sk,
                    skpbFromDb = result.Skpb,
                    webRootPath = webRootPath
                }
            });
        }

        // Endpoint untuk mendapatkan template code sebagai DTO
        [HttpGet("template-code")]
        public IActionResult GetTemplateCode()
        {
            try
            {
                var aspxCode = @"<%@ Page Title="""" Language=""C#"" AutoEventWireup=""true"" CodeBehind=""SK_Drop_Out.aspx.cs"" Inherits=""PolmanAstra_SIA.Reports.SK_Drop_Out"" %>
<!DOCTYPE html>
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head runat=""server"">
    <title></title>
</head>
<body>
    <form id=""form1"" runat=""server"" autocomplete=""off"">
        <div>
            <asp:Label runat=""server"" ID=""err""></asp:Label>
        </div>
    </form>
</body>
</html>";

                var aspxCsCode = @"using System;using System.Configuration;using PolmanAstra_SIA.Classes;using CrystalDecisions.CrystalReports.Engine;using CrystalDecisions.Shared;using System.Data;using System.Web.Configuration;namespace PolmanAstra_SIA.Reports{public partial class SK_Drop_Out : System.Web.UI.Page{PolmanAstraLibrary.PolmanAstraLibrary lib = new PolmanAstraLibrary.PolmanAstraLibrary(PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(ConfigurationManager.ConnectionStrings[""DefaultConnection""].ToString(), ""PoliteknikAstra_ConfigurationKey""));LDAPAuthentication adAuth = new LDAPAuthentication();DataTable dt = new DataTable();protected void Page_Load(object sender, EventArgs e){            try{using (ReportDocument reportdocument = new ReportDocument()){String id = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(Request.QueryString[""token""].ToString(), ""PolmanAstra_SIA"").Split('#')[0];dt = lib.CallProcedure(""sia_detailDO"", new string[] { id });String rep = lib.CallProcedure(""sia_checkReportDropOut"", new string[] { id }).Rows[0][0].ToString();//reportdocument.Load(Server.MapPath(""Report_SK_Drop_Out"" + rep));reportdocument.Load(Server.MapPath(""Report_SK_Drop_Out_2.rpt""));reportdocument.SetDatabaseLogon(PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(WebConfigurationManager.AppSettings[""linkUserID""], ""PoliteknikAstra_ConfigurationKey""), PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(WebConfigurationManager.AppSettings[""linkPassword""], ""PoliteknikAstra_ConfigurationKey""), PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(WebConfigurationManager.AppSettings[""linkServerName""], ""PoliteknikAstra_ConfigurationKey""), PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(WebConfigurationManager.AppSettings[""linkDatabaseName""], ""PoliteknikAstra_ConfigurationKey""));reportdocument.SetParameterValue(""@p1"", id);reportdocument.SetParameterValue(""@p2"", """");reportdocument.SetParameterValue(""@p3"", """");reportdocument.SetParameterValue(""@p4"", """");reportdocument.SetParameterValue(""@p5"", """");reportdocument.SetParameterValue(""@p6"", """");reportdocument.SetParameterValue(""@p7"", """");reportdocument.SetParameterValue(""@p8"", """");reportdocument.SetParameterValue(""@p9"", """");reportdocument.SetParameterValue(""@p10"", """");reportdocument.SetParameterValue(""@p11"", """");reportdocument.SetParameterValue(""@p12"", """");reportdocument.SetParameterValue(""@p13"", """");reportdocument.SetParameterValue(""@p14"", """");reportdocument.SetParameterValue(""@p15"", """");reportdocument.SetParameterValue(""@p16"", """");reportdocument.SetParameterValue(""@p17"", """");reportdocument.SetParameterValue(""@p18"", """");reportdocument.SetParameterValue(""@p19"", """");reportdocument.SetParameterValue(""@p20"", """");reportdocument.SetParameterValue(""@p21"", """");reportdocument.SetParameterValue(""@p22"", """");reportdocument.SetParameterValue(""@p23"", """");reportdocument.SetParameterValue(""@p24"", """");reportdocument.SetParameterValue(""@p25"", """");reportdocument.SetParameterValue(""@p26"", """");reportdocument.SetParameterValue(""@p27"", """");reportdocument.SetParameterValue(""@p28"", """");reportdocument.SetParameterValue(""@p29"", """");reportdocument.SetParameterValue(""@p30"", """");reportdocument.SetParameterValue(""@p31"", """");reportdocument.SetParameterValue(""@p32"", """");reportdocument.SetParameterValue(""@p33"", """");reportdocument.SetParameterValue(""@p34"", """");reportdocument.SetParameterValue(""@p35"", """");reportdocument.SetParameterValue(""@p36"", """");reportdocument.SetParameterValue(""@p37"", """");reportdocument.SetParameterValue(""@p38"", """");reportdocument.SetParameterValue(""@p39"", """");reportdocument.SetParameterValue(""@p40"", """");reportdocument.SetParameterValue(""@p41"", """");reportdocument.SetParameterValue(""@p42"", """");reportdocument.SetParameterValue(""@p43"", """");reportdocument.SetParameterValue(""@p44"", """");reportdocument.SetParameterValue(""@p45"", """");reportdocument.SetParameterValue(""@p46"", """");reportdocument.SetParameterValue(""@p47"", """");reportdocument.SetParameterValue(""@p48"", """");reportdocument.SetParameterValue(""@p49"", """");reportdocument.SetParameterValue(""@p50"", """");reportdocument.SetParameterValue(""@p1"", id, reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p2"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p3"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p4"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p5"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p6"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p7"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p8"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p9"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p10"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p11"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p12"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p13"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p14"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p15"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p16"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p17"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p18"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p19"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p20"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p21"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p22"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p23"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p24"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p25"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p26"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p27"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p28"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p29"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p30"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p31"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p32"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p33"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p34"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p35"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p36"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p37"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p38"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p39"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p40"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p41"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p42"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p43"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p44"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p45"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p46"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p47"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p48"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p49"", """", reportdocument.Subreports[0].Name);reportdocument.SetParameterValue(""@p50"", """", reportdocument.Subreports[0].Name);reportdocument.ExportToHttpResponse(ExportFormatType.PortableDocFormat, Response, false, ""SK_Drop_Out_No."" + dt.Rows[0][9].ToString());}}catch{err.Text = ""Error:<br>- Terjadi kesalahan dalam pembuatan surat. Mohon hubungi MIS!"";}}}};";

                var aspxDesignerCode = @"//------------------------------------------------------------------------------// <auto-generated>//     This code was generated by a tool.////     Changes to this file may cause incorrect behavior and will be lost if//     the code is regenerated. // </auto-generated>//------------------------------------------------------------------------------namespace PolmanAstra_SIA.Reports {public partial class SK_Drop_Out {/// <summary>/// form1 control./// </summary>/// <remarks>/// Auto-generated field./// To modify move field declaration from designer file to code-behind file./// </remarks>protected global::System.Web.UI.HtmlControls.HtmlForm form1;/// <summary>/// err control./// </summary>/// <remarks>/// Auto-generated field./// To modify move field declaration from designer file to code-behind file./// </remarks>protected global::System.Web.UI.WebControls.Label err;}}";

                var reportLogic = @"ALUR SISTEM DOWNLOAD TEMPLATE SK & SKPB:

INPUT: ID Mahasiswa
OUTPUT: Download Template SK dan SKPB

CARA KERJA KODE ASLI:
1. USER INPUT: ID Mahasiswa (encrypted dalam query string token)
2. DECRYPT ID: Ambil ID dari token yang di-encrypt
3. AMBIL DATA: Execute stored procedure 'sia_detailDO' dengan parameter ID
4. LOAD TEMPLATE: Load Crystal Report 'Report_SK_Drop_Out_2.rpt'
5. SET CONNECTION: Set database connection dengan encrypted credentials
6. SET PARAMETERS: Set 98 parameter (@p1-@p50 untuk main report dan subreport)
   - @p1 = ID mahasiswa (parameter utama)
   - @p2-@p50 = kosong (parameter tambahan)
7. GENERATE PDF: Crystal Report generate PDF berdasarkan template dan data
8. DOWNLOAD: User langsung download file PDF SK

KODE ASLI (98 BARIS PARAMETER):
- Main report: 49 baris SetParameterValue (@p1-@p50)
- Subreport: 49 baris SetParameterValue (@p1-@p50)
- Total: 98 baris (TIDAK DIOPTIMASI, PAKAI KODE ASLI)

CRYSTAL REPORT YANG BERPERAN:
- Template design (.rpt file)
- Database connection
- Parameter binding
- PDF generation";

                var response = new TemplateCodeResponse
                {
                    AspxCode = aspxCode,
                    AspxCsCode = aspxCsCode,
                    AspxDesignerCode = aspxDesignerCode,
                    ReportLogic = reportLogic,
                    Description = "Template ASP.NET WebForms untuk download SK dan SKPB Drop Out (KODE ASLI - 98 baris parameter)",
                    RequiredFiles = new List<string>
                    {
                        "SK_Drop_Out.aspx",
                        "SK_Drop_Out.aspx.cs", 
                        "SK_Drop_Out.aspx.designer.cs",
                        "Report_SK_Drop_Out_2.rpt",
                        "PolmanAstraLibrary.dll",
                        "CrystalDecisions.CrystalReports.Engine.dll"
                    },
                    ConfigurationSteps = new Dictionary<string, string>
                    {
                        { "1", "Deploy files ke folder /Reports/ di IIS server" },
                        { "2", "Pastikan Crystal Reports runtime terinstall di server" },
                        { "3", "Set connection string 'DefaultConnection' di web.config (encrypted)" },
                        { "4", "Set encrypted credentials di appSettings (linkUserID, linkPassword, linkServerName, linkDatabaseName)" },
                        { "5", "Pastikan stored procedure 'sia_detailDO' dan 'sia_checkReportDropOut' tersedia" },
                        { "6", "Test akses: /Reports/SK_Drop_Out.aspx?token=encrypted_mahasiswa_id" },
                        { "7", "ALUR: Input ID Mahasiswa → Crystal Report generate → Download PDF SK & SKPB" }
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Error getting template code", 
                    error = ex.Message 
                });
            }
        }

        // Endpoint untuk download template SK berdasarkan ID mahasiswa (sesuai pola dokumentasi)
        [RequiresPermission("drop_out.view")]
        [HttpPost("DownloadTemplateSK/{mahasiswaId}")]
        public async Task<IActionResult> DownloadTemplateSKByMahasiswa(string mahasiswaId)
        {
            try
            {
                // TEMPORARY: Check if we should use mock/dummy response
                var useMock = Configuration["ReportService:UseMock"] == "true";
                
                if (useMock)
                {
                    // Return dummy response for testing
                    return Ok(new { 
                        message = "MOCK MODE: Template SK download untuk testing",
                        mahasiswaId = mahasiswaId,
                        reportServiceUrl = Configuration["ReportService:Url"],
                        note = "Set ReportService:UseMock = false di appsettings.json untuk menggunakan service report asli"
                    });
                }

                // Call service report untuk generate template SK
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                
                var reportServiceUrl = Configuration["ReportService:Url"] ?? "http://10.5.0.94/api/Report/GetReport";

                Console.WriteLine($"=== Calling Report Service for Template SK ===");
                Console.WriteLine($"URL: {reportServiceUrl}");
                Console.WriteLine($"MahasiswaId: {mahasiswaId}");

                // Request body sesuai dengan kode asli yang diberikan
                var requestBody = new
                {
                    reportName = "Report_SK_Drop_Out_2",
                    parameters = new
                    {
                        mahasiswaId = mahasiswaId,
                        // Parameter sesuai kode asli (98 parameter)
                        p1 = mahasiswaId,
                        p2 = "",
                        p3 = "",
                        p4 = "",
                        p5 = "",
                        p6 = "",
                        p7 = "",
                        p8 = "",
                        p9 = "",
                        p10 = "",
                        // ... parameter lainnya akan di-handle oleh service report
                    }
                };

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(requestBody),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                Console.WriteLine($"Request Body: {System.Text.Json.JsonSerializer.Serialize(requestBody)}");

                // POST ke service report
                var response = await client.PostAsync(reportServiceUrl, content);

                Console.WriteLine($"Response Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Response: {errorContent}");
                    
                    return StatusCode((int)response.StatusCode, new { 
                        message = "Gagal mengambil template SK dari service report",
                        error = errorContent,
                        reportServiceUrl = reportServiceUrl
                    });
                }

                // Baca PDF sebagai byte array
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                Console.WriteLine($"Template PDF Size: {pdfBytes.Length} bytes");

                // Return file PDF template ke client
                var fileName = $"Template_SK_DropOut_{mahasiswaId}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HttpRequestException: {ex.Message}");
                
                return StatusCode(503, new { 
                    message = "Service report tidak dapat diakses untuk template download",
                    error = ex.Message,
                    reportServiceUrl = Configuration["ReportService:Url"]
                });
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"TaskCanceledException (Timeout): {ex.Message}");
                
                return StatusCode(504, new { 
                    message = "Request template download timeout",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                
                return StatusCode(500, new { 
                    message = "Terjadi kesalahan saat download template SK",
                    error = ex.Message 
                });
            }
        }

        //IHIRRRRR
    }
}


