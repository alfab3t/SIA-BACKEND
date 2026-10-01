# Quick Guide: SP Pengunduran Diri Update

## TL;DR

SP `sia_getDataPengunduranDiri` sekarang mengadopsi pola dari `sia_getDataPendingDO`:
- ✅ Support mahasiswa (NIM) dan karyawan (username)
- ✅ Support multiple status (comma-separated)
- ✅ Role-based access control
- ✅ Backward compatible (no backend changes needed)

---

## Quick Comparison

| Feature | Old SP | New SP |
|---------|--------|--------|
| Mahasiswa support | ❌ | ✅ |
| Multiple status | ❌ | ✅ |
| Role-based | ⚠️ Limited | ✅ Full |
| Code structure | 4 separate modes | Single dynamic SQL |

---

## How It Works

### **Username Detection:**

```sql
-- Check if username is mahasiswa or karyawan
IF EXISTS (SELECT 1 FROM sia_msmahasiswa WHERE mhs_id = @username)
    -- Mahasiswa: show own data only
ELSE
    -- Karyawan: role-based access
```

### **Role-Based Access:**

| User Type | str_main_id | What They See |
|-----------|-------------|---------------|
| Mahasiswa | - | Own data only |
| Wadir 1 | 2 | Belum Disetujui Wadir 1 |
| Direktur | 1 | Multiple status |
| Admin | 14, 54, 26, 19 | Most status |
| Staff SK | 27, 23, 28 | Disetujui, Menunggu Upload SK |
| Sekprodi | Others | Konsentrasi data |

---

## Usage Examples

### **Mahasiswa:**
```sql
EXEC sia_getDataPengunduranDiri
    @username = '2101010001',  -- NIM
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;
```

### **Karyawan:**
```sql
EXEC sia_getDataPengunduranDiri
    @username = 'john.doe',  -- Username karyawan
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;
```

### **Multiple Status:**
```sql
EXEC sia_getDataPengunduranDiri
    @username = 'admin',
    @pdi_status = '',
    @kry_id = '',
    @status = 'Draft,Revisi,Belum Disetujui Prodi',  -- Comma-separated
    @Page = 1,
    @PageSize = 10;
```

---

## Deployment Steps

### **1. Execute SQL:**
```sql
-- Run: SQL_UPDATE_SP_GET_DATA_PENGUNDURAN_DIRI_NEW.sql
```

### **2. Test:**
```sql
-- Run: SQL_TEST_PENGUNDURAN_DIRI_PENGAJUAN.sql
```

### **3. Restart Backend:**
```bash
dotnet run
```

### **4. Test API:**
```bash
GET /api/pengundurandiri?page=1&pageSize=10
GET /api/pengundurandiri?status=Draft,Revisi&page=1&pageSize=10
```

---

## Backend Changes

**NONE!** Backend code tetap sama karena:
- Parameter names sama
- Output columns sama
- Pagination logic sama

---

## Rollback

Jika ada masalah:
```sql
-- Restore SP lama dari backup
ALTER PROCEDURE [dbo].[sia_getDataPengunduranDiri]
...
-- (paste SP lama)
```

---

## Files

| File | Purpose |
|------|---------|
| `SQL_UPDATE_SP_GET_DATA_PENGUNDURAN_DIRI_NEW.sql` | Update script |
| `SQL_TEST_PENGUNDURAN_DIRI_PENGAJUAN.sql` | Test script |
| `SP_PENGUNDURAN_DIRI_PENGAJUAN_UPDATE.md` | Full documentation |
| `PENGUNDURAN_DIRI_SP_QUICK_GUIDE.md` | This file |

---

## Status

✅ **READY TO DEPLOY**

---

**Created:** April 7, 2026
