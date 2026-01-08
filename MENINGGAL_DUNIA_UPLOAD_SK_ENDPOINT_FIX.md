# Meninggal Dunia - Upload SK Endpoint Fix (JSON to File Upload)

## Problem
Endpoint `/api/MeninggalDunia/upload-sk` menggunakan JSON body dengan string fields untuk SK dan SPKB:

```json
{
  "mduId": "string",
  "sk": "string",      // ← String, bukan file
  "skpb": "string",    // ← String, bukan file  
  "modifiedBy": "string"
}
```

User ingin field `sk` dan `skpb` menjadi **choose file** (file upload).

## Solution: Convert to Multipart/Form-Data

### Changes Made

#### 1. Updated DTO for File Upload
**File**: `sia-backend-main/DTOs/MeninggalDunia/UploadSKMeninggalRequest.cs`

```csharp
// Before (JSON)
public class UploadSKMeninggalRequest
{
    public string MduId { get; set; } = "";
    public string SK { get; set; } = "";      // String
    public string SKPB { get; set; } = "";    // String
    public string ModifiedBy { get; set; } = "";
}

// After (File Upload)
public class UploadSKMeninggalRequest
{
    public string MduId { get; set; } = "";
    public IFormFile? SK { get; set; }        // File upload
    public IFormFile? SKPB { get; set; }      // File upload
    public string ModifiedBy { get; set; } = "";
}
```

#### 2. Updated Controller Endpoint
**File**: `sia-backend-main/Controllers/MeninggalDuniaController.cs`

```csharp
// Before (JSON Body)
[HttpPut("upload-sk")]
public async Task<IActionResult> UploadSK([FromBody] UploadSKMeninggalRequest request)

// After (Form Data)
[HttpPut("upload-sk")]
public async Task<IActionResult> UploadSK([FromForm] UploadSKMeninggalRequest request)
```

#### 3. Added File Validation
- **File Type**: PDF, DOC, DOCX, JPG, PNG
- **File Size**: Max 10MB per file
- **Required**: Both SK and SPKB files required
- **Error Messages**: Specific validation messages

#### 4. Updated Service Layer
**File**: `sia-backend-main/Services/Implementations/MeninggalDuniaService.cs`

```csharp
public async Task<bool> UploadSKMeninggalAsync(UploadSKMeninggalRequest request)
{
    // Save files to server
    string? skFileName = await SaveFileAsync(request.SK, "meninggal");
    string? spkbFileName = await SaveFileAsync(request.SKPB, "meninggal");
    
    // Call repository with file names (uses existing UploadSKAsync method)
    return await _repo.UploadSKAsync(request.MduId, skFileName, spkbFileName, request.ModifiedBy);
}
```

## API Usage

### New Endpoint Format
```
PUT /api/MeninggalDunia/upload-sk
Content-Type: multipart/form-data
```

### Parameters
- **MduId**: string (required) - Meninggal Dunia ID
- **SK**: file (required) - Choose SK file
- **SKPB**: file (required) - Choose SPKB file  
- **ModifiedBy**: string (optional) - Auto-set if empty

### Supported File Types
- PDF (.pdf)
- Word (.doc, .docx)
- Images (.jpg, .jpeg, .png)
- Max size: 10MB per file

## Test Examples

### ✅ Using cURL
```bash
curl -X 'PUT' \
  'http://localhost:5234/api/MeninggalDunia/upload-sk' \
  -H 'accept: */*' \
  -H 'Content-Type: multipart/form-data' \
  -F 'MduId=012/PA-MD/I/2026' \
  -F 'SK=@sk_file.pdf;type=application/pdf' \
  -F 'SKPB=@spkb_file.pdf;type=application/pdf' \
  -F 'ModifiedBy=user_admin'
```

### ✅ Using Swagger UI
```
Parameters:
- MduId: 012/PA-MD/I/2026
- SK: [Choose File] → Select SK file
- SKPB: [Choose File] → Select SPKB file  
- ModifiedBy: user_admin
```

### ✅ Using Frontend Form
```html
<form enctype="multipart/form-data">
  <input type="text" name="MduId" value="012/PA-MD/I/2026" />
  <input type="file" name="SK" accept=".pdf,.doc,.docx,.jpg,.jpeg,.png" />
  <input type="file" name="SKPB" accept=".pdf,.doc,.docx,.jpg,.jpeg,.png" />
  <input type="text" name="ModifiedBy" value="user_admin" />
</form>
```

## Expected Results

### ✅ Success Response
```json
{
  "message": "Upload SK berhasil. Status meninggal dunia telah diubah menjadi 'Disetujui'. Nomor SK akan ditampilkan otomatis dengan format tahun 2026.",
  "success": true,
  "mduId": "012/PA-MD/I/2026",
  "skFileName": "sk_file.pdf",
  "spkbFileName": "spkb_file.pdf",
  "modifiedBy": "user_admin"
}
```

### ❌ Validation Error Examples
```json
// Missing SK file
{
  "message": "File SK harus diupload."
}

// Missing SPKB file
{
  "message": "File SPKB harus diupload."
}

// Invalid file type
{
  "message": "Tipe file SK tidak diizinkan. Gunakan: .pdf, .doc, .docx, .jpg, .jpeg, .png"
}

// File too large
{
  "message": "Ukuran file SK maksimal 10MB."
}
```

### ✅ GetAll Response (with Generated SK Number)
```json
{
  "id": "012/PA-MD/I/2026",
  "noPengajuan": "012/PA-MD/I/2026",
  "tanggalPengajuan": "08 Jan 2026",
  "namaMahasiswa": "JOHN DOE",
  "nim": "0320250001",
  "prodi": "Manajemen Informatika",
  "nomorSK": "012/PA-WADIR-I/SKM/I/2026",  // ← Generated with 2026!
  "status": "Disetujui"
}
```

## Key Features

### ✅ File Upload Support
- **Choose File**: Frontend dapat menggunakan file picker
- **Dual Files**: Support SK dan SPKB file upload
- **Auto Save**: Files disimpan otomatis ke server dengan unique names

### ✅ Comprehensive Validation
- **File Type**: PDF, DOC, DOCX, JPG, PNG
- **File Size**: Max 10MB per file
- **Required Fields**: MduId, SK file, SPKB file
- **Auto ModifiedBy**: Set otomatis jika kosong

### ✅ Integration with Existing Logic
- **Uses UploadSKAsync**: Menggunakan method yang sudah ada dan teruji
- **Bypass Foreign Key**: Tetap menggunakan solusi bypass constraint
- **Generate SK Number**: Tetap generate nomor SK dengan format 2026
- **Status Update**: Update status ke "Disetujui" dan mahasiswa status

### ✅ Better User Experience
- **Clear Error Messages**: Specific validation messages
- **File Info in Response**: Nama file yang diupload dalam response
- **Detailed Logging**: Console logging untuk debugging

## Files Modified
1. `sia-backend-main/DTOs/MeninggalDunia/UploadSKMeninggalRequest.cs`
2. `sia-backend-main/Controllers/MeninggalDuniaController.cs`
3. `sia-backend-main/Services/Implementations/MeninggalDuniaService.cs`
4. `sia-backend-main/Repositories/Implementations/MeninggalDuniaRepository.cs`

## Summary

### Before ❌
```json
PUT /api/MeninggalDunia/upload-sk
Content-Type: application/json

{
  "mduId": "string",
  "sk": "string",      // ← String field
  "skpb": "string",    // ← String field
  "modifiedBy": "string"
}
```

### After ✅
```
PUT /api/MeninggalDunia/upload-sk
Content-Type: multipart/form-data

Parameters:
- MduId: string
- SK: file (choose file)      // ← File upload
- SKPB: file (choose file)    // ← File upload
- ModifiedBy: string
```

Sekarang endpoint `/api/MeninggalDunia/upload-sk` sudah support **choose file** untuk SK dan SPKB dengan validation lengkap dan tetap menggunakan logic yang sudah teruji!