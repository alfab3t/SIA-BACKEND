# Fix: Prodi Draft Document Reading Issue

## Problem
Ketika role prodi membuat draft cuti akademik, dokumen/surat tidak bisa dibaca karena field `lampiranSP` dan `lampiran` kosong (empty strings).

### Contoh Data Bermasalah
```json
{
  "id": "1766405599562",
  "lampiranSP": "",
  "lampiran": "",
  "status": "Draft",
  "createdBy": "nda_prodi"
}
```

## Root Cause
1. **DTO Issue**: `CreateCutiProdiRequest` menggunakan `string` untuk field file, bukan `IFormFile`
2. **Repository Issue**: Method `CreateDraftByProdiAsync` tidak memproses file upload dengan benar
3. **Mismatch**: Controller menggunakan `[FromForm]` tapi DTO tidak mendukung file upload

## Solution

### 1. Update DTO untuk Support File Upload
**File**: `sia-backend-main/DTOs/CutiAkademik/CreateCutiProdiRequest.cs`

**Before**:
```csharp
public class CreateCutiProdiRequest
{
    public string LampiranSuratPengajuan { get; set; } = "";
    public string Lampiran { get; set; } = "";
    // ... other fields
}
```

**After**:
```csharp
using Microsoft.AspNetCore.Http;

public class CreateCutiProdiRequest
{
    public IFormFile? LampiranSuratPengajuan { get; set; }
    public IFormFile? Lampiran { get; set; }
    // ... other fields
}
```

### 2. Update Repository untuk Process File Upload
**File**: `sia-backend-main/Repositories/Implementations/CutiAkademikRepository.cs`

#### Method: `CreateDraftByProdiAsync`
**Before**:
```csharp
cmd.Parameters.AddWithValue("@p4", dto.LampiranSuratPengajuan ?? "");
cmd.Parameters.AddWithValue("@p5", dto.Lampiran ?? "");
```

**After**:
```csharp
// Simpan file terlebih dahulu (sama seperti mahasiswa)
var fileSP = SaveFile(dto.LampiranSuratPengajuan);
var fileLampiran = SaveFile(dto.Lampiran);

cmd.Parameters.AddWithValue("@p4", fileSP ?? "");
cmd.Parameters.AddWithValue("@p5", fileLampiran ?? "");
```

#### Method: `CreateDraftByProdiDirectAsync` (Fallback)
**Before**:
```csharp
cmd.Parameters.AddWithValue("@lampiran_sp", dto.LampiranSuratPengajuan ?? "");
cmd.Parameters.AddWithValue("@lampiran", dto.Lampiran ?? "");
```

**After**:
```csharp
// Simpan file terlebih dahulu
var fileSP = SaveFile(dto.LampiranSuratPengajuan);
var fileLampiran = SaveFile(dto.Lampiran);

cmd.Parameters.AddWithValue("@lampiran_sp", fileSP ?? "");
cmd.Parameters.AddWithValue("@lampiran", fileLampiran ?? "");
```

## How It Works Now

1. **Frontend** mengirim form data dengan file upload ke endpoint `/api/CutiAkademik/prodi/draft`
2. **Controller** menerima `CreateCutiProdiRequest` dengan `IFormFile` properties
3. **Repository** memanggil `SaveFile()` untuk:
   - Menyimpan file ke `wwwroot/uploads/cuti/`
   - Generate unique filename dengan GUID
   - Return filename untuk disimpan ke database
4. **Database** menyimpan filename (bukan empty string)
5. **Frontend** bisa membaca dokumen dengan mengakses `/api/CutiAkademik/file/{filename}`

## Testing

### Test Case 1: Create Draft by Prodi with Files
```bash
POST http://localhost:5234/api/CutiAkademik/prodi/draft
Content-Type: multipart/form-data

{
  "MhsId": "0220170060",
  "TahunAjaran": "2023/2024",
  "Semester": "Ganjil",
  "LampiranSuratPengajuan": [file upload],
  "Lampiran": [file upload],
  "Menimbang": "<p>Pertimbangan prodi</p>",
  "ApprovalProdi": "nda_prodi"
}
```

**Expected Result**:
- Draft created successfully
- `lampiranSP` contains filename (e.g., "74e3c9d7-3dd2-4563-9c1d-9c7b8dffbeb3.docx")
- `lampiran` contains filename (e.g., "563291b7-57a5-4902-b944-7f1626d8e18f.docx")
- Files saved to `wwwroot/uploads/cuti/`

### Test Case 2: Get Detail Draft
```bash
GET http://localhost:5234/api/CutiAkademik/detail?id=1766405599562
```

**Expected Result**:
```json
{
  "id": "1766405599562",
  "lampiranSP": "74e3c9d7-3dd2-4563-9c1d-9c7b8dffbeb3.docx",
  "lampiran": "563291b7-57a5-4902-b944-7f1626d8e18f.docx",
  "status": "Draft",
  "createdBy": "nda_prodi"
}
```

### Test Case 3: Download File
```bash
GET http://localhost:5234/api/CutiAkademik/file/74e3c9d7-3dd2-4563-9c1d-9c7b8dffbeb3.docx
```

**Expected Result**:
- File downloaded successfully
- Content-Type: application/octet-stream

## Files Modified
1. ✅ `sia-backend-main/DTOs/CutiAkademik/CreateCutiProdiRequest.cs`
2. ✅ `sia-backend-main/Repositories/Implementations/CutiAkademikRepository.cs`

## Status
✅ **FIXED** - Prodi sekarang bisa upload dokumen saat membuat draft cuti akademik dan dokumen bisa dibaca dengan benar.

## Notes
- File upload menggunakan `IFormFile` yang merupakan standard ASP.NET Core
- File disimpan dengan GUID untuk menghindari collision
- Method `SaveFile()` sudah ada dan digunakan oleh mahasiswa, sekarang juga digunakan oleh prodi
- Fallback method juga sudah diupdate untuk konsistensi
