# Template SK Pengunduran Diri

## File yang Tersedia

✅ **SK_Pengunduran_Diri.aspx** - Halaman web ASP.NET untuk menampilkan SK
✅ **SK_Pengunduran_Diri.aspx.cs** - Code-behind/logic untuk halaman ASP.NET
✅ **SK_Pengunduran_Diri.aspx.designer.cs** - Designer code ASP.NET (auto-generated)
✅ **Report_SK_Pengunduran_Diri_2.rpt** - Template Crystal Reports (3.8 MB)
✅ **Report_SK_Pengunduran_Diri_2.cs** - Wrapper class untuk .rpt

🎉 **Semua file sudah lengkap!**

## Cara Download via API

### Download file .aspx (Halaman web)
```
GET /api/pengundurandiri/template-sk?type=aspx
```

### Download file .aspx.cs (Code-behind)
```
GET /api/pengundurandiri/template-sk?type=aspx-cs
```

### Download file .aspx.designer.cs (Designer code)
```
GET /api/pengundurandiri/template-sk?type=aspx-designer
```

### Download file .rpt (Crystal Reports) - Setelah ditambahkan
```
GET /api/pengundurandiri/template-sk?type=rpt
```

### Download file .cs (Wrapper class) - Setelah ditambahkan
```
GET /api/pengundurandiri/template-sk?type=cs
```

### Lihat daftar semua file
```
GET /api/pengundurandiri/template-sk/list
```

## Catatan Penting

File-file ini adalah bagian dari aplikasi ASP.NET Web Forms yang terpisah dari backend API ini. Backend API hanya menyediakan endpoint untuk download file template.

Untuk menggunakan template ini:
1. Deploy file-file ini ke aplikasi ASP.NET Web Forms
2. Pastikan file Report_SK_Pengunduran_Diri_2.rpt sudah ada di folder yang sama
3. Konfigurasi connection string dan app settings sesuai dengan environment
4. Akses halaman dengan URL: `https://your-app.com/Reports/SK_Pengunduran_Diri.aspx?token={encrypted_id}`

## File yang Masih Perlu Ditambahkan

Untuk melengkapi template, tambahkan file berikut ke folder ini:
- **Report_SK_Pengunduran_Diri_2.rpt** - File Crystal Reports template
- **Report_SK_Pengunduran_Diri_2.cs** - Wrapper class untuk load .rpt file

Setelah file ditambahkan, endpoint API akan otomatis bisa download file tersebut.
