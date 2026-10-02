using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using astratech_apps_backend.DTOs.PengunduranDiri;
using astratech_apps_backend.Helpers;
using astratech_apps_backend.Repositories.Interfaces;

namespace astratech_apps_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PengunduranDiriController(IPengunduranDiriRepository repo) : ControllerBase
    {
        private readonly IPengunduranDiriRepository _repo = repo;

        [HttpGet("GetAllPengunduranDiri")]
        [RequiresPermission("pengunduran_diri.view")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string p1 = "",
            [FromQuery] string keyword = "",
            [FromQuery] string sortBy = "",
            [FromQuery] string konId = "",
            [FromQuery] string status = "",
            [FromQuery] int? page = 1,
            [FromQuery] int? pageSize = 10)
        {
            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var role = User.FindFirstValue("idrole") ?? User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var userToUse = string.IsNullOrEmpty(p1) ? username : p1;

            if (page.HasValue && pageSize.HasValue)
            {
                var pageVal = page.Value < 1 ? 1 : page.Value;
                var pageSizeVal = pageSize.Value < 1 ? 10 : (pageSize.Value > 100 ? 100 : pageSize.Value);

                var (list, totalData) = await _repo.GetPendingPaginatedAsync(
                    userToUse, keyword, sortBy, konId, status, role, pageVal, pageSizeVal);

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

            var allData = await _repo.GetPendingAsync(userToUse, keyword, sortBy, konId, status, role);
            return Ok(allData);
        }

        [HttpGet("RiwayatPengunduranDiri")]
        [RequiresPermission("pengunduran_diri.view")]
        public async Task<IActionResult> GetRiwayat(
            [FromQuery] string p1 = "",
            [FromQuery] string keyword = "",
            [FromQuery] string sortBy = "",
            [FromQuery] string konId = "",
            [FromQuery] string status = "",
            [FromQuery] int? page = 1,
            [FromQuery] int? pageSize = 10)
        {
            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var role = User.FindFirstValue("idrole") ?? User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var userToUse = string.IsNullOrEmpty(p1) ? username : p1;

            if (page.HasValue && pageSize.HasValue)
            {
                var pageVal = page.Value < 1 ? 1 : page.Value;
                var pageSizeVal = pageSize.Value < 1 ? 10 : (pageSize.Value > 100 ? 100 : pageSize.Value);

                var (list, totalData) = await _repo.GetRiwayatPaginatedAsync(
                    userToUse, status, keyword, sortBy, konId, role, pageVal, pageSizeVal);

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

            var allRiwayat = await _repo.GetRiwayatAsync(userToUse, status, keyword, sortBy, konId, role);
            return Ok(allRiwayat);
        }

        [HttpGet("ExportExcelRiwayatPengunduranDiri")]
        [RequiresPermission("pengunduran_diri.export")]
        public async Task<IActionResult> GetRiwayatExcel(
            [FromQuery] string konsentrasi = "",
            [FromQuery] string orderBy = "")
        {
            var list = await _repo.GetRiwayatExcelAsync(konsentrasi, orderBy);
            return Ok(list);
        }

        [HttpGet("DetailPengunduranDiri")]
        [RequiresPermission("pengunduran_diri.view")]
        public async Task<IActionResult> GetDetail([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "Parameter id wajib diisi." });

            var data = await _repo.GetDetailAsync(id);
            if (data == null)
                return NotFound(new { message = "Data Pengunduran Diri tidak ditemukan." });

            return Ok(data);
        }

        [HttpPost("CreatePengajuanPengunduranDiri")]
        [RequiresPermission("pengunduran_diri.create")]
        public async Task<IActionResult> CreatePengajuan([FromBody] CreatePengunduranDiriRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sanitizedDto = SanitizerHelper.EncodeObject(dto);
            if (sanitizedDto == null) return BadRequest("Invalid request data");

            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var (success, message, newId) = await _repo.CreatePengajuanAsync(sanitizedDto, username);

            if (!success)
                return BadRequest(new { error = true, message });

            return Ok(new { success = true, message, id = newId, pdiId = newId, draftId = newId });
        }

        [HttpPost("CreateDraftPengunduranDiriProdi")]
        [RequiresPermission("pengunduran_diri.create")]
        public async Task<IActionResult> CreateByProdiDraft([FromBody] CreatePengunduranDiriByProdiRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sanitizedDto = SanitizerHelper.EncodeObject(dto);
            if (sanitizedDto == null) return BadRequest("Invalid request data");

            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            if (string.IsNullOrEmpty(sanitizedDto.CreatedBy))
                sanitizedDto.CreatedBy = username;

            var (success, message, newId) = await _repo.CreateByProdiAsync(sanitizedDto);

            if (!success)
                return BadRequest(new { error = true, message });

            return Ok(new { success = true, message, id = newId, pdiId = newId, draftId = newId });
        }

        [HttpPut("SubmitDraftPengunduranDiri/{draftId}")]
        [RequiresPermission("pengunduran_diri.create")]
        public async Task<IActionResult> SubmitDraft(string draftId)
        {
            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var (success, message, noPengajuan) = await _repo.SubmitDraftAsync(draftId, username);

            if (!success)
                return BadRequest(new { error = true, message });

            return Ok(new { success = true, message, id = noPengajuan, newId = noPengajuan });
        }

        [HttpPut("EditPengunduranDiri/{id}")]
        [RequiresPermission("pengunduran_diri.edit")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdatePengunduranDiriRequest dto)
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

        [HttpPut("ApprovePengunduranDiri")]
        [RequiresPermission("pengunduran_diri.approve")]
        public async Task<IActionResult> Approve(
            [FromQuery] string id,
            [FromBody] ApprovePengunduranDiriRequest? dto)
        {
            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var tokenRole = User.FindFirstValue("idrole") ?? User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var roleToUse = !string.IsNullOrEmpty(dto?.Role) ? dto.Role : tokenRole;
            var (success, message) = await _repo.ApproveAsync(id, roleToUse, username);

            if (!success)
                return BadRequest(new { error = true, message });

            return Ok(new { success = true, message });
        }

        [HttpPut("RejectPengunduranDiri")]
        [RequiresPermission("pengunduran_diri.reject")]
        public async Task<IActionResult> Reject(
            [FromQuery] string id,
            [FromBody] RejectPengunduranDiriRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var tokenRole = User.FindFirstValue("idrole") ?? User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var roleToUse = !string.IsNullOrEmpty(dto.Role) ? dto.Role : tokenRole;
            var (success, message) = await _repo.RejectAsync(id, roleToUse, dto.Reason, username);

            if (!success)
                return BadRequest(new { error = true, message });

            return Ok(new { success = true, message });
        }

        [HttpPost("UploadSKFilePengunduranDiri")]
        [Consumes("multipart/form-data")]
        [RequiresPermission("pengunduran_diri.edit")]
        public async Task<IActionResult> UploadSKFile([FromForm] UploadSKFileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var baseUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "pengundurandiri");
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
                skPath = $"/uploads/pengundurandiri/sk/{skFileName}";
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
                skpbPath = $"/uploads/pengundurandiri/skpb/{skpbFileName}";
            }

            return Ok(new
            {
                success = true,
                message = "File berhasil diupload",
                sk = skPath,
                skpb = skpbPath
            });
        }

        [HttpPut("UploadSKPengunduranDiri")]
        [RequiresPermission("pengunduran_diri.edit")]
        public async Task<IActionResult> UploadSK(
            [FromBody] UploadSKPengunduranDiriRequest dto,
            [FromQuery] string? id = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var targetId = !string.IsNullOrEmpty(id) ? id : string.Empty;
            if (string.IsNullOrEmpty(targetId))
                return BadRequest(new { message = "Parameter id wajib diisi." });

            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var (success, message) = await _repo.UploadSKAsync(targetId, dto, username);

            if (!success)
                return BadRequest(new { error = true, message });

            return Ok(new { success = true, message });
        }

        [HttpDelete("DeletePengunduranDiri/{id}")]
        [RequiresPermission("pengunduran_diri.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var (success, message) = await _repo.DeleteAsync(id, username);

            if (!success)
                return BadRequest(new { error = true, message });

            return Ok(new { success = true, message });
        }

        [HttpGet("GetInfoSKPengunduranDiri/{id}")]
        [RequiresPermission("pengunduran_diri.export")]
        public async Task<IActionResult> DownloadSK(string id)
        {
            var detail = await _repo.GetByIdAsync(id);
            if (detail == null)
                return NotFound(new { message = "Data Pengunduran Diri tidak ditemukan." });

            if (string.IsNullOrEmpty(detail.Sk))
                return NotFound(new { message = "File SK belum diupload." });

            string fullPath = Path.IsPathRooted(detail.Sk)
                ? detail.Sk
                : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", detail.Sk.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { message = $"File tidak ditemukan di server: {detail.Sk}" });

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

        [HttpGet("DownloadFilePengunduranDiri/{filename}")]
        [AllowAnonymous]
        public IActionResult DownloadFilePengunduranDiri(string filename)
        {
            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var possiblePaths = new[]
            {
                Path.Combine(webRoot, "uploads", "pengundurandiri", filename),
                Path.Combine(webRoot, "Uploads", "pengundurandiri", filename),
                Path.Combine(webRoot, "uploads", "pengundurandiri", "lampiran", filename),
                Path.Combine(webRoot, "uploads", "pengundurandiri", "sk", filename),
                Path.Combine(webRoot, "uploads", "pengundurandiri", "skpb", filename)
            };

            var foundPath = possiblePaths.FirstOrDefault(System.IO.File.Exists);
            if (foundPath == null)
            {
                return NotFound(new { message = "Berkas file tidak ditemukan.", filename });
            }

            var fileBytes = System.IO.File.ReadAllBytes(foundPath);
            var extension = Path.GetExtension(foundPath).ToLower();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
            return File(fileBytes, contentType, filename);
        }

        // ==========================================
        // Helper Master Data Dropdowns
        // ==========================================

        [HttpGet("GetProdiByUser")]
        [RequiresPermission("pengunduran_diri.view")]
        public async Task<IActionResult> GetProdi()
        {
            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var data = await _repo.GetProdiByUserAsync(username);
            return Ok(data);
        }

        [HttpGet("GetListProdi")]
        [RequiresPermission("pengunduran_diri.view")]
        public async Task<IActionResult> GetListProdi()
        {
            var data = await _repo.GetListProdiAsync();
            return Ok(data);
        }

        [HttpGet("GetListMahasiswa")]
        [RequiresPermission("pengunduran_diri.view")]
        public async Task<IActionResult> GetMahasiswaList()
        {
            var data = await _repo.GetMahasiswaListAsync();
            return Ok(data);
        }

        [HttpGet("GetMahasiswaByProdi")]
        [RequiresPermission("pengunduran_diri.view")]
        public async Task<IActionResult> GetMahasiswaByProdi()
        {
            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var data = await _repo.GetMahasiswaByProdiAsync(username);
            return Ok(data);
        }

        [HttpGet("GetMahasiswaByKonsentrasi")]
        [RequiresPermission("pengunduran_diri.view")]
        public async Task<IActionResult> GetMahasiswaByKonsentrasi()
        {
            var username = User.FindFirstValue("namaakun") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var data = await _repo.GetMahasiswaByKonsentrasiAsync(username);
            return Ok(data);
        }

        [HttpGet("GetProdiMahasiswa/{mhsId}")]
        [RequiresPermission("pengunduran_diri.view")]
        public async Task<IActionResult> GetMahasiswaProdi(string mhsId)
        {
            var data = await _repo.GetMahasiswaProdiAsync(mhsId);
            if (data == null)
                return NotFound(new { message = "Data prodi mahasiswa tidak ditemukan." });

            return Ok(data);
        }

        [HttpGet("GetAngkatanMahasiswa/{mhsId}")]
        [RequiresPermission("pengunduran_diri.view")]
        public async Task<IActionResult> GetMahasiswaAngkatan(string mhsId)
        {
            var data = await _repo.GetMahasiswaAngkatanAsync(mhsId);
            return Ok(data);
        }

        [HttpGet("CekBebasTanggungan/{mhsId}")]
        [RequiresPermission("pengunduran_diri.view")]
        public async Task<IActionResult> CekBebasTanggungan(string mhsId)
        {
            var result = await _repo.CekBebasTanggunganAsync(mhsId);
            return Ok(result);
        }

        [HttpGet("GetProfilMahasiswa/{mhsId}")]
        [RequiresPermission("pengunduran_diri.view")]
        public async Task<IActionResult> GetProfilMahasiswa(string mhsId)
        {
            var result = await _repo.GetProfilMahasiswaAsync(mhsId);
            if (result == null)
                return NotFound(new { message = "Data profil mahasiswa tidak ditemukan." });

            return Ok(result);
        }
    }
}
