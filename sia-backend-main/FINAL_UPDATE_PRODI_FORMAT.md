# Final Update - Format Prodi Pengunduran Diri

## Status: ✅ READY TO EXECUTE

Semua perubahan untuk menyamakan format prodi sudah selesai di backend. Tinggal execute SQL script di database.

---

## Summary Perubahan

### Stored Procedures yang Diupdate (2 SP)

1. **`sia_getDataPengunduranDiri`**
   - Endpoint: `GET /api/pengundurandiri`
   - DTO: `PengunduranDiriListResponse`

2. **`sia_getDataRiwayatPengunduranDiri`**
   - Endpoint: `GET /api/pengundurandiri/riwayat`
   - DTO: `PengunduranDiriRiwayatResponse`

### Format Perubahan

**Before**:
```sql
pro_singkatan + ' (' + kon_singkatan + ')' AS kon_singkatan
-- Output: "TI (SE)"
```

**After**:
```sql
d.pro_jenjang + ' ' + d.pro_nama AS prodi_nama,
c.kon_singkatan AS konsentrasi
-- Output: 
--   prodi_nama: "D3 Teknik Informatika"
--   konsentrasi: "SE"
```

---

## Files Modified

### Backend (✅ Done)

1. **DTOs**:
   - ✅ `DTOs/PengunduranDiri/PengunduranDiriListResponse.cs`
   - ✅ `DTOs/PengunduranDiri/PengunduranDiriRiwayatResponse.cs`

2. **Repository**:
   - ✅ `Repositories/Implementations/PengunduranDiriRepository.cs`

3. **SQL Scripts**:
   - ✅ `SQL_UPDATE_SP_GET_DATA_PENGUNDURAN_DIRI.sql`
   - ✅ `SQL_UPDATE_SP_RIWAYAT_PENGUNDURAN_DIRI_FORMAT.sql`
   - ✅ `SQL_UPDATE_ALL_SP_PRODI_FORMAT.sql` (Combined)

### Database (⏳ Pending)

Execute SQL script:
```sql
-- File: SQL_UPDATE_ALL_SP_PRODI_FORMAT.sql
-- Updates 2 stored procedures
```

---

## Response Structure Changes

### 1. GET /api/pengundurandiri

**Before**:
```json
{
  "pdiId": "001/PMA/PD/I/2026",
  "mhsId": "2101010001",
  "status": "Draft"
}
```

**After**:
```json
{
  "pdiId": "001/PMA/PD/I/2026",
  "mhsId": "2101010001",
  "status": "Draft",
  "prodiNama": "D3 Teknik Informatika",
  "konsentrasi": "SE"
}
```

### 2. GET /api/pengundurandiri/riwayat

**Before**:
```json
{
  "pdiId": "001/PMA/PD/I/2026",
  "namaMahasiswa": "John Doe",
  "konsentrasi": "TI (SE)",
  "status": "Disetujui"
}
```

**After**:
```json
{
  "pdiId": "001/PMA/PD/I/2026",
  "namaMahasiswa": "John Doe",
  "prodiNama": "D3 Teknik Informatika",
  "konsentrasi": "SE",
  "status": "Disetujui"
}
```

---

## Execution Steps

### 1. Execute SQL Script (Database)

```sql
-- Open SQL Server Management Studio
-- Connect to: ERP_PolmanAstra_NDA
-- Open file: SQL_UPDATE_ALL_SP_PRODI_FORMAT.sql
-- Execute (F5)

-- Expected output:
-- ✓ SP sia_getDataPengunduranDiri updated
-- ✓ SP sia_getDataRiwayatPengunduranDiri updated
-- ALL SP UPDATED SUCCESSFULLY!
```

### 2. Restart Backend (Already Done)

```bash
# Backend code already updated
# Just restart the application
dotnet run
```

### 3. Test API Endpoints

```bash
# Test 1: Get all pengunduran diri
GET /api/pengundurandiri
Authorization: Bearer {token}

# Expected: Response includes prodiNama and konsentrasi

# Test 2: Get riwayat
GET /api/pengundurandiri/riwayat?pdi_status=Disetujui
Authorization: Bearer {token}

# Expected: Response includes prodiNama and konsentrasi
```

### 4. Update Frontend

Update all places that use these endpoints:

**Before**:
```javascript
// Display konsentrasi
<td>{item.konsentrasi}</td>  // "TI (SE)"
```

**After**:
```javascript
// Option 1: Display separately
<td>{item.prodiNama}</td>      // "D3 Teknik Informatika"
<td>{item.konsentrasi}</td>    // "SE"

// Option 2: Combine manually
<td>{item.prodiNama} ({item.konsentrasi})</td>  
// "D3 Teknik Informatika (SE)"
```

---

## Impact Analysis

### Backend
- ✅ 2 DTOs updated
- ✅ 1 Repository updated
- ✅ 0 compilation errors
- ✅ Build successful

### Database
- ⏳ 2 SPs need to be updated
- ⏳ Execute SQL script required

### Frontend
- ⚠️ Breaking change
- ⚠️ Need to update all components using these endpoints
- ⚠️ Update table columns
- ⚠️ Update filters/search
- ⚠️ Update export Excel

---

## Benefits

✅ **Konsistensi**: Format sama dengan `sia_getListProdi`
✅ **Lebih Jelas**: Prodi dan konsentrasi terpisah
✅ **Lebih Informatif**: Menampilkan jenjang (D3/D4) dan nama lengkap
✅ **Fleksibel**: Frontend bisa display terpisah atau gabung

---

## Rollback Plan

If needed, revert changes:

### 1. Revert SQL
```sql
-- Kembalikan ke format lama
pro_singkatan + ' (' + kon_singkatan + ')' AS kon_singkatan
```

### 2. Revert DTOs
```csharp
// Remove ProdiNama field
// Keep only Konsentrasi with old format
```

### 3. Revert Repository
```csharp
// Remove ProdiNama mapping
Konsentrasi = reader["kon_singkatan"].ToString()
```

---

## Checklist

### Backend
- [x] Update DTOs
- [x] Update Repository mapping
- [x] Build successful
- [x] No diagnostics errors
- [x] Create SQL scripts

### Database
- [ ] Execute SQL script
- [ ] Verify SP updated
- [ ] Test SP manually

### Testing
- [ ] Test GET /api/pengundurandiri
- [ ] Test GET /api/pengundurandiri/riwayat
- [ ] Verify response structure
- [ ] Check all status modes

### Frontend
- [ ] Update table components
- [ ] Update filters
- [ ] Update export Excel
- [ ] Test end-to-end

---

## Next Action

**Execute SQL script di database:**
```
File: SQL_UPDATE_ALL_SP_PRODI_FORMAT.sql
```

Setelah itu, restart backend dan test API!

---

**Created**: February 4, 2026
**Status**: ✅ Backend Ready, ⏳ Database Pending
**Breaking Change**: ⚠️ Yes (Frontend needs update)
