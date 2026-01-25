using astratech_apps_backend.DTOs.DropOut;
using astratech_apps_backend.DTOs.PengunduranDiri;
using astratech_apps_backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace astratech_apps_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DropOutController : ControllerBase
    {
        private readonly IDropOutService _service;
        private readonly IConfiguration Configuration;

        public DropOutController(IDropOutService service, IConfiguration configuration)
        {
            _service = service;
            Configuration = configuration;
        }



        [Authorize]
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
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string keyword = "",
            [FromQuery] string sortBy = "a.dro_created_date desc",
            [FromQuery] string konsentrasi = "")
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
            
            return Ok(await _service.GetRiwayatAsync(username, keyword, sortBy, konsentrasi, roleToUse, displayName));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var data = await _service.GetByIdAsync(id);
            return data == null ? NotFound() : Ok(data);
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetDetail([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "ID wajib diisi." });

            var data = await _service.GetDetailAsync(id);

            if (data == null)
                return NotFound(new { message = "Data Drop Out tidak ditemukan." });

            return Ok(data);
        }



        [HttpGet("{id}/check-report")]
        public async Task<IActionResult> CheckReport(string id)
        {
            var data = await _service.CheckReportAsync(id);

            if (data == null)
                return NotFound(new { message = "Report suffix tidak ditemukan." });

            return Ok(new { suffix = data });
        }

        [HttpGet("report-suket/{suratNo}")]
        public async Task<IActionResult> GetReportSuket(string suratNo)
        {
            var data = await _service.GetReportSuketAsync(suratNo);

            if (data == null)
                return NotFound(new { message = "Data surat keterangan tidak ditemukan." });

            return Ok(data);
        }

        [HttpGet("download-sk/{droId}")]
        public async Task<IActionResult> DownloadSK(string droId)
        {
            var result = await _service.DownloadSKAsync(droId);

            if (result == null)
                return NotFound(new { message = "SK tidak ditemukan untuk DropOut ini." });

            return Ok(result);
        }

        [HttpGet("download-sk-file/{droId}")]
        public async Task<IActionResult> DownloadSKFile(string droId)
        {
            var result = await _service.DownloadSKAsync(droId);

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
        //    var data = await _service.GetRiwayatAsync(
        //        username, keyword, sortBy, konsentrasi, role, sekprodi
        //    );

        //    return Ok(data);
        //}

        [HttpGet("riwayat")]
        public async Task<IActionResult> GetRiwayat(
           [FromQuery] string username,
           [FromQuery] string keyword = "",
           [FromQuery] string sortBy = "a.dro_created_date desc",
           [FromQuery] string konsentrasi = "",
           [FromQuery] string role = "",
           [FromQuery] string displayName = "")
        {
            var data = await _service.GetRiwayatAsync(username, keyword, sortBy, konsentrasi, role, displayName);
            return Ok(data);
        }


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
            var data = await _service.GetRiwayatExcelAsync(
                username, keyword, sortBy, konsentrasi, role, sekprodi
            );

            return Ok(data);
        }

        [HttpGet("{id}/report-sk")]
        public async Task<IActionResult> GetReportSK(string id)
        {
            var data = await _service.GetReportSKDOAsync(id);

            if (data == null)
                return NotFound(new { message = "Data laporan SK DO tidak ditemukan." });

            return Ok(data);
        }

        [HttpGet("{id}/report-sk-sub")]
        public async Task<IActionResult> GetReportSKDOSub(string id)
        {
            var data = await _service.GetReportSKDOSubAsync(id);

            if (data == null || data.Count == 0)
                return NotFound(new { message = "Subreport SK DO tidak ditemukan." });

            return Ok(data);
        }






        [Authorize]
        [HttpPost("create-pengajuan")]
        public async Task<IActionResult> CreatePengajuanDO([FromBody] CreatePengajuanDORequest dto)
        {
            // Prioritas: dari JWT token, fallback ke body, fallback ke "system"
            var createdBy = User.FindFirst("namaakun")?.Value 
                         ?? dto.CreatedBy 
                         ?? "system";

            var newId = await _service.CreatePengajuanDOAsync(dto, createdBy);

            if (newId == null)
                return BadRequest(new { message = "Gagal membuat pengajuan Draft DropOut." });

            return Ok(new
            {
                message = "Pengajuan DO Draft berhasil dibuat.",
                id = newId,
                createdBy = createdBy
            });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateDropOutRequest dto)
        {
            var updatedBy = User.FindFirst("namaakun")?.Value ?? "system";
            var success = await _service.UpdateAsync(id, dto, updatedBy);

            if (!success)
                return NotFound(new { message = "Gagal memperbarui data Drop Out." });

            return Ok(new { message = "Data Drop Out berhasil diperbarui." });
        }

        [Authorize]
        [HttpPut("draft/{id}/generate-id")]
        public async Task<IActionResult> GenerateIdFromDraft(string id)
        {
            try
            {
                Console.WriteLine($"DEBUG GenerateIdFromDraft - Input ID: '{id}'");
                
                if (string.IsNullOrEmpty(id))
                    return BadRequest(new { message = "ID draft wajib diisi" });

                var data = await _service.GetIdByDraftAsync(id);

                if (data == null)
                {
                    Console.WriteLine($"DEBUG GenerateIdFromDraft - No data returned for ID: '{id}'");
                    return NotFound(new { message = "Draft tidak ditemukan atau gagal generate ID" });
                }

                Console.WriteLine($"DEBUG GenerateIdFromDraft - Success, new ID: '{data.Id}'");
                return Ok(new { 
                    message = "ID DO berhasil di-generate",
                    oldId = id,
                    newId = data.Id 
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


        [Authorize]
        [HttpPut("wadir/approve")]
        public async Task<IActionResult> ApproveByWadir(
            [FromQuery] string id,
            [FromBody] ApproveDropOutRequest dto)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "ID wajib diisi." });

            var success = await _service.ApproveByWadirAsync(id, dto);

            if (!success)
                return BadRequest(new { message = "Approve gagal." });

            return Ok(new { message = "Drop Out berhasil disetujui Wadir 1" });
        }

        [Authorize]
        [HttpPut("wadir/reject")]
        public async Task<IActionResult> RejectByWadir(
            [FromQuery] string id,
            [FromBody] RejectDropOutRequest dto)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "ID wajib diisi." });

            var success = await _service.RejectByWadirAsync(id, dto);

            if (!success)
                return BadRequest(new { message = "Reject gagal." });

            return Ok(new { message = "Drop Out berhasil ditolak" });
        }

        [HttpPut("upload-sk")]
        public async Task<IActionResult> UploadSKDO([FromBody] UploadSKDORequest request)
        {
            var result = await _service.UploadSKDOAsync(request);

            if (!result)
                return BadRequest(new { message = "Gagal upload SK DO" });

            return Ok(new { message = "Upload SK DO berhasil" });
        }

        [Authorize]
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
            
            // Generate nomor SK untuk penamaan file
            string nomorSK = await GenerateNoSKAsync(request.DroId);

            // Upload SK file ke folder sk/
            if (request.SkFile != null && request.SkFile.Length > 0)
            {
                var ext = Path.GetExtension(request.SkFile.FileName);
                // Gunakan nomor SK sebagai nama file
                var skFileName = $"SK_DO_{nomorSK.Replace("/", "-")}{ext}";
                var skFullPath = Path.Combine(skFolderPath, skFileName);
                
                using (var stream = new FileStream(skFullPath, FileMode.Create))
                {
                    await request.SkFile.CopyToAsync(stream);
                }
                skPath = $"/uploads/dropout/sk/{skFileName}";
            }

            // Upload SKPB file ke folder skpb/
            if (request.SkpbFile != null && request.SkpbFile.Length > 0)
            {
                var ext = Path.GetExtension(request.SkpbFile.FileName);
                // Gunakan nomor SK sebagai nama file
                var skpbFileName = $"SKPB_DO_{nomorSK.Replace("/", "-")}{ext}";
                var skpbFullPath = Path.Combine(skpbFolderPath, skpbFileName);
                
                using (var stream = new FileStream(skpbFullPath, FileMode.Create))
                {
                    await request.SkpbFile.CopyToAsync(stream);
                }
                skpbPath = $"/uploads/dropout/skpb/{skpbFileName}";
            }

            // Update database dengan path file
            var modifiedBy = User.FindFirst("namaakun")?.Value ?? "system";
            var uploadRequest = new UploadSKDORequest
            {
                DroId = request.DroId,
                SK = skPath,
                SKPB = skpbPath,
                ModifiedBy = modifiedBy
            };

            var result = await _service.UploadSKDOAsync(uploadRequest);

            if (!result)
                return BadRequest(new { message = "Gagal menyimpan data SK DO ke database" });

            return Ok(new { 
                message = "Upload SK DO berhasil",
                nomorSK = nomorSK,
                skPath = skPath,
                skpbPath = skpbPath
            });
        }

        /// <summary>
        /// Generate nomor SK untuk Drop Out
        /// </summary>
        private async Task<string> GenerateNoSKAsync(string droId)
        {
            try
            {
                // Get data Drop Out untuk ambil konsentrasi
                var detail = await _service.GetByIdAsync(droId);
                if (detail == null)
                    return $"SK_DO_{DateTime.Now:yyyyMMddHHmmss}";

                // TODO: Sesuaikan jenisSuratId dengan ID di database
                // Contoh: "JS001" untuk jenis surat Drop Out
                string jenisSuratId = "JS_DROP_OUT"; // Ganti dengan ID yang sesuai
                
                var connString = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
                    Configuration.GetConnectionString("DefaultConnection")!,
                    Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
                );
                
                var generator = new Helpers.NoSuratGenerator(connString);
                
                // Generate nomor surat
                // Jika ada konsentrasi ID, bisa ditambahkan sebagai parameter kedua
                return await generator.GenerateNoSKForFileAsync(jenisSuratId);
            }
            catch
            {
                // Fallback jika gagal generate
                return $"SK_DO_{DateTime.Now:yyyyMMddHHmmss}";
            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _service.DeleteAsync(id);

            if (!success)
                return NotFound(new { message = "Data DropOut tidak ditemukan atau gagal dihapus." });

            return Ok(new { message = "DropOut berhasil dihapus." });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(
    [FromQuery] string username,
    [FromQuery] string keyword = "",
    [FromQuery] string sortBy = "a.dro_created_date desc",
    [FromQuery] string konsentrasi = "",
    [FromQuery] string role = "",
    [FromQuery] string displayName = ""
)
        {
            return Ok(await _service.GetPendingAsync(
                username, keyword, sortBy, konsentrasi, role, displayName
            ));
        }

        [HttpGet("mahasiswa-by-konsentrasi")]
        public async Task<IActionResult> GetMahasiswaByKonsentrasi(
    [FromQuery] string konsentrasiId)
        {
            if (string.IsNullOrEmpty(konsentrasiId))
                return BadRequest(new { message = "Konsentrasi wajib diisi." });

            var data = await _service.GetMahasiswaByKonsentrasiAsync(konsentrasiId);

            return Ok(data);
        }

        [Authorize]
        [HttpGet("prodi")]
        public async Task<IActionResult> GetProdi()
        {
            var username = User.FindFirst("namaakun")?.Value ?? "";
            return Ok(await _service.GetProdiAsync(username));
        }

        [HttpGet("prodi/list")]
        public async Task<IActionResult> GetListProdi()
        {
            return Ok(await _service.GetListProdiAsync());
        }

        [Authorize]
        [HttpGet("konsentrasi")]
        public async Task<IActionResult> GetKonsentrasiByProdi(
    [FromQuery] string prodiId
)
        {
            if (string.IsNullOrEmpty(prodiId))
                return BadRequest("prodiId wajib diisi");

            // username dipakai utk filter sekprodi (opsional)
            var username = User.FindFirst("namaakun")?.Value ?? "";

            var data = await _service.GetKonsentrasiByProdiAsync(prodiId, username);
            return Ok(data);
        }




        [HttpGet("mahasiswa")]
        public async Task<IActionResult> GetMahasiswa([FromQuery] string konsentrasiId)
        {
            return Ok(await _service.GetMahasiswaByKonsentrasiAsync(konsentrasiId));
        }


        [HttpGet("angkatan-by-mahasiswa")]
        public async Task<IActionResult> GetAngkatanByMahasiswa(
    [FromQuery] string mhsId
)
        {
            if (string.IsNullOrEmpty(mhsId))
                return BadRequest(new { message = "Mahasiswa ID wajib diisi." });

            var angkatan = await _service.GetAngkatanByMahasiswaAsync(mhsId);

            if (angkatan == null)
                return NotFound(new { message = "Angkatan mahasiswa tidak ditemukan." });

            return Ok(new { angkatan });
        }

        [HttpGet("mahasiswa/{mhsId}/bebas-tanggungan")]
        public async Task<IActionResult> CekBebasTanggungan(string mhsId)
        {
            var result = await _service.CekBebasTanggunganAsync(mhsId);
            
            if (result == null)
                return NotFound(new { message = "Data mahasiswa tidak ditemukan" });
                
            return Ok(result);
        }

        [HttpGet("mahasiswa/{mhsId}/profil")]
        public async Task<IActionResult> GetProfilMahasiswaDetail(string mhsId)
        {
            var result = await _service.GetProfilMahasiswaDetailAsync(mhsId);
            
            if (result == null)
                return NotFound(new { message = "Data profil mahasiswa tidak ditemukan" });
                
            return Ok(result);
        }

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
                    new { type = "aspx-designer", name = "SK_Drop_Out.aspx.designer.cs", description = "Designer code ASP.NET", url = "/api/dropout/template-sk?type=aspx-designer" }
                }
            });
        }

        // Download file SK dengan query parameter (untuk ID yang ada slash)
        [HttpGet("download-sk-file")]
        public async Task<IActionResult> DownloadSKFileByQuery([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "Parameter id wajib diisi" });

            var result = await _service.DownloadSKAsync(id);

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

        // Download SK + SKPB sekaligus dalam ZIP
        [HttpGet("download-all-sk")]
        public async Task<IActionResult> DownloadAllSK([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "Parameter id wajib diisi" });

            var result = await _service.DownloadSKAsync(id);
            if (result == null)
                return NotFound(new { message = "Data tidak ditemukan" });

            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var files = new List<(string path, string name)>();

            // Cek SK
            if (!string.IsNullOrEmpty(result.Sk))
            {
                var skPath = Path.Combine(webRootPath, result.Sk.TrimStart('/').Replace("/", "\\"));
                if (System.IO.File.Exists(skPath))
                    files.Add((skPath, "SK_" + Path.GetFileName(skPath)));
            }

            // Cek SKPB
            if (!string.IsNullOrEmpty(result.Skpb))
            {
                var skpbPath = Path.Combine(webRootPath, result.Skpb.TrimStart('/').Replace("/", "\\"));
                if (System.IO.File.Exists(skpbPath))
                    files.Add((skpbPath, "SKPB_" + Path.GetFileName(skpbPath)));
            }

            if (files.Count == 0)
                return NotFound(new { message = "Tidak ada file SK/SKPB yang tersedia" });

            // Buat ZIP
            using var memoryStream = new MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                foreach (var (path, name) in files)
                {
                    var entry = archive.CreateEntry(name);
                    using var entryStream = entry.Open();
                    using var fileStream = System.IO.File.OpenRead(path);
                    await fileStream.CopyToAsync(entryStream);
                }
            }

            memoryStream.Position = 0;
            var safeId = id.Replace("/", "-");
            return File(memoryStream.ToArray(), "application/zip", $"SK_SKPB_DO_{safeId}.zip");
        }

        //IHIRRRRR
    }
}
