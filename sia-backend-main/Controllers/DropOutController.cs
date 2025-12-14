using astratech_apps_backend.DTOs.DropOut;
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

        public DropOutController(IDropOutService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string keyword = "",
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            return Ok(await _service.GetAllAsync(keyword, page, limit));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var data = await _service.GetByIdAsync(id);
            return data == null ? NotFound() : Ok(data);
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetDetail(string id)
        {
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






        [HttpPost]
        public async Task<IActionResult> Create(CreateDropOutRequest dto)
        {
            var id = await _service.CreateAsync(dto, "system");
            return Ok(new { id });
        }

        [HttpPost("create-pengajuan")]
        public async Task<IActionResult> CreatePengajuanDO([FromBody] CreatePengajuanDORequest dto)
        {
            var createdBy = HttpContext.Items["UserId"]?.ToString() ?? "system";

            var newId = await _service.CreatePengajuanDOAsync(dto, createdBy);

            if (newId == null)
                return BadRequest(new { message = "Gagal membuat pengajuan Draft DropOut." });

            return Ok(new
            {
                message = "Pengajuan DO Draft berhasil dibuat.",
                id = newId
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateDropOutRequest dto)
        {
            var success = await _service.UpdateAsync(id, dto, "system");

            if (!success)
                return NotFound(new { message = "Data Drop Out tidak ditemukan." });

            return Ok(new { updated = true });
        }

        [HttpPut("draft/{id}/generate-id")]
        public async Task<IActionResult> GenerateIdFromDraft(string id)
        {
            var data = await _service.GetIdByDraftAsync(id);

            if (data == null)
                return NotFound();

            return Ok(data);
        }


        [Authorize]
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(string id, [FromBody] ApproveDropOutRequest dto)
        {
            var success = await _service.ApproveDropOutAsync(id, dto);

            if (!success)
                return BadRequest(new { message = "Gagal menyetujui Drop Out." });

            return Ok(new { message = "Drop Out berhasil disetujui oleh Wadir 1." });
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(string id, [FromBody] RejectDropOutRequest dto)
        {
            var success = await _service.RejectAsync(id, dto);

            if (!success)
                return BadRequest(new { message = "Gagal menolak pengajuan Drop Out." });

            return Ok(new { message = "Pengajuan Drop Out berhasil ditolak." });
        }

        [HttpPut("upload-sk")]
        public async Task<IActionResult> UploadSKDO([FromBody] UploadSKDORequest request)
        {
            var result = await _service.UploadSKDOAsync(request);

            if (!result)
                return BadRequest(new { message = "Gagal upload SK DO" });

            return Ok(new { message = "Upload SK DO berhasil" });
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
    [FromQuery] string konsentrasi = ""
)
        {
            var data = await _service.GetPendingAsync(username, keyword, sortBy, konsentrasi);
            return Ok(data);
        }



    }
}
