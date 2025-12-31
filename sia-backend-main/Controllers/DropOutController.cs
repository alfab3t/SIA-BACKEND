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
        //public async Task<IActionResult> Create(CreateDropOutRequest dto)
        //{
        //    var id = await _service.CreateAsync(dto, "system");
        //    return Ok(new { id });
        //}

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

        [HttpGet("prodi")]
        public async Task<IActionResult> GetProdi()
        {
            return Ok(await _service.GetProdiAsync());
        }

        [HttpGet("konsentrasi")]
        public async Task<IActionResult> GetKonsentrasiByProdi(
    [FromQuery] string prodiId
)
        {
            if (string.IsNullOrEmpty(prodiId))
                return BadRequest("prodiId wajib diisi");

            // username dipakai utk filter sekprodi (opsional)
            var username = HttpContext.Items["Username"]?.ToString() ?? "";

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

        [HttpGet("mahasiswa-profil")]
        public async Task<IActionResult> GetProfilMahasiswa(
    [FromQuery] string mhsId)
        {
            if (string.IsNullOrEmpty(mhsId))
                return BadRequest("mhsId wajib diisi");

            var data = await _service.GetMahasiswaProfilAsync(mhsId);
            if (data == null)
                return NotFound(new { message = "Mahasiswa tidak ditemukan / tidak aktif" });

            return Ok(data);
        }


        //IHIRRRRR
    }
}
