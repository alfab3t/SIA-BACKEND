# Troubleshooting: Prodi Approval Endpoint

## Issue Fixed
The prodi approval endpoint was returning 400 Bad Request because the stored procedure `sia_setujuiCutiAkademikProdi` requires the `menimbang` parameter, but the repository method was passing an empty string instead of using the DTO value.

## Root Cause
In `CutiAkademikRepository.cs`, the `ApproveProdiCutiAsync` method was hardcoded:
```csharp
cmd.Parameters.AddWithValue("@p2", ""); // Menimbang kosong - WRONG!
```

## Fix Applied
Updated to use the DTO value:
```csharp
cmd.Parameters.AddWithValue("@p2", dto.Menimbang ?? ""); // Gunakan menimbang dari DTO - CORRECT!
```

## How to Test in Swagger

### 1. Check if Data Exists First
- Go to `GET /api/CutiAkademik/debug/check-id/{id}`
- Replace `{id}` with `033%2FPMA%2FCA%2FXIII%2F2025` (URL encoded)
- This will tell you if the ID exists and its current status

### 2. Navigate to Prodi Approval Endpoint
- Go to `PUT /api/CutiAkademik/approve/prodi`
- Click "Try it out"

### 3. Use This Request Body Format
```json
{
  "id": "036/PMA/CA/XII/2025",
  "menimbang": "Mahasiswa memenuhi syarat untuk cuti akademik sesuai dengan ketentuan yang berlaku di institusi",
  "approvedBy": "nda_prodi"
}
```

### 4. Required Fields
- **id**: ID cuti akademik yang akan disetujui
- **menimbang**: Pertimbangan/alasan persetujuan (WAJIB diisi, minimal 10 karakter)
- **approvedBy**: Username prodi yang menyetujui

### 5. Common Frontend Issues
- **Empty Menimbang**: Frontend mengirim `"menimbang": ""` (string kosong) → Backend akan reject dengan error "Menimbang/pertimbangan harus diisi dan tidak boleh kosong"
- **Missing Fields**: Pastikan semua field required terisi dengan benar

### 5. Expected Response
**Success (200):**
```json
{
  "message": "Cuti akademik berhasil disetujui oleh prodi."
}
```

**Error (400):**
```json
{
  "message": "Gagal menyetujui cuti akademik. Periksa apakah ID valid dan data dapat diupdate.",
  "error": "Error details..."
}
```

## Testing Other Approval Endpoints

### General Approval (Wadir1/Finance)
**Endpoint:** `PUT /api/CutiAkademik/approve`
```json
{
  "id": "033/PMA/CA/XIII/2025",
  "role": "wadir1",
  "approvedBy": "wadir1_user"
}
```

### Rejection
**Endpoint:** `PUT /api/CutiAkademik/reject`
```json
{
  "id": "033/PMA/CA/XIII/2025",
  "role": "prodi"
}
```

## Files Modified
1. `sia-backend-main/Repositories/Implementations/CutiAkademikRepository.cs`
   - Fixed `ApproveProdiCutiAsync` method to use `dto.Menimbang`
2. `sia-backend-main/Controllers/CutiAkademikController.cs`
   - Updated documentation to reflect menimbang requirement

## Status
✅ **FIXED** - Prodi approval endpoint now properly uses menimbang field from request body.