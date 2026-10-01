# Cara Menggunakan Template Download SK Drop Out

## Overview
Dokumen ini menjelaskan **cara implementasi endpoint API** untuk **mengunduh template SK** dari service report eksternal menggunakan **ASP.NET Core**.

**Catatan Penting:**
Endpoint pada dokumen ini **bukan endpoint final**. Developer diharapkan **meng-ATM (mengambil & menyesuaikan)** pola implementasi sesuai kebutuhan bisnis masing-masing.

## Tujuan
- Menyediakan pola standar download template SK
- Menjadi referensi implementasi internal
- Menjaga konsistensi antar endpoint report

## Konsep Alur
```
Client → API (DownloadTemplate) → Service Report → PDF File → Client
```

## Contoh Endpoint

### **POST /DownloadTemplate/{example}**

Endpoint ini hanya contoh penamaan. Silakan sesuaikan nama endpoint, parameter, dan report sesuai konteks aplikasi.

### **Path Parameter**

| Parameter | Type | Deskripsi |
|-----------|------|-----------|
| example | string | Parameter dinamis yang dikirim ke service report |

Parameter `example` dapat diganti menjadi:
- nim
- idMahasiswa  
- periode
- noTransaksi

## Contoh Implementasi Controller

```csharp
[HttpPost("DownloadTemplate/{example}")]
public async Task<IActionResult> DownloadTemplate(string example)
{
    var client = _httpClientFactory.CreateClient();
    var url = "url_service_report";
    
    var requestBody = new
    {
        reportName = "Report_Institusi_Indonesia",
        parameters = new { example }
    };
    
    var content = new StringContent(
        JsonSerializer.Serialize(requestBody),
        Encoding.UTF8,
        "application/json"
    );
    
    var response = await client.PostAsync(url, content);
    
    if (!response.IsSuccessStatusCode)
        return BadRequest("Gagal mengambil file PDF");
    
    var pdfBytes = await response.Content.ReadAsByteArrayAsync();
    
    return File(pdfBytes, "application/pdf", $"Report_{example}.pdf");
}
```

## Best Practice

- Gunakan HttpClientFactory
- Simpan URL service di appsettings.json  
- Lakukan validasi parameter
- Gunakan penamaan report yang konsisten
- Tambahkan logging untuk error handling

## Checklist Endpoint Baru

- [ ] Endpoint sesuai kebutuhan bisnis
- [ ] Parameter sesuai dengan report
- [ ] reportName sudah benar
- [ ] Nama file output sesuai standar
- [ ] Error handling tersedia

---

Dokumentasi ini dibuat sebagai **template implementasi internal**. Silakan dikembangkan sesuai standar dan kebutuhan organisasi.

## Implementasi untuk SK Drop Out

### **Endpoint Khusus SK Drop Out:**

```csharp
[HttpPost("DownloadTemplateSK/{mahasiswaId}")]
public async Task<IActionResult> DownloadTemplateSK(string mahasiswaId)
{
    var client = _httpClientFactory.CreateClient();
    var url = Configuration["ReportService:Url"];
    
    var requestBody = new
    {
        reportName = "Report_SK_Drop_Out_2",
        parameters = new { 
            mahasiswaId = mahasiswaId,
            // Parameter tambahan sesuai kebutuhan Crystal Report
            p1 = mahasiswaId,
            p2 = "",
            p3 = "",
            // ... sampai p50
        }
    };
    
    var content = new StringContent(
        JsonSerializer.Serialize(requestBody),
        Encoding.UTF8,
        "application/json"
    );
    
    var response = await client.PostAsync(url, content);
    
    if (!response.IsSuccessStatusCode)
        return BadRequest("Gagal mengambil template SK");
    
    var pdfBytes = await response.Content.ReadAsByteArrayAsync();
    
    return File(pdfBytes, "application/pdf", $"Template_SK_DropOut_{mahasiswaId}.pdf");
}
```

### **Cara Penggunaan:**

1. **Frontend Call:**
```javascript
// Download template SK untuk mahasiswa tertentu
fetch('/api/dropout/DownloadTemplateSK/12345', {
    method: 'POST',
    headers: {
        'Authorization': 'Bearer ' + token
    }
})
.then(response => response.blob())
.then(blob => {
    // Download file PDF
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'Template_SK_DropOut.pdf';
    a.click();
});
```

2. **Input:** ID Mahasiswa (12345)
3. **Process:** API call service report dengan Crystal Report template
4. **Output:** Download PDF template SK & SKPB

### **Konfigurasi Required:**

```json
// appsettings.json
{
  "ReportService": {
    "Url": "http://10.5.0.94/api/Report/GetReport",
    "UseMock": false
  }
}
```

### **Dependencies:**
- Crystal Report template: `Report_SK_Drop_Out_2.rpt`
- Stored procedure: `sia_detailDO`
- Service report eksternal
- HttpClientFactory

**Template download siap digunakan dengan pola implementasi yang konsisten!**