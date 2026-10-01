# Pagination Implementation - Pengunduran Diri Module

## Summary

Pagination telah diimplementasikan untuk modul Pengunduran Diri dengan approach yang sama seperti Drop Out module, dengan menyesuaikan struktur asli SP yang memiliki multiple modes.

## Changes Made

### 1. SQL Stored Procedures
**File:** `SQL_UPDATE_PENGUNDURAN_DIRI_ADD_PAGINATION.sql`

Updated 2 stored procedures sesuai struktur asli:

#### A. `sia_getDataRiwayatPengunduranDiri` - 4 Modes
Struktur asli SP memiliki 4 mode berbeda berdasarkan `@pdi_status`:
- **Mode 1:** Status "Belum Disetujui Wadir 1" (untuk Wadir 1)
- **Mode 2:** Status "Belum Disetujui Prodi" (untuk Prodi/Sekprodi)
- **Mode 3:** Status "Menunggu Upload SK" (untuk Admin)
- **Mode 4:** Status lainnya (default)

**Parameters Added:**
- `@Page INT = NULL` (optional)
- `@PageSize INT = NULL` (optional)
- `@TotalRecords INT OUTPUT` (output parameter)

**Original Parameters Preserved:**
- `@username VARCHAR(50)`
- `@pdi_status VARCHAR(50)`
- `@unused VARCHAR(50)` - Parameter asli yang tidak digunakan tapi tetap ada
- `@keyword VARCHAR(MAX)`
- `@order_by VARCHAR(100)`
- `@kon_id VARCHAR(50)`

#### B. `sia_getDataPengunduranDiri` - 2 Modes
Struktur asli SP memiliki 2 mode berbeda:
- **Mode 1:** Mahasiswa melihat data sendiri (`@pdi_status = ''`)
- **Mode 2:** Admin/Staff melihat berdasarkan status atau data yang dibuat sendiri

**Parameters Added:**
- `@Page INT = NULL` (optional)
- `@PageSize INT = NULL` (optional)
- `@TotalRecords INT OUTPUT` (output parameter)

**Original Parameters Preserved:**
- `@user_id VARCHAR(50)`
- `@pdi_status VARCHAR(50)`
- `@kry_id VARCHAR(50)` - Parameter asli (tidak digunakan dalam logic tapi tetap ada)

**Backward Compatible:** Jika @Page dan @PageSize NULL, return semua data seperti sebelumnya.

### 2. DTOs
Menggunakan DTOs yang sudah ada:
- `PaginatedResponse<T>` (from `DTOs/Common/PaginatedResponse.cs`)
- `PaginationInfo` (from `DTOs/Common/PaginationInfo.cs`)

### 3. Repository Interface
**File:** `Repositories/Interfaces/IPengunduranDiriRepository.cs`

Added methods:
```csharp
Task<PaginatedResponse<PengunduranDiriListResponse>> GetAllPaginatedAsync(
    string p1, string status, string userId, int page, int pageSize);

Task<PaginatedResponse<PengunduranDiriRiwayatResponse>> GetRiwayatPaginatedAsync(
    string username, string status, string keyword, string orderBy, 
    string konsentrasi, int page, int pageSize);
```

### 4. Repository Implementation
**File:** `Repositories/Implementations/PengunduranDiriRepository.cs`

Implemented:
- `GetAllPaginatedAsync()` - Calls `sia_getDataPengunduranDiri` with pagination
- `GetRiwayatPaginatedAsync()` - Calls `sia_getDataRiwayatPengunduranDiri` with pagination (includes `@unused` parameter)

### 5. Controller
**File:** `Controllers/PengunduranDiriController.cs`

Updated endpoints:
- `GET /api/PengunduranDiri` - Default page=1, pageSize=10
- `GET /api/PengunduranDiri/riwayat` - Default page=1, pageSize=10

## API Usage

### Endpoint: GET /api/PengunduranDiri (Pengajuan)
```bash
# Default pagination (10 data per page)
GET /api/PengunduranDiri?p1=&status=

# Custom pagination
GET /api/PengunduranDiri?p1=&status=&page=2&pageSize=20

# With filters
GET /api/PengunduranDiri?p1=&status=Draft&page=1&pageSize=10
```

**Response:**
```json
{
  "data": [
    {
      "pdiId": "PD001",
      "mhsId": "123456",
      "namaMahasiswa": "John Doe",
      "status": "Draft",
      ...
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalRecords": 45,
    "totalPages": 5
  }
}
```

### Endpoint: GET /api/PengunduranDiri/riwayat
```bash
# Default pagination (10 data per page)
GET /api/PengunduranDiri/riwayat

# Custom pagination
GET /api/PengunduranDiri/riwayat?page=2&pageSize=20

# With filters
GET /api/PengunduranDiri/riwayat?status=Disetujui&keyword=john&page=1&pageSize=10
```

**Response:** Same format as above

## Default Values

- **Default Page:** 1
- **Default PageSize:** 10
- **Maximum PageSize:** 100
- **Page Index:** 1-based (page 1 = first page)

## Testing Steps

### 1. Run SQL Script
```sql
-- Execute: SQL_UPDATE_PENGUNDURAN_DIRI_ADD_PAGINATION.sql
-- This will ALTER the existing SPs
```

### 2. Test SQL Directly

#### Test Riwayat (4 modes)
```sql
-- Mode 1: Belum Disetujui Wadir 1
DECLARE @total INT;
EXEC sia_getDataRiwayatPengunduranDiri 
    @username = 'admin',
    @pdi_status = 'Belum Disetujui Wadir 1',
    @unused = '',
    @keyword = '',
    @order_by = 'pdi_created_date desc',
    @kon_id = '',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
SELECT @total AS TotalRecords;

-- Mode 2: Belum Disetujui Prodi
DECLARE @total INT;
EXEC sia_getDataRiwayatPengunduranDiri 
    @username = 'sekprodi_username',
    @pdi_status = 'Belum Disetujui Prodi',
    @unused = '',
    @keyword = '',
    @order_by = 'pdi_created_date desc',
    @kon_id = '',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
SELECT @total AS TotalRecords;

-- Mode 3: Menunggu Upload SK
DECLARE @total INT;
EXEC sia_getDataRiwayatPengunduranDiri 
    @username = 'admin',
    @pdi_status = 'Menunggu Upload SK',
    @unused = '',
    @keyword = '',
    @order_by = 'pdi_created_date desc',
    @kon_id = '',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
SELECT @total AS TotalRecords;

-- Mode 4: Status lainnya (default)
DECLARE @total INT;
EXEC sia_getDataRiwayatPengunduranDiri 
    @username = 'admin',
    @pdi_status = 'Disetujui',
    @unused = '',
    @keyword = '',
    @order_by = 'pdi_created_date desc',
    @kon_id = '',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
SELECT @total AS TotalRecords;
```

#### Test Pengajuan (2 modes)
```sql
-- Mode 1: Mahasiswa (status kosong)
DECLARE @total INT;
EXEC sia_getDataPengunduranDiri 
    @user_id = '123456',
    @pdi_status = '',
    @kry_id = '',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
SELECT @total AS TotalRecords;

-- Mode 2: Admin/Staff (dengan status)
DECLARE @total INT;
EXEC sia_getDataPengunduranDiri 
    @user_id = 'admin',
    @pdi_status = 'Draft',
    @kry_id = 'admin',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
SELECT @total AS TotalRecords;
```

### 3. Test API via Swagger
```
http://localhost:5234/swagger
```

Test endpoints:
- `GET /api/PengunduranDiri?page=1&pageSize=10`
- `GET /api/PengunduranDiri/riwayat?page=1&pageSize=10`

### 4. Verify Response Format
Check that response includes:
- `data` array with records
- `pagination` object with currentPage, pageSize, totalRecords, totalPages

## Important Notes

### SP Structure Preserved
Kedua SP mempertahankan struktur asli dengan multiple modes:

1. **sia_getDataRiwayatPengunduranDiri** - 4 modes berbeda berdasarkan status
2. **sia_getDataPengunduranDiri** - 2 modes (mahasiswa vs admin/staff)

### Parameter Compatibility
- Semua parameter asli dipertahankan (termasuk `@unused` dan `@kry_id` yang tidak digunakan)
- Parameter pagination bersifat optional (NULL = return all data)
- Backward compatible dengan code yang sudah ada

### Pagination Logic
- Menggunakan `OFFSET @Offset ROWS FETCH NEXT ISNULL(@PageSize, 2147483647) ROWS ONLY`
- Jika @PageSize NULL, fetch semua data (2147483647 = max int)
- Total count dihitung sebelum pagination untuk setiap mode

## Files Modified

### SQL:
- ✅ `SQL_UPDATE_PENGUNDURAN_DIRI_ADD_PAGINATION.sql` (NEW - Updated to match original SP structure)

### C#:
- ✅ `Repositories/Interfaces/IPengunduranDiriRepository.cs`
- ✅ `Repositories/Implementations/PengunduranDiriRepository.cs` (includes @unused parameter)
- ✅ `Controllers/PengunduranDiriController.cs`

### DTOs (Already Exist):
- ✅ `DTOs/Common/PaginatedResponse.cs`
- ✅ `DTOs/Common/PaginationInfo.cs`

## Status

✅ **COMPLETE** - Pagination implemented for Pengunduran Diri module
- SQL SPs updated with pagination support (preserving original 4-mode and 2-mode structure)
- C# backend code updated
- Default pageSize = 10 for all endpoints
- All original parameters preserved for backward compatibility
- Ready for testing and frontend integration

## Comparison with Drop Out Module

Both modules now have consistent pagination implementation:
- Default pageSize: 10
- Maximum pageSize: 100
- Same response format (PaginatedResponse)
- Backward compatible
- Optional pagination parameters

