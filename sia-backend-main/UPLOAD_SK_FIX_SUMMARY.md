# Upload SK Fix Summary

## ✅ Perubahan yang Sudah Dilakukan

### 1. Pisahkan Upload File dan Update Database

**POST `/api/DropOut/upload-sk-file`** - Upload file saja
- Menerima `DroId`, `SkFile`, `SkpbFile`
- Upload file ke folder server
- Generate nama file unik: `SK_DO_{droId}_{timestamp}_{guid}.pdf`
- Return path file ke frontend
- **TIDAK update database**
- **TIDAK memanggil GetByIdAsync** (sudah dihapus)

**PUT `/api/DropOut/upload-sk`** - Update database
- Menerima `DroId`, `SK` path, `SKPB` path
- Update database dengan path file
- Panggil SP `sia_uploadSKDO`

### 2. Fix GetByIdAsync dengan Safe Getters
- Ditambahkan `SafeGetString()` dan `SafeGetDateTime()`
- Handle kolom yang tidak ada di SP `sia_detailDO`
- Tidak akan crash lagi jika kolom tidak ada

## 🔧 Cara Restart Aplikasi

### Option 1: Kill Process dan Rebuild
```cmd
# 1. Kill process yang sedang berjalan
taskkill /F /PID <PID>

# 2. Clean dan rebuild
cd sia-backend-main
dotnet clean
dotnet build

# 3. Run aplikasi
dotnet run
```

### Option 2: Restart dari IDE
- Stop aplikasi dari Visual Studio / Rider
- Clean Solution
- Rebuild Solution
- Run aplikasi

## 📋 Flow Upload SK yang Benar

### Frontend Flow:
```javascript
// Step 1: Upload file ke server (POST)
const formData = new FormData();
formData.append('DroId', droId);
formData.append('SkFile', skFile);      // optional
formData.append('SkpbFile', skpbFile);  // optional

const uploadResponse = await fetch('/api/DropOut/upload-sk-file', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
  },
  body: formData
});

const result = await uploadResponse.json();
// result = { message, droId, skPath, skpbPath }

// Step 2: Update database dengan path file (PUT)
const updateResponse = await fetch('/api/DropOut/upload-sk', {
  method: 'PUT',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({
    DroId: result.droId,
    SK: result.skPath,
    SKPB: result.skpbPath,
    ModifiedBy: username
  })
});
```

## 🎯 Endpoint Details

### POST `/api/DropOut/upload-sk-file`
**Request:**
- Content-Type: `multipart/form-data`
- Body:
  - `DroId` (required): string
  - `SkFile` (optional): file
  - `SkpbFile` (optional): file

**Response:**
```json
{
  "message": "File berhasil diupload ke server",
  "droId": "DO001",
  "skPath": "/uploads/dropout/sk/SK_DO_DO001_20260305123456_abc12345.pdf",
  "skpbPath": "/uploads/dropout/skpb/SKPB_DO_DO001_20260305123456_abc12345.pdf"
}
```

### PUT `/api/DropOut/upload-sk`
**Request:**
- Content-Type: `application/json`
- Body:
```json
{
  "DroId": "DO001",
  "SK": "/uploads/dropout/sk/SK_DO_DO001_20260305123456_abc12345.pdf",
  "SKPB": "/uploads/dropout/skpb/SKPB_DO_DO001_20260305123456_abc12345.pdf",
  "ModifiedBy": "admin"
}
```

**Response:**
```json
{
  "message": "Upload SK DO berhasil"
}
```

## ⚠️ Troubleshooting

### Error: IndexOutOfRangeException srt_no
**Penyebab:** Aplikasi masih menggunakan compiled DLL lama

**Solusi:**
1. Stop aplikasi
2. Hapus folder `bin` dan `obj`:
   ```cmd
   cd sia-backend-main
   rmdir /s /q bin
   rmdir /s /q obj
   ```
3. Rebuild:
   ```cmd
   dotnet build
   dotnet run
   ```

### Error: File tidak terupload
**Penyebab:** Folder tidak ada atau permission denied

**Solusi:**
- Pastikan folder `wwwroot/uploads/dropout/sk/` dan `wwwroot/uploads/dropout/skpb/` ada
- Check permission folder (harus writable)

## 📝 Notes
- Nama file menggunakan timestamp dan GUID untuk menghindari konflik
- File disimpan di folder terpisah: `sk/` untuk SK, `skpb/` untuk SKPB
- Path file yang disimpan di database adalah relative path: `/uploads/dropout/sk/...`
- Minimal satu file harus diupload (SK atau SKPB)
