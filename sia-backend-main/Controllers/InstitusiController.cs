using astratech_apps_backend.DTOs.Institusi;
using astratech_apps_backend.Helpers;
using astratech_apps_backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace astratech_apps_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InstitusiController(IInstitusiRepository repo) : ControllerBase
    {
        private readonly IInstitusiRepository _repo = repo;

        [HttpGet("GetAllInstitusi")]
        [RequiresPermission("institusi.view")]
        public async Task<IActionResult> GetAllInstitusi([FromQuery] GetAllInstitusiRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sanitizedDto = SanitizerHelper.EncodeObject(dto);
            if (sanitizedDto == null) return NotFound();

            var (list, totalData) = await _repo.GetAllAsync(sanitizedDto);

            var dataDTO = list.Select(m => new InstitusiDto
            {
                Id = m.Id,
                NamaInstitusi = m.NamaInstitusi,
                NamaDirektur = m.NamaDirektur,
                TanggalSK = m.TanggalSK,
                NomorSK = m.NomorSK,
                Status = m.Status
            }).ToList();

            var response = new GetAllInstitusiResponse
            {
                Data = dataDTO,
                TotalData = totalData,
                TotalHalaman = ((totalData - 1) / dto.PageSize) + 1
            };

            return Ok(response);
        }

        [HttpGet("DetailInstitusi/{id}")]
        [RequiresPermission("institusi.view")]
        public async Task<IActionResult> GetInstitusiById(short id)
        {
            var dataDto = await _repo.GetByIdAsync(id);
            if (dataDto == null)
            {
                return NotFound(new { message = "Data institusi tidak ditemukan." });
            }
            return Ok(dataDto);
        }

        [HttpPost("CreateInstitusi")]
        [RequiresPermission("institusi.create")]
        public async Task<IActionResult> CreateInstitusi([FromBody] CreateInstitusiRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.FindFirstValue("namaakun");

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var sanitizedDto = SanitizerHelper.EncodeObject(dto);
            if (sanitizedDto == null) return NotFound();

            var newId = await _repo.CreateAsync(dto, username);

            return Ok(new { message = "SUCCESS", id = newId });
        }

        [HttpPut("EditInstitusi")]
        [RequiresPermission("institusi.edit")]
        public async Task<IActionResult> UpdateInstitusi([FromBody] UpdateInstitusiRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.FindFirstValue("namaakun");

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var sanitizedDto = SanitizerHelper.EncodeObject(dto);
            if (sanitizedDto == null) return NotFound();

            var success = await _repo.UpdateAsync(dto, username);
            if (!success)
            {
                return NotFound(new { message = "Data institusi tidak ditemukan." });
            }
            return Ok(new { message = "SUCCESS" });
        }

        [HttpPost("SetStatusInstitusi/{id}")]
        [RequiresPermission("institusi.edit")]
        public async Task<IActionResult> SetStatusInstitusi(short id)
        {
            var username = User.FindFirstValue("namaakun");

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var success = await _repo.SetStatusAsync(id, username);
            if (!success)
            {
                return NotFound(new { message = "Data institusi tidak ditemukan." });
            }
            return Ok(new { message = "SUCCESS" });
        }
    }
}
