using astratech_apps_backend.DTOs.PengunduranDiri;
using astratech_apps_backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace astratech_apps_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PengunduranDiriController : ControllerBase
    {
        private readonly IPengunduranDiriService _service;
        private readonly IConfiguration Configuration;

        public PengunduranDiriController(IPengunduranDiriService service, IConfiguration configuration)
        {
            _service = service;
            Configuration = configuration;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string p1 = "",
            [FromQuery] string status = "")
        {
            // Ambil username dari JWT token (claim "namaakun")
            var userId = User.FindFirst("namaakun")?.Value ?? "";
            
            Console.WriteLine($"DEBUG GetAll - p1: '{p1}', status: '{status}', userId: '{userId}'");
            
            var result = await _service.GetAllAsync(p1, status, userId);
            
            Console.WriteLine($"DEBUG GetAll - Result count: {result.Count()}");
            
            return Ok(result);
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetDetail([FromQuery] string id)
        {
            var detail = await _service.GetDetailAsync(id);

            if (detail == null)
                return NotFound(new { message = "Data tidak ditemukan" });

            return Ok(detail);
        }

        // Upload file lampiran (bisa 1 atau 2 file sekaligus)
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile? lampiranSuratPengajuan, IFormFile? lampiran)
        {
            if ((lampiranSuratPengajuan == null || lampiranSuratPengajuan.Length == 0) && 
                (lampiran == null || lampiran.Length == 0))
                return BadRequest(new { message = "Minimal satu file harus diupload" });

            var baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/pengundurandiri");
            var folderSuratPengajuan = Path.Combine(baseFolder, "suratpengajuan");
            var folderLampiran = Path.Combine(baseFolder, "lampiran");

            if (!Directory.Exists(folderSuratPengajuan))
                Directory.CreateDirectory(folderSuratPengajuan);
            if (!Directory.Exists(folderLampiran))
                Directory.CreateDirectory(folderLampiran);

            string? fileNameSuratPengajuan = null;
            string? fileNameLampiran = null;
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            // Upload Lampiran Surat Pengajuan
            if (lampiranSuratPengajuan != null && lampiranSuratPengajuan.Length > 0)
            {
                var originalName = Path.GetFileNameWithoutExtension(lampiranSuratPengajuan.FileName);
                var ext = Path.GetExtension(lampiranSuratPengajuan.FileName);
                fileNameSuratPengajuan = $"{originalName}_{timestamp}{ext}";
                var filePath = Path.Combine(folderSuratPengajuan, fileNameSuratPengajuan);
                using var stream = new FileStream(filePath, FileMode.Create);
                await lampiranSuratPengajuan.CopyToAsync(stream);
            }

            // Upload Lampiran
            if (lampiran != null && lampiran.Length > 0)
            {
                var originalName = Path.GetFileNameWithoutExtension(lampiran.FileName);
                var ext = Path.GetExtension(lampiran.FileName);
                fileNameLampiran = $"{originalName}_{timestamp}{ext}";
                var filePath = Path.Combine(folderLampiran, fileNameLampiran);
                using var stream = new FileStream(filePath, FileMode.Create);
                await lampiran.CopyToAsync(stream);
            }

            return Ok(new { 
                lampiranSuratPengajuan = fileNameSuratPengajuan,
                lampiran = fileNameLampiran,
                message = "File berhasil diupload" 
            });
        }

        // Download file lampiran (cari otomatis di folder root, suratpengajuan, atau lampiran)
        [HttpGet("file/{filename}")]
        public IActionResult DownloadFile(string filename)
        {
            var baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "pengundurandiri");
            
            Console.WriteLine($"DEBUG DownloadFile - filename: {filename}");
            Console.WriteLine($"DEBUG DownloadFile - baseFolder: {baseFolder}");
            
            // Cek di folder root dulu
            var pathRoot = Path.Combine(baseFolder, filename);
            Console.WriteLine($"DEBUG DownloadFile - pathRoot: {pathRoot}, exists: {System.IO.File.Exists(pathRoot)}");
            if (System.IO.File.Exists(pathRoot))
            {
                var fileBytes = System.IO.File.ReadAllBytes(pathRoot);
                return File(fileBytes, "application/octet-stream", filename);
            }

            // Cek di folder suratpengajuan
            var pathSuratPengajuan = Path.Combine(baseFolder, "suratpengajuan", filename);
            Console.WriteLine($"DEBUG DownloadFile - pathSuratPengajuan: {pathSuratPengajuan}, exists: {System.IO.File.Exists(pathSuratPengajuan)}");
            if (System.IO.File.Exists(pathSuratPengajuan))
            {
                var fileBytes = System.IO.File.ReadAllBytes(pathSuratPengajuan);
                return File(fileBytes, "application/octet-stream", filename);
            }

            // Cek di folder lampiran
            var pathLampiran = Path.Combine(baseFolder, "lampiran", filename);
            Console.WriteLine($"DEBUG DownloadFile - pathLampiran: {pathLampiran}, exists: {System.IO.File.Exists(pathLampiran)}");
            if (System.IO.File.Exists(pathLampiran))
            {
                var fileBytes = System.IO.File.ReadAllBytes(pathLampiran);
                return File(fileBytes, "application/octet-stream", filename);
            }

            return NotFound(new { message = "File tidak ditemukan", searchedPaths = new[] { pathRoot, pathSuratPengajuan, pathLampiran } });
        }


        [HttpGet("notif/{id}")]
        public async Task<IActionResult> GetNotif(string id)
        {
            var data = await _service.GetNotifAsync(id);

            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            return Ok(data);
        }

        [HttpGet("riwayat")]
        public async Task<IActionResult> GetRiwayat(
        [FromQuery] string status = "",
        [FromQuery] string keyword = "",
        [FromQuery] string orderBy = "",
        [FromQuery] string konsentrasi = ""
        )
        {
            var username = User?.Identity?.Name ?? "SYSTEM";

            var data = await _service.GetRiwayatAsync(
                username,
                status,
                keyword,
                orderBy,
                konsentrasi
            );

            return Ok(data);
        }

        [HttpGet("riwayat-excel")]
        public async Task<IActionResult> GetRiwayatExcel(
        [FromQuery] string orderBy = "",
        [FromQuery] string konsentrasi = ""
        )
        {
            var data = await _service.GetRiwayatExcelAsync(orderBy, konsentrasi);
            return Ok(data);
        }

        //[HttpGet("riwayat-excel/download")]
        //public async Task<IActionResult> DownloadExcel(
        //[FromQuery] string orderBy = "",
        //[FromQuery] string konsentrasi = ""
        //)
        //{
        //    var data = await _service.GetRiwayatExcelAsync(orderBy, konsentrasi);

        //    using var wb = new XLWorkbook();
        //    var ws = wb.Worksheets.Add("Riwayat");

        //    ws.Cell(1, 1).Value = "NIM";
        //    ws.Cell(1, 2).Value = "Nama Mahasiswa";
        //    ws.Cell(1, 3).Value = "Konsentrasi";
        //    ws.Cell(1, 4).Value = "Tanggal Pengajuan";
        //    ws.Cell(1, 5).Value = "No SK";
        //    ws.Cell(1, 6).Value = "No Pengajuan";

        //    int row = 2;
        //    foreach (var item in data)
        //    {
        //        ws.Cell(row, 1).Value = item.NIM;
        //        ws.Cell(row, 2).Value = item.NamaMahasiswa;
        //        ws.Cell(row, 3).Value = item.Konsentrasi;
        //        ws.Cell(row, 4).Value = item.TanggalPengajuan;
        //        ws.Cell(row, 5).Value = item.NoSk;
        //        ws.Cell(row, 6).Value = item.NoPengajuan;
        //        row++;
        //    }

        //    using var stream = new MemoryStream();
        //    wb.SaveAs(stream);
        //    stream.Position = 0;

        //    return File(stream.ToArray(),
        //        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //        "RiwayatPengunduranDiri.xlsx");
        //}

        // POST /create - Buat Draft dengan lampiran
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreatePengunduranDiriRequest dto)
        {
            var createdBy = dto.CreatedBy ?? "system";
            
            // STEP1: Buat draft dengan lampiran
            var draftId = await _service.CreateStep1Async(
                dto.MhsId, 
                createdBy, 
                dto.LampiranSuratPengajuan, 
                dto.Lampiran
            );
            
            if (string.IsNullOrEmpty(draftId))
                return BadRequest(new { message = "Gagal membuat draft pengunduran diri" });

            return Ok(new { 
                message = "Draft berhasil dibuat",
                draftId = draftId,
                status = "Draft"
            });
        }

        // PUT /submit/{draftId} - Ajukan Draft (STEP2) → status jadi "Belum Disetujui Prodi"
        [HttpPut("submit/{draftId}")]
        public async Task<IActionResult> Submit(string draftId)
        {
            var modifiedBy = User.FindFirst("namaakun")?.Value ?? "system";
            
            // STEP2: Generate ID resmi dan ubah status
            var data = await _service.CreateStep2Async(draftId, modifiedBy);

            if (data == null)
                return BadRequest(new { message = "Gagal mengajukan pengunduran diri" });

            return Ok(data);
        }

        [HttpPost("create-by-prodi")]
        public async Task<IActionResult> CreateByProdi([FromBody] CreatePengunduranDiriByProdiRequest dto)
        {
            var result = await _service.CreateByProdiAsync(dto);
            return Ok(result);
        }

        // STEP 1 - Buat Draft by Prodi
        [HttpPost("create-by-prodi/draft")]
        public async Task<IActionResult> CreateByProdiStep1([FromBody] CreatePengunduranDiriByProdiRequest dto)
        {
            Console.WriteLine($"DEBUG CreateByProdiStep1 - MhsId: '{dto.MhsId}', CreatedBy: '{dto.CreatedBy}'");
            
            var draftId = await _service.CreateByProdiStep1Async(
                dto.MhsId,
                dto.CreatedBy,
                dto.LampiranSuratPengajuan,
                dto.Lampiran
            );

            if (string.IsNullOrEmpty(draftId))
                return BadRequest(new { message = "Gagal membuat draft pengunduran diri" });

            return Ok(new
            {
                message = "Draft berhasil dibuat",
                draftId = draftId,
                status = "Draft"
            });
        }

        // STEP 2 - Submit Draft by Prodi
        [HttpPut("create-by-prodi/submit/{draftId}")]
        public async Task<IActionResult> CreateByProdiStep2(string draftId)
        {
            var modifiedBy = User.FindFirst("namaakun")?.Value ?? "system";
            var result = await _service.CreateByProdiStep2Async(draftId, modifiedBy);

            if (result == null)
                return BadRequest(new { message = "Gagal mengajukan pengunduran diri" });

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdatePengunduranDiriRequest dto)
        {
            var updatedBy = User?.Identity?.Name ?? "system";

            var success = await _service.UpdateAsync(id, dto, updatedBy);

            if (!success)
                return BadRequest(new { message = "Gagal memperbarui pengunduran diri." });

            return Ok(new { message = "Pengunduran diri berhasil diperbarui." });
        }

        [Authorize]
        [HttpPut("approve")]
        public async Task<IActionResult> Approve([FromQuery] string id, [FromBody] ApprovePengunduranDiriRequest dto)
        {
            Console.WriteLine($"DEBUG Controller Approve - START");
            Console.WriteLine($"DEBUG Controller Approve - id from query: '{id}'");
            Console.WriteLine($"DEBUG Controller Approve - dto.Role: '{dto.Role}'");
            
            dto.ApprovedBy = User.FindFirst("namaakun")?.Value ?? "system";
            
            Console.WriteLine($"DEBUG Controller Approve - dto.ApprovedBy: '{dto.ApprovedBy}'");

            var success = await _service.ApproveAsync(id, dto);

            Console.WriteLine($"DEBUG Controller Approve - success: {success}");

            if (!success)
                return BadRequest(new { message = "Gagal menyetujui pengunduran diri." });

            return Ok(new { message = $"Pengajuan berhasil disetujui oleh {dto.Role}." });
        }

        [Authorize]
        [HttpPut("reject")]
        public async Task<IActionResult> Reject([FromQuery] string id, [FromBody] RejectPengunduranDiriRequest dto)
        {
            var success = await _service.RejectAsync(id, dto);

            if (!success)
                return BadRequest(new { message = "Gagal menolak pengunduran diri." });

            return Ok(new
            {
                message = $"Pengajuan berhasil ditolak oleh {dto.Role}.",
                reason = dto.Reason
            });
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> SoftDelete([FromQuery] string id)
        {
            var updatedBy = User.FindFirst("namaakun")?.Value ?? "system";

            var success = await _service.SoftDeleteAsync(id, updatedBy);

            if (!success)
                return BadRequest(new { message = "Gagal menghapus data pengunduran diri." });

            return Ok(new { message = "Pengunduran diri berhasil dihapus (soft delete)." });
        }


        [HttpGet("check-report/{pdiId}")]
        public async Task<IActionResult> CheckReport(string pdiId)
        {
            var report = await _service.CheckReportAsync(pdiId);

            if (report == null)
                return NotFound(new { message = "Laporan tidak ditemukan." });

            return Ok(new { file = report });
        }

        [HttpPut("sk/{id}")]
        public async Task<IActionResult> CreateSK(string id, [FromBody] UploadSKPengunduranDiriRequest dto)
        {
            var updatedBy = User?.Identity?.Name ?? "system";

            var success = await _service.CreateSKAsync(id, dto, updatedBy);

            if (!success)
                return BadRequest(new { message = "Gagal memperbarui SK Pengunduran Diri." });

            return Ok(new { message = "SK Pengunduran Diri berhasil diperbarui." });
        }

        [HttpGet("mahasiswa")]
        public async Task<IActionResult> GetMahasiswaList()
        {
            var mahasiswaList = await _service.GetMahasiswaListAsync();
            return Ok(mahasiswaList);
        }

        [Authorize]
        [HttpGet("mahasiswa/by-konsentrasi")]
        public async Task<IActionResult> GetMahasiswaByKonsentrasi()
        {
            var username = User.FindFirst("namaakun")?.Value ?? "";
            var mahasiswaList = await _service.GetMahasiswaByKonsentrasiAsync(username);
            return Ok(mahasiswaList);
        }

        [Authorize]
        [HttpGet("prodi")]
        public async Task<IActionResult> GetProdi()
        {
            var username = User.FindFirst("namaakun")?.Value ?? "";
            var prodiList = await _service.GetProdiByUserAsync(username);
            return Ok(prodiList);
        }

        [HttpGet("prodi/list")]
        public async Task<IActionResult> GetListProdi()
        {
            var prodiList = await _service.GetListProdiAsync();
            return Ok(prodiList);
        }

        [HttpGet("mahasiswa/{mhsId}/prodi")]
        public async Task<IActionResult> GetMahasiswaProdi(string mhsId)
        {
            var result = await _service.GetMahasiswaProdiAsync(mhsId);
            
            if (result == null)
                return NotFound(new { message = "Data mahasiswa tidak ditemukan" });
                
            return Ok(result);
        }

        [HttpGet("mahasiswa/{mhsId}/angkatan")]
        public async Task<IActionResult> GetMahasiswaAngkatan(string mhsId)
        {
            var result = await _service.GetMahasiswaAngkatanAsync(mhsId);
            
            if (result == null)
                return NotFound(new { message = "Data angkatan mahasiswa tidak ditemukan" });
                
            return Ok(result);
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
        public async Task<IActionResult> GetProfilMahasiswa(string mhsId)
        {
            var result = await _service.GetProfilMahasiswaAsync(mhsId);
            
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
                { "rpt", ("wwwroot/uploads/pengundurandiri/templates/Report_SK_Pengunduran_Diri_2.rpt", "application/octet-stream", "Report_SK_Pengunduran_Diri_2.rpt") },
                { "cs", ("wwwroot/uploads/pengundurandiri/templates/Report_SK_Pengunduran_Diri_2.cs", "text/plain", "Report_SK_Pengunduran_Diri_2.cs") },
                { "aspx", ("wwwroot/uploads/pengundurandiri/templates/SK_Pengunduran_Diri.aspx", "text/plain", "SK_Pengunduran_Diri.aspx") },
                { "aspx-cs", ("wwwroot/uploads/pengundurandiri/templates/SK_Pengunduran_Diri.aspx.cs", "text/plain", "SK_Pengunduran_Diri.aspx.cs") },
                { "aspx-designer", ("wwwroot/uploads/pengundurandiri/templates/SK_Pengunduran_Diri.aspx.designer.cs", "text/plain", "SK_Pengunduran_Diri.aspx.designer.cs") }
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
                message = "Daftar file template SK Pengunduran Diri",
                files = new[]
                {
                    new { type = "rpt", name = "Report_SK_Pengunduran_Diri_2.rpt", description = "Template report Crystal Reports", url = "/api/pengundurandiri/template-sk?type=rpt" },
                    new { type = "cs", name = "Report_SK_Pengunduran_Diri_2.cs", description = "Wrapper class untuk .rpt", url = "/api/pengundurandiri/template-sk?type=cs" },
                    new { type = "aspx", name = "SK_Pengunduran_Diri.aspx", description = "Halaman web ASP.NET", url = "/api/pengundurandiri/template-sk?type=aspx" },
                    new { type = "aspx-cs", name = "SK_Pengunduran_Diri.aspx.cs", description = "Logic/controller ASP.NET", url = "/api/pengundurandiri/template-sk?type=aspx-cs" },
                    new { type = "aspx-designer", name = "SK_Pengunduran_Diri.aspx.designer.cs", description = "Designer code ASP.NET", url = "/api/pengundurandiri/template-sk?type=aspx-designer" }
                }
            });
        }

        /// <summary>
        /// Generate nomor SK untuk Pengunduran Diri
        /// </summary>
        private async Task<string> GenerateNoSKAsync(string pdiId)
        {
            try
            {
                // Get data Pengunduran Diri untuk ambil konsentrasi
                var detail = await _service.GetDetailAsync(pdiId);
                if (detail == null)
                    return $"SK_PD_{DateTime.Now:yyyyMMddHHmmss}";

                // TODO: Sesuaikan jenisSuratId dengan ID di database
                // Contoh: "JS002" untuk jenis surat Pengunduran Diri
                string jenisSuratId = "JS_PENGUNDURAN_DIRI"; // Ganti dengan ID yang sesuai
                
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
                return $"SK_PD_{DateTime.Now:yyyyMMddHHmmss}";
            }
        }

        // Upload file SK dan SKPB ke folder terpisah
        [Authorize]
        [HttpPost("upload-sk-file")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadSKFile([FromForm] UploadSKPdiFileRequest request)
        {
            if (string.IsNullOrEmpty(request.PdiId))
                return BadRequest(new { message = "PdiId wajib diisi" });

            if (request.SkFile == null && request.SkpbFile == null)
                return BadRequest(new { message = "Minimal satu file harus diupload (SK atau SKPB)" });

            var baseUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "pengundurandiri");
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
            string nomorSK = await GenerateNoSKAsync(request.PdiId);

            // Upload SK file ke folder sk/
            if (request.SkFile != null && request.SkFile.Length > 0)
            {
                var ext = Path.GetExtension(request.SkFile.FileName);
                // Gunakan nomor SK sebagai nama file
                var skFileName = $"SK_PD_{nomorSK.Replace("/", "-")}{ext}";
                var skFullPath = Path.Combine(skFolderPath, skFileName);
                
                using (var stream = new FileStream(skFullPath, FileMode.Create))
                {
                    await request.SkFile.CopyToAsync(stream);
                }
                skPath = $"/uploads/pengundurandiri/sk/{skFileName}";
            }

            // Upload SKPB file ke folder skpb/
            if (request.SkpbFile != null && request.SkpbFile.Length > 0)
            {
                var ext = Path.GetExtension(request.SkpbFile.FileName);
                // Gunakan nomor SK sebagai nama file
                var skpbFileName = $"SKPB_PD_{nomorSK.Replace("/", "-")}{ext}";
                var skpbFullPath = Path.Combine(skpbFolderPath, skpbFileName);
                
                using (var stream = new FileStream(skpbFullPath, FileMode.Create))
                {
                    await request.SkpbFile.CopyToAsync(stream);
                }
                skpbPath = $"/uploads/pengundurandiri/skpb/{skpbFileName}";
            }

            // Update database dengan path file
            var modifiedBy = User.FindFirst("namaakun")?.Value ?? "system";
            var uploadRequest = new UploadSKPengunduranDiriRequest
            {
                Sk = skPath,
                Skpb = skpbPath
            };

            var result = await _service.CreateSKAsync(request.PdiId, uploadRequest, modifiedBy);

            if (!result)
                return BadRequest(new { message = "Gagal menyimpan data SK ke database" });

            return Ok(new { 
                message = "Upload SK Pengunduran Diri berhasil",
                nomorSK = nomorSK,
                skPath = skPath,
                skpbPath = skpbPath
            });
        }

        // Download file SK
        [HttpGet("download-sk-file/{pdiId}")]
        public async Task<IActionResult> DownloadSKFile(string pdiId)
        {
            var detail = await _service.GetDetailAsync(pdiId);

            if (detail == null)
                return NotFound(new { message = "Data Pengunduran Diri tidak ditemukan" });

            if (string.IsNullOrEmpty(detail.SK))
                return NotFound(new { message = "File SK belum diupload" });

            // Cek apakah path sudah lengkap atau perlu digabung dengan wwwroot
            string fullPath;
            if (Path.IsPathRooted(detail.SK))
            {
                fullPath = detail.SK;
            }
            else
            {
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                fullPath = Path.Combine(webRootPath, detail.SK.TrimStart('/').Replace("/", "\\"));
            }

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { message = $"File tidak ditemukan di server: {detail.SK}" });

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

        // Get info path SK
        [HttpGet("download-sk/{pdiId}")]
        public async Task<IActionResult> GetSKInfo(string pdiId)
        {
            var detail = await _service.GetDetailAsync(pdiId);

            if (detail == null)
                return NotFound(new { message = "Data Pengunduran Diri tidak ditemukan" });

            return Ok(new { 
                sk = detail.SK ?? ""
            });
        }

        // Download file SK dengan query parameter (untuk ID yang ada slash)
        [HttpGet("download-sk-file")]
        public async Task<IActionResult> DownloadSKFileByQuery([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "Parameter id wajib diisi" });

            var detail = await _service.GetDetailAsync(id);

            if (detail == null)
                return NotFound(new { message = "Data Pengunduran Diri tidak ditemukan" });

            if (string.IsNullOrEmpty(detail.SK))
                return NotFound(new { message = "File SK belum diupload" });

            string fullPath;
            if (Path.IsPathRooted(detail.SK))
            {
                fullPath = detail.SK;
            }
            else
            {
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                fullPath = Path.Combine(webRootPath, detail.SK.TrimStart('/').Replace("/", "\\"));
            }

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { message = $"File tidak ditemukan di server: {detail.SK}" });

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

        // Download file SK langsung dengan filename (tanpa query database)
        [HttpGet("sk/{filename}")]
        public IActionResult DownloadSKByFilename(string filename)
        {
            var baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "pengundurandiri", "sk");
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

        // Download file SKPB langsung dengan filename (tanpa query database)
        [HttpGet("skpb/{filename}")]
        public IActionResult DownloadSKPBByFilename(string filename)
        {
            var baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "pengundurandiri", "skpb");
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

            var detail = await _service.GetDetailAsync(id);
            if (detail == null)
                return NotFound(new { message = "Data tidak ditemukan" });

            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var files = new List<(string path, string name)>();

            // Cek SK
            if (!string.IsNullOrEmpty(detail.SK))
            {
                var skPath = Path.Combine(webRootPath, detail.SK.TrimStart('/').Replace("/", "\\"));
                if (System.IO.File.Exists(skPath))
                    files.Add((skPath, "SK_" + Path.GetFileName(skPath)));
            }

            // Cek SKPB
            if (!string.IsNullOrEmpty(detail.Skpb))
            {
                var skpbPath = Path.Combine(webRootPath, detail.Skpb.TrimStart('/').Replace("/", "\\"));
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
            return File(memoryStream.ToArray(), "application/zip", $"SK_SKPB_{safeId}.zip");
        }

        // DEBUG ENDPOINT - Remove in production
        [HttpGet("debug-approve/{id}")]
        public async Task<IActionResult> DebugApprove(string id)
        {
            try
            {
                var detail = await _service.GetDetailAsync(id);
                
                if (detail == null)
                    return NotFound(new { 
                        message = "Data tidak ditemukan",
                        pdi_id = id
                    });

                return Ok(new {
                    message = "Data ditemukan",
                    data = new {
                        pdi_id = detail.Id,
                        mhs_id = detail.MhsId,
                        nama = detail.NamaMahasiswa,
                        status = detail.Status,
                        approval_prodi_by = detail.ApprovalProdiBy,
                        approval_dir1_by = detail.ApprovalDir1By,
                        created_by = detail.CreatedBy
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "Error saat mengambil data",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // DEBUG ENDPOINT - Test approve without auth
        [HttpPost("debug-test-approve")]
        public async Task<IActionResult> DebugTestApprove([FromBody] DebugApproveRequest request)
        {
            try
            {
                Console.WriteLine($"DEBUG TestApprove - id: '{request.Id}', role: '{request.Role}', approvedBy: '{request.ApprovedBy}'");
                
                var dto = new ApprovePengunduranDiriRequest
                {
                    Role = request.Role,
                    ApprovedBy = request.ApprovedBy
                };

                var success = await _service.ApproveAsync(request.Id, dto);

                return Ok(new {
                    success = success,
                    message = success ? "Approve berhasil" : "Approve gagal",
                    request = request
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "Error saat approve",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

    }
}
