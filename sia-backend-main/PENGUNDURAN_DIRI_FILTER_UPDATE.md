# Update Pengunduran Diri - Tambah Filter Konsentrasi & Keyword

## Status: ✅ COMPLETE

SP dan backend sudah diupdate untuk support filter konsentrasi/prodi dan keyword search, sama seperti Drop Out.

---

## 1. Stored Procedure Update

### File: `SQL_UPDATE_SP_GET_DATA_PENGUNDURAN_DIRI_NEW.sql`

**Parameter Baru:**
```sql
@username VARCHAR(50),
@keyword VARCHAR(MAX) = '',         -- NEW: Search keyword
@sort_by VARCHAR(100) = '',         -- NEW: Custom sorting
@kon_id VARCHAR(50) = '',           -- NEW: Filter konsentrasi/prodi
@pdi_status VARCHAR(50) = '',       -- Legacy (not used)
@kry_id VARCHAR(50) = '',           -- Legacy (not used)
@status VARCHAR(MAX) = '',          -- Multiple status
@Page INT = NULL,
@PageSize INT = NULL
```

**Fitur Baru:**
- ✅ Filter konsentrasi/prodi via `@kon_id` (support `pro_id` atau `pro_nama`)
- ✅ Search keyword via `@keyword` (search di: pdi_id, mhs_nama, kon_nama, srt_no)
- ✅ Custom sorting via `@sort_by`
- ✅ Format output sama dengan Drop Out: `pro_singkatan (kon_singkatan)`

**Contoh Filter Konsentrasi:**
```sql
-- Filter by pro_id
WHERE d.pro_id = 'PRO001'

-- Filter by pro_nama
WHERE d.pro_nama = 'Teknik Informatika'

-- Jika kosong, tampilkan semua
WHERE @kon_id = ''
```

**Contoh Keyword Search:**
```sql
AND (a.pdi_id LIKE '%keyword%' 
OR b.mhs_nama LIKE '%keyword%' 
OR c.kon_nama LIKE '%keyword%' 
OR a.srt_no LIKE '%keyword%')
```

---

## 2. Backend Update

### Controller: `PengunduranDiriController.cs`

**Endpoint GET /api/pengundurandiri:**
```csharp
[HttpGet]
public async Task<IActionResult> GetAll(
    [FromQuery] string p1 = "",
    [FromQuery] string keyword = "",      // NEW
    [FromQuery] string sortBy = "",       // NEW
    [FromQuery] string konId = "",        // NEW
    [FromQuery] string status = "",
    [FromQuery] int? page = 1,
    [FromQuery] int? pageSize = 10)
{
    var userId = User.FindFirst("namaakun")?.Value ?? "";
    var result = await _repo.GetAllPaginatedAsync(
        p1, keyword, sortBy, konId, status, userId, pageVal, pageSizeVal);
    return Ok(result);
}
```

### Repository: `PengunduranDiriRepository.cs`

**Method Signature:**
```csharp
public async Task<PaginatedResponse<PengunduranDiriListResponse>> GetAllPaginatedAsync(
    string p1,          // username
    string keyword,     // NEW
    string sortBy,      // NEW
    string konId,       // NEW
    string status,
    string userId,
    int page,
    int pageSize)
{
    cmd.Parameters.AddWithValue("@username", p1 ?? "");
    cmd.Parameters.AddWithValue("@keyword", keyword ?? "");
    cmd.Parameters.AddWithValue("@sort_by", sortBy ?? "");
    cmd.Parameters.AddWithValue("@kon_id", konId ?? "");
    cmd.Parameters.AddWithValue("@status", status ?? "");
    // ...
}
```

### Interface: `IPengunduranDiriRepository.cs`

```csharp
Task<IEnumerable<PengunduranDiriListResponse>> GetAllAsync(
    string p1, string keyword, string sortBy, string konId, string status, string userId);

Task<PaginatedResponse<PengunduranDiriListResponse>> GetAllPaginatedAsync(
    string p1, string keyword, string sortBy, string konId, string status, string userId, 
    int page, int pageSize);
```

---

## 3. Response Format Update

**Output Column Changes:**
```csharp
// OLD format
ProdiNama = "Teknik Informatika"
Konsentrasi = "SE"

// NEW format (sama dengan Drop Out)
kon_nama = "TI (SE)"  // pro_singkatan (kon_singkatan)
```

**Response Fields:**
- `pdi_id` - ID Pengunduran Diri
- `mhs_id` - NIM Mahasiswa
- `mhs_nama` - Format: "NIM - Nama Mahasiswa"
- `kon_nama` - Format: "PRO_SINGKATAN (KON_SINGKATAN)"
- `pdi_created_date` - Tanggal pengajuan
- `pdi_created_by` - Dibuat oleh
- `srt_no` - Nomor surat
- `pdi_status` - Status pengajuan
- `Count` - Total records (untuk pagination)

---

## 4. API Usage Examples

### Test 1: Filter by Konsentrasi (pro_id)
```bash
GET /api/pengundurandiri?p1=admin&konId=PRO001&page=1&pageSize=10
```

### Test 2: Filter by Konsentrasi (pro_nama)
```bash
GET /api/pengundurandiri?p1=admin&konId=Teknik%20Informatika&page=1&pageSize=10
```

### Test 3: Search by Keyword
```bash
GET /api/pengundurandiri?p1=admin&keyword=budi&page=1&pageSize=10
```

### Test 4: Kombinasi Filter + Search + Status
```bash
GET /api/pengundurandiri?p1=admin&keyword=budi&konId=Teknik%20Informatika&status=Draft,Revisi&page=1&pageSize=10
```

### Test 5: Mahasiswa (NIM)
```bash
GET /api/pengundurandiri?p1=2101010001&page=1&pageSize=10
```

---

## 5. SQL Test Script

**File: `SQL_TEST_PENGUNDURAN_DIRI_WITH_FILTER.sql`**

10 test scenarios:
1. Mahasiswa (NIM) - lihat data sendiri
2. Wadir 1 - lihat yang perlu approval
3. Admin - dengan keyword search
4. Admin - dengan filter konsentrasi (pro_id)
5. Admin - dengan filter konsentrasi (pro_nama)
6. Admin - dengan multiple status
7. Admin - kombinasi keyword + konsentrasi + status
8. Sekprodi - lihat data konsentrasi sendiri
9. Staff SK - lihat yang perlu upload SK
10. Pagination - page 2

---

## 6. Compilation Status

✅ **No compilation errors**
- `PengunduranDiriController.cs` - Clean
- `PengunduranDiriRepository.cs` - Clean
- `IPengunduranDiriRepository.cs` - Clean

---

## 7. Comparison with Drop Out

| Feature | Drop Out | Pengunduran Diri | Status |
|---------|----------|------------------|--------|
| Filter Konsentrasi | ✅ `@kon_id` | ✅ `@kon_id` | ✅ Same |
| Keyword Search | ✅ `@keyword` | ✅ `@keyword` | ✅ Same |
| Custom Sorting | ✅ `@sort_by` | ✅ `@sort_by` | ✅ Same |
| Multiple Status | ✅ `@status` | ✅ `@status` | ✅ Same |
| Pagination | ✅ ROW_NUMBER | ✅ ROW_NUMBER | ✅ Same |
| Output Format | `pro_singkatan (kon_singkatan)` | `pro_singkatan (kon_singkatan)` | ✅ Same |
| Role-based Access | ✅ str_main_id | ✅ str_main_id | ✅ Same |

---

## 8. Next Steps

### Database Execution
1. Execute SQL script:
   ```sql
   -- File: SQL_UPDATE_SP_GET_DATA_PENGUNDURAN_DIRI_NEW.sql
   ```

2. Test dengan script:
   ```sql
   -- File: SQL_TEST_PENGUNDURAN_DIRI_WITH_FILTER.sql
   ```

### API Testing
Test endpoint dengan berbagai kombinasi filter:

```bash
# Test filter konsentrasi
GET /api/pengundurandiri?konId=PRO001

# Test keyword search
GET /api/pengundurandiri?keyword=budi

# Test kombinasi
GET /api/pengundurandiri?konId=Teknik%20Informatika&keyword=budi&status=Draft,Revisi
```

---

## Summary

SP dan backend Pengunduran Diri sudah disamakan dengan Drop Out:
- ✅ Support filter konsentrasi/prodi (`@kon_id`)
- ✅ Support keyword search (`@keyword`)
- ✅ Support custom sorting (`@sort_by`)
- ✅ Output format sama: `pro_singkatan (kon_singkatan)`
- ✅ Backward compatibility terjaga (legacy parameters)
- ✅ No compilation errors

Tinggal execute SQL script di database dan test API endpoints.
