# Drop Out Status "Revisi" - Fix Summary

## 🔍 Issue Found

**Problem**: Data dengan status "Revisi" tidak muncul di API meskipun ada di database

**Evidence**:
```
Database: 3 data dengan status "Revisi"
- 16/PMA/DO/I/2026 (mhs: 0720250090, by: nda_prodi, date: 2026-01-22)
- 7/PMA/DO/I/2026 (mhs: 0720240012, by: system, date: 2026-01-13)
- 1/PMA/DO/XII/2025 (mhs: 0320240062, by: system, date: 2025-12-21)

API Response: 0 data dengan status "Revisi" (tidak muncul)
```

---

## 🐛 Root Cause

### Bug di Stored Procedure `sia_getDataRiwayatDO`

**Kondisi untuk user dengan `str_id = '26'` (Prodi)**:

```sql
-- ❌ SEBELUM (SALAH)
ELSE IF @str = '14' OR @str = '54' OR @str = '26' OR @str = '19' 
BEGIN
    SET @viewBy = ' AND (dro_status = ''Menunggu Upload SK'' 
                      OR dro_status = ''Disetujui'' 
                      OR dro_status = ''Draft'' 
                      OR dro_status = ''Belum Disetujui Wadir 1'' 
                      OR dro_status <> ''Revisi'')';  -- ❌ BUG DI SINI!
END
```

**Masalah**: 
- Kondisi `OR dro_status <> 'Revisi'` artinya "ATAU status BUKAN Revisi"
- Ini membuat SEMUA status (termasuk yang tidak disebutkan) ikut masuk
- Kondisi sebelumnya jadi tidak berguna

---

## ✅ Solution

### Fixed Stored Procedure

```sql
-- ✅ SESUDAH (BENAR)
ELSE IF @str = '14' OR @str = '54' OR @str = '26' OR @str = '19' 
BEGIN
    -- Prodi/Sekprodi bisa lihat semua status termasuk Revisi
    SET @viewBy = ' AND dro_status IN (''Menunggu Upload SK'', 
                                       ''Disetujui'', 
                                       ''Draft'', 
                                       ''Belum Disetujui Wadir 1'', 
                                       ''Revisi'')';  -- ✅ ADDED!
END
```

**Perubahan**:
1. ✅ Hapus kondisi `OR dro_status <> 'Revisi'` yang salah
2. ✅ Tambahkan `'Revisi'` ke dalam list status yang boleh dilihat
3. ✅ Gunakan `IN` clause untuk lebih jelas

---

## 📋 Implementation Steps

### 1. Backup Database (WAJIB!)

```sql
-- Backup stored procedure sebelum diubah
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.sia_getDataRiwayatDO'))
```

### 2. Run Fix Script

Jalankan file: `SQL_FIX_SP_RIWAYAT_DO_REVISI.sql`

```sql
-- File ini akan:
-- 1. ALTER stored procedure sia_getDataRiwayatDO
-- 2. Fix kondisi untuk str_id 26
-- 3. Test SP dengan sample data
```

### 3. Verify Fix

```sql
-- Test SP setelah fix
EXEC sia_getDataRiwayatDO 
    @username = 'nda_prodi',  -- User dengan str_id = 26
    @keyword = '',
    @sort_by = 'a.dro_created_date desc',
    @kon_id = '',
    @role_id = '',
    @display_name = '';

-- Expected: Harus muncul 3 data dengan status "Revisi"
```

### 4. Test dari API

```bash
# Test dari backend API
GET /api/DropOut/riwayat?username=nda_prodi

# Expected response harus include:
{
  "droId": "16/PMA/DO/I/2026",
  "status": "Revisi",
  ...
}
```

---

## 🎯 Expected Results

### Before Fix:
```json
// API Response - TIDAK ADA status "Revisi"
[
  { "status": "Draft" },
  { "status": "Belum Disetujui Wadir 1" },
  { "status": "Menunggu Upload SK" },
  { "status": "Disetujui" }
]
// Total: 110 data (3 data "Revisi" tidak muncul)
```

### After Fix:
```json
// API Response - ADA status "Revisi"
[
  { "droId": "16/PMA/DO/I/2026", "status": "Revisi" },  // ✅ NEW!
  { "droId": "7/PMA/DO/I/2026", "status": "Revisi" },   // ✅ NEW!
  { "droId": "1/PMA/DO/XII/2025", "status": "Revisi" }, // ✅ NEW!
  { "status": "Draft" },
  { "status": "Belum Disetujui Wadir 1" },
  { "status": "Menunggu Upload SK" },
  { "status": "Disetujui" }
]
// Total: 113 data (semua data muncul)
```

---

## 📊 Impact Analysis

### Who Can See "Revisi" Status?

| User Type | str_id | Before Fix | After Fix |
|-----------|--------|------------|-----------|
| Mahasiswa | (mhs_id) | ✅ Yes | ✅ Yes |
| Direktur | 1, 2 | ❌ No | ❌ No (by design) |
| **Prodi/Sekprodi** | **14, 54, 26, 19** | **❌ No** | **✅ Yes** |
| Role tertentu | 27, 23, 28 | ❌ No | ❌ No (by design) |
| Mahasiswa (role) | ROL23 | ✅ Yes | ✅ Yes |

**Note**: Direktur dan role tertentu memang tidak boleh lihat "Revisi" by design.

---

## 🧪 Testing Checklist

- [ ] Backup database sebelum update
- [ ] Run SQL fix script
- [ ] Verify SP definition updated
- [ ] Test SP directly in SQL
- [ ] Restart backend API
- [ ] Test API endpoint `/api/DropOut/riwayat`
- [ ] Verify 3 data "Revisi" muncul
- [ ] Test dengan user lain (str_id berbeda)
- [ ] Verify filter masih bekerja dengan benar

---

## 📁 Related Files

1. `SQL_CHECK_STATUS_REVISI.sql` - Query untuk cek status Revisi di database
2. `SQL_FIX_SP_RIWAYAT_DO_REVISI.sql` - Script untuk fix stored procedure
3. `DROPOUT_REVISI_FIX_SUMMARY.md` - Dokumentasi lengkap (file ini)

---

## 🚀 Deployment

### Development
1. Run fix script di dev database
2. Test thoroughly
3. Verify no regression

### Production
1. Schedule maintenance window
2. Backup production database
3. Run fix script
4. Verify immediately
5. Monitor API logs

---

## ⚠️ Rollback Plan

Jika ada masalah setelah fix:

```sql
-- Rollback ke versi sebelumnya
ALTER PROCEDURE [dbo].[sia_getDataRiwayatDO]
    -- ... (paste backup SP definition)
```

---

## 📞 Support

Jika ada masalah:
1. Cek backend logs untuk error
2. Verify SP definition dengan query backup
3. Test SP directly di SQL Server
4. Cek user str_id dengan query di `SQL_CHECK_USER_STR_MAIN_ID.sql`

---

## ✅ Status

- [x] Issue identified
- [x] Root cause found
- [x] Fix script created
- [ ] Fix deployed to dev
- [ ] Fix tested
- [ ] Fix deployed to production

**Last Updated**: 2026-02-06
