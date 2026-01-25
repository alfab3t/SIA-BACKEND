# Quick Reference - Nomor Surat Generator

## ✅ What's Done

Nomor surat generator sudah diimplementasikan untuk:
- ✅ Drop Out Controller
- ✅ Pengunduran Diri Controller

## 📁 Files Modified

1. `Helpers/NoSuratGenerator.cs` - Helper class untuk generate nomor
2. `Controllers/DropOutController.cs` - Upload endpoint menggunakan generator
3. `Controllers/PengunduranDiriController.cs` - Upload endpoint menggunakan generator

## 🔧 How It Works

### Upload SK Drop Out
```bash
POST /api/dropout/upload-sk-file
- DroId: "001/PMA/DO/I/2026"
- SkFile: [file.pdf]
- SkpbFile: [file.pdf]

# File akan disimpan sebagai:
# SK_DO_001-PMA-DO-I-2026.pdf
# SKPB_DO_001-PMA-DO-I-2026.pdf
```

### Upload SK Pengunduran Diri
```bash
POST /api/pengundurandiri/upload-sk-file
- PdiId: "001/PMA/PD/I/2026"
- SkFile: [file.pdf]
- SkpbFile: [file.pdf]

# File akan disimpan sebagai:
# SK_PD_001-PMA-PD-I-2026.pdf
# SKPB_PD_001-PMA-PD-I-2026.pdf
```

## ⚙️ Configuration Needed

### 1. Update Jenis Surat ID

Cek ID di database:
```sql
SELECT jsu_id, jsu_nama, jsu_format_no 
FROM sia_msjenissurat
WHERE jsu_status = 'Aktif'
```

Update di controller:
```csharp
// DropOutController.cs line ~550
string jenisSuratId = "JS_DROP_OUT"; // ← Ganti dengan ID yang benar

// PengunduranDiriController.cs line ~550
string jenisSuratId = "JS_PENGUNDURAN_DIRI"; // ← Ganti dengan ID yang benar
```

### 2. Format Nomor Surat

Format diambil dari database kolom `jsu_format_no`:
- `XXX` → Nomor urut (001, 002, ...)
- `MM` → Bulan romawi (I, II, III, ...)
- `YYYY` → Tahun (2026)
- `PPP` → Konsentrasi (TI, TM, ...)

## 🧪 Testing

1. Upload file SK melalui endpoint
2. Cek nama file yang tersimpan di folder:
   - `wwwroot/uploads/dropout/sk/`
   - `wwwroot/uploads/dropout/skpb/`
   - `wwwroot/uploads/pengundurandiri/sk/`
   - `wwwroot/uploads/pengundurandiri/skpb/`
3. Verify nomor surat sesuai dengan format di database

## 📝 Notes

- Nomor surat di-generate otomatis saat upload
- Tidak insert ke database (hanya untuk penamaan file)
- Slash "/" diganti dengan "-" untuk file system
- Nomor urut reset setiap tahun baru
- Jika gagal generate, fallback ke timestamp

## 📚 Full Documentation

- `NOMOR_SURAT_GENERATOR.md` - Dokumentasi lengkap
- `IMPLEMENTATION_SUMMARY.md` - Summary implementasi
- `QUICK_REFERENCE.md` - Quick reference (file ini)

---

**Status**: ✅ Ready for testing
**Build**: ✅ No errors, 0 warnings
