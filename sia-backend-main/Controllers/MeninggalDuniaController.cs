using astratech_apps_backend.DTOs.MeninggalDunia;
using astratech_apps_backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace astratech_apps_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeninggalDuniaController : ControllerBase
    {
        private readonly IMeninggalDuniaService _service;

        public MeninggalDuniaController(IMeninggalDuniaService service)
        {
            _service = service;
        }


        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllMeninggalDuniaRequest req)
        {
            ModelState.Clear();
            return Ok(await _service.GetAllAsync(req));
        }

        [HttpGet("mahasiswa")]
        public async Task<IActionResult> GetMahasiswa([FromQuery] string? search = null)
        {
            var data = await _service.GetMahasiswaListAsync(search);
            return Ok(data);
        }

        [HttpGet("mahasiswa-dropdown")]
        public async Task<IActionResult> GetMahasiswaDropdown()
        {
            var data = await _service.GetMahasiswaDropdownAsync();
            return Ok(data);
        }

        [HttpGet("mahasiswa/{mhsId}")]
        public async Task<IActionResult> GetMahasiswaDetail(string mhsId)
        {
            var data = await _service.GetMahasiswaDetailAsync(mhsId);
            if (data == null)
                return NotFound(new { message = "Data mahasiswa tidak ditemukan" });
            
            return Ok(data);
        }

        [HttpGet("mahasiswa/{mhsId}/prodi")]
        public async Task<IActionResult> GetMahasiswaProdi(string mhsId)
        {
            var data = await _service.GetMahasiswaProdiAsync(mhsId);
            if (data == null)
                return NotFound(new { message = "Data prodi mahasiswa tidak ditemukan" });
            
            return Ok(data);
        }

        [HttpGet("program-studi")]
        public async Task<IActionResult> GetProgramStudi()
        {
            var data = await _service.GetProgramStudiListAsync();
            return Ok(data);
        }



        //[HttpGet("{id}")]
        //public async Task<IActionResult> Get(string id)
        //{
        //    var data = await _service.GetByIdAsync(id);
        //    return data == null ? NotFound() : Ok(data);
        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(string id)
        {
            var data = await _service.GetDetailAsync(id);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpGet("report/{id}")]
        public async Task<IActionResult> GetReport(string id)
        {
            var data = await _service.GetReportAsync(id);

            if (data == null)
                return NotFound();

            return Ok(data);
        }





        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateMeninggalDuniaRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdBy = HttpContext.Items["UserId"]?.ToString() ?? "system";
            var id = await _service.CreateAsync(dto, createdBy);
            return Ok(new { id });
        }

        [HttpPost("finalize/{draftId}")]
        public async Task<IActionResult> Finalize(string draftId)
        {
            var updatedBy = HttpContext.Items["UserId"]?.ToString() ?? "system";
            var officialId = await _service.FinalizeAsync(draftId, updatedBy);
            return Ok(new { officialId });
        }

        

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateMeninggalDuniaRequest dto)
        {
            var updatedBy = "system";

            var success = await _service.UpdateAsync(id, dto, updatedBy);

            return success
                ? Ok(new { message = "Data berhasil diperbarui." })
                : NotFound();
        }

        [HttpPut("upload-sk")]
        public async Task<IActionResult> UploadSK([FromBody] UploadSKMeninggalRequest request)
        {
            var result = await _service.UploadSKMeninggalAsync(request);

            if (!result)
                return BadRequest(new { message = "Gagal upload SK Meninggal Dunia" });

            return Ok(new { message = "Upload SK berhasil" });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(string id)
        {
            var updatedBy = HttpContext.Items["UserId"]?.ToString() ?? "system";

            var result = await _service.SoftDeleteAsync(id, updatedBy);

            if (!result)
                return BadRequest(new { message = "Gagal menghapus data." });

            return Ok(new { message = "Data meninggal dunia berhasil dihapus (soft delete)." });
        }


        [HttpPut("sk/{id}")]
        public async Task<IActionResult> UpdateSK(string id, UpdateSKMeninggalDuniaRequest dto)
        {
            var updatedBy = User?.Identity?.Name ?? "SYSTEM";

            var success = await _service.UpdateSKAsync(id, dto, updatedBy);

            if (!success)
                return BadRequest(new { message = "Gagal memperbarui SK Meninggal Dunia." });

            return Ok(new { message = "SK berhasil diperbarui." });
        }

        [HttpPut("approve/{id}")]
        public async Task<IActionResult> Approve(string id, [FromBody] ApproveMeninggalDuniaRequest dto)
        {
            var result = await _service.ApproveAsync(id, dto);

            if (!result)
                return NotFound();

            return Ok(new { approved = true });
        }

        [HttpPut("reject/{id}")]
        public async Task<IActionResult> Reject(string id, [FromBody] RejectMeninggalDuniaRequest dto)
        {
            var success = await _service.RejectAsync(id, dto);

            if (!success)
                return BadRequest(new { message = "Gagal menolak pengajuan." });

            return Ok(new
            {
                message = $"Pengajuan Meninggal Dunia ditolak oleh {dto.Role}"
            });
        }


        [Authorize]
        [HttpPost("{id}/upload-sk")]
        public async Task<IActionResult> UploadSK(string id, [FromForm] UploadSKMeninggalDuniaDto dto)
        {
            var updatedBy = HttpContext.Items["UserId"]?.ToString() ?? "system";

            var success = await _service.UploadSKAsync(id, dto.SkFile, dto.SpkbFile, updatedBy);

            if (!success)
                return BadRequest(new { message = "Gagal upload SK meninggal dunia." });

            return Ok(new { message = "SK berhasil diupload dan status diperbarui." });
        }

        [HttpGet("Riwayat")]
        public async Task<IActionResult> GetRiwayat([FromQuery] GetRiwayatMeninggalDuniaRequest req)
        {
            return Ok(await _service.GetRiwayatAsync(req));
        }



        [HttpGet("riwayat/excel")]
        public async Task<IActionResult> GetRiwayatExcel(
        [FromQuery] string sort = "",
        [FromQuery] string konsentrasi = ""
        )
        {
            var data = await _service.GetRiwayatExcelAsync(sort, konsentrasi);
            return Ok(data); // bisa dikembangkan menjadi file excel
        }



    }
}
