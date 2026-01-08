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
            try
            {
                // Set default page size to show more records if not specified
                if (req.PageSize <= 0)
                {
                    req.PageSize = 50; // Increase default page size
                }
                
                Console.WriteLine($"[GetAll] Received request - Status: '{req.Status}', RoleId: '{req.RoleId}', SearchKeyword: '{req.SearchKeyword}', PageNumber: {req.PageNumber}, PageSize: {req.PageSize}");
                
                ModelState.Clear();
                var result = await _service.GetAllAsync(req);
                
                Console.WriteLine($"[GetAll] Returning {result.Data.Count()} records, Total: {result.TotalData}");
                
                // Log the status distribution for debugging
                var statusCounts = result.Data.GroupBy(x => x.Status).Select(g => new { Status = g.Key, Count = g.Count() });
                foreach (var statusCount in statusCounts)
                {
                    Console.WriteLine($"[GetAll] Status '{statusCount.Status}': {statusCount.Count} records in current page");
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAll] ERROR: {ex.Message}");
                return BadRequest(new { message = "Terjadi kesalahan saat mengambil data.", error = ex.Message });
            }
        }

        // Debug endpoint untuk melihat data yang dihapus
        [HttpGet("GetAll/deleted")]
        public async Task<IActionResult> GetAllDeleted([FromQuery] GetAllMeninggalDuniaRequest req)
        {
            // Override status untuk melihat data yang dihapus
            req.Status = "Dihapus";
            ModelState.Clear();
            return Ok(await _service.GetAllAsync(req));
        }

        // Debug endpoint untuk melihat data spesifik sebelum delete
        [HttpGet("debug/{id}")]
        public async Task<IActionResult> DebugGetById(string id)
        {
            try
            {
                var data = await _service.GetDetailAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan", id = id });
                }
                return Ok(new { 
                    message = "Data ditemukan", 
                    id = id,
                    data = data,
                    canDelete = !string.IsNullOrEmpty(data.MhsId)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = "Error saat mengambil data", 
                    id = id, 
                    error = ex.Message 
                });
            }
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

        [HttpGet("debug/role/{username}")]
        public async Task<IActionResult> DebugRoleDetection(string username)
        {
            try
            {
                var detectedRole = await _service.DetectUserRoleAsync(username);
                return Ok(new { 
                    username = username,
                    detectedRole = detectedRole,
                    success = !string.IsNullOrEmpty(detectedRole),
                    message = string.IsNullOrEmpty(detectedRole) ? "Role detection failed" : "Role detected successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    username = username,
                    error = ex.Message,
                    message = "Error during role detection"
                });
            }
        }

        [HttpGet("debug/ids")]
        public async Task<IActionResult> GetAllIds()
        {
            try
            {
                var data = await _service.GetAllAsync(new GetAllMeninggalDuniaRequest { PageSize = 100 });
                var ids = data.Data.Select(x => new { 
                    Id = x.Id, 
                    NoPengajuan = x.NoPengajuan,
                    Status = x.Status 
                }).ToList();
                
                return Ok(new { 
                    message = "Daftar ID yang tersedia", 
                    totalData = data.TotalData,
                    ids = ids 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error mengambil daftar ID", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(string id)
        {
            try
            {
                // Decode URL jika perlu
                id = Uri.UnescapeDataString(id);
                
                var data = await _service.GetDetailAsync(id);

                if (data == null)
                    return NotFound(new { message = $"Data dengan ID '{id}' tidak ditemukan" });

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Terjadi kesalahan saat mengambil detail data", error = ex.Message });
            }
        }

        [HttpGet("report/{id}")]
        public async Task<IActionResult> GetReport(string id)
        {
            var data = await _service.GetReportAsync(id);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ============================================
        // DOWNLOAD FILE
        // ============================================
        [HttpGet("file/{filename}")]
        public IActionResult DownloadFile(string filename)
        {
            // Coba beberapa lokasi file yang mungkin
            var possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "meninggal", filename),
                Path.Combine(Directory.GetCurrentDirectory(), "uploads", "meninggal", filename),
                Path.Combine(Directory.GetCurrentDirectory(), "uploads", "meninggal", "lampiran", filename),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "meninggal", "lampiran", filename)
            };

            string? foundPath = null;
            foreach (var path in possiblePaths)
            {
                if (System.IO.File.Exists(path))
                {
                    foundPath = path;
                    break;
                }
            }

            if (foundPath == null)
                return NotFound(new { 
                    message = "File tidak ditemukan.", 
                    filename = filename,
                    searchedPaths = possiblePaths.Select(p => p.Replace(Directory.GetCurrentDirectory(), "")).ToArray()
                });

            var fileBytes = System.IO.File.ReadAllBytes(foundPath);
            var contentType = GetContentType(filename);
            
            return File(fileBytes, contentType, filename);
        }

        private string GetContentType(string filename)
        {
            var extension = Path.GetExtension(filename).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
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
            try
            {
                var updatedBy = HttpContext.Items["UserId"]?.ToString() ?? "system";
                var officialId = await _service.FinalizeAsync(draftId, updatedBy);
                
                if (string.IsNullOrEmpty(officialId))
                {
                    return BadRequest(new { 
                        message = "Gagal memfinalisasi draft. Draft mungkin tidak ditemukan, sudah diproses, atau terjadi kesalahan dalam generate ID resmi.",
                        draftId = draftId
                    });
                }

                return Ok(new { 
                    message = "Draft berhasil difinalisasi menjadi pengajuan resmi.",
                    draftId = draftId,
                    officialId = officialId,
                    updatedBy = updatedBy
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat memfinalisasi draft.",
                    draftId = draftId,
                    error = ex.Message
                });
            }
        }

        

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromForm] UpdateMeninggalDuniaRequest dto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { message = "ID tidak boleh kosong." });
                }

                var updatedBy = HttpContext.Items["UserId"]?.ToString() ?? "system";

                // Validate file if provided
                if (dto.LampiranFile != null)
                {
                    var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(dto.LampiranFile.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { 
                            message = $"Tipe file tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" 
                        });
                    }

                    // Validate file size (max 10MB)
                    if (dto.LampiranFile.Length > 10 * 1024 * 1024)
                    {
                        return BadRequest(new { message = "Ukuran file maksimal 10MB." });
                    }
                }

                var success = await _service.UpdateAsync(id, dto, updatedBy);

                if (!success)
                {
                    return BadRequest(new { 
                        message = "Gagal memperbarui data. Data mungkin tidak ditemukan.",
                        id = id
                    });
                }

                return Ok(new { 
                    message = "Data berhasil diperbarui.",
                    id = id,
                    updatedBy = updatedBy,
                    hasFile = dto.LampiranFile != null,
                    mhsId = dto.MhsId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat memperbarui data.",
                    error = ex.Message,
                    id = id
                });
            }
        }

        [HttpPut("upload-sk")]
        public async Task<IActionResult> UploadSK([FromForm] UploadSKMeninggalRequest request)
        {
            try
            {
                Console.WriteLine($"[Controller] Upload SK Meninggal Dunia - MduId: {request.MduId}");
                Console.WriteLine($"[Controller] SK File: {request.SK?.FileName}");
                Console.WriteLine($"[Controller] SPKB File: {request.SKPB?.FileName}");
                Console.WriteLine($"[Controller] ModifiedBy: {request.ModifiedBy}");

                // Validate input
                if (string.IsNullOrEmpty(request.MduId))
                {
                    Console.WriteLine("[Controller] ERROR: MduId is required");
                    return BadRequest(new { message = "MduId harus diisi." });
                }

                if (request.SK == null || request.SK.Length == 0)
                {
                    Console.WriteLine("[Controller] ERROR: SK File is required");
                    return BadRequest(new { message = "File SK harus diupload." });
                }

                if (request.SKPB == null || request.SKPB.Length == 0)
                {
                    Console.WriteLine("[Controller] ERROR: SPKB File is required");
                    return BadRequest(new { message = "File SPKB harus diupload." });
                }

                if (string.IsNullOrEmpty(request.ModifiedBy))
                {
                    // Auto-set dari context jika tidak ada
                    request.ModifiedBy = HttpContext.Items["UserId"]?.ToString() ?? "system";
                    Console.WriteLine($"[Controller] Auto-set ModifiedBy to: {request.ModifiedBy}");
                }

                // Validate file types
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
                
                var skFileExtension = Path.GetExtension(request.SK.FileName).ToLowerInvariant();
                var spkbFileExtension = Path.GetExtension(request.SKPB.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(skFileExtension))
                {
                    Console.WriteLine($"[Controller] ERROR: Invalid SK file type: {skFileExtension}");
                    return BadRequest(new { message = $"Tipe file SK tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" });
                }

                if (!allowedExtensions.Contains(spkbFileExtension))
                {
                    Console.WriteLine($"[Controller] ERROR: Invalid SPKB file type: {spkbFileExtension}");
                    return BadRequest(new { message = $"Tipe file SPKB tidak diizinkan. Gunakan: {string.Join(", ", allowedExtensions)}" });
                }

                // Validate file sizes (max 10MB each)
                if (request.SK.Length > 10 * 1024 * 1024)
                {
                    Console.WriteLine($"[Controller] ERROR: SK file too large: {request.SK.Length} bytes");
                    return BadRequest(new { message = "Ukuran file SK maksimal 10MB." });
                }

                if (request.SKPB.Length > 10 * 1024 * 1024)
                {
                    Console.WriteLine($"[Controller] ERROR: SPKB file too large: {request.SKPB.Length} bytes");
                    return BadRequest(new { message = "Ukuran file SPKB maksimal 10MB." });
                }

                Console.WriteLine("[Controller] Calling service UploadSKAsync...");
                // Use existing UploadSKAsync method instead of UploadSKMeninggalAsync
                var result = await _service.UploadSKAsync(request.MduId, request.SK, request.SKPB, request.ModifiedBy);
                Console.WriteLine($"[Controller] Service returned: {result}");

                if (!result)
                {
                    Console.WriteLine("[Controller] Upload SK failed");
                    return BadRequest(new { message = "Gagal upload SK Meninggal Dunia. Periksa apakah MduId valid dan status adalah 'Menunggu Upload SK'." });
                }

                Console.WriteLine("[Controller] Upload SK successful");
                return Ok(new { 
                    message = "Upload SK berhasil. Status meninggal dunia telah diubah menjadi 'Disetujui'. Nomor SK akan ditampilkan otomatis dengan format tahun 2026.",
                    success = true,
                    mduId = request.MduId,
                    skFileName = request.SK.FileName,
                    spkbFileName = request.SKPB.FileName,
                    modifiedBy = request.ModifiedBy
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Controller] ERROR in UploadSK: {ex.Message}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengupload SK.", 
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

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
            try
            {
                // Decode URL jika perlu
                id = Uri.UnescapeDataString(id);
                
                Console.WriteLine($"[Approve] Starting approval for ID: {id}, Username: {dto.Username}");
                
                // Auto-detect role based on username using stored procedure
                var detectedRole = await _service.DetectUserRoleAsync(dto.Username);
                if (string.IsNullOrEmpty(detectedRole))
                {
                    Console.WriteLine($"[Approve] Could not detect role for username: {dto.Username}");
                    return BadRequest(new { 
                        message = "Tidak dapat mendeteksi role pengguna. Pastikan username valid.",
                        username = dto.Username
                    });
                }
                
                Console.WriteLine($"[Approve] Detected role: {detectedRole} for username: {dto.Username}");
                
                // Override role dengan hasil deteksi
                dto.Role = detectedRole;
                
                var result = await _service.ApproveAsync(id, dto);

                if (!result)
                {
                    Console.WriteLine($"[Approve] Approval failed for ID: {id}");
                    return BadRequest(new { 
                        message = "Gagal menyetujui pengajuan. Data mungkin tidak ditemukan atau sudah diproses.",
                        id = id,
                        detectedRole = detectedRole,
                        username = dto.Username
                    });
                }

                Console.WriteLine($"[Approve] Successfully approved ID: {id} by {dto.Username} as {detectedRole}");
                return Ok(new { 
                    approved = true,
                    id = id,
                    approvedBy = dto.Username,
                    role = detectedRole,
                    message = $"Pengajuan berhasil disetujui oleh {detectedRole}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Approve] Error: {ex.Message}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat menyetujui pengajuan.",
                    error = ex.Message,
                    id = id
                });
            }
        }

        [HttpPut("reject/{id}")]
        public async Task<IActionResult> Reject(string id, [FromBody] RejectMeninggalDuniaRequest dto)
        {
            try
            {
                // Decode URL jika perlu
                id = Uri.UnescapeDataString(id);
                
                Console.WriteLine($"[Reject] Starting rejection for ID: {id}, Username: {dto.Username}");
                
                // Auto-detect role based on username using stored procedure
                var detectedRole = await _service.DetectUserRoleAsync(dto.Username);
                if (string.IsNullOrEmpty(detectedRole))
                {
                    Console.WriteLine($"[Reject] Could not detect role for username: {dto.Username}");
                    return BadRequest(new { 
                        message = "Tidak dapat mendeteksi role pengguna. Pastikan username valid.",
                        username = dto.Username
                    });
                }
                
                Console.WriteLine($"[Reject] Detected role: {detectedRole} for username: {dto.Username}");
                
                // Override role dengan hasil deteksi
                dto.Role = detectedRole;
                
                var success = await _service.RejectAsync(id, dto);

                if (!success)
                {
                    Console.WriteLine($"[Reject] Rejection failed for ID: {id}");
                    return BadRequest(new { 
                        message = "Gagal menolak pengajuan. Data mungkin tidak ditemukan atau sudah diproses.",
                        id = id,
                        detectedRole = detectedRole,
                        username = dto.Username
                    });
                }

                Console.WriteLine($"[Reject] Successfully rejected ID: {id} by {dto.Username} as {detectedRole}");
                return Ok(new
                {
                    rejected = true,
                    id = id,
                    rejectedBy = dto.Username,
                    role = detectedRole,
                    message = $"Pengajuan berhasil ditolak oleh {detectedRole}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Reject] Error: {ex.Message}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat menolak pengajuan.",
                    error = ex.Message,
                    id = id
                });
            }
        }


        [HttpPost("{id}/upload-sk")]
        public async Task<IActionResult> UploadSK(string id, [FromForm] UploadSKMeninggalDuniaDto dto)
        {
            try
            {
                Console.WriteLine($"[Controller] Upload SK Meninggal Dunia - ID: {id}");
                Console.WriteLine($"[Controller] SK File: {dto.SkFile?.FileName}");
                Console.WriteLine($"[Controller] SPKB File: {dto.SpkbFile?.FileName}");

                var updatedBy = HttpContext.Items["UserId"]?.ToString() ?? "system";

                var success = await _service.UploadSKAsync(id, dto.SkFile, dto.SpkbFile, updatedBy);

                if (!success)
                {
                    Console.WriteLine("[Controller] Upload SK failed");
                    return BadRequest(new { message = "Gagal upload SK meninggal dunia. Periksa apakah ID valid dan status adalah 'Menunggu Upload SK'." });
                }

                Console.WriteLine("[Controller] Upload SK successful");
                return Ok(new { 
                    message = "SK berhasil diupload. Status meninggal dunia telah diubah menjadi 'Disetujui'. Nomor SK akan ditampilkan otomatis di daftar.",
                    success = true,
                    id = id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Controller] ERROR in UploadSK: {ex.Message}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat mengupload SK.", 
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("Riwayat")]
        public async Task<IActionResult> GetRiwayat([FromQuery] GetRiwayatMeninggalDuniaRequest req)
        {
            return Ok(await _service.GetRiwayatAsync(req));
        }
            


        /// <summary>
        /// Get riwayat meninggal dunia data as Excel file (only approved status)
        /// </summary>
        [HttpGet("riwayat/excel")]
        [ProducesResponseType(typeof(FileResult), 200)]
        public async Task<IActionResult> GetRiwayatExcel(
        [FromQuery] string sort = "",
        [FromQuery] string konsentrasi = ""
        )
        {
            try
            {
                var data = await _service.GetRiwayatExcelAsync(sort, konsentrasi);
                
                // Create Excel file using ClosedXML
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Riwayat Meninggal Dunia");

                // Add headers
                worksheet.Cell(1, 1).Value = "NIM";
                worksheet.Cell(1, 2).Value = "Nama Mahasiswa";
                worksheet.Cell(1, 3).Value = "Konsentrasi";
                worksheet.Cell(1, 4).Value = "Tanggal Pengajuan";
                worksheet.Cell(1, 5).Value = "No SK";
                worksheet.Cell(1, 6).Value = "No Pengajuan";

                // Style headers
                var headerRange = worksheet.Range(1, 1, 1, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                // Add data rows
                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.NIM;
                    worksheet.Cell(row, 2).Value = item.NamaMahasiswa;
                    worksheet.Cell(row, 3).Value = item.Konsentrasi;
                    worksheet.Cell(row, 4).Value = item.TanggalPengajuan;
                    worksheet.Cell(row, 5).Value = item.NoSK;
                    worksheet.Cell(row, 6).Value = item.NoPengajuan;
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Add borders to data
                if (row > 2)
                {
                    var dataRange = worksheet.Range(2, 1, row - 1, 6);
                    dataRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                }

                // Generate file
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"RiwayatMeninggalDunia_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating Excel: {ex.Message}");
                return BadRequest(new { 
                    message = "Terjadi kesalahan saat membuat file Excel.", 
                    error = ex.Message 
                });
            }
        }



    }
}
