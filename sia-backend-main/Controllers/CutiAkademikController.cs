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
        public async Task<IActionResult> CreateDraftByProdi([FromForm] CreateCutiProdiRequest dto)
        {
            var id = await _service.CreateDraftByProdiAsync(dto);
            return Ok(new { draftId = id });
        }

        // ============================================
        // GENERATE FINAL ID (Prodi)
        // ============================================
        [HttpPut("prodi/generate-id")]
        public async Task<IActionResult> GenerateIdByProdi([FromBody] GenerateCutiProdiIdRequest dto)
        {
            var id = await _service.GenerateIdByProdiAsync(dto);
            if (id == null)
                return BadRequest(new { message = "Gagal generate id final (prodi)." });

            return Ok(new { finalId = id });
        }

        // ============================================
        // GET ALL CUTI
        // ============================================
        /// <summary>
        /// Mendapatkan semua data cuti akademik dengan filter
        /// </summary>
        /// <param name="mhsId">ID Mahasiswa untuk filter (default: % untuk semua)</param>
        /// <param name="status">Status cuti untuk filter. Contoh: 'disetujui', 'belum disetujui prodi'</param>
        /// <param name="userId">ID User untuk filter berdasarkan role</param>
        /// <param name="role">Role user untuk menentukan akses data</param>
        /// <param name="search">Kata kunci pencarian</param>
        /// <returns>List data cuti akademik</returns>
        /// <response code="200">Berhasil mendapatkan data</response>
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
        public async Task<IActionResult> UpdateDraft(string id, [FromForm] UpdateCutiAkademikRequest dto)
        {
            var success = await _service.UpdateAsync(id, dto);

            if (success)
                return Ok(new { message = "Cuti Akademik berhasil diupdate." });
            
            return BadRequest(new { message = "Gagal mengupdate Cuti Akademik." });
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
        /// <summary>
        /// Mendapatkan riwayat data cuti akademik
        /// </summary>
        /// <param name="userId">ID User untuk filter berdasarkan role (opsional)</param>
        /// <param name="status">Status cuti akademik untuk filter (opsional). Contoh: 'disetujui', 'belum disetujui prodi', 'menunggu upload sk'</param>
        /// <param name="search">Kata kunci pencarian berdasarkan NIM atau ID cuti (opsional)</param>
        /// <returns>List riwayat cuti akademik</returns>
        /// <response code="200">Berhasil mendapatkan data riwayat</response>
        /// <response code="500">Terjadi kesalahan server</response>
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
        /// Mendapatkan data riwayat cuti akademik dalam format Excel
        /// </summary>
        /// <param name="userId">ID User untuk filter data (opsional)</param>
        /// <returns>Data riwayat dalam format yang siap untuk export Excel</returns>
        /// <response code="200">Berhasil mendapatkan data untuk Excel</response>
        [HttpGet("riwayat/excel")]
        [ProducesResponseType(typeof(IEnumerable<CutiAkademikRiwayatExcelResponse>), 200)]
        public async Task<IActionResult> GetRiwayatExcel([FromQuery] string userId = "")
        {
            var data = await _service.GetRiwayatExcelAsync(userId);
            return Ok(data);
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
    }
}