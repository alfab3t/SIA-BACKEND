# Perbaikan Update MeninggalDunia - Edit dengan File Upload

## Masalah
- Endpoint update hanya bisa mengedit lampiran (string)
- Tidak bisa upload file lampiran
- Tidak bisa mengedit mhsId, prodi, angkatan
- Parameter tidak sesuai dengan kebutuhan frontend

## Perbaikan yang Dilakukan

### 1. Update DTO (UpdateMeninggalDuniaRequest.cs)
```csharp
public class UpdateMeninggalDuniaRequest
{
    public string? MhsId { get; set; }           // Bisa edit mahasiswa ID
    public string? Lampiran { get; set; }        // Lampiran string (optional)
    public IFormFile? LampiranFile { get; set; } // Upload file lampiran (optional)
}
```

### 2. Controller - Support File Upload
```csharp
[HttpPut("{id}")]
public async Task<IActionResult> Update(string id, [FromForm] UpdateMeninggalDuniaRequest dto)
```

**Perubahan:**
- ✅ `[FromForm]` untuk mendukung file upload
- ✅ **File validation** (tipe dan ukuran)
- ✅ **Error handling** yang lebih baik
- ✅ **Response informatif** dengan detail update

### 3. Repository - Hybrid Implementation
```csharp
// Handle file upload
if (dto.LampiranFile != null) {
    // Save file ke uploads/meninggal/lampiran/
    fileName = $"{Guid.NewGuid()}_{dto.LampiranFile.FileName}";
}

// Update dengan direct SQL (lebih fleksibel)
UPDATE sia_msmeninggaldunia 
SET mdu_lampiran = @lampiran,
    mdu_modif_by = @updatedBy,
    mdu_modif_date = GETDATE()
    [, mhs_id = @mhsId]  // Jika MhsId disediakan
WHERE mdu_id = @id

// Fallback ke stored procedure jika perlu
```

## Endpoint Usage

### Update dengan File Upload
```
PUT /api/MeninggalDunia/{id}
Content-Type: multipart/form-data

Form Data:
- MhsId: "202012345" (optional)
- LampiranFile: [file] (optional)
- Lampiran: "nama_file.pdf" (optional)
```

### Update hanya MhsId
```
PUT /api/MeninggalDunia/{id}
Content-Type: multipart/form-data

Form Data:
- MhsId: "202012345"
```

### Update hanya File
```
PUT /api/MeninggalDunia/{id}
Content-Type: multipart/form-data

Form Data:
- LampiranFile: [file]
```

## File Validation
- **Tipe file**: .pdf, .doc, .docx, .jpg, .jpeg, .png
- **Ukuran maksimal**: 10MB
- **Lokasi penyimpanan**: `uploads/meninggal/lampiran/`
- **Nama file**: `{GUID}_{original_filename}`

## Response Examples

### Success Response
```json
{
  "message": "Data berhasil diperbarui.",
  "id": "3",
  "updatedBy": "system",
  "hasFile": true,
  "mhsId": "202012345"
}
```

### Error Response - File Invalid
```json
{
  "message": "Tipe file tidak diizinkan. Gunakan: .pdf, .doc, .docx, .jpg, .jpeg, .png"
}
```

### Error Response - File Too Large
```json
{
  "message": "Ukuran file maksimal 10MB."
}
```

## Testing

### Test 1: Update dengan File
```bash
curl -X PUT "http://localhost:5234/api/MeninggalDunia/3" \
  -F "MhsId=202012345" \
  -F "LampiranFile=@document.pdf"
```

### Test 2: Update hanya MhsId
```bash
curl -X PUT "http://localhost:5234/api/MeninggalDunia/3" \
  -F "MhsId=202012345"
```

### Test 3: Update hanya File
```bash
curl -X PUT "http://localhost:5234/api/MeninggalDunia/3" \
  -F "LampiranFile=@document.pdf"
```

## Catatan Penting
- **MhsId**: Bisa diupdate untuk mengganti mahasiswa
- **Prodi & Angkatan**: Akan otomatis berubah berdasarkan MhsId baru (relasi tabel)
- **File Upload**: Opsional, jika tidak ada file baru, lampiran lama tetap
- **Backward Compatibility**: Stored procedure tetap sebagai fallback
- **Logging**: Console log untuk debugging file upload dan update process