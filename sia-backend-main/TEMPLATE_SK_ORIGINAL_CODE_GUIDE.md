# Template SK Drop Out - Original Code Guide

## Overview
Endpoint untuk mendapatkan template ASP.NET WebForms **KODE ASLI** (tanpa optimisasi) untuk download SK dan SKPB Drop Out.

## Alur Sistem

### **Input:** 
ID Mahasiswa (encrypted)

### **Output:** 
Download Template SK dan SKPB (PDF)

## Cara Kerja Kode Asli

### **1. User Input**
```
https://server.com/Reports/SK_Drop_Out.aspx?token=encrypted_mahasiswa_id
```

### **2. Process Flow**
```csharp
// 1. Decrypt ID mahasiswa dari token
String id = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(Request.QueryString["token"].ToString(), "PolmanAstra_SIA").Split('#')[0];

// 2. Ambil data mahasiswa dari database
dt = lib.CallProcedure("sia_detailDO", new string[] { id });

// 3. Load Crystal Report template
reportdocument.Load(Server.MapPath("Report_SK_Drop_Out_2.rpt"));

// 4. Set database connection (encrypted credentials)
reportdocument.SetDatabaseLogon(userID, password, server, database);

// 5. Set 98 parameter (KODE ASLI - TIDAK DIOPTIMASI)
reportdocument.SetParameterValue("@p1", id);
reportdocument.SetParameterValue("@p2", "");
reportdocument.SetParameterValue("@p3", "");
// ... sampai @p50 (49 baris)

// Subreport parameters
reportdocument.SetParameterValue("@p1", id, reportdocument.Subreports[0].Name);
reportdocument.SetParameterValue("@p2", "", reportdocument.Subreports[0].Name);
// ... sampai @p50 (49 baris)

// 6. Generate dan download PDF
reportdocument.ExportToHttpResponse(ExportFormatType.PortableDocFormat, Response, false, fileName);
```

### **3. Crystal Report Process**
- Crystal Report ambil data dari stored procedure `sia_detailDO`
- Format data sesuai template design (.rpt file)
- Generate PDF SK dan SKPB
- User langsung download file

## API Endpoint

### **GET /api/dropout/template-code**

**Response:**
```json
{
  "aspxCode": "HTML markup lengkap",
  "aspxCsCode": "KODE ASLI C# (98 baris SetParameterValue)",
  "aspxDesignerCode": "Auto-generated designer code",
  "reportLogic": "Penjelasan alur sistem",
  "description": "Template ASP.NET WebForms untuk download SK dan SKPB Drop Out (KODE ASLI - 98 baris parameter)",
  "requiredFiles": [
    "SK_Drop_Out.aspx",
    "SK_Drop_Out.aspx.cs", 
    "SK_Drop_Out.aspx.designer.cs",
    "Report_SK_Drop_Out_2.rpt",
    "PolmanAstraLibrary.dll",
    "CrystalDecisions.CrystalReports.Engine.dll"
  ],
  "configurationSteps": {
    "1": "Deploy files ke folder /Reports/ di IIS server",
    "2": "Pastikan Crystal Reports runtime terinstall di server",
    "3": "Set connection string 'DefaultConnection' di web.config (encrypted)",
    "4": "Set encrypted credentials di appSettings",
    "5": "Pastikan stored procedure tersedia",
    "6": "Test akses: /Reports/SK_Drop_Out.aspx?token=encrypted_mahasiswa_id",
    "7": "ALUR: Input ID Mahasiswa → Crystal Report generate → Download PDF SK & SKPB"
  }
}
```

## Kode Asli vs Optimized

### **Kode Asli (98 baris):**
```csharp
// Main report (49 baris)
reportdocument.SetParameterValue("@p1", id);
reportdocument.SetParameterValue("@p2", "");
reportdocument.SetParameterValue("@p3", "");
// ... sampai @p50

// Subreport (49 baris)  
reportdocument.SetParameterValue("@p1", id, reportdocument.Subreports[0].Name);
reportdocument.SetParameterValue("@p2", "", reportdocument.Subreports[0].Name);
// ... sampai @p50
```

### **Optimized (8 baris):**
```csharp
// Main report (4 baris)
reportdocument.SetParameterValue("@p1", id);
for (int i = 2; i <= 50; i++) {
    reportdocument.SetParameterValue($"@p{i}", "");
}

// Subreport (4 baris)
reportdocument.SetParameterValue("@p1", id, reportdocument.Subreports[0].Name);
for (int i = 2; i <= 50; i++) {
    reportdocument.SetParameterValue($"@p{i}", "", reportdocument.Subreports[0].Name);
}
```

**ENDPOINT INI MENGGUNAKAN KODE ASLI (98 baris) SESUAI PERMINTAAN**

## Testing

### **Test Endpoint:**
```bash
curl -X GET "http://localhost:5234/api/dropout/template-code" \
  -H "Authorization: Bearer your_token"
```

### **Expected Result:**
- Status: 200 OK
- Content: JSON dengan semua template code
- aspxCsCode: Berisi 98 baris SetParameterValue (kode asli)

## Key Points

1. **Kode Asli Dipertahankan:** Tidak ada optimisasi, pakai 98 baris persis seperti yang diberikan
2. **Alur Tetap Sama:** Input ID Mahasiswa → Output Download SK & SKPB
3. **Crystal Report Focus:** Yang penting adalah template .rpt dan parameter binding
4. **Database Integration:** Stored procedure `sia_detailDO` untuk ambil data mahasiswa
5. **Encryption:** Semua credentials dan ID di-encrypt untuk security

---

**Template code siap digunakan dengan kode asli yang tidak dimodifikasi!**