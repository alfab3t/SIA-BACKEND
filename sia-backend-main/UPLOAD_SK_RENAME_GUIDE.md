# Upload SK dengan Auto-Rename Menggunakan Nomor Surat

## 📋 Overview

Saat user upload file SK dan SKPB untuk Drop Out, file akan otomatis di-rename menggunakan **nomor surat yang sudah ada** di database (bukan generate baru).

## 🔄 Flow

```
1. User upload SK/SKPB dari FE
   ↓
2. Backend ambil data Drop Out by ID
   ↓
3. Ambil nomor surat (NoSkDo) dari database
   ↓
4. Sanitize nomor surat (ganti / dengan -)
   ↓
5. Rename file: SK_DO_{nomor_surat}.pdf
   ↓
6. Save file dengan nama baru
   ↓
7. Update database dengan path file
```

## 💡 Implementasi

### Endpoint: `POST /api/DropOut/upload-sk-file`

**Request** (multipart/form-data):
```
DroId: "16/PMA/DO/I/2026"
SkFile: [file]
SkpbFile: [file]
```

**Process**:
1. Ambil data Drop Out dari database
2. Ambil `NoSkDo` (contoh: `015/PA-WADIR-I/SK/DO/II/2026`)
3. Sanitize: `015-PA-WADIR-I-SK-DO-II-2026`
4. Rename file:
   - SK: `SK_DO_015-PA-WADIR-I-SK-DO-II-2026.pdf`
   - SKPB: `SKPB_DO_015-PA-WADIR-I-SK-DO-II-2026.pdf`

**Response**:
```json
{
  "message": "Upload SK DO berhasil",
  "nomorSK": "015/PA-WADIR-I/SK/DO/II/2026",
  "skPath": "/uploads/dropout/sk/SK_DO_015-PA-WADIR-I-SK-DO-II-2026.pdf",
  "skpbPath": "/uploads/dropout/skpb/SKPB_DO_015-PA-WADIR-I-SK-DO-II-2026.pdf"
}
```

## 📁 Struktur Folder

```
wwwroot/
└── uploads/
    └── dropout/
        ├── sk/
        │   └── SK_DO_015-PA-WADIR-I-SK-DO-II-2026.pdf
        └── skpb/
            └── SKPB_DO_015-PA-WADIR-I-SK-DO-II-2026.pdf
```

## 🔧 Code Changes

### File: `Controllers/DropOutController.cs`

**Method**: `UploadSKFile`

**Perubahan**:
```csharp
// ❌ SEBELUM: Generate nomor baru
string nomorSK = await GenerateNoSKAsync(request.DroId);

// ✅ SESUDAH: Pakai nomor yang sudah ada
var dropoutData = await _repo.GetByIdAsync(request.DroId);
string nomorSK = dropoutData.NoSkDo ?? $"DO_{request.DroId}";
string safeNomorSK = nomorSK.Replace("/", "-").Replace("\\", "-");
```

## 📊 Contoh Penamaan File

| Nomor Surat (NoSkDo) | Nama File SK | Nama File SKPB |
|---------------------|--------------|----------------|
| `015/PA-WADIR-I/SK/DO/II/2026` | `SK_DO_015-PA-WADIR-I-SK-DO-II-2026.pdf` | `SKPB_DO_015-PA-WADIR-I-SK-DO-II-2026.pdf` |
| `001/PA-WADIR-I/SK/DO/I/2026` | `SK_DO_001-PA-WADIR-I-SK-DO-I-2026.pdf` | `SKPB_DO_001-PA-WADIR-I-SK-DO-I-2026.pdf` |
| `10/PMA-WADIR-I/SK/DO/XII/2018` | `SK_DO_10-PMA-WADIR-I-SK-DO-XII-2018.pdf` | `SKPB_DO_10-PMA-WADIR-I-SK-DO-XII-2018.pdf` |

## ⚠️ Fallback

Jika `NoSkDo` kosong/null:
```
Nama file: SK_DO_DO_{droId}.pdf
Contoh: SK_DO_DO_16-PMA-DO-I-2026.pdf
```

## 🧪 Testing

### Test Case 1: Upload SK dengan Nomor Surat Ada

**Request**:
```bash
POST /api/DropOut/upload-sk-file
Content-Type: multipart/form-data

DroId: 16/PMA/DO/I/2026
SkFile: [file.pdf]
```

**Expected**:
- File tersimpan: `wwwroot/uploads/dropout/sk/SK_DO_016-PA-WADIR-I-SK-DO-I-2026.pdf`
- Database updated dengan path: `/uploads/dropout/sk/SK_DO_016-PA-WADIR-I-SK-DO-I-2026.pdf`

### Test Case 2: Upload SK tanpa Nomor Surat (Draft)

**Request**:
```bash
POST /api/DropOut/upload-sk-file
Content-Type: multipart/form-data

DroId: 4
SkFile: [file.pdf]
```

**Expected**:
- File tersimpan: `wwwroot/uploads/dropout/sk/SK_DO_DO_4.pdf`
- Database updated dengan path: `/uploads/dropout/sk/SK_DO_DO_4.pdf`

### Test Case 3: Upload SK + SKPB Sekaligus

**Request**:
```bash
POST /api/DropOut/upload-sk-file
Content-Type: multipart/form-data

DroId: 2/PMA/DO/II/2026
SkFile: [sk.pdf]
SkpbFile: [skpb.pdf]
```

**Expected**:
- SK: `wwwroot/uploads/dropout/sk/SK_DO_002-PA-WADIR-I-SK-DO-II-2026.pdf`
- SKPB: `wwwroot/uploads/dropout/skpb/SKPB_DO_002-PA-WADIR-I-SK-DO-II-2026.pdf`

## 🎯 Keuntungan Approach Ini

1. ✅ **Konsisten**: Nama file sama dengan nomor surat di database
2. ✅ **Traceable**: Mudah cari file berdasarkan nomor surat
3. ✅ **No Duplicate**: Tidak generate nomor baru yang bisa conflict
4. ✅ **Clean**: Tidak perlu panggil SP `sia_createNoSurat` lagi
5. ✅ **Simple**: Langsung pakai data yang sudah ada

## 📝 Notes

- Nomor surat (`NoSkDo`) sudah di-generate sebelumnya saat approval
- Karakter `/` dan `\` di-replace dengan `-` untuk keamanan filename
- Jika upload ulang, file lama akan di-overwrite (FileMode.Create)
- Extension file diambil dari file asli yang diupload

## 🔗 Related Files

- `Controllers/DropOutController.cs` - Upload logic
- `Repositories/Implementations/DropOutRepository.cs` - Database operations
- `DTOs/DropOut/UploadSKFileRequest.cs` - Request DTO

## ✅ Status

**IMPLEMENTED** - Ready for testing

**Last Updated**: 2026-02-06
