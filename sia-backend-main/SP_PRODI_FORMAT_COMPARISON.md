# Perbandingan Format Nama Prodi di Stored Procedure

## Current State (Sekarang)

### 1. SP: `sia_getListProdi`
**Digunakan di**: `GET /api/dropout/prodi/list`

**Format**:
```sql
pro_jenjang + ' ' + pro_nama as pro_nama
```

**Contoh Output**:
```
"D3 Teknik Informatika"
"D3 Teknik Mesin"
"D4 Teknik Elektro"
```

**Karakteristik**:
- ✅ Menampilkan jenjang (D3/D4)
- ✅ Menampilkan nama lengkap prodi
- ❌ Tidak menampilkan konsentrasi
- ❌ Satu prodi = satu row (tidak ada duplikasi per konsentrasi)

---

### 2. SP: `sia_getDataRiwayatPengunduranDiri`
**Digunakan di**: `GET /api/pengundurandiri/riwayat`

**Format**:
```sql
pro_singkatan + ' (' + kon_singkatan + ')' AS kon_singkatan
```

**Contoh Output**:
```
"TI (SE)"   -- Teknik Informatika - Software Engineering
"TI (DS)"   -- Teknik Informatika - Data Science
"TM (MFG)"  -- Teknik Mesin - Manufacturing
```

**Karakteristik**:
- ✅ Menampilkan singkatan prodi
- ✅ Menampilkan singkatan konsentrasi
- ✅ Format ringkas
- ❌ Tidak menampilkan jenjang
- ❌ Tidak menampilkan nama lengkap

---

## Opsi Penyamaan

### Opsi A: Update `sia_getListProdi` → Format Riwayat
**Ubah**: `sia_getListProdi` mengikuti format `sia_getDataRiwayatPengunduranDiri`

**SQL**:
```sql
ALTER PROCEDURE [dbo].[sia_getListProdi]
AS
BEGIN
    SELECT 
        a.pro_id,
        a.pro_singkatan + ' (' + b.kon_singkatan + ')' AS pro_nama
    FROM sia_msprodi a
    INNER JOIN sia_mskonsentrasi b ON a.pro_id = b.pro_id
    WHERE a.pro_status = 'Aktif'
      AND b.kon_status = 'Aktif'
    ORDER BY a.pro_nama, b.kon_singkatan;
END
```

**Hasil**:
```json
[
  { "value": "PRO001", "text": "TI (SE)" },
  { "value": "PRO001", "text": "TI (DS)" },
  { "value": "PRO002", "text": "TM (MFG)" }
]
```

**Keuntungan**:
- ✅ Format konsisten di semua endpoint
- ✅ Lebih ringkas
- ✅ Menampilkan konsentrasi
- ✅ User bisa pilih prodi + konsentrasi sekaligus

**Kekurangan**:
- ❌ Tidak ada jenjang (D3/D4)
- ❌ Tidak ada nama lengkap
- ❌ Satu prodi bisa muncul berkali-kali (per konsentrasi)
- ⚠️ **Breaking change** untuk frontend yang sudah pakai endpoint ini

---

### Opsi B: Update Riwayat → Format `sia_getListProdi`
**Ubah**: `sia_getDataRiwayatPengunduranDiri` mengikuti format `sia_getListProdi`

**SQL**:
```sql
-- Di semua SELECT statement, ganti:
pro_singkatan + ' (' + kon_singkatan + ')' AS kon_singkatan

-- Menjadi:
d.pro_jenjang + ' ' + d.pro_nama AS prodi_nama,
c.kon_singkatan AS konsentrasi
```

**Hasil**:
```json
{
  "prodiNama": "D3 Teknik Informatika",
  "konsentrasi": "SE"
}
```

**Keuntungan**:
- ✅ Menampilkan jenjang
- ✅ Menampilkan nama lengkap
- ✅ Prodi dan konsentrasi terpisah (lebih fleksibel)

**Kekurangan**:
- ❌ Response structure berubah
- ⚠️ **Breaking change** untuk frontend yang sudah pakai endpoint riwayat

---

### Opsi C: Buat Format Baru (Hybrid)
**Format Baru**: Gabungan keduanya

**SQL**:
```sql
-- Format: "D3 Teknik Informatika (SE)"
d.pro_jenjang + ' ' + d.pro_nama + ' (' + c.kon_singkatan + ')' AS prodi_lengkap
```

**Hasil**:
```
"D3 Teknik Informatika (SE)"
"D3 Teknik Informatika (DS)"
"D3 Teknik Mesin (MFG)"
```

**Keuntungan**:
- ✅ Lengkap: jenjang + nama + konsentrasi
- ✅ Mudah dibaca
- ✅ Informasi maksimal

**Kekurangan**:
- ❌ Terlalu panjang
- ⚠️ **Breaking change** untuk kedua endpoint

---

## Rekomendasi

### Jika Ingin Konsistensi Tanpa Breaking Change:
**Buat endpoint baru** dengan format yang diinginkan, jangan ubah yang lama:

```
GET /api/dropout/prodi/list/v2  → Format baru
GET /api/dropout/prodi/list     → Format lama (tetap)
```

### Jika Boleh Breaking Change:
**Pilih Opsi C (Hybrid)** karena paling lengkap dan informatif.

---

## Impact Analysis

### Jika Update `sia_getListProdi` (Opsi A):
**Endpoint yang terpengaruh**:
- ✅ `GET /api/dropout/prodi/list`
- ✅ `GET /api/pengundurandiri/prodi/list` (jika ada)
- ✅ `GET /api/cutiakademik/prodi/list` (jika ada)

**Frontend yang perlu update**:
- Dropdown prodi di form Drop Out
- Dropdown prodi di form Pengunduran Diri
- Dropdown prodi di form Cuti Akademik
- Filter prodi di halaman riwayat

### Jika Update Riwayat (Opsi B):
**Endpoint yang terpengaruh**:
- ✅ `GET /api/pengundurandiri/riwayat`

**Frontend yang perlu update**:
- Tabel riwayat Pengunduran Diri
- Export Excel riwayat

---

## Pilihan Anda?

Mana yang Anda inginkan?

**A**: Update `sia_getListProdi` → format singkat (TI (SE))
**B**: Update riwayat → format lengkap (D3 Teknik Informatika)
**C**: Buat format hybrid baru (D3 Teknik Informatika (SE))
**D**: Buat endpoint baru, jangan ubah yang lama

Silakan pilih, saya akan buatkan SQL script-nya!
