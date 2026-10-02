using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using astratech_apps_backend.DTOs.DropOut;
using astratech_apps_backend.Helpers;
using astratech_apps_backend.Repositories.Interfaces;

namespace astratech_apps_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DropOutController(IDropOutRepository repo) : ControllerBase
    {
        private readonly IDropOutRepository _repo = repo;

        [HttpGet("GetAllDropOut")]
        [RequiresPermission("drop_out.view")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? page = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] string keyword = "",
            [FromQuery] string sortBy = "a.dro_created_date desc",
            [FromQuery] string konsentrasi = "",
            [FromQuery] string status = "")
        {
            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var role = User.FindFirstValue("idrole") ?? User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            var displayName = User.FindFirstValue("displayname") ?? User.FindFirstValue("name") ?? username;

            if (page.HasValue && pageSize.HasValue)
            {
                var pageVal = page.Value < 1 ? 1 : page.Value;
                var pageSizeVal = pageSize.Value < 1 ? 10 : (pageSize.Value > 100 ? 100 : pageSize.Value);

                var (list, totalData) = await _repo.GetPendingPaginatedAsync(
                    username, keyword, sortBy, konsentrasi, role, displayName, status, pageVal, pageSizeVal);

                return Ok(new
                {
                    data = list,
                    pagination = new
                    {
                        page = pageVal,
                        pageSize = pageSizeVal,
                        totalRecords = totalData,
                        totalPages = ((totalData - 1) / pageSizeVal) + 1
                    }
                });
            }

            var allData = await _repo.GetPendingAsync(username, keyword, sortBy, konsentrasi, role, displayName);
            return Ok(allData);
        }

        [HttpGet("RiwayatDropOut")]
        [RequiresPermission("drop_out.view")]
        public async Task<IActionResult> GetRiwayat(
            [FromQuery] int? page = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] string keyword = "",
            [FromQuery] string sortBy = "a.dro_created_date desc",
            [FromQuery] string konsentrasi = "",
            [FromQuery] string status = "")
        {
            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var role = User.FindFirstValue("idrole") ?? User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            var displayName = User.FindFirstValue("displayname") ?? User.FindFirstValue("name") ?? username;

            if (page.HasValue && pageSize.HasValue)
            {
                var pageVal = page.Value < 1 ? 1 : page.Value;
                var pageSizeVal = pageSize.Value < 1 ? 10 : (pageSize.Value > 100 ? 100 : pageSize.Value);

                var (list, totalData) = await _repo.GetRiwayatPaginatedAsync(
                    username, keyword, sortBy, konsentrasi, role, displayName, status, pageVal, pageSizeVal);

                return Ok(new
                {
                    data = list,
                    pagination = new
                    {
                        page = pageVal,
                        pageSize = pageSizeVal,
                        totalRecords = totalData,
                        totalPages = ((totalData - 1) / pageSizeVal) + 1
                    }
                });
            }

            var allRiwayat = await _repo.GetRiwayatAsync(username, keyword, sortBy, konsentrasi, role, displayName);
            return Ok(allRiwayat);
        }

        [HttpGet("ExportExcelRiwayatDropOut")]
        [RequiresPermission("drop_out.export")]
        public async Task<IActionResult> GetRiwayatExcel(
            [FromQuery] string keyword = "",
            [FromQuery] string sortBy = "a.dro_created_date desc",
            [FromQuery] string konsentrasi = "")
        {
            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var role = User.FindFirstValue("idrole") ?? User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            var displayName = User.FindFirstValue("displayname") ?? User.FindFirstValue("name") ?? username;

            var list = await _repo.GetRiwayatExcelAsync(username, keyword, sortBy, konsentrasi, role, displayName);
            return Ok(list);
        }

        [HttpGet("DetailDropOut")]
        [RequiresPermission("drop_out.view")]
        public async Task<IActionResult> GetDetail([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "Parameter id wajib diisi." });

            var data = await _repo.GetDetailAsync(id);
            if (data == null)
                return NotFound(new { message = "Data Drop Out tidak ditemukan." });

            return Ok(data);
        }

        [HttpPost("CreatePengajuanDropOut")]
        [RequiresPermission("drop_out.create")]
        public async Task<IActionResult> CreatePengajuan([FromBody] CreatePengajuanDORequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sanitizedDto = SanitizerHelper.EncodeObject(dto);
            if (sanitizedDto == null) return BadRequest("Invalid request data");

            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var (success, message, newId) = await _repo.CreatePengajuanDOAsync(sanitizedDto, username);

            if (!success)
                return BadRequest(new { error = true, message });

            return Ok(new { success = true, message, id = newId, newId });
        }

        [HttpPut("EditDropOut/{id}")]
        [RequiresPermission("drop_out.edit")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateDropOutRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sanitizedDto = SanitizerHelper.EncodeObject(dto);
            if (sanitizedDto == null) return BadRequest("Invalid request data");

            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var (success, message) = await _repo.UpdateAsync(id, sanitizedDto, username);

            if (!success)
                return BadRequest(new { error = true, message });

            return Ok(new { success = true, message });
        }

        [HttpPut("SubmitDraftDropOut")]
        [HttpPut("SubmitDraftDropOut/{id}")]
        [RequiresPermission("drop_out.create")]
        public async Task<IActionResult> SubmitDraft([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "Parameter id wajib diisi." });

            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var (success, message, noPengajuan) = await _repo.SubmitDraftAsync(id, username);

            if (!success)
                return BadRequest(new { error = true, message });

            return Ok(new { success = true, message, id = noPengajuan, newId = noPengajuan });
        }

        [HttpPut("ApproveWadirDropOut")]
        [RequiresPermission("drop_out.approve")]
        public async Task<IActionResult> Approve(
            [FromQuery] string id,
            [FromBody] ApproveDropOutRequest? dto)
        {
            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var role = User.FindFirstValue("idrole") ?? User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var catatan = dto?.Catatan ?? string.Empty;
            var (success, message) = await _repo.ApproveAsync(id, role, catatan, username);

            if (!success)
                return BadRequest(new { error = true, message });

            return Ok(new { success = true, message });
        }

        [HttpPut("RejectWadirDropOut")]
        [RequiresPermission("drop_out.reject")]
        public async Task<IActionResult> Reject(
            [FromQuery] string id,
            [FromBody] RejectDropOutRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var (success, message) = await _repo.RejectAsync(id, dto.Reason, username);

            if (!success)
                return BadRequest(new { error = true, message });

            return Ok(new { success = true, message });
        }

        [HttpPost("UploadSKFileDropOut")]
        [Consumes("multipart/form-data")]
        [RequiresPermission("drop_out.edit")]
        public async Task<IActionResult> UploadSKFile([FromForm] UploadSKFileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var baseUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "dropout");
            var skFolderPath = Path.Combine(baseUploadPath, "sk");
            var skpbFolderPath = Path.Combine(baseUploadPath, "skpb");

            if (!Directory.Exists(skFolderPath))
                Directory.CreateDirectory(skFolderPath);
            if (!Directory.Exists(skpbFolderPath))
                Directory.CreateDirectory(skpbFolderPath);

            string skPath = string.Empty;
            string skpbPath = string.Empty;
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

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

            return Ok(new
            {
                success = true,
                message = "File berhasil diupload",
                sk = skPath,
                skpb = skpbPath
            });
        }

        [HttpPut("UploadSKDropOut")]
        [RequiresPermission("drop_out.edit")]
        public async Task<IActionResult> UploadSK([FromBody] UploadSKDORequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            request.ModifiedBy = string.IsNullOrEmpty(request.ModifiedBy) ? username : request.ModifiedBy;

            var (success, message) = await _repo.UploadSKDOAsync(request);

            if (!success)
                return BadRequest(new { error = true, message });

            return Ok(new { success = true, message });
        }

        [HttpDelete("DeleteDropOut/{id}")]
        [RequiresPermission("drop_out.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var (success, message) = await _repo.DeleteAsync(id, username);

            if (!success)
                return BadRequest(new { error = true, message });

            return Ok(new { success = true, message });
        }

        [HttpGet("GetInfoSKDropOut/{droId}")]
        [RequiresPermission("drop_out.export")]
        public async Task<IActionResult> DownloadSK(string droId)
        {
            var result = await _repo.DownloadSKAsync(droId);
            if (result == null)
                return NotFound(new { message = "SK tidak ditemukan untuk DropOut ini." });

            return Ok(result);
        }

        [HttpGet("DownloadSKFileDropOut")]
        [RequiresPermission("drop_out.export")]
        public async Task<IActionResult> DownloadSKFile([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "Parameter id wajib diisi." });

            var result = await _repo.DownloadSKAsync(id);
            if (result == null || string.IsNullOrEmpty(result.Sk))
                return NotFound(new { message = "File SK belum diupload atau tidak ditemukan." });

            string fullPath = Path.IsPathRooted(result.Sk)
                ? result.Sk
                : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", result.Sk.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

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

        [HttpGet("CheckSKStatusDropOut")]
        [RequiresPermission("drop_out.view")]
        public async Task<IActionResult> CheckSKStatus([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "Parameter id wajib diisi." });

            var result = await _repo.DownloadSKAsync(id);
            if (result == null)
                return NotFound(new { message = "Data Drop Out tidak ditemukan." });

            bool skExists = !string.IsNullOrEmpty(result.Sk);
            bool skpbExists = !string.IsNullOrEmpty(result.Skpb);

            return Ok(new
            {
                hasSK = skExists,
                hasSKPB = skpbExists,
                skPath = result.Sk,
                skpbPath = result.Skpb
            });
        }

        [HttpGet("TemplateSKDropOut")]
        public IActionResult DownloadTemplateSK([FromQuery] string? type = "rpt")
        {
            var fileMapping = new Dictionary<string, (string Path, string ContentType, string FileName)>
            {
                { "rpt", ("wwwroot/uploads/dropout/templates/Report_SK_Drop_Out_2.rpt", "application/octet-stream", "Report_SK_Drop_Out_2.rpt") },
                { "cs", ("wwwroot/uploads/dropout/templates/Report_SK_Drop_Out_2.cs", "text/plain", "Report_SK_Drop_Out_2.cs") },
                { "aspx", ("wwwroot/uploads/dropout/templates/SK_Drop_Out.aspx", "text/plain", "SK_Drop_Out.aspx") }
            };

            if (type == null || !fileMapping.TryGetValue(type, out var fileInfo))
                return BadRequest(new { message = "Type tidak valid. Gunakan: rpt, cs, aspx" });

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileInfo.Path.Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = $"File template {fileInfo.FileName} tidak ditemukan" });

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, fileInfo.ContentType, fileInfo.FileName);
        }

        // ==========================================
        // Helper Master Data Dropdowns
        // ==========================================

        [HttpGet("GetProdiByUser")]
        [RequiresPermission("drop_out.view")]
        public async Task<IActionResult> GetProdi()
        {
            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var data = await _repo.GetProdiAsync(username);
            return Ok(data);
        }

        [HttpGet("GetListProdi")]
        [RequiresPermission("drop_out.view")]
        public async Task<IActionResult> GetListProdi()
        {
            var data = await _repo.GetListProdiAsync();
            return Ok(data);
        }

        [HttpGet("GetListKonsentrasi")]
        [RequiresPermission("drop_out.view")]
        public async Task<IActionResult> GetKonsentrasiByProdi([FromQuery] string prodiId)
        {
            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var data = await _repo.GetKonsentrasiByProdiAsync(prodiId, username);
            return Ok(data);
        }

        [HttpGet("GetMahasiswaByKonsentrasi")]
        [RequiresPermission("drop_out.view")]
        public async Task<IActionResult> GetMahasiswaByKonsentrasi([FromQuery] string konsentrasiId)
        {
            var data = await _repo.GetMahasiswaByKonsentrasiAsync(konsentrasiId);
            return Ok(data);
        }

        [HttpGet("GetAngkatanByMahasiswa")]
        [RequiresPermission("drop_out.view")]
        public async Task<IActionResult> GetAngkatanByMahasiswa([FromQuery] string mhsId)
        {
            var angkatan = await _repo.GetAngkatanByMahasiswaAsync(mhsId);
            return Ok(new { angkatan = angkatan ?? string.Empty });
        }

        [HttpGet("CekBebasTanggungan/{mhsId}")]
        [RequiresPermission("drop_out.view")]
        public async Task<IActionResult> CekBebasTanggungan(string mhsId)
        {
            var result = await _repo.CekBebasTanggunganAsync(mhsId);
            return Ok(result);
        }

        [HttpGet("GetProfilMahasiswa/{mhsId}")]
        [RequiresPermission("drop_out.view")]
        public async Task<IActionResult> GetProfilMahasiswaDetail(string mhsId)
        {
            var result = await _repo.GetProfilMahasiswaDetailAsync(mhsId);
            if (result == null)
                return NotFound(new { message = "Data profil mahasiswa tidak ditemukan." });

            return Ok(result);
        }
    }
}
