using astratech_apps_backend.DTOs.CutiAkademik;
using astratech_apps_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("list")]
        [HttpGet("CutiAkademikListResponse")]
        public async Task<IActionResult> GetListResponse(
         [FromQuery] string? status = "",
         [FromQuery] string? search = "",
         [FromQuery] string? urut = "",
         [FromQuery] int pageNumber = 1,
         [FromQuery] int pageSize = 10
        )
        {

            var data = await _service.GetListResponseAsync(
                status, search, urut, pageNumber, pageSize);

            return Ok(new
            {
                data = data,
                totalData = data.Count
            });
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetDetail(string id)
        {
            var data = await _service.GetDetailAsync(id);

            if (data == null)
                return NotFound(new { message = "Detail Cuti Akademik tidak ditemukan." });

            return Ok(data);
        }

        [HttpGet("detail-notif/{id}")]
        public async Task<IActionResult> GetDetailNotif(string id)
        {
            var data = await _service.GetDetailNotifAsync(id);

            if (data == null)
                return NotFound(new { message = "Data detail notifikasi Cuti Akademik tidak ditemukan." });

            return Ok(data);
        }

        [HttpGet("riwayat")]
        public async Task<IActionResult> GetRiwayat(
        [FromQuery] string username,
        [FromQuery] string status = "",
        [FromQuery] string keyword = "")
        {
            var result = await _service.GetRiwayatAsync(username, status, keyword);
            return Ok(result);
        }


        // ---------- STEP 1 (Draft) ----------
        [HttpPost("draft")]
        public async Task<IActionResult> CreateDraft([FromBody] CreateDraftCutiRequest dto)
        {
            var id = await _service.CreateDraftAsync(dto);
            return Ok(new { draftId = id });
        }

        // ---------- STEP 2 (Generate Final ID) ----------
        [HttpPut("generate-id")]
        public async Task<IActionResult> GenerateId([FromBody] GenerateCutiIdRequest dto)
        {
            var id = await _service.GenerateIdAsync(dto);
            return Ok(new { finalId = id });
        }

        // ---------------------------------------------
        // STEP1 - Create draft by prodi
        // ---------------------------------------------
        [HttpPost("prodi/draft")]
        public async Task<IActionResult> CreateDraftByProdi([FromBody] CreateCutiProdiRequest dto)
        {
            var id = await _service.CreateDraftByProdiAsync(dto);
            return Ok(new { draftId = id });
        }

        // ---------------------------------------------
        // STEP2 - Generate final ID
        // ---------------------------------------------
        [HttpPut("prodi/generate-id")]
        public async Task<IActionResult> GenerateIdByProdi([FromBody] GenerateCutiProdiIdRequest dto)
        {
            var id = await _service.GenerateIdByProdiAsync(dto);
            return Ok(new { finalId = id });
        }

        [HttpPut("sk")]
        public async Task<IActionResult> CreateSKCutiAkademik([FromBody] CreateSKCutiAkademikRequest request)
        {
            var result = await _service.CreateSKCutiAkademikAsync(request);

            if (!result)
                return BadRequest(new { message = "Gagal membuat SK Cuti Akademik." });

            return Ok(new { message = "SK Cuti Akademik berhasil dibuat." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateCutiAkademikRequest dto)
        {
            var success = await _service.UpdateAsync(id, dto);

            if (!success)
                return NotFound(new { message = "Data Cuti Akademik tidak ditemukan." });

            return Ok(new { message = "Cuti Akademik berhasil diupdate." });
        }

        [HttpPut("approve-prodi/{id}")]
        public async Task<IActionResult> ApproveProdi(string id, [FromBody] CutiAkademikApproveProdiRequest req)
        {
            var success = await _service.ApproveProdiAsync(id, req);

            if (!success)
                return BadRequest(new { message = "Gagal menyetujui Cuti Akademik oleh Prodi." });

            return Ok(new { message = "Cuti Akademik berhasil disetujui oleh Prodi." });
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(string id, [FromBody] CutiAkademikRejectRequest req)
        {
            var success = await _service.RejectAsync(id, req);

            if (!success)
                return BadRequest(new { message = "Gagal menolak Cuti Akademik." });

            return Ok(new { message = "Cuti Akademik berhasil ditolak." });
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var modifiedBy = HttpContext.Items["UserId"]?.ToString() ?? "system";

            var success = await _service.DeleteAsync(id, modifiedBy);

            if (!success)
                return BadRequest(new { message = "Gagal menghapus Cuti Akademik." });

            return Ok(new { message = "Cuti Akademik berhasil dihapus." });
        }

        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveCuti(string id, [FromBody] CutiAkademikApproveRequest req)
        {
            var success = await _service.ApproveAsync(id, req);

            if (!success)
                return BadRequest(new { message = "Gagal menyetujui pengajuan." });

            return Ok(new { message = "Pengajuan berhasil disetujui." });
        }



    }
}
