# Perbandingan Format Nama Prodi di Stored Procedure

## Format yang Digunakan

### ❌ Format Lama (Singkatan)
```sql
pro_singkatan + ' (' + kon_singkatan + ')' AS kon_singkatan
```

**Output**: `TI (SE)`, `TM (MFG)`

**Masalah**: 
- Terlalu singkat, tidak jelas
- User harus hafal singkatan

---

### ✅ Format Baru (Nama Lengkap)
```sql
pro_nama + ' (' + kon_singkatan + ')' AS prodi_nama
```

**Output**: `Teknik Informatika (SE)`, `Teknik Mesin (MFG)`

**Keuntungan**:
- Lebih jelas dan mudah dibaca
- User langsung tahu nama prodi lengkap
- Konsentrasi tetap ada (dalam kurung)

---

## Contoh Data

### Tabel sia_msprodi
| pro_id | pro_nama | pro_singkatan |
|--------|----------|---------------|
| PRO001 | Teknik Informatika | TI |
| PRO002 | Teknik Mesin | TM |
| PRO003 | Teknik Elektro | TE |

### Tabel sia_mskonsentrasi
| kon_id | kon_nama | kon_singkatan | pro_id |
|--------|----------|---------------|--------|
| KON001 | Software Engineering | SE | PRO001 |
| KON002 | Data Science | DS | PRO001 |
| KON003 | Manufacturing | MFG | PRO002 |
| KON004 | Automation | AUTO | PRO002 |

---

## Output Comparison

### Format Lama (Singkatan)
```json
{
  "pdi_id": "001/PMA/PD/I/2026",
  "mhs_id": "123456",
  "mhs_nama": "John Doe",
  "kon_singkatan": "TI (SE)",  // ❌ Kurang jelas
  "status": "Belum Disetujui Prodi"
}
```

### Format Baru (Nama Lengkap)
```json
{
  "pdi_id": "001/PMA/PD/I/2026",
  "mhs_id": "123456",
  "mhs_nama": "John Doe",
  "prodi_nama": "Teknik Informatika (SE)",  // ✅ Lebih jelas!
  "status": "Belum Disetujui Prodi"
}
```

---

## Stored Procedure yang Sudah Diupdate

### 1. sia_getDataPengunduranDiri ✅
**File**: `SQL_UPDATE_SP_PENGUNDURAN_DIRI.sql`

**Perubahan**:
- Tambah JOIN ke `sia_mskonsentrasi` dan `sia_msprodi`
- Tambah kolom `mhs_nama`
- Tambah kolom `prodi_nama` dengan format: `pro_nama + ' (' + kon_singkatan + ')'`

**Mode**:
- Mode 1: Mahasiswa (status = '')
- Mode 2: Admin/Staff (status != '')

---

## Stored Procedure Lain yang Perlu Diupdate

### 2. sia_getDataRiwayatPengunduranDiri
**Status**: Perlu diupdate juga

**Current**:
```sql
pro_singkatan + ' (' + kon_singkatan + ')' AS kon_singkatan
```

**Should be**:
```sql
pro_nama + ' (' + kon_singkatan + ')' AS prodi_nama
```

---

## Cara Update

### 1. Jalankan SQL Script
```sql
-- Jalankan file ini di SQL Server Management Studio
SQL_UPDATE_SP_PENGUNDURAN_DIRI.sql
```

### 2. Test SP
```sql
-- Test Mode 1: Mahasiswa
EXEC sia_getDataPengunduranDiri 
  @user_id = '123456', 
  @pdi_status = '', 
  @kry_id = ''

-- Test Mode 2: Admin
EXEC sia_getDataPengunduranDiri 
  @user_id = 'admin', 
  @pdi_status = 'Belum Disetujui Prodi', 
  @kry_id = ''
```

### 3. Verify Output
Pastikan kolom `prodi_nama` muncul dengan format:
```
Teknik Informatika (SE)
Teknik Mesin (MFG)
Teknik Elektro (AUTO)
```

---

## Impact ke Frontend

### Before
```javascript
// Frontend harus decode singkatan
const prodiText = "TI (SE)";  // User bingung: TI itu apa?
```

### After
```javascript
// Frontend langsung dapat nama lengkap
const prodiText = "Teknik Informatika (SE)";  // ✅ Jelas!
```

**Tidak perlu ubah code Frontend**, karena:
- Kolom baru: `prodi_nama` (bukan `kon_singkatan`)
- Frontend tinggal ganti dari `kon_singkatan` ke `prodi_nama`
- Format lebih user-friendly

---

## Rekomendasi

### Konsistensi Format
Gunakan format yang sama di semua SP:
```sql
-- ✅ RECOMMENDED
pro_nama + ' (' + kon_singkatan + ')' AS prodi_nama

-- ❌ AVOID
pro_singkatan + ' (' + kon_singkatan + ')' AS kon_singkatan
```

### Nama Kolom
- Gunakan `prodi_nama` (bukan `kon_singkatan`)
- Lebih jelas dan konsisten
- Mudah dipahami developer

---

**Created**: February 2, 2026
**Status**: ✅ Ready to Execute
