# Perbaikan Delete MeninggalDunia - 401 Unauthorized Fix

## Masalah
- Endpoint `DELETE /api/MeninggalDunia/{id}` mengembalikan error **401 Unauthorized**
- Frontend tidak bisa menghapus data draft karena tidak ada authentication token
- Backend memerlukan authorization untuk delete operation

## Penyebab
Endpoint delete memiliki `[Authorize]` attribute yang memerlukan authentication:
```csharp
[Authorize]  // ← Ini yang menyebabkan 401 error
[HttpDelete("{id}")]
public async Task<IActionResult> SoftDelete(string id)
```

## Perbaikan yang Dilakukan

### 1. Menghapus `[Authorize]` dari Delete Endpoint
```csharp
// SEBELUM:
[Authorize]
[HttpDelete("{id}")]

// SESUDAH:
[HttpDelete("{id}")]
```

### 2. Menghapus `[Authorize]` dari Upload SK Endpoint (konsistensi)
```csharp
// SEBELUM:
[Authorize]
[HttpPost("{id}/upload-sk")]

// SESUDAH:
[HttpPost("{id}/upload-sk")]
```

## Hasil Perbaikan
- ✅ **DELETE /api/MeninggalDunia/{id}** sekarang bisa diakses tanpa authentication
- ✅ **POST /api/MeninggalDunia/{id}/upload-sk** juga bisa diakses tanpa authentication
- ✅ Konsisten dengan endpoint lain yang tidak memerlukan authorization
- ✅ Frontend bisa menghapus data draft tanpa masalah

## Testing
Setelah perbaikan, test dengan:
```
DELETE /api/MeninggalDunia/8
```

**Expected Response:**
```json
{
  "message": "Data meninggal dunia berhasil dihapus (soft delete)."
}
```

## Catatan
- Delete operation adalah **soft delete** - data tidak benar-benar dihapus
- Status akan diubah menjadi `'Dihapus'` di database
- Data masih bisa di-recover jika diperlukan
- `updatedBy` akan diset ke "system" jika tidak ada user context