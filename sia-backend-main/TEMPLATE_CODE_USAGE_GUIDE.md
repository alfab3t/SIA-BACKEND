# Template Code Usage Guide - SK Drop Out

## Overview
Panduan lengkap cara menggunakan template ASP.NET WebForms untuk generate PDF SK Drop Out menggunakan Crystal Reports.

## Cara Menggunakan Ketiga File Template

### **1. Setup di Sistem Lama (WebForms)**

#### **File Structure:**
```
/Reports/
├── SK_Drop_Out.aspx              ← HTML markup
├── SK_Drop_Out.aspx.cs           ← Logic code (Page_Load)
├── SK_Drop_Out.aspx.designer.cs  ← Auto-generated controls
└── Report_SK_Drop_Out_2.rpt      ← Crystal Report template
```

#### **Dependencies:**
```
/bin/
├── PolmanAstraLibrary.dll        ← Custom library untuk database & encryption
├── CrystalDecisions.CrystalReports.Engine.dll
├── CrystalDecisions.Shared.dll
└── CrystalDecisions.Web.dll
```

#### **Configuration (web.config):**
```xml
<connectionStrings>
    <add name="DefaultConnection" connectionString="encrypted_connection_string" />
</connectionStrings>

<appSettings>
    <add key="linkUserID" value="encrypted_db_user" />
    <add key="linkPassword" value="encrypted_db_password" />
    <add key="linkServerName" value="encrypted_server_name" />
    <add key="linkDatabaseName" value="encrypted_database_name" />
</appSettings>
```

### **2. Cara User Download SK:**

#### **Step 1: Frontend Request**
```javascript
// Frontend redirect ke halaman WebForms
window.location.href = `/Reports/SK_Drop_Out.aspx?token=${encryptedId}`;
```

#### **Step 2: Server Process**
```csharp
// Page_Load otomatis dijalankan saat halaman diakses
protected void Page_Load(object sender, EventArgs e)
{
    // 1. Decrypt ID dari query string
    String id = Decrypt(Request.QueryString["token"]);
    
    // 2. Ambil data dari database
    dt = lib.CallProcedure("sia_detailDO", new string[] { id });
    
    // 3. Load Crystal Report template
    reportdocument.Load(Server.MapPath("Report_SK_Drop_Out_2.rpt"));
    
    // 4. Set database connection
    reportdocument.SetDatabaseLogon(userID, password, server, database);
    
    // 5. Set parameters (p1-p50)
    reportdocument.SetParameterValue("@p1", id);
    for (int i = 2; i <= 50; i++) {
        reportdocument.SetParameterValue($"@p{i}", "");
    }
    
    // 6. Export to PDF dan download
    reportdocument.ExportToHttpResponse(ExportFormatType.PortableDocFormat, Response, false, fileName);
}
```

#### **Step 3: User Gets PDF**
- Browser otomatis download file PDF
- Nama file: `SK_Drop_Out_No.{nomor_surat}.pdf`

### **3. Konversi ke Sistem API Modern**

#### **Endpoint Baru:**
```
GET /api/dropout/template-code
```

#### **Response DTO:**
```json
{
    "aspxCode": "HTML markup code",
    "aspxCsCode": "C# logic code", 
    "aspxDesignerCode": "Auto-generated designer code",
    "reportLogic": "Penjelasan cara kerja sistem",
    "description": "Template ASP.NET WebForms untuk generate PDF SK Drop Out",
    "requiredFiles": [
        "SK_Drop_Out.aspx",
        "SK_Drop_Out.aspx.cs",
        "SK_Drop_Out.aspx.designer.cs",
        "Report_SK_Drop_Out_2.rpt",
        "PolmanAstraLibrary.dll"
    ],
    "configurationSteps": {
        "1": "Deploy files ke folder /Reports/ di IIS server",
        "2": "Pastikan Crystal Reports runtime terinstall",
        "3": "Set connection string di web.config",
        "4": "Set encrypted credentials di appSettings",
        "5": "Test akses halaman"
    }
}
```

## Optimisasi yang Sudah Dilakukan

### **Original Code (98 baris):**
```csharp
// Main report parameters (49 baris)
reportdocument.SetParameterValue("@p1", id);
reportdocument.SetParameterValue("@p2", "");
reportdocument.SetParameterValue("@p3", "");
// ... sampai p50

// Subreport parameters (49 baris)
reportdocument.SetParameterValue("@p1", id, reportdocument.Subreports[0].Name);
reportdocument.SetParameterValue("@p2", "", reportdocument.Subreports[0].Name);
// ... sampai p50
```

### **Optimized Code (8 baris):**
```csharp
// Main report parameters (4 baris)
reportdocument.SetParameterValue("@p1", id);
for (int i = 2; i <= 50; i++) {
    reportdocument.SetParameterValue($"@p{i}", "");
}

// Subreport parameters (4 baris)
reportdocument.SetParameterValue("@p1", id, reportdocument.Subreports[0].Name);
for (int i = 2; i <= 50; i++) {
    reportdocument.SetParameterValue($"@p{i}", "", reportdocument.Subreports[0].Name);
}
```

## Troubleshooting

### **Common Issues:**

1. **Crystal Reports Error:**
   - Install Crystal Reports runtime di server
   - Pastikan .rpt file accessible

2. **Database Connection Error:**
   - Cek encrypted credentials di web.config
   - Test koneksi database manual

3. **Permission Error:**
   - Set IIS application pool identity
   - Grant access ke folder Reports

4. **File Not Found:**
   - Pastikan semua file di folder yang benar
   - Cek case-sensitive path

### **Testing:**
```
1. Test URL: /Reports/SK_Drop_Out.aspx?token=test_encrypted_id
2. Expected: PDF download atau error message
3. Check IIS logs untuk debugging
```

## Migration Path ke ASP.NET Core

### **Option 1: Service Pattern**
```csharp
public class ReportService : IReportService
{
    public async Task<byte[]> GeneratePdfAsync(string droId)
    {
        // Logic dari Page_Load dipindah ke sini
        // Return byte[] instead of HttpResponse
    }
}
```

### **Option 2: External Report Service**
```csharp
[HttpGet("{id}/generate-pdf-sk")]
public async Task<IActionResult> GeneratePdfSK(string id)
{
    // Call external report service
    var client = _httpClientFactory.CreateClient();
    var response = await client.PostAsync(reportServiceUrl, content);
    var pdfBytes = await response.Content.ReadAsByteArrayAsync();
    return File(pdfBytes, "application/pdf", fileName);
}
```

## API Endpoints Summary

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/dropout/template-sk?type=aspx` | GET | Download .aspx file |
| `/api/dropout/template-sk?type=aspx-cs` | GET | Download .aspx.cs file |
| `/api/dropout/template-sk?type=aspx-designer` | GET | Download .designer.cs file |
| `/api/dropout/template-code` | GET | Get all template code as DTO |
| `/api/dropout/template-sk/list` | GET | List all available templates |

## Permissions Required

- **Template Download:** `drop_out.view`
- **Template Code Access:** `drop_out.view`

---

**Note:** Template code ini adalah untuk referensi dan pembelajaran. Untuk implementasi production, disarankan menggunakan service pattern atau external report service.