# Nomor Surat Generator

## Overview

Helper class untuk generate nomor surat otomatis berdasarkan format yang ada di database, tanpa perlu memanggil stored procedure `sia_createNoSurat`.

## Fitur

- ✅ Generate nomor surat dengan format yang sama dengan SP `sia_createNoSurat`
- ✅ Auto-increment nomor urut (XXX)
- ✅ Konversi bulan ke romawi (MM)
- ✅ Tahun otomatis (YYYY)
- ✅ Support konsentrasi/prodi (PPP)
- ✅ Reset nomor urut setiap tahun baru
- ✅ Tidak insert ke database (khusus untuk penamaan file)

## Cara Penggunaan

### 1. Inject IConfiguration ke Controller

```csharp
public class DropOutController : ControllerBase
{
    private readonly IDropOutService _service;
    private readonly IConfiguration Configuration;

    public DropOutController(IDropOutService service, IConfiguration configuration)
    {
        _service = service;
        Configuration = configuration;
    }
}
```

### 2. Generate Nomor SK untuk Penamaan File

```csharp
private async Task<string> GenerateNoSKAsync(string droId)
{
    try
    {
        // Get connection string
        var connString = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
            Configuration.GetConnectionString("DefaultConnection")!,
            Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
        );
        
        // Create generator
        var generator = new Helpers.NoSuratGenerator(connString);
        
        // Generate nomor surat
        // Parameter 1: jenisSuratId (sesuai dengan sia_msjenissurat)
        // Parameter 2: konsentrasiId (optional)
        string jenisSuratId = "JS_DROP_OUT"; // Ganti dengan ID yang sesuai
        return await generator.GenerateNoSKForFileAsync(jenisSuratId);
    }
    catch
    {
        // Fallback jika gagal
        return $"SK_DO_{DateTime.Now:yyyyMMddHHmmss}";
    }
}
```

### 3. Gunakan untuk Penamaan File

```csharp
// Generate nomor SK
string nomorSK = await GenerateNoSKAsync(request.DroId);

// Gunakan untuk nama file (replace "/" dengan "-" untuk file system)
var skFileName = $"SK_DO_{nomorSK.Replace("/", "-")}{ext}";
```

## Format Nomor Surat

Format nomor surat diambil dari tabel `sia_msjenissurat` kolom `jsu_format_no`.

Contoh format:
- `XXX/PMA/DO/MM/YYYY` → `001/PMA/DO/I/2026`
- `XXX/PMA/PD/PPP/MM/YYYY` → `001/PMA/PD/TI/I/2026`

Placeholder yang di-replace:
- `XXX` → Nomor urut 3 digit (001, 002, 003, ...)
- `MM` → Bulan dalam romawi (I, II, III, ..., XII)
- `YYYY` → Tahun 4 digit (2026)
- `PPP` → Singkatan konsentrasi/prodi (TI, TM, dll)

## Konfigurasi Jenis Surat ID

Sesuaikan `jenisSuratId` dengan data di tabel `sia_msjenissurat`:

```sql
-- Cek jenis surat yang tersedia
SELECT jsu_id, jsu_nama, jsu_format_no 
FROM sia_msjenissurat
WHERE jsu_status = 'Aktif'
```

Contoh:
- Drop Out: `JS_DROP_OUT` atau sesuai ID di database
- Pengunduran Diri: `JS_PENGUNDURAN_DIRI` atau sesuai ID di database
- Cuti Akademik: `JS_CUTI_AKADEMIK` atau sesuai ID di database

## Implementasi di Controller Lain

### Pengunduran Diri Controller ✅ IMPLEMENTED

```csharp
private async Task<string> GenerateNoSKAsync(string pdiId)
{
    try
    {
        var connString = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
            Configuration.GetConnectionString("DefaultConnection")!,
            Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
        );
        
        var generator = new Helpers.NoSuratGenerator(connString);
        string jenisSuratId = "JS_PENGUNDURAN_DIRI"; // Sesuaikan
        
        return await generator.GenerateNoSKForFileAsync(jenisSuratId);
    }
    catch
    {
        return $"SK_PD_{DateTime.Now:yyyyMMddHHmmss}";
    }
}
```

**Status**: ✅ Sudah diimplementasikan di `PengunduranDiriController.cs`
- Method `GenerateNoSKAsync` sudah ditambahkan
- Endpoint `upload-sk-file` sudah menggunakan generator
- File naming format: `SK_PD_{nomorSK}` dan `SKPB_PD_{nomorSK}`

### Cuti Akademik Controller

```csharp
private async Task<string> GenerateNoSKAsync(string cakId)
{
    try
    {
        var connString = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
            Configuration.GetConnectionString("DefaultConnection")!,
            Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
        );
        
        var generator = new Helpers.NoSuratGenerator(connString);
        string jenisSuratId = "JS_CUTI_AKADEMIK"; // Sesuaikan
        
        return await generator.GenerateNoSKForFileAsync(jenisSuratId);
    }
    catch
    {
        return $"SK_CA_{DateTime.Now:yyyyMMddHHmmss}";
    }
}
```

## Catatan Penting

1. **Tidak Insert ke Database**: Method `GenerateNoSKForFileAsync` hanya generate nomor untuk penamaan file, tidak insert ke tabel `sia_mssurat`

2. **Fallback**: Selalu sediakan fallback dengan timestamp jika generate gagal

3. **File System Safe**: Replace karakter "/" dengan "-" untuk nama file:
   ```csharp
   nomorSK.Replace("/", "-")
   ```

4. **Jenis Surat ID**: Pastikan `jenisSuratId` sesuai dengan data di database

5. **Konsentrasi ID**: Jika format nomor surat menggunakan placeholder `PPP`, pass konsentrasi ID sebagai parameter kedua:
   ```csharp
   await generator.GenerateNoSKForFileAsync(jenisSuratId, konsentrasiId);
   ```

## Testing

Test dengan upload file SK dan cek nama file yang di-generate:

### Drop Out
```bash
# Upload SK Drop Out
POST /api/dropout/upload-sk-file
Content-Type: multipart/form-data

DroId: 001/PMA/DO/I/2026
SkFile: [file]

# Response
{
  "message": "Upload SK DO berhasil",
  "nomorSK": "001/PMA/DO/I/2026",
  "skPath": "/uploads/dropout/sk/SK_DO_001-PMA-DO-I-2026.pdf",
  "skpbPath": "/uploads/dropout/skpb/SKPB_DO_001-PMA-DO-I-2026.pdf"
}
```

### Pengunduran Diri
```bash
# Upload SK Pengunduran Diri
POST /api/pengundurandiri/upload-sk-file
Content-Type: multipart/form-data

PdiId: 001/PMA/PD/I/2026
SkFile: [file]

# Response
{
  "message": "Upload SK Pengunduran Diri berhasil",
  "nomorSK": "001/PMA/PD/I/2026",
  "skPath": "/uploads/pengundurandiri/sk/SK_PD_001-PMA-PD-I-2026.pdf",
  "skpbPath": "/uploads/pengundurandiri/skpb/SKPB_PD_001-PMA-PD-I-2026.pdf"
}
```

## Troubleshooting

**Error: Format nomor surat tidak ditemukan**
- Cek apakah `jenisSuratId` ada di tabel `sia_msjenissurat`
- Pastikan `jsu_status = 'Aktif'`

**Nomor urut tidak increment**
- Cek apakah ada data di tabel `sia_mssurat` dengan format yang sama
- Pastikan query `GetNextNomorUrutAsync` berjalan dengan benar

**Konsentrasi tidak muncul**
- Pastikan pass `konsentrasiId` sebagai parameter kedua
- Cek apakah format nomor surat menggunakan placeholder `PPP`
