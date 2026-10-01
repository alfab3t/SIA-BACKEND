# Update Stored Procedure - sia_getDataRiwayatDO

## Perubahan

### Before (Singkatan Prodi):
```sql
pro_singkatan + ' (' + kon_singkatan + ')' AS kon_nama
```

**Output:**
```
TPM (TPM)
MI (MIN)
MO (MOT)
```

### After (Nama Lengkap Prodi):
```sql
d.pro_nama + ' (' + c.kon_singkatan + ')' AS kon_nama
```

**Output:**
```
Teknik Produksi dan Proses Manufaktur (TPM)
Manajemen Informatika (MIN)
Mesin Otomotif (MOT)
```

---

## Perubahan Detail

### 1. **Kolom kon_nama**
- **Before**: `pro_singkatan + ' (' + kon_singkatan + ')'`
- **After**: `pro_nama + ' (' + kon_singkatan + ')'`

### 2. **Search Keyword**
Ditambahkan pencarian berdasarkan nama lengkap prodi:
```sql
OR d.pro_nama LIKE '%' + @keyword + '%'
```

Sekarang user bisa search dengan:
- Nama mahasiswa
- NIM
- Singkatan konsentrasi
- Nama konsentrasi
- **Nama lengkap prodi** (NEW!)
- Nomor surat

### 3. **JOIN Statement**
Diubah dari old-style JOIN ke INNER JOIN untuk clarity:
```sql
-- Before
FROM sia_msdropout a, sia_msmahasiswa b, sia_mskonsentrasi c, sia_msprodi d
WHERE d.pro_id = c.pro_id AND a.mhs_id = b.mhs_id AND b.kon_id = c.kon_id

-- After
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
INNER JOIN sia_msprodi d ON d.pro_id = c.pro_id
```

---

## Cara Update

### 1. **Backup SP Lama** (Opsional tapi disarankan)
```sql
-- Backup SP lama
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.sia_getDataRiwayatDO'))
```

### 2. **Execute Script Update**
```sql
-- Run file: SQL_UPDATE_SP_RIWAYAT_DO.sql
-- Atau copy-paste script ke SSMS dan execute
```

### 3. **Test SP**
```sql
-- Test dengan data dummy
EXEC sia_getDataRiwayatDO 
    @username = 'admin',
    @keyword = '',
    @sort_by = 'a.dro_created_date desc',
    @kon_id = '',
    @role_id = 'ROLE_ADMIN',
    @display_name = 'Administrator'

-- Cek kolom kon_nama, seharusnya menampilkan nama lengkap prodi
```

---

## Impact ke Frontend

### Response API Berubah:

**Before:**
```json
{
  "droId": "001/PMA/DO/I/2026",
  "prodi": "TPM (TPM)"  // Singkatan
}
```

**After:**
```json
{
  "droId": "001/PMA/DO/I/2026",
  "prodi": "Teknik Produksi dan Proses Manufaktur (TPM)"  // Nama lengkap
}
```

### Frontend Adjustment:

Jika frontend menggunakan kolom `prodi` untuk display, **tidak perlu perubahan code**.
Hanya tampilan yang berubah dari singkatan ke nama lengkap.

Jika frontend ada logic yang parse singkatan prodi (misal: extract "TPM" dari "TPM (TPM)"), 
perlu disesuaikan untuk extract dari format baru: "Teknik Produksi dan Proses Manufaktur (TPM)"

**Extract singkatan dari format baru:**
```javascript
// JavaScript
const prodi = "Teknik Produksi dan Proses Manufaktur (TPM)";
const singkatan = prodi.match(/\(([^)]+)\)/)[1];  // Result: "TPM"

// C#
var prodi = "Teknik Produksi dan Proses Manufaktur (TPM)";
var singkatan = prodi.Split('(')[1].TrimEnd(')');  // Result: "TPM"
```

---

## Rollback (Jika Diperlukan)

Jika ingin kembali ke format lama (singkatan):

```sql
ALTER PROCEDURE [dbo].[sia_getDataRiwayatDO]
    -- ... parameters sama ...
AS
BEGIN
    -- ... logic sama ...
    
    -- Kembalikan ke pro_singkatan
    SET @SQL = '
    SELECT 
        a.dro_id,
        b.mhs_id,
        b.mhs_id + '' - '' + b.mhs_nama AS mhs_nama,
        d.pro_singkatan + '' ('' + c.kon_singkatan + '')'' AS kon_nama,  -- ROLLBACK
        -- ... rest sama ...
```

---

## Testing Checklist

- [ ] Execute script update SP
- [ ] Test SP di SSMS dengan berbagai parameter
- [ ] Verify kolom kon_nama menampilkan nama lengkap prodi
- [ ] Test API endpoint `/api/dropout/riwayat`
- [ ] Verify response JSON menampilkan nama lengkap
- [ ] Test search dengan nama lengkap prodi
- [ ] Test di frontend, pastikan tampilan sesuai
- [ ] Verify tidak ada error di console

---

**Update Date**: January 28, 2026
**Status**: Ready to execute
**Impact**: Low (hanya perubahan display, tidak ada breaking change)
