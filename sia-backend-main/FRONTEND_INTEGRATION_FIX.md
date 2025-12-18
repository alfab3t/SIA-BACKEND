# Frontend Integration Fix - Approval/Rejection Endpoints

## 🔴 Problem Identified

From the frontend logs, the issue is clear:
```javascript
Payload: {"Id": "036/PMA/CA/XII/2025","Menimbang": "","ApprovedBy": "nda_prodi"}
```

**The `Menimbang` field is being sent as an EMPTY STRING**, which causes the backend to fail.

## ✅ Backend Fixes Applied

### 1. Added Validation in Controller
The backend now validates that `Menimbang` is not empty:
```csharp
if (string.IsNullOrWhiteSpace(dto.Menimbang))
{
    return BadRequest(new { message = "Menimbang/pertimbangan harus diisi dan tidak boleh kosong." });
}
```

### 2. Added Data Annotations to DTO
```csharp
[Required(ErrorMessage = "Menimbang/pertimbangan harus diisi")]
[MinLength(10, ErrorMessage = "Menimbang minimal 10 karakter")]
public string Menimbang { get; set; } = "";
```

## 🔧 Frontend Fix Required

The frontend needs to ensure the `Menimbang` field is filled before sending the request.

### Current Frontend Code (WRONG):
```javascript
Payload: {
  "Id": "036/PMA/CA/XII/2025",
  "Menimbang": "",  // ❌ EMPTY STRING
  "ApprovedBy": "nda_prodi"
}
```

### Fixed Frontend Code (CORRECT):
```javascript
Payload: {
  "Id": "036/PMA/CA/XII/2025",
  "Menimbang": "Mahasiswa memenuhi syarat untuk cuti akademik sesuai dengan ketentuan yang berlaku",  // ✅ FILLED
  "ApprovedBy": "nda_prodi"
}
```

## 📋 Frontend Changes Needed

### For Prodi Approval (`/api/CutiAkademik/approve/prodi`)

1. **Add a text input or textarea** for the `Menimbang` field in the approval form/modal
2. **Make it required** with minimum 10 characters
3. **Validate before sending** the API request

Example validation:
```javascript
if (!menimbang || menimbang.trim().length < 10) {
  alert("Menimbang/pertimbangan harus diisi minimal 10 karakter");
  return;
}
```

### For Rejection (`/api/CutiAkademik/reject`)

Similarly, the `Keterangan` field should be filled:
```javascript
Payload: {
  "Id": "036/PMA/CA/XII/2025",
  "Role": "prodi",
  "Keterangan": "Dokumen tidak lengkap"  // ✅ FILLED
}
```

## 🧪 Testing Steps

### 1. Test with Swagger First
Before fixing the frontend, test the backend with Swagger:

**Endpoint:** `PUT /api/CutiAkademik/approve/prodi`

**Request Body:**
```json
{
  "id": "036/PMA/CA/XII/2025",
  "menimbang": "Mahasiswa memenuhi syarat untuk cuti akademik sesuai dengan ketentuan yang berlaku di institusi",
  "approvedBy": "nda_prodi"
}
```

**Expected Response (200):**
```json
{
  "message": "Cuti akademik berhasil disetujui oleh prodi."
}
```

### 2. Test with Empty Menimbang
**Request Body:**
```json
{
  "id": "036/PMA/CA/XII/2025",
  "menimbang": "",
  "approvedBy": "nda_prodi"
}
```

**Expected Response (400):**
```json
{
  "message": "Menimbang/pertimbangan harus diisi dan tidak boleh kosong."
}
```

## 📝 API Endpoint Summary

### Prodi Approval
- **Endpoint:** `PUT /api/CutiAkademik/approve/prodi`
- **Required Fields:**
  - `id` (string): ID cuti akademik
  - `menimbang` (string, min 10 chars): Pertimbangan persetujuan
  - `approvedBy` (string): Username prodi

### General Approval (Wadir1/Finance)
- **Endpoint:** `PUT /api/CutiAkademik/approve`
- **Required Fields:**
  - `id` (string): ID cuti akademik
  - `role` (string): "wadir1" or "finance"
  - `approvedBy` (string): Username

### Rejection
- **Endpoint:** `PUT /api/CutiAkademik/reject`
- **Required Fields:**
  - `id` (string): ID cuti akademik
  - `role` (string): "prodi", "wadir1", or "finance"
  - `keterangan` (string): Alasan penolakan

## 🎯 Root Cause

The stored procedure `sia_setujuiCutiAkademikProdi` expects:
- `@p1` = ID cuti akademik
- `@p2` = Menimbang (cannot be empty)
- `@p3` = Username prodi

When `@p2` is empty, the stored procedure either:
1. Returns 0 rows affected (no update)
2. Or has validation that rejects empty values

## ✅ Status

- ✅ Backend validation added
- ✅ DTO annotations added
- ✅ Enhanced logging added
- ⏳ **Frontend needs to be updated** to collect and send the `Menimbang` value

## 📞 Next Steps for Frontend Developer

1. Add a form field for `Menimbang` in the approval UI
2. Make it required with validation (min 10 characters)
3. Pass the value to the API request
4. Test with the backend

Once the frontend sends a non-empty `Menimbang` value, the approval will work correctly.
