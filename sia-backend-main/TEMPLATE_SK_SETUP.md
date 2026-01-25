# Setup Template SK (Crystal Reports & ASP.NET)

## Status File Template

### Drop Out - ✅ 100% LENGKAP!
File template Drop Out sudah tersedia di folder:
```
wwwroot/uploads/dropout/templates/
├── SK_Drop_Out.aspx ✅
├── SK_Drop_Out.aspx.cs ✅
├── SK_Drop_Out.aspx.designer.cs ✅
├── Report_SK_Drop_Out_2.cs ✅
└── Report_SK_Drop_Out_2.rpt ✅ (3.8 MB)
```

### Pengunduran Diri - ✅ 100% LENGKAP!
File template Pengunduran Diri sudah tersedia di folder:
```
wwwroot/uploads/pengundurandiri/templates/
├── SK_Pengunduran_Diri.aspx ✅
├── SK_Pengunduran_Diri.aspx.cs ✅
├── SK_Pengunduran_Diri.aspx.designer.cs ✅
├── Report_SK_Pengunduran_Diri_2.cs ✅
└── Report_SK_Pengunduran_Diri_2.rpt ✅ (3.8 MB)
```

**🎉 Semua file template sudah lengkap dan siap digunakan!**

## Struktur Folder

Untuk **Pengunduran Diri**, letakkan file di:
```
wwwroot/uploads/pengundurandiri/templates/
├── Report_SK_Pengunduran_Diri_2.rpt
├── Report_SK_Pengunduran_Diri_2.cs
├── SK_Pengunduran_Diri.aspx
├── SK_Pengunduran_Diri.aspx.cs
└── SK_Pengunduran_Diri.aspx.designer.cs
```

Untuk **Drop Out**, letakkan file di:
```
wwwroot/uploads/dropout/templates/
├── Report_SK_Drop_Out_2.rpt
├── Report_SK_Drop_Out_2.cs
├── SK_Drop_Out.aspx
├── SK_Drop_Out.aspx.cs
└── SK_Drop_Out.aspx.designer.cs
```

## Cara Menggunakan API

### 1. Lihat Daftar Template yang Tersedia

**Pengunduran Diri:**
```
GET /api/pengundurandiri/template-sk/list
```

**Drop Out:**
```
GET /api/dropout/template-sk/list
```

Response:
```json
{
  "message": "Daftar file template SK Pengunduran Diri",
  "files": [
    {
      "type": "rpt",
      "name": "Report_SK_Pengunduran_Diri_2.rpt",
      "description": "Template report Crystal Reports",
      "url": "/api/pengundurandiri/template-sk?type=rpt"
    },
    {
      "type": "cs",
      "name": "Report_SK_Pengunduran_Diri_2.cs",
      "description": "Wrapper class untuk .rpt",
      "url": "/api/pengundurandiri/template-sk?type=cs"
    },
    ...
  ]
}
```

### 2. Download Template Spesifik

**Download file .rpt (Crystal Reports):**
```
GET /api/pengundurandiri/template-sk?type=rpt
GET /api/dropout/template-sk?type=rpt
```

**Download file .cs (Wrapper class):**
```
GET /api/pengundurandiri/template-sk?type=cs
GET /api/dropout/template-sk?type=cs
```

**Download file .aspx (Halaman web):**
```
GET /api/pengundurandiri/template-sk?type=aspx
GET /api/dropout/template-sk?type=aspx
```

**Download file .aspx.cs (Logic/controller):**
```
GET /api/pengundurandiri/template-sk?type=aspx-cs
GET /api/dropout/template-sk?type=aspx-cs
```

**Download file .aspx.designer.cs (Designer code):**
```
GET /api/pengundurandiri/template-sk?type=aspx-designer
GET /api/dropout/template-sk?type=aspx-designer
```

## Catatan

- File `.rpt` adalah template Crystal Reports yang digunakan untuk generate SK
- File `.aspx` adalah halaman ASP.NET Web Forms untuk menampilkan SK
- File `.cs` adalah code-behind untuk halaman ASP.NET
- Semua file ini biasanya digunakan di aplikasi ASP.NET terpisah, bukan di backend API ini
- Backend API ini hanya menyediakan endpoint untuk download file template tersebut

## Backward Compatibility

Endpoint lama masih bisa digunakan dengan default type=rpt:
```
GET /api/pengundurandiri/template-sk
GET /api/dropout/template-sk
```

Akan otomatis download file `.rpt`
