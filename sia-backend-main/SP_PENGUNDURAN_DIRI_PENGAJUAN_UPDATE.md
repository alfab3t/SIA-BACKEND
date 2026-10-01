# Update SP Pengajuan Pengunduran Diri - Adopsi Pola Drop Out

## Overview

Update stored procedure `sia_getDataPengunduranDiri` untuk mengadopsi pola dari `sia_getDataPendingDO` yang lebih robust dan flexible.

## Perubahan Utama

### **Before (Mode-based dengan 4 kondisi terpisah):**
```sql
-- Mode 1: Belum Disetujui Wadir 1
IF (@pdi_status = 'Belum Disetujui Wadir 1')
BEGIN
    -- Query terpisah
END
-- Mode 2: Belum Disetujui Prodi
ELSE IF (@pdi_status = 'Belum Disetujui Prodi')
BEGIN
    -- Query terpisah dengan lookup konsentrasi
END
-- Mode 3: Menunggu Upload SK
-- Mode 4: Default
```

**Masalah:**
- ❌ Username hanya support karyawan di Mode 2
- ❌ Tidak support mahasiswa
- ❌ Tidak support multiple status
- ❌ Logic terpisah per mode (sulit maintain)

### **After (Role-based dengan dynamic SQL):**
```sql
-- Ambil struktur organisasi
SELECT @str = str_main_id FROM ess_mskaryawan WHERE kry_username = @username;
SELECT @kryid = kry_id FROM ess_mskaryawan WHERE kry_username = @username;

-- Logic filter berdasarkan role/struktur
IF EXISTS (SELECT 1 FROM sia_msmahasiswa WHERE mhs_id = @username)
    SET @viewBy = ' AND a.mhs_id = ''' + @username + '''';
ELSE IF @str = '2' 
    SET @viewBy = ' AND a.pdi_status IN (''Belum Disetujui Wadir 1'')';
ELSE IF @str = '1' 
    SET @viewBy = ' AND a.pdi_status IN (''Belum Disetujui Direktur'', ''Belum Disetujui Wadir 1'', ''Draft'', ''Revisi'')';
-- ... dst

-- Single dynamic SQL query
SET @SQL = 'SELECT ... FROM ... WHERE 1=1 ' + @viewBy + @statusFilter;
EXEC(@SQL);
```

**Keuntungan:**
- ✅ Support mahasiswa (NIM)
- ✅ Support karyawan (username)
- ✅ Support multiple status (comma-separated)
- ✅ Single query logic (mudah maintain)
- ✅ Flexible role-based access

---

## Detail Perubahan

### 1. **Support Mahasiswa & Karyawan**

**Before:**
```sql
-- Hanya support karyawan di Mode 2
WHERE kry_username = @username
```

**After:**
```sql
-- Check apakah mahasiswa atau karyawan
IF EXISTS (SELECT 1 FROM sia_msmahasiswa WHERE mhs_id = @username)
BEGIN
    -- Mahasiswa: lihat data sendiri
    SET @viewBy = ' AND a.mhs_id = ''' + @username + '''';
END
ELSE
BEGIN
    -- Karyawan: berdasarkan role/struktur
    -- ... logic role-based
END
```

### 2. **Role-Based Access Control**

| str_main_id | Role | Access |
|-------------|------|--------|
| - | Mahasiswa (NIM) | Data sendiri saja |
| 2 | Wadir 1 | Belum Disetujui Wadir 1 |
| 1 | Direktur | Belum Disetujui Direktur, Wadir 1, Draft, Revisi |
| 14, 54, 26, 19 | Admin/Staff | Draft, Belum Disetujui Wadir 1, Direktur, Revisi, Menunggu Upload SK |
| 27, 23, 28 | Staff Upload SK | Disetujui, Menunggu Upload SK |
| Lainnya | Sekprodi | Data konsentrasi sendiri |

### 3. **Multiple Status Support**

**Before:**
```sql
-- Hanya single status via @pdi_status
WHERE a.pdi_status = @pdi_status
```

**After:**
```sql
-- Support comma-separated status via @status
-- Contoh: @status = 'Draft,Revisi,Belum Disetujui Prodi'
IF @status IS NOT NULL AND @status != ''
BEGIN
    DECLARE @statusPattern VARCHAR(MAX) = ',' + @status + ',';
    SET @statusFilter = ' AND CHARINDEX('','' + a.pdi_status + '','', ''' + @statusPattern + ''') > 0';
END
```

### 4. **Sorting Priority**

```sql
ORDER BY 
    CASE WHEN a.pdi_status IN ('Draft', 'Revisi') THEN 0 ELSE 1 END,
    COALESCE(a.pdi_modif_date, a.pdi_created_date) DESC
```

**Priority:**
1. Draft & Revisi di atas (priority 0)
2. Status lain di bawah (priority 1)
3. Sorted by last modified date (newest first)

### 5. **Pagination dengan ROW_NUMBER**

```sql
SELECT * FROM (
    SELECT 
        ROW_NUMBER() OVER (ORDER BY ...) AS rownum,
        ...
        COUNT(*) OVER() AS [Count]
    FROM ...
) res
WHERE rownum BETWEEN @StartRow AND @EndRow
```

**Keuntungan:**
- Efficient pagination
- Total count dalam satu query
- Consistent ordering

---

## Parameter Changes

### **Input Parameters:**

| Parameter | Type | Description | Changes | Used? |
|-----------|------|-------------|---------|-------|
| @username | VARCHAR(50) | Username karyawan ATAU NIM mahasiswa | ✅ Now supports both | ✅ YES |
| @pdi_status | VARCHAR(50) | Single status (legacy) | ⚠️ Deprecated, use @status | ❌ NO |
| @kry_id | VARCHAR(50) | Karyawan ID (legacy parameter) | - No change | ❌ NO |
| @status | VARCHAR(MAX) | Multiple status (comma-separated) | ✅ NEW | ✅ YES |
| @Page | INT | Page number (1-based) | - No change | ✅ YES |
| @PageSize | INT | Records per page | - No change | ✅ YES |

**Note:** 
- `@kry_id` dan `@pdi_status` adalah legacy parameters yang dipertahankan untuk backward compatibility
- Backend tidak perlu diubah karena parameter masih diterima oleh SP (meskipun tidak digunakan)

### **Output Columns:**

| Column | Type | Description | Changes |
|--------|------|-------------|---------|
| rownum | INT | Row number for pagination | - No change |
| pdi_id | VARCHAR | Pengunduran Diri ID | - No change |
| id | VARCHAR | Alternative ID (same as pdi_id) | - No change |
| mhs_id | VARCHAR | Mahasiswa ID | - No change |
| mhs_nama | VARCHAR | Nama mahasiswa | - No change |
| approve_prodi | VARCHAR | Approved by Prodi | - No change |
| approve_dir1 | VARCHAR | Approved by Wadir 1 | - No change |
| tanggal | VARCHAR | Created date | - No change |
| tanggal_disetujui | VARCHAR | Approval date | - No change |
| srt_no | VARCHAR | Nomor surat | - No change |
| status | VARCHAR | Status pengajuan | - No change |
| pdi_created_by | VARCHAR | Created by | - No change |
| prodi_nama | VARCHAR | Nama prodi | ✅ Format: "Teknik Informatika" |
| konsentrasi | VARCHAR | Konsentrasi singkatan | ✅ Format: "SE", "DS" |
| Count | INT | Total records | - No change |

---

## Usage Examples

### **1. Mahasiswa melihat data sendiri**
```sql
EXEC sia_getDataPengunduranDiri
    @username = '2101010001',  -- NIM mahasiswa
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;

-- Result: Hanya data mahasiswa dengan NIM 2101010001
```

### **2. Wadir 1 melihat pending approval**
```sql
EXEC sia_getDataPengunduranDiri
    @username = 'wadir1.username',  -- Username Wadir 1 (str_main_id = 2)
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;

-- Result: Semua data dengan status "Belum Disetujui Wadir 1"
```

### **3. Sekprodi melihat data konsentrasi**
```sql
EXEC sia_getDataPengunduranDiri
    @username = 'sekprodi.username',  -- Username Sekprodi
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;

-- Result: Data mahasiswa di konsentrasi yang dikelola Sekprodi
```

### **4. Admin filter multiple status**
```sql
EXEC sia_getDataPengunduranDiri
    @username = 'admin.username',  -- Username Admin (str_main_id = 14)
    @pdi_status = '',
    @kry_id = '',
    @status = 'Draft,Revisi,Belum Disetujui Prodi',  -- Multiple status
    @Page = 1,
    @PageSize = 10;

-- Result: Data dengan status Draft, Revisi, atau Belum Disetujui Prodi
```

### **5. Staff upload SK**
```sql
EXEC sia_getDataPengunduranDiri
    @username = 'staff.username',  -- Username Staff (str_main_id = 27)
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;

-- Result: Data dengan status Disetujui atau Menunggu Upload SK
```

---

## Testing Checklist

### **Test Cases:**

- [ ] **Test 1:** Mahasiswa login dengan NIM → hanya lihat data sendiri
- [ ] **Test 2:** Wadir 1 login → lihat "Belum Disetujui Wadir 1"
- [ ] **Test 3:** Direktur login → lihat multiple status
- [ ] **Test 4:** Admin login → lihat hampir semua status
- [ ] **Test 5:** Sekprodi login → lihat data konsentrasi sendiri
- [ ] **Test 6:** Staff upload SK → lihat "Disetujui" dan "Menunggu Upload SK"
- [ ] **Test 7:** Filter multiple status → return correct data
- [ ] **Test 8:** Pagination → correct page and total count
- [ ] **Test 9:** Sorting → Draft/Revisi on top, then by date
- [ ] **Test 10:** Empty result → return empty with Count = 0

### **SQL Test Script:**

```sql
-- Test 1: Mahasiswa
EXEC sia_getDataPengunduranDiri 
    @username = '2101010001', 
    @pdi_status = '', 
    @kry_id = '', 
    @status = '', 
    @Page = 1, 
    @PageSize = 10;

-- Test 2: Wadir 1
EXEC sia_getDataPengunduranDiri 
    @username = 'wadir1.username', 
    @pdi_status = '', 
    @kry_id = '', 
    @status = '', 
    @Page = 1, 
    @PageSize = 10;

-- Test 3: Multiple status
EXEC sia_getDataPengunduranDiri 
    @username = 'admin.username', 
    @pdi_status = '', 
    @kry_id = '', 
    @status = 'Draft,Revisi', 
    @Page = 1, 
    @PageSize = 10;

-- Test 4: Pagination
EXEC sia_getDataPengunduranDiri 
    @username = 'admin.username', 
    @pdi_status = '', 
    @kry_id = '', 
    @status = '', 
    @Page = 2, 
    @PageSize = 5;
```

---

## Backend Changes Required

### **No Changes Needed!**

Backend code sudah compatible karena:
- ✅ Parameter names sama
- ✅ Output columns sama
- ✅ Pagination logic sama
- ✅ Multiple status sudah di-handle

**Repository code tetap sama:**
```csharp
public async Task<PaginatedResponse<PengunduranDiriListResponse>> GetAllPaginatedAsync(
    string p1,      // @username
    string status,  // @status (multiple)
    string userId,  // @kry_id
    int page, 
    int pageSize)
{
    cmd.Parameters.AddWithValue("@username", p1 ?? "");
    cmd.Parameters.AddWithValue("@pdi_status", "");  // Legacy, not used
    cmd.Parameters.AddWithValue("@kry_id", userId ?? "");
    cmd.Parameters.AddWithValue("@status", status ?? "");  // Multiple status
    cmd.Parameters.AddWithValue("@Page", page);
    cmd.Parameters.AddWithValue("@PageSize", pageSize);
    
    // ... rest of code
}
```

---

## Migration Steps

### **1. Backup Current SP**
```sql
-- Backup SP definition
SELECT OBJECT_DEFINITION(OBJECT_ID('sia_getDataPengunduranDiri'));
```

### **2. Execute Update Script**
```sql
-- Run: SQL_UPDATE_SP_GET_DATA_PENGUNDURAN_DIRI_NEW.sql
```

### **3. Test All Scenarios**
```sql
-- Run test cases above
```

### **4. Verify Backend**
```bash
# No code changes needed, just restart
dotnet run
```

### **5. Test API Endpoints**
```bash
# Test mahasiswa
GET /api/pengundurandiri?page=1&pageSize=10

# Test with multiple status
GET /api/pengundurandiri?status=Draft,Revisi&page=1&pageSize=10
```

---

## Rollback Plan

Jika ada masalah, rollback dengan SP lama:

```sql
-- Restore from backup
-- Copy paste SP definition dari backup
ALTER PROCEDURE [dbo].[sia_getDataPengunduranDiri]
...
-- (paste SP lama)
```

---

## Benefits Summary

| Aspect | Before | After |
|--------|--------|-------|
| **Mahasiswa Support** | ❌ No | ✅ Yes |
| **Karyawan Support** | ⚠️ Partial | ✅ Full |
| **Multiple Status** | ❌ No | ✅ Yes |
| **Role-Based Access** | ⚠️ Limited | ✅ Comprehensive |
| **Code Maintainability** | ⚠️ 4 separate modes | ✅ Single dynamic SQL |
| **Pagination** | ✅ Yes | ✅ Yes (improved) |
| **Sorting** | ⚠️ Basic | ✅ Priority-based |
| **Backward Compatible** | - | ✅ Yes |

---

## Status

✅ **READY TO EXECUTE**

- SQL script created
- Documentation complete
- Test cases defined
- No backend changes required
- Backward compatible

---

**Created:** April 7, 2026  
**File:** `SQL_UPDATE_SP_GET_DATA_PENGUNDURAN_DIRI_NEW.sql`  
**Status:** Ready for deployment
