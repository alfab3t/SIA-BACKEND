using astratech_apps_backend.DTOs.PengunduranDiri;
using astratech_apps_backend.Helpers;
using astratech_apps_backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace astratech_apps_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PengunduranDiriController : ControllerBase
    {
        private readonly IPengunduranDiriRepository _repo;
        private readonly IConfiguration Configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public PengunduranDiriController(IPengunduranDiriRepository repo, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _repo = repo;
            Configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [Authorize]
        [RequiresPermission("pengunduran_diri.view")]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string p1 = "",
            [FromQuery] string keyword = "",
            [FromQuery] string sortBy = "",
            [FromQuery] string konId = "",
            [FromQuery] string status = "",  // Support single or multiple status (comma-separated)
            [FromQuery] int? page = 1,
            [FromQuery] int? pageSize = 10)
        {
            // Ambil username dari JWT token (claim "namaakun")
            var userId = User.FindFirst("namaakun")?.Value ?? "";
            
            Console.WriteLine($"DEBUG GetAll - p1: '{p1}', keyword: '{keyword}', konId: '{konId}', status: '{status}', userId: '{userId}'");
            
            // Default pagination: page=1, pageSize=10
            var pageVal = page.HasValue && page.Value >= 1 ? page.Value : 1;
            var pageSizeVal = pageSize.HasValue && pageSize.Value >= 1 ? 
                (pageSize.Value > 100 ? 100 : pageSize.Value) : 10;
            
            var result = await _repo.GetAllPaginatedAsync(p1, keyword, sortBy, konId, status, userId, pageVal, pageSizeVal);
            
            Console.WriteLine($"DEBUG GetAll - Result count: {result.Data.Count()}");
            
            return Ok(result);
        }

        [RequiresPermission("pengunduran_diri.view")]
        [HttpGet("detail")]
        public async Task<IActionResult> GetDetail([FromQuery] string id)
        {
            var detail = await _repo.GetDetailAsync(id);

            if (detail == null)
                return NotFound(new { message = "Data tidak ditemukan" });

            return Ok(detail);
        }

        // Upload file lampiran (bisa 1 atau 2 file sekaligus)
        [RequiresPermission("pengunduran_diri.import")]
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
        [RequiresPermission("pengunduran_diri.export")]
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


        [RequiresPermission("pengunduran_diri.view")]
        [HttpGet("notif/{id}")]
        public async Task<IActionResult> GetNotif(string id)
        {
            var data = await _repo.GetNotifAsync(id);

            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            return Ok(data);
        }

        [RequiresPermission("pengunduran_diri.view")]
        [HttpGet("riwayat")]
        public async Task<IActionResult> GetRiwayat(
        [FromQuery] string status = "",  // Support single or multiple status (comma-separated)
        [FromQuery] string keyword = "",
        [FromQuery] string orderBy = "pdi_created_date desc",
        [FromQuery] string konsentrasi = "",
        [FromQuery] int? page = 1,
        [FromQuery] int? pageSize = 10)
        {
            var username = User?.Identity?.Name ?? "SYSTEM";

            // Default pagination: page=1, pageSize=10
            var pageVal = page.HasValue && page.Value >= 1 ? page.Value : 1;
            var pageSizeVal = pageSize.HasValue && pageSize.Value >= 1 ? 
                (pageSize.Value > 100 ? 100 : pageSize.Value) : 10;

            var result = await _repo.GetRiwayatPaginatedAsync(
                username,
                status,
                keyword,
                orderBy,
                konsentrasi,
                pageVal,
                pageSizeVal
            );

            return Ok(result);
        }

        [RequiresPermission("pengunduran_diri.view")]
        [HttpGet("riwayat-excel")]
        public async Task<IActionResult> GetRiwayatExcel(
        [FromQuery] string orderBy = "",
        [FromQuery] string konsentrasi = ""
        )
        {
            var data = await _repo.GetRiwayatExcelAsync(orderBy, konsentrasi);
            return Ok(data);
        }

        //[HttpGet("riwayat-excel/download")]
        //public async Task<IActionResult> DownloadExcel(
        //[FromQuery] string orderBy = "",
        //[FromQuery] string konsentrasi = ""
        //)
        //{
        //    var data = await _repo.GetRiwayatExcelAsync(orderBy, konsentrasi);

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
        [RequiresPermission("pengunduran_diri.create")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreatePengunduranDiriRequest dto)
        {
            var createdBy = dto.CreatedBy ?? "system";
            
            // STEP1: Buat draft dengan lampiran
            var draftId = await _repo.CreateStep1Async(
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

        // PUT /submit/{draftId} - Ajukan Draft (STEP2) ? status jadi "Belum Disetujui Prodi"
        [RequiresPermission("pengunduran_diri.edit")]
        [HttpPut("submit/{draftId}")]
        public async Task<IActionResult> Submit(string draftId)
        {
            var modifiedBy = User.FindFirst("namaakun")?.Value ?? "system";
            
            // STEP2: Generate ID resmi dan ubah status
            var data = await _repo.CreateStep2Async(draftId, modifiedBy);

            if (data == null)
                return BadRequest(new { message = "Gagal mengajukan pengunduran diri" });

            return Ok(data);
        }

        [RequiresPermission("pengunduran_diri.create")]
        [HttpPost("create-by-prodi")]
        public async Task<IActionResult> CreateByProdi([FromBody] CreatePengunduranDiriByProdiRequest dto)
        {
            var result = await _repo.CreateByProdiAsync(dto);
            return Ok(result);
        }

        // STEP 1 - Buat Draft by Prodi
        [RequiresPermission("pengunduran_diri.create")]
        [HttpPost("create-by-prodi/draft")]
        public async Task<IActionResult> CreateByProdiStep1([FromBody] CreatePengunduranDiriByProdiRequest dto)
        {
            Console.WriteLine($"DEBUG CreateByProdiStep1 - MhsId: '{dto.MhsId}', CreatedBy: '{dto.CreatedBy}'");
            
            var draftId = await _repo.CreateByProdiStep1Async(
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
        [RequiresPermission("pengunduran_diri.edit")]
        [HttpPut("create-by-prodi/submit/{draftId}")]
        public async Task<IActionResult> CreateByProdiStep2(string draftId)
        {
            var modifiedBy = User.FindFirst("namaakun")?.Value ?? "system";
            var result = await _repo.CreateByProdiStep2Async(draftId, modifiedBy);

            if (result == null)
                return BadRequest(new { message = "Gagal mengajukan pengunduran diri" });

            return Ok(result);
        }

        [RequiresPermission("pengunduran_diri.edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdatePengunduranDiriRequest dto)
        {
            var updatedBy = User?.Identity?.Name ?? "system";

            var success = await _repo.UpdateAsync(id, dto, updatedBy);

            if (!success)
                return BadRequest(new { message = "Gagal memperbarui pengunduran diri." });

            return Ok(new { message = "Pengunduran diri berhasil diperbarui." });
        }

        [Authorize]
        [RequiresPermission("pengunduran_diri.approve_reject")]
        [HttpPut("approve")]
        public async Task<IActionResult> Approve([FromQuery] string id, [FromBody] ApprovePengunduranDiriRequest dto)
        {
            Console.WriteLine($"DEBUG Controller Approve - START");
            Console.WriteLine($"DEBUG Controller Approve - id from query: '{id}'");
            Console.WriteLine($"DEBUG Controller Approve - dto.Role: '{dto.Role}'");
            
            dto.ApprovedBy = User.FindFirst("namaakun")?.Value ?? "system";
            
            Console.WriteLine($"DEBUG Controller Approve - dto.ApprovedBy: '{dto.ApprovedBy}'");

            var success = await _repo.ApproveAsync(id, dto);

            Console.WriteLine($"DEBUG Controller Approve - success: {success}");

            if (!success)
                return BadRequest(new { message = "Gagal menyetujui pengunduran diri." });

            return Ok(new { message = $"Pengajuan berhasil disetujui oleh {dto.Role}." });
        }

        [Authorize]
        [RequiresPermission("pengunduran_diri.approve_reject")]
        [HttpPut("reject")]
        public async Task<IActionResult> Reject([FromQuery] string id, [FromBody] RejectPengunduranDiriRequest dto)
        {
            var success = await _repo.RejectAsync(id, dto);

            if (!success)
                return BadRequest(new { message = "Gagal menolak pengunduran diri." });

            return Ok(new
            {
                message = $"Pengajuan berhasil ditolak oleh {dto.Role}.",
                reason = dto.Reason
            });
        }

        [Authorize]
        [RequiresPermission("pengunduran_diri.delete")]
        [HttpDelete("delete")]
        public async Task<IActionResult> SoftDelete([FromQuery] string id)
        {
            var updatedBy = User.FindFirst("namaakun")?.Value ?? "system";

            var success = await _repo.SoftDeleteAsync(id, updatedBy);

            if (!success)
                return BadRequest(new { message = "Gagal menghapus data pengunduran diri." });

            return Ok(new { message = "Pengunduran diri berhasil dihapus (soft delete)." });
        }


        [RequiresPermission("pengunduran_diri.view")]
        [HttpGet("check-report/{pdiId}")]
        public async Task<IActionResult> CheckReport(string pdiId)
        {
            var report = await _repo.CheckReportAsync(pdiId);

            if (report == null)
                return NotFound(new { message = "Laporan tidak ditemukan." });

            return Ok(new { file = report });
        }

        [RequiresPermission("pengunduran_diri.import")]
        [HttpPut("sk/{id}")]
        public async Task<IActionResult> CreateSK(string id, [FromBody] UploadSKPengunduranDiriRequest dto)
        {
            var updatedBy = User?.Identity?.Name ?? "system";

            var success = await _repo.CreateSKAsync(id, dto, updatedBy);

            if (!success)
                return BadRequest(new { message = "Gagal memperbarui SK Pengunduran Diri." });

            return Ok(new { message = "SK Pengunduran Diri berhasil diperbarui." });
        }

        [RequiresPermission("pengunduran_diri.view")]
        [HttpGet("mahasiswa")]
        public async Task<IActionResult> GetMahasiswaList()
        {
            var mahasiswaList = await _repo.GetMahasiswaListAsync();
            return Ok(mahasiswaList);
        }

        [Authorize]
        [RequiresPermission("pengunduran_diri.view")]
        [HttpGet("mahasiswa/by-konsentrasi")]
        public async Task<IActionResult> GetMahasiswaByKonsentrasi()
        {
            var username = User.FindFirst("namaakun")?.Value ?? "";
            var mahasiswaList = await _repo.GetMahasiswaByKonsentrasiAsync(username);
            return Ok(mahasiswaList);
        }

        [Authorize]
        [RequiresPermission("pengunduran_diri.view")]
        [HttpGet("prodi")]
        public async Task<IActionResult> GetProdi()
        {
            var username = User.FindFirst("namaakun")?.Value ?? "";
            var prodiList = await _repo.GetProdiByUserAsync(username);
            return Ok(prodiList);
        }

        [RequiresPermission("pengunduran_diri.view")]
        [HttpGet("prodi/list")]
        public async Task<IActionResult> GetListProdi()
        {
            var prodiList = await _repo.GetListProdiAsync();
            return Ok(prodiList);
        }

        [RequiresPermission("pengunduran_diri.view")]
        [HttpGet("mahasiswa/{mhsId}/prodi")]
        public async Task<IActionResult> GetMahasiswaProdi(string mhsId)
        {
            var result = await _repo.GetMahasiswaProdiAsync(mhsId);
            
            if (result == null)
                return NotFound(new { message = "Data mahasiswa tidak ditemukan" });
                
            return Ok(result);
        }

        [RequiresPermission("pengunduran_diri.view")]
        [HttpGet("mahasiswa/{mhsId}/angkatan")]
        public async Task<IActionResult> GetMahasiswaAngkatan(string mhsId)
        {
            var result = await _repo.GetMahasiswaAngkatanAsync(mhsId);
            
            if (result == null)
                return NotFound(new { message = "Data angkatan mahasiswa tidak ditemukan" });
                
            return Ok(result);
        }

        [RequiresPermission("pengunduran_diri.view")]
        [HttpGet("mahasiswa/{mhsId}/bebas-tanggungan")]
        public async Task<IActionResult> CekBebasTanggungan(string mhsId)
        {
            var result = await _repo.CekBebasTanggunganAsync(mhsId);
            
            if (result == null)
                return NotFound(new { message = "Data mahasiswa tidak ditemukan" });
                
            return Ok(result);
        }

        [RequiresPermission("pengunduran_diri.view")]
        [HttpGet("mahasiswa/{mhsId}/profil")]
        public async Task<IActionResult> GetProfilMahasiswa(string mhsId)
        {
            var result = await _repo.GetProfilMahasiswaAsync(mhsId);
            
            if (result == null)
                return NotFound(new { message = "Data profil mahasiswa tidak ditemukan" });
                
            return Ok(result);
        }

        [RequiresPermission("pengunduran_diri.view")]
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

        [RequiresPermission("pengunduran_diri.view")]
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

        // Upload file SK dan SKPB ke folder terpisah (tanpa rename)
        [Authorize]
        [RequiresPermission("pengunduran_diri.import")]
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

            // Upload SK file ke folder sk/ (gunakan nama asli dari FE)
            if (request.SkFile != null && request.SkFile.Length > 0)
            {
                var skFileName = request.SkFile.FileName;
                var skFullPath = Path.Combine(skFolderPath, skFileName);
                
                using (var stream = new FileStream(skFullPath, FileMode.Create))
                {
                    await request.SkFile.CopyToAsync(stream);
                }
                skPath = $"/uploads/pengundurandiri/sk/{skFileName}";
            }

            // Upload SKPB file ke folder skpb/ (gunakan nama asli dari FE)
            if (request.SkpbFile != null && request.SkpbFile.Length > 0)
            {
                var skpbFileName = request.SkpbFile.FileName;
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

            var result = await _repo.CreateSKAsync(request.PdiId, uploadRequest, modifiedBy);

            if (!result)
                return BadRequest(new { message = "Gagal menyimpan data SK ke database" });

            return Ok(new { 
                message = "Upload SK Pengunduran Diri berhasil",
                skPath = skPath,
                skpbPath = skpbPath
            });
        }

        // Download file SK
        [RequiresPermission("pengunduran_diri.export")]
        [HttpGet("download-sk-file/{pdiId}")]
        public async Task<IActionResult> DownloadSKFile(string pdiId)
        {
            var detail = await _repo.GetDetailAsync(pdiId);

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
        [RequiresPermission("pengunduran_diri.export")]
        [HttpGet("download-sk/{pdiId}")]
        public async Task<IActionResult> GetSKInfo(string pdiId)
        {
            var detail = await _repo.GetDetailAsync(pdiId);

            if (detail == null)
                return NotFound(new { message = "Data Pengunduran Diri tidak ditemukan" });

            return Ok(new { 
                sk = detail.SK ?? ""
            });
        }

        // Download file SK dengan query parameter (untuk ID yang ada slash)
        [RequiresPermission("pengunduran_diri.export")]
        [HttpGet("download-sk-file")]
        public async Task<IActionResult> DownloadSKFileByQuery([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "Parameter id wajib diisi" });

            var detail = await _repo.GetDetailAsync(id);

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
        [RequiresPermission("pengunduran_diri.export")]
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
        [RequiresPermission("pengunduran_diri.export")]
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

        // Get info SK + SKPB untuk download terpisah (frontend trigger 2 download)
        [RequiresPermission("pengunduran_diri.export")]
        [HttpGet("download-all-sk")]
        public async Task<IActionResult> GetAllSKInfo([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "Parameter id wajib diisi" });

            var detail = await _repo.GetDetailAsync(id);
            if (detail == null)
                return NotFound(new { message = "Data tidak ditemukan" });

            var result = new
            {
                pdiId = id,
                sk = new
                {
                    available = !string.IsNullOrEmpty(detail.SK),
                    path = detail.SK ?? "",
                    filename = !string.IsNullOrEmpty(detail.SK) ? Path.GetFileName(detail.SK) : "",
                    downloadUrl = !string.IsNullOrEmpty(detail.SK) 
                        ? $"/api/pengundurandiri/sk/{Path.GetFileName(detail.SK)}" 
                        : ""
                },
                skpb = new
                {
                    available = !string.IsNullOrEmpty(detail.Skpb),
                    path = detail.Skpb ?? "",
                    filename = !string.IsNullOrEmpty(detail.Skpb) ? Path.GetFileName(detail.Skpb) : "",
                    downloadUrl = !string.IsNullOrEmpty(detail.Skpb) 
                        ? $"/api/pengundurandiri/skpb/{Path.GetFileName(detail.Skpb)}" 
                        : ""
                }
            };

            if (!result.sk.available && !result.skpb.available)
                return NotFound(new { message = "Tidak ada file SK/SKPB yang tersedia" });

            return Ok(result);
        }

        // DEBUG ENDPOINT - Remove in production
        [RequiresPermission("pengunduran_diri.view")]
        [HttpGet("debug-approve/{id}")]
        public async Task<IActionResult> DebugApprove(string id)
        {
            try
            {
                var detail = await _repo.GetDetailAsync(id);
                
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
        [RequiresPermission("pengunduran_diri.approve_reject")]
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

                var success = await _repo.ApproveAsync(request.Id, dto);

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

        // Generate PDF SK Pengunduran Diri
        [RequiresPermission("pengunduran_diri.export")]
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
                        pdiId = id,
                        reportServiceUrl = Configuration["ReportService:Url"],
                        note = "Set ReportService:UseMock = false di appsettings.json untuk menggunakan service report asli"
                    });
                }

                // Langsung panggil service report tanpa ambil data dari database
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30); // Set timeout 30 detik
                
                var reportServiceUrl = Configuration["ReportService:Url"] ?? "http://10.5.0.94/api/Report/GetReport";

                Console.WriteLine($"=== Calling Report Service for Pengunduran Diri ===");
                Console.WriteLine($"URL: {reportServiceUrl}");
                Console.WriteLine($"PdiId: {id}");

                // Siapkan request body untuk service report
                var requestBody = new
                {
                    reportName = "Report_SK_Pengunduran_Diri_2",
                    parameters = new
                    {
                        pdiId = id
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
                var fileName = $"SK_PD_{id.Replace("/", "-")}_{DateTime.Now:yyyyMMdd}.pdf";
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
                    message = "Terjadi kesalahan saat generate PDF SK Pengunduran Diri",
                    error = ex.Message 
                });
            }
        }

    }
}

