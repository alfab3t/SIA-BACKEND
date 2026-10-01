# Fix: Drop Out Menimbang & Mengingat Fields

## Issue Summary
Field **Menimbang** dan **Mengingat** tidak tersimpan di database meskipun frontend sudah mengirim data dengan benar.

## Root Cause
1. DTO `CreatePengajuanDORequest` tidak memiliki property `Menimbang` dan `Mengingat`
2. Repository method `CreatePengajuanDOAsync` tidak menyimpan field tersebut ke database

## Solution Implemented

### Approach: UPDATE After INSERT
Karena stored procedure `sia_createPengajuanDO` yang sudah ada **TIDAK BOLEH DIGANTI**, solusinya adalah:
1. Panggil SP untuk INSERT data dasar (mhs_id, lampiran, created_by)
2. Ambil ID yang baru dibuat
3. Lakukan UPDATE untuk menyimpan `menimbang` dan `mengingat`

### Changes Made

#### 1. DTO Update
**File**: `sia-backend-main/DTOs/DropOut/CreatePengajuanDORequest.cs`

```csharp
public class CreatePengajuanDORequest
{
    public string MhsId { get; set; } = "";
    public string Menimbang { get; set; } = "";      // ✅ ADDED
    public string Mengingat { get; set; } = "";      // ✅ ADDED
    public string Lampiran { get; set; } = "";
    public string LampiranSuratPengajuan { get; set; } = "";
    public string? CreatedBy { get; set; }
}
```

#### 2. Repository Update
**File**: `sia-backend-main/Repositories/Implementations/DropOutRepository.cs`

**Method**: `CreatePengajuanDOAsync`

```csharp
public async Task<string?> CreatePengajuanDOAsync(CreatePengajuanDORequest dto, string createdBy)
{
    await using var conn = new SqlConnection(_conn);
    await conn.OpenAsync();
    
    // 1. Panggil SP untuk INSERT data dasar
    await using var cmd = new SqlCommand("sia_createPengajuanDO", conn)
    {
        CommandType = CommandType.StoredProcedure
    };

    cmd.Parameters.AddWithValue("@mhs_id", dto.MhsId);
    cmd.Parameters.AddWithValue("@dro_lampiran", dto.Lampiran ?? "");
    cmd.Parameters.AddWithValue("@dro_lampiran_surat_pengajuan", dto.LampiranSuratPengajuan ?? "");
    cmd.Parameters.AddWithValue("@dro_created_by", createdBy);

    await cmd.ExecuteNonQueryAsync();

    // 2. Ambil ID yang baru dibuat
    var cmd2 = new SqlCommand(@"
        SELECT TOP 1 dro_id 
        FROM sia_msdropout 
        WHERE dro_created_by = @createdBy 
        ORDER BY dro_created_date DESC",
        conn
    );
    cmd2.Parameters.AddWithValue("@createdBy", createdBy);
    var newId = (string?)await cmd2.ExecuteScalarAsync();

    // 3. UPDATE menimbang dan mengingat jika ada
    if (!string.IsNullOrEmpty(newId) && (!string.IsNullOrEmpty(dto.Menimbang) || !string.IsNullOrEmpty(dto.Mengingat)))
    {
        var cmd3 = new SqlCommand(@"
            UPDATE sia_msdropout 
            SET dro_menimbang = @menimbang,
                dro_mengingat = @mengingat,
                dro_modif_date = GETDATE()
            WHERE dro_id = @dro_id",
            conn
        );
        cmd3.Parameters.AddWithValue("@dro_id", newId);
        cmd3.Parameters.AddWithValue("@menimbang", dto.Menimbang ?? "");
        cmd3.Parameters.AddWithValue("@mengingat", dto.Mengingat ?? "");
        
        await cmd3.ExecuteNonQueryAsync();
    }

    return newId;
}
```

## Testing

### Test Case 1: Create with Menimbang & Mengingat
**Request**:
```json
POST /api/DropOut/create-pengajuan
{
  "mhsId": "0720250061",
  "menimbang": "<p>dinf</p>",
  "mengingat": "<p>jhuygftgv hjbgytfr fvbhbjgytfdncf</p>",
  "lampiran": "",
  "lampiranSuratPengajuan": "",
  "createdBy": "nda_prodi"
}
```

**Expected Response**:
```json
{
  "message": "Pengajuan DO Draft berhasil dibuat.",
  "id": "5",
  "createdBy": "nda_prodi"
}
```

### Test Case 2: Verify Data Saved
**Request**:
```
GET /api/DropOut/detail?id=5
```

**Expected Response**:
```json
{
  "id": "5",
  "mhsId": "0720250061",
  "mhsText": "0720250061 - MUHAMAD RIDWAN FATUR RIZKI",
  "prodi": "Teknologi Rekayasa Pemeliharaan Alat Berat",
  "angkatan": "2025",
  "status": "Draft",
  "menimbang": "<p>dinf</p>",           // ✅ SHOULD BE SAVED
  "mengingat": "<p>jhuygftgv hjbgytfr fvbhbjgytfdncf</p>",  // ✅ SHOULD BE SAVED
  "createdBy": "nda_prodi"
}
```

## Database Requirements

### Table Structure
Pastikan tabel `sia_msdropout` memiliki kolom:
- `dro_menimbang` (VARCHAR/TEXT) - untuk menyimpan HTML content
- `dro_mengingat` (VARCHAR/TEXT) - untuk menyimpan HTML content

### Stored Procedure
**TIDAK ADA PERUBAHAN** pada stored procedure `sia_createPengajuanDO`.
SP tetap digunakan untuk INSERT data dasar, kemudian dilakukan UPDATE terpisah.

## Advantages of This Approach

1. ✅ **No SP Changes**: Stored procedure yang sudah ada tidak perlu diubah
2. ✅ **Backward Compatible**: Tidak mempengaruhi kode lain yang menggunakan SP yang sama
3. ✅ **Flexible**: Mudah menambah field lain di masa depan tanpa mengubah SP
4. ✅ **Safe**: Jika UPDATE gagal, data dasar tetap tersimpan

## Notes

- Frontend mengirim HTML content (dengan tags `<p>`, `<br>`, dll)
- Database column harus tipe TEXT atau VARCHAR(MAX) untuk menyimpan HTML
- Tidak ada HTML sanitization yang menghapus content
- UPDATE hanya dilakukan jika `menimbang` atau `mengingat` tidak kosong

## Status
✅ **FIXED** - Ready for testing

## Date
2026-02-05
