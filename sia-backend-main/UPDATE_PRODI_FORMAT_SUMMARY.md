# Update Format Prodi - Summary

## Status: ✅ READY TO EXECUTE

Perubahan format nama prodi di SP riwayat Pengunduran Diri untuk menyamakan dengan format `sia_getListProdi`.

---

## Perubahan yang Dilakukan

### 1. Stored Procedure: `sia_getDataRiwayatPengunduranDiri`
**File SQL**: `SQL_UPDATE_SP_RIWAYAT_PENGUNDURAN_DIRI_FORMAT.sql`

**Before**:
```sql
pro_singkatan + ' (' + kon_singkatan + ')' AS kon_singkatan
```
Output: `"TI (SE)"`

**After**:
```sql
d.pro_jenjang + ' ' + d.pro_nama AS prodi_nama,
c.kon_singkatan AS konsentrasi
```
Output: 
- `prodi_nama`: `"D3 Teknik Informatika"`
- `konsentrasi`: `"SE"`

---

### 2. DTO: `PengunduranDiriRiwayatResponse.cs`
**File**: `sia-backend-main/DTOs/PengunduranDiri/PengunduranDiriRiwayatResponse.cs`

**Before**:
```csharp
public string Konsentrasi { get; set; } = ""; // Format: "TI (SE)"
```

**After**:
```csharp
public string ProdiNama { get; set; } = ""; // Format: "D3 Teknik Informatika"
public string Konsentrasi { get; set; } = ""; // Format: "SE", "DS", dll
```

---

### 3. Repository: `PengunduranDiriRepository.cs`
**File**: `sia-backend-main/Repositories/Implementations/PengunduranDiriRepository.cs`

**Before**:
```csharp
Konsentrasi = reader["kon_singkatan"].ToString()
```

**After**:
```csharp
ProdiNama = reader["prodi_nama"].ToString(), // Format: "D3 Teknik Informatika"
Konsentrasi = reader["konsentrasi"].ToString() // Format: "SE", "DS", dll
```

---

## Response Structure Changes

### Before
```json
{
  "pdiId": "001/PMA/PD/I/2026",
  "mhsId": "2101010001",
  "namaMahasiswa": "John Doe",
  "konsentrasi": "TI (SE)",
  "status": "Disetujui"
}
```

### After
```json
{
  "pdiId": "001/PMA/PD/I/2026",
  "mhsId": "2101010001",
  "namaMahasiswa": "John Doe",
  "prodiNama": "D3 Teknik Informatika",
  "konsentrasi": "SE",
  "status": "Disetujui"
}
```

---

## Impact Analysis

### Backend
- ✅ DTO updated
- ✅ Repository mapping updated
- ✅ No compilation errors
- ✅ Backward compatible (field added, not removed)

### Frontend
⚠️ **Breaking Change** - Frontend perlu update:

**Before**:
```javascript
// Display konsentrasi
<td>{item.konsentrasi}</td>  // "TI (SE)"
```

**After**:
```javascript
// Display prodi dan konsentrasi terpisah
<td>{item.prodiNama}</td>      // "D3 Teknik Informatika"
<td>{item.konsentrasi}</td>    // "SE"

// Atau gabungkan manual jika perlu
<td>{item.prodiNama} ({item.konsentrasi})</td>  // "D3 Teknik Informatika (SE)"
```

---

## Execution Steps

### 1. Execute SQL Script
```sql
-- Run this in SQL Server Management Studio
-- File: SQL_UPDATE_SP_RIWAYAT_PENGUNDURAN_DIRI_FORMAT.sql

USE [ERP_PolmanAstra_NDA]
GO

-- Execute the ALTER PROCEDURE statement
```

### 2. Restart Backend
```bash
# Stop current process
# Rebuild and run
dotnet build
dotnet run
```

### 3. Test API
```bash
# Test riwayat endpoint
GET /api/pengundurandiri/riwayat?pdi_status=Disetujui

# Expected response should have:
# - prodiNama: "D3 Teknik Informatika"
# - konsentrasi: "SE"
```

### 4. Update Frontend
Update semua tempat yang menggunakan field `konsentrasi` untuk handle format baru:
- Tabel riwayat
- Export Excel
- Filter/Search
- Display detail

---

## Rollback Plan

Jika ada masalah, rollback dengan:

### 1. Revert SP
```sql
-- Kembalikan ke format lama
ALTER PROCEDURE [dbo].[sia_getDataRiwayatPengunduranDiri]
...
pro_singkatan + ' (' + kon_singkatan + ')' AS kon_singkatan
...
```

### 2. Revert DTO
```csharp
// Hapus field ProdiNama
// public string ProdiNama { get; set; } = "";

// Kembalikan Konsentrasi ke format lama
public string Konsentrasi { get; set; } = ""; // Format: "TI (SE)"
```

### 3. Revert Repository
```csharp
Konsentrasi = reader["kon_singkatan"].ToString()
```

---

## Benefits

✅ **Konsistensi**: Format sama dengan `sia_getListProdi`
✅ **Lebih Jelas**: Prodi dan konsentrasi terpisah
✅ **Lebih Informatif**: Menampilkan jenjang (D3/D4) dan nama lengkap
✅ **Fleksibel**: Frontend bisa display terpisah atau gabung sesuai kebutuhan

---

## Files Modified

1. ✅ `SQL_UPDATE_SP_RIWAYAT_PENGUNDURAN_DIRI_FORMAT.sql` - SQL script
2. ✅ `DTOs/PengunduranDiri/PengunduranDiriRiwayatResponse.cs` - DTO
3. ✅ `Repositories/Implementations/PengunduranDiriRepository.cs` - Repository mapping

---

## Next Steps

1. **Execute SQL script** di database
2. **Restart backend** untuk apply changes
3. **Test API** untuk verify response structure
4. **Update frontend** untuk handle format baru
5. **Test end-to-end** untuk ensure everything works

---

**Created**: February 4, 2026
**Status**: ✅ Ready to Execute
**Breaking Change**: ⚠️ Yes (Frontend needs update)
