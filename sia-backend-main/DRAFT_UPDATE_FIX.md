# Fix Update Draft Cuti Akademik - Mahasiswa Login

## 🚨 Masalah yang Terjadi

**Error:** `PUT http://localhost:5234/api/CutiAkademik/1766021334465 400 (Bad Request)`

**Context:**
- User login sebagai mahasiswa
- Mencoba update data draft cuti akademik
- ID draft menggunakan format timestamp: `1766021334465`

**Root Cause:**
1. Stored procedure `sia_editCutiAkademik` tidak bisa handle draft ID format baru
2. Stored procedure `sia_detailCutiAkademik` juga tidak bisa handle draft ID
3. Draft ID menggunakan format timestamp, bukan format PMA tradisional

## ✅ Solusi yang Diterapkan

### 1. Hybrid Approach untuk Update
```csharp
// Tentukan apakah ini draft ID atau final ID
bool isDraftId = !id.Contains("PMA") && !id.Contains("CA");

if (isDraftId)
{
    // Untuk draft ID, gunakan direct SQL (lebih reliable)
    var updateSql = @"
        UPDATE sia_mscutiakademik 
        SET cak_tahunajaran = @tahunajaran,
            cak_semester = @semester,
            cak_lampiran_suratpengajuan = @lampiran_sp,
            cak_lampiran = @lampiran,
            cak_modif_date = GETDATE(),
            cak_modif_by = @modified_by
        WHERE cak_id = @id";
}
else
{
    // Untuk final ID, gunakan stored procedure
    var cmd = new SqlCommand("sia_editCutiAkademik", conn);
}
```

### 2. Hybrid Approach untuk GetDetail
```csharp
bool isDraftId = !id.Contains("PMA") && !id.Contains("CA");

if (isDraftId)
{
    // Direct SQL query dengan JOIN untuk mendapatkan data lengkap
    var sql = @"
        SELECT a.cak_id, a.mhs_id, b.mhs_nama, c.kon_nama, ...
        FROM sia_mscutiakademik a
        LEFT JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        LEFT JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
        LEFT JOIN sia_msprodi d ON c.pro_id = d.pro_id
        WHERE a.cak_id = @id";
}
else
{
    // Gunakan stored procedure untuk final ID
    var cmd = new SqlCommand("sia_detailCutiAkademik", conn);
}
```

### 3. Enhanced Error Handling
```csharp
try
{
    // Debug logging untuk troubleshooting
    Console.WriteLine($"Update request for ID: {id}");
    
    // Auto-set ModifiedBy jika kosong
    if (string.IsNullOrEmpty(dto.ModifiedBy))
    {
        dto.ModifiedBy = HttpContext.Items["UserId"]?.ToString() ?? "system";
    }

    var success = await _service.UpdateAsync(id, dto);
    // ... rest of the code
}
catch (Exception ex)
{
    return BadRequest(new { 
        message = "Terjadi kesalahan saat mengupdate data.", 
        error = ex.Message,
        details = ex.InnerException?.Message
    });
}
```

## 🔧 Perbaikan yang Dilakukan

### 1. Repository Layer
- ✅ **UpdateAsync**: Hybrid approach - direct SQL untuk draft, SP untuk final
- ✅ **GetDetailAsync**: Hybrid approach - direct SQL untuk draft, SP untuk final
- ✅ **Column Name Fix**: Menggunakan `cak_modif_by` dan `cak_modif_date` sesuai SP

### 2. Controller Layer
- ✅ **Enhanced Error Handling**: Detailed error messages dengan logging
- ✅ **Auto ModifiedBy**: Set otomatis dari context
- ✅ **Debug Logging**: Console logging untuk troubleshooting

### 3. ID Detection Logic
```csharp
bool isDraftId = !id.Contains("PMA") && !id.Contains("CA");
```
- **Draft ID**: Format timestamp (contoh: `1766021334465`)
- **Final ID**: Format PMA (contoh: `012/PMA/CA/IX/2025`)

## 🧪 Testing Scenarios

### Test Case 1: Update Draft ID (Mahasiswa)
```bash
PUT /api/CutiAkademik/1766021334465
Content-Type: multipart/form-data

{
  "tahunAjaran": "2024/2025",
  "semester": "Genap",
  "modifiedBy": "0420240032"
}
```

**Expected Response:**
```json
{
  "message": "Cuti Akademik berhasil diupdate."
}
```

### Test Case 2: Update Final ID
```bash
PUT /api/CutiAkademik/012/PMA/CA/IX/2025
```

**Expected:** Menggunakan stored procedure

### Test Case 3: Update dengan File Upload
```bash
PUT /api/CutiAkademik/1766021334465
Content-Type: multipart/form-data

{
  "tahunAjaran": "2024/2025",
  "semester": "Genap",
  "lampiranSuratPengajuan": [file],
  "lampiran": [file]
}
```

## 🚀 Cara Testing

### 1. Via Frontend (Mahasiswa Login)
1. Login sebagai mahasiswa
2. Buka halaman edit cuti akademik
3. Edit data (tahun ajaran, semester, file)
4. Klik "Simpan Perubahan"
5. ✅ **Expected**: Berhasil update tanpa error 400

### 2. Via Swagger
1. Restart aplikasi: `dotnet run`
2. Buka: `http://localhost:5234/swagger`
3. Test `PUT /api/CutiAkademik/{id}`
4. Gunakan ID draft: `1766021334465`

### 3. Check Console Logs
```
Update request for ID: 1766021334465
TahunAjaran: 2024/2025
Semester: Genap
ModifiedBy: 0420240032
Auto-set ModifiedBy to: 0420240032
Update successful
```

## 📋 Troubleshooting

### Jika Masih Error 400
1. **Check Console Logs**: Lihat error message di console
2. **Check ID Format**: Pastikan ID benar (draft vs final)
3. **Check Required Fields**: Pastikan tahunAjaran dan semester tidak kosong
4. **Check Database**: Pastikan data dengan ID tersebut ada

### Jika Error "Data tidak ditemukan"
1. **Verify ID**: Cek apakah ID ada di database
2. **Check Connection**: Pastikan koneksi database OK
3. **Check Table**: Pastikan tabel `sia_mscutiakademik` accessible

### Jika File Upload Bermasalah
1. **Check Folder**: Pastikan `wwwroot/uploads/cuti/` ada dan writable
2. **Check File Size**: Pastikan file tidak terlalu besar
3. **Check File Type**: Pastikan file type diizinkan

## 🎯 Keuntungan Solusi Hybrid

1. **Backward Compatibility**: Final ID tetap menggunakan SP yang sudah teruji
2. **Draft ID Support**: Draft ID baru bisa di-handle dengan direct SQL
3. **Performance**: Direct SQL lebih cepat untuk operasi sederhana
4. **Reliability**: Mengurangi dependency pada SP untuk draft operations
5. **Debugging**: Error logging yang lebih baik untuk troubleshooting

## 📝 Format ID yang Didukung

### Draft ID (Mahasiswa)
- **Format**: Timestamp numeric
- **Contoh**: `1766021334465`, `1766020731447`
- **Method**: Direct SQL query

### Final ID (Setelah Generate)
- **Format**: PMA standard
- **Contoh**: `012/PMA/CA/IX/2025`
- **Method**: Stored procedure

Sekarang mahasiswa bisa update draft cuti akademik mereka tanpa error 400!