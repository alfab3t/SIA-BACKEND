# Backend Update Complete - Pengunduran Diri

## Status: ✅ COMPLETE

Backend sudah disesuaikan dengan SP baru `sia_getDataPengunduranDiri` yang mengadopsi pola dari `sia_getDataPendingDO`.

---

## 1. Stored Procedure (SP) Baru

### File: `SQL_UPDATE_SP_GET_DATA_PENGUNDURAN_DIRI_NEW.sql`

**Parameter SP:**
```sql
@username VARCHAR(50),        -- Support mahasiswa (NIM) dan karyawan (username)
@pdi_status VARCHAR(50),      -- Legacy parameter (not used)
@kry_id VARCHAR(50),          -- Legacy parameter (not used)
@status VARCHAR(MAX) = '',    -- Multiple status (comma-separated)
@Page INT = NULL,             -- Pagination
@PageSize INT = NULL          -- Pagination
```

**Fitur SP:**
- ✅ Support mahasiswa (NIM) dan karyawan (username) via `@username`
- ✅ Role-based access control menggunakan `str_main_id` lookup
- ✅ Multiple status support via `@status` parameter (comma-separated)
- ✅ Single dynamic SQL query (tidak ada mode 1-4 lagi)
- ✅ ROW_NUMBER pagination dengan COUNT(*) OVER() untuk total records
- ✅ Priority sorting: Draft/Revisi on top, then by modified date DESC

**Role-Based Access:**
- Mahasiswa (NIM) → own data only
- Wadir 1 (str_main_id=2) → Belum Disetujui Wadir 1
- Direktur (str_main_id=1) → multiple status
- Admin (str_main_id=14,54,26,19) → most status
- Staff SK (str_main_id=27,23,28) → Disetujui, Menunggu Upload SK
- Sekprodi → konsentrasi data

---

## 2. Repository Update

### File: `Repositories/Implementations/PengunduranDiriRepository.cs`

### ✅ GetAllAsync Method
```csharp
public async Task<IEnumerable<PengunduranDiriListResponse>> GetAllAsync(
    string p1,      // username (NIM atau username karyawan)
    string status,  // multiple status (comma-separated)
    string userId)  // legacy parameter
{
    // Parameter sesuai SP baru
    cmd.Parameters.AddWithValue("@username", p1 ?? "");
    cmd.Parameters.AddWithValue("@pdi_status", "");            // Legacy (not used)
    cmd.Parameters.AddWithValue("@kry_id", userId ?? "");      // Legacy (not used)
    cmd.Parameters.AddWithValue("@status", status ?? "");      // Multiple status
    
    // SP baru menggunakan ROW_NUMBER, ada kolom rownum dan Count
    // Read Count from result set
}
```

### ✅ GetAllPaginatedAsync Method
```csharp
public async Task<PaginatedResponse<PengunduranDiriListResponse>> GetAllPaginatedAsync(
    string p1, 
    string status, 
    string userId,
    int page, 
    int pageSize)
{
    // Parameter sesuai SP baru
    cmd.Parameters.AddWithValue("@username", p1 ?? "");
    cmd.Parameters.AddWithValue("@pdi_status", "");            // Legacy (not used)
    cmd.Parameters.AddWithValue("@kry_id", userId ?? "");      // Legacy (not used)
    cmd.Parameters.AddWithValue("@status", status ?? "");      // Multiple status
    
    // Pagination parameters
    cmd.Parameters.AddWithValue("@Page", page);
    cmd.Parameters.AddWithValue("@PageSize", pageSize);
    
    // Read totalRecords from Count column in result set
    totalRecords = Convert.ToInt32(reader["Count"]);
}
```

**Key Changes:**
- ✅ Changed `@user_id` → `@username`
- ✅ Added `@status` parameter for multiple status
- ✅ Removed OUTPUT parameter logic
- ✅ Now reads Count from result set (COUNT(*) OVER())
- ✅ Legacy parameters `@pdi_status` and `@kry_id` kept for backward compatibility

---

## 3. Controller Update

### File: `Controllers/PengunduranDiriController.cs`

### ✅ GetAll Endpoint
```csharp
[HttpGet]
public async Task<IActionResult> GetAll(
    [FromQuery] string p1 = "",
    [FromQuery] string status = "",  // Support single or multiple status (comma-separated)
    [FromQuery] int? page = 1,
    [FromQuery] int? pageSize = 10)
{
    var userId = User.FindFirst("namaakun")?.Value ?? "";
    
    var result = await _repo.GetAllPaginatedAsync(p1, status, userId, pageVal, pageSizeVal);
    
    return Ok(result);
}
```

**Key Points:**
- ✅ Controller calls repository with correct parameters
- ✅ Supports pagination by default
- ✅ Supports multiple status via comma-separated string

---

## 4. Compilation Status

✅ **No compilation errors**
- `PengunduranDiriRepository.cs` - Clean
- `PengunduranDiriController.cs` - Clean

---

## 5. Next Steps

### Database Execution
1. Execute SQL script:
   ```sql
   -- File: SQL_UPDATE_SP_GET_DATA_PENGUNDURAN_DIRI_NEW.sql
   ```

2. Test dengan script:
   ```sql
   -- File: SQL_TEST_PENGUNDURAN_DIRI_PENGAJUAN.sql
   ```

### API Testing
Test endpoint dengan berbagai skenario:

```bash
# Test 1: Mahasiswa (NIM)
GET /api/pengundurandiri?p1=2101010001&status=Draft,Revisi&page=1&pageSize=10

# Test 2: Karyawan (username)
GET /api/pengundurandiri?p1=wadir1&status=Belum Disetujui Wadir 1&page=1&pageSize=10

# Test 3: Multiple status
GET /api/pengundurandiri?p1=admin&status=Draft,Belum Disetujui Wadir 1,Revisi&page=1&pageSize=10

# Test 4: All data (empty status)
GET /api/pengundurandiri?p1=admin&status=&page=1&pageSize=10
```

---

## 6. Backward Compatibility

✅ **Legacy parameters preserved:**
- `@pdi_status` - kept in SP signature (not used in logic)
- `@kry_id` - kept in SP signature (not used in logic)

✅ **New parameters:**
- `@username` - replaces `@user_id`, supports both NIM and username
- `@status` - replaces single status, supports comma-separated multiple status

---

## 7. Documentation Files

- ✅ `SP_PENGUNDURAN_DIRI_PENGAJUAN_UPDATE.md` - Full documentation
- ✅ `PENGUNDURAN_DIRI_SP_QUICK_GUIDE.md` - Quick reference
- ✅ `SQL_UPDATE_SP_GET_DATA_PENGUNDURAN_DIRI_NEW.sql` - SP script
- ✅ `SQL_TEST_PENGUNDURAN_DIRI_PENGAJUAN.sql` - Test script

---

## Summary

Backend code sudah disesuaikan dengan SP baru. Tinggal execute SQL script di database dan test API endpoints.

**Pattern yang sama dengan Drop Out:**
- Single SP untuk semua role
- Role-based access via `str_main_id`
- Multiple status support
- ROW_NUMBER pagination
- Priority sorting (Draft/Revisi on top)
