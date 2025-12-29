# Debug Delete MeninggalDunia - Troubleshooting Guide

## Masalah
- Data ID 3 ada di GetAll tapi delete gagal dengan pesan "Data mungkin tidak ditemukan atau sudah dihapus"
- Stored procedure mungkin tidak berfungsi dengan benar

## Perbaikan yang Dilakukan

### 1. Hybrid Delete Implementation
Repository sekarang mencoba 2 metode:
1. **Direct SQL Query** (lebih reliable)
2. **Stored Procedure** (fallback)

```csharp
// Coba direct SQL dulu
UPDATE sia_msmeninggaldunia 
SET mdu_status = 'Dihapus',
    mdu_modif_by = @updatedBy,
    mdu_modif_date = GETDATE()
WHERE mdu_id = @id

// Jika gagal, coba stored procedure
EXEC sia_deleteMeninggalDunia @p1, @p2, ...
```

### 2. Enhanced Logging
Console akan menampilkan:
```
[SoftDeleteAsync] Direct SQL - ID: 3, UpdatedBy: system, Rows affected: 1
[SoftDeleteAsync] SP - ID: 3, UpdatedBy: system, Rows affected: 0
```

### 3. Debug Endpoints

#### A. Cek Data Sebelum Delete
```
GET /api/MeninggalDunia/debug/3
```
**Response:**
```json
{
  "message": "Data ditemukan",
  "id": "3",
  "data": { ... },
  "canDelete": true
}
```

#### B. Lihat Data yang Sudah Dihapus
```
GET /api/MeninggalDunia/GetAll/deleted
```

## Testing Steps

### Step 1: Cek Data Sebelum Delete
```
GET /api/MeninggalDunia/debug/3
```
- Pastikan data ditemukan
- Pastikan `canDelete: true`

### Step 2: Coba Delete
```
DELETE /api/MeninggalDunia/3
```

### Step 3: Cek Console Log
Lihat di console aplikasi:
- Apakah Direct SQL berhasil (rows affected > 0)?
- Apakah SP dipanggil sebagai fallback?

### Step 4: Verifikasi Hasil
```
GET /api/MeninggalDunia/GetAll
```
Data ID 3 seharusnya hilang

```
GET /api/MeninggalDunia/GetAll/deleted
```
Data ID 3 seharusnya muncul dengan status 'Dihapus'

## Expected Results

### Jika Direct SQL Berhasil:
```json
{
  "message": "Data meninggal dunia berhasil dihapus (soft delete).",
  "id": "3",
  "deletedBy": "system",
  "success": true
}
```

Console log:
```
[SoftDeleteAsync] Direct SQL - ID: 3, UpdatedBy: system, Rows affected: 1
```

### Jika Masih Gagal:
Console akan menunjukkan error detail dan stack trace untuk debugging lebih lanjut.

## Troubleshooting

### Jika Direct SQL Gagal (rows affected: 0):
- Cek apakah ID benar-benar ada di database
- Cek apakah ada constraint yang mencegah update
- Cek apakah tipe data ID cocok (string vs int)

### Jika SP Juga Gagal:
- Cek apakah stored procedure ada di database
- Cek apakah parameter sesuai dengan definisi SP
- Cek apakah ada permission issue

### Jika Keduanya Gagal:
- Cek connection string
- Cek apakah tabel `sia_msmeninggaldunia` ada
- Cek apakah kolom `mdu_id`, `mdu_status`, `mdu_modif_by`, `mdu_modif_date` ada