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

        public PengunduranDiriController(IPengunduranDiriService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
        [FromQuery] string p1 = "",
        [FromQuery] string status = "",
        [FromQuery] string userId = "")
        {
            var result = await _service.GetAllAsync(p1, status, userId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(string id)
        {
            var detail = await _service.GetDetailAsync(id);

            if (detail == null)
                return NotFound(new { message = "Data tidak ditemukan" });

            return Ok(detail);
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

        [HttpPost("step1")]
        public async Task<IActionResult> Step1([FromBody] CreatePengunduranDiriRequest dto)
        {
            var draftId = await _service.CreateStep1Async(dto.MhsId, "system");
            return Ok(new { draftId });
        }

        [HttpPost("step2")]
        public async Task<IActionResult> Step2([FromBody] CreatePengunduranDiriRequest dto)
        {
            var data = await _service.CreateStep2Async(dto.DraftId, "system");

            if (data == null)
                return BadRequest(new { message = "Gagal generate PD" });

            return Ok(data);
        }

        [HttpPost("create-by-prodi")]
        public async Task<IActionResult> CreateByProdi([FromBody] CreatePengunduranDiriByProdiRequest dto)
        {
            var result = await _service.CreateByProdiAsync(dto);
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
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(string id, [FromBody] ApprovePengunduranDiriRequest dto)
        {
            dto.ApprovedBy = HttpContext.Items["UserId"]?.ToString() ?? "system";

            var success = await _service.ApproveAsync(id, dto);

            if (!success)
                return BadRequest(new { message = "Gagal menyetujui pengunduran diri." });

            return Ok(new { message = $"Pengajuan berhasil disetujui oleh {dto.Role}." });
        }

        [Authorize]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(string id, [FromBody] RejectPengunduranDiriRequest dto)
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(string id)
        {
            var updatedBy = HttpContext.Items["UserId"]?.ToString() ?? "system";

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

    }
}
