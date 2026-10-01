# Multiple Status Filter - Implementation Guide

## Overview

Fitur ini memungkinkan parameter `@status` untuk menerima multiple values (comma-separated), sehingga bisa filter berdasarkan beberapa status sekaligus dalam satu request.

## Features

✅ **Single Status** - Backward compatible dengan filter status tunggal
✅ **Multiple Status** - Filter beberapa status sekaligus (comma-separated)
✅ **Empty Status** - Return semua data (tidak ada filter status)
✅ **Pagination Support** - Bekerja dengan pagination yang sudah ada

## SQL Implementation

### Helper Function: `fn_SplitString`

Function untuk split string menjadi table (untuk SQL Server versi lama yang tidak punya `STRING_SPLIT`):

```sql
CREATE FUNCTION [dbo].[fn_SplitString]
(
    @String NVARCHAR(MAX),
    @Delimiter CHAR(1)
)
RETURNS @Result TABLE (Value NVARCHAR(255))
```

### Updated Stored Procedures

1. **sia_getDataRiwayatDO** - Added `@status VARCHAR(MAX)` parameter
2. **sia_getDataPendingDO** - Added `@status VARCHAR(MAX)` parameter

## API Usage

### Drop Out Module

#### Endpoint: GET /api/DropOut/riwayat

**Single Status:**
```bash
GET /api/DropOut/riwayat?username=admin&status=Draft
```

**Multiple Status (comma-separated):**
```bash
GET /api/DropOut/riwayat?username=admin&status=Draft,Belum Disetujui Wadir 1,Revisi
```

**All Status (empty):**
```bash
GET /api/DropOut/riwayat?username=admin&status=
# or
GET /api/DropOut/riwayat?username=admin
```

**With Pagination:**
```bash
GET /api/DropOut/riwayat?username=admin&status=Draft,Revisi&page=1&pageSize=10
```

#### Endpoint: GET /api/DropOut/pending

**Single Status:**
```bash
GET /api/DropOut/pending?username=admin&status=Belum Disetujui Wadir 1
```

**Multiple Status:**
```bash
GET /api/DropOut/pending?username=admin&status=Draft,Belum Disetujui Wadir 1,Belum Disetujui Direktur
```

## Response Format

Response tetap sama dengan format pagination yang sudah ada:

```json
{
  "data": [
    {
      "droId": "DO001",
      "mhsId": "123456",
      "namaMahasiswa": "John Doe",
      "status": "Draft",
      ...
    },
    {
      "droId": "DO002",
      "mhsId": "123457",
      "namaMahasiswa": "Jane Smith",
      "status": "Revisi",
      ...
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalRecords": 25,
    "totalPages": 3
  }
}
```

## SQL Examples

### Test Single Status
```sql
DECLARE @total INT;
EXEC sia_getDataRiwayatDO 
    @username = 'admin',
    @keyword = '',
    @sort_by = 'a.dro_created_date desc',
    @kon_id = '',
    @role_id = 'admin',
    @display_name = '',
    @status = 'Draft',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
    
SELECT @total AS TotalRecords;
```

### Test Multiple Status
```sql
DECLARE @total INT;
EXEC sia_getDataRiwayatDO 
    @username = 'admin',
    @keyword = '',
    @sort_by = 'a.dro_created_date desc',
    @kon_id = '',
    @role_id = 'admin',
    @display_name = '',
    @status = 'Draft,Belum Disetujui Wadir 1,Revisi',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
    
SELECT @total AS TotalRecords;
```

### Test Empty Status (All Data)
```sql
DECLARE @total INT;
EXEC sia_getDataRiwayatDO 
    @username = 'admin',
    @keyword = '',
    @sort_by = 'a.dro_created_date desc',
    @kon_id = '',
    @role_id = 'admin',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
    
SELECT @total AS TotalRecords;
```

## Implementation Details

### SQL Logic

```sql
-- Build status filter (support multiple status)
IF @status IS NOT NULL AND @status != ''
BEGIN
    -- Jika ada comma, berarti multiple status
    IF CHARINDEX(',', @status) > 0
    BEGIN
        -- Build IN clause untuk multiple status
        -- Split by comma dan build: dro_status IN ('Draft', 'Revisi', ...)
        SET @statusFilter = ' AND dro_status IN (' + @statusList + ')';
    END
    ELSE
    BEGIN
        -- Single status
        SET @statusFilter = ' AND dro_status = ''' + @status + '''';
    END
END
```

### C# Controller

```csharp
[HttpGet("riwayat")]
public async Task<IActionResult> GetRiwayat(
   [FromQuery] string username,
   [FromQuery] string status = "",  // Support comma-separated values
   ...)
{
    var result = await _repo.GetRiwayatPaginatedAsync(
        username, keyword, sortBy, konsentrasi, role, displayName, status, pageVal, pageSizeVal);
    
    return Ok(result);
}
```

### C# Repository

```csharp
public async Task<PaginatedResponse<DropOutRiwayatResponse>> GetRiwayatPaginatedAsync(
    string username,
    string keyword,
    string sortBy,
    string konsentrasi,
    string role,
    string displayName,
    string status,  // NEW: Multiple status support
    int page,
    int pageSize)
{
    cmd.Parameters.AddWithValue("@status", status ?? "");
    // ...
}
```

## Common Status Values

### Drop Out Module
- `Draft`
- `Belum Disetujui Wadir 1`
- `Belum Disetujui Direktur`
- `Revisi`
- `Menunggu Upload SK`
- `Disetujui`

### Example Combinations

**Draft & Revisi:**
```
status=Draft,Revisi
```

**Pending Approval (Wadir & Direktur):**
```
status=Belum Disetujui Wadir 1,Belum Disetujui Direktur
```

**All Approved:**
```
status=Menunggu Upload SK,Disetujui
```

## Frontend Integration

### JavaScript/TypeScript Example

```typescript
// Single status
const response = await fetch('/api/DropOut/riwayat?username=admin&status=Draft');

// Multiple status
const statuses = ['Draft', 'Revisi', 'Belum Disetujui Wadir 1'];
const statusParam = statuses.join(',');
const response = await fetch(`/api/DropOut/riwayat?username=admin&status=${statusParam}`);

// With pagination
const response = await fetch(
  `/api/DropOut/riwayat?username=admin&status=${statusParam}&page=1&pageSize=10`
);
```

### React Example

```tsx
const [selectedStatuses, setSelectedStatuses] = useState<string[]>(['Draft', 'Revisi']);

const fetchData = async () => {
  const statusParam = selectedStatuses.join(',');
  const response = await fetch(
    `/api/DropOut/riwayat?username=${username}&status=${statusParam}&page=${page}&pageSize=10`
  );
  const data = await response.json();
  // Handle data.data and data.pagination
};
```

## Testing Checklist

- [ ] Test single status filter
- [ ] Test multiple status filter (2 statuses)
- [ ] Test multiple status filter (3+ statuses)
- [ ] Test empty status (all data)
- [ ] Test with pagination
- [ ] Test with keyword search + status filter
- [ ] Test with konsentrasi filter + status filter
- [ ] Verify total count is correct
- [ ] Verify pagination info is correct

## Files Modified

### SQL:
- ✅ `SQL_ADD_MULTIPLE_STATUS_FILTER.sql` (NEW)

### C#:
- ✅ `Repositories/Interfaces/IDropOutRepository.cs` - Added status parameter
- ✅ `Repositories/Implementations/DropOutRepository.cs` - Implemented status parameter
- ✅ `Controllers/DropOutController.cs` - Added status query parameter

## Benefits

1. **Flexible Filtering** - Frontend bisa filter multiple status dalam satu request
2. **Better Performance** - Satu request vs multiple requests
3. **Cleaner Code** - Tidak perlu logic kompleks di frontend untuk merge data
4. **Backward Compatible** - Single status masih bekerja seperti biasa
5. **User Experience** - User bisa select multiple status di UI

## Notes

- Status values are case-sensitive
- Comma (`,`) is used as delimiter
- No spaces around comma recommended: `Draft,Revisi` (not `Draft, Revisi`)
- Empty status returns all data (no filter applied)
- Works seamlessly with existing pagination and other filters

## Status

✅ **COMPLETE** - Multiple status filter implemented for Drop Out module
- SQL function created
- SPs updated
- C# code updated
- Ready for testing
