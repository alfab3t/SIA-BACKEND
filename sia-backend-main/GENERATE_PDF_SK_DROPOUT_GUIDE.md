# Generate PDF SK Drop Out - Implementation Guide

## Overview
Endpoint untuk generate PDF SK Drop Out dengan memanggil service report eksternal di `http://10.5.0.94/api/Report/GetReport`.

## Endpoint Baru

### GET /api/DropOut/{id}/generate-pdf-sk

Generate dan download PDF SK Drop Out dari service report eksternal.

**Permission Required:** `drop_out.export`

**Path Parameter:**
- `id` (string, required) - ID Drop Out

**Response:**
- Success: File PDF (application/pdf)
- Error 404: Data SK Drop Out tidak ditemukan
- Error 500: Gagal generate PDF dari service report

**Contoh Request:**
```
GET /api/DropOut/1%2FPMA%2FDO%2FIII%2F2026/generate-pdf-sk
Authorization: Bearer {token}
```

**Contoh Response (Success):**
```
Content-Type: application/pdf
Content-Disposition: attachment; filename="SK_DO_1-PMA-DO-III-2026_20260310.pdf"

[PDF Binary Data]
```

## Alur Kerja

```
Client → Backend API → Service Report (10.5.0.94) → PDF → Client
```

1. Client memanggil endpoint dengan ID Drop Out
2. Backend mengambil data report dari database:
   - `GetReportSKDOAsync(id)` - Data utama SK
   - `GetReportSKDOSubAsync(id)` - Data subreport
3. Backend membuat HTTP POST request ke service report eksternal
4. Service report generate PDF berdasarkan data yang dikirim
5. Backend menerima PDF sebagai byte array
6. Backend return PDF ke client dengan nama file yang sesuai

## Konfigurasi

### 1. appsettings.json

Tambahkan konfigurasi URL service report:

```json
{
  "ReportService": {
    "Url": "http://10.5.0.94/api/Report/GetReport"
  }
}
```

### 2. Program.cs

HttpClient sudah ditambahkan:

```csharp
builder.Services.AddHttpClient();
```

### 3. DropOutController.cs

Constructor sudah diupdate untuk inject `IHttpClientFactory`:

```csharp
private readonly IHttpClientFactory _httpClientFactory;

public DropOutController(
    IDropOutRepository repo, 
    IConfiguration configuration, 
    IHttpClientFactory httpClientFactory)
{
    _repo = repo;
    Configuration = configuration;
    _httpClientFactory = httpClientFactory;
}
```

## Request Body ke Service Report

Format request yang dikirim ke `http://10.5.0.94/api/Report/GetReport`:

```json
{
  "reportName": "Report_SK_Drop_Out_2",
  "parameters": {
    "droId": "1/PMA/DO/III/2026",
    "reportData": {
      // Data dari GetReportSKDOAsync
    },
    "reportSubData": [
      // Data dari GetReportSKDOSubAsync
    ]
  }
}
```

## Error Handling

Endpoint ini menangani berbagai error:

1. **Data tidak ditemukan (404)**
   ```json
   {
     "message": "Data SK Drop Out tidak ditemukan."
   }
   ```

2. **Service report gagal (Status Code dari service)**
   ```json
   {
     "message": "Gagal generate PDF dari service report",
     "error": "Error detail dari service"
   }
   ```

3. **Exception (500)**
   ```json
   {
     "message": "Terjadi kesalahan saat generate PDF SK",
     "error": "Exception message"
   }
   ```

## Testing

### 1. Test dengan Postman/Thunder Client

```
GET http://localhost:5234/api/DropOut/1%2FPMA%2FDO%2FIII%2F2026/generate-pdf-sk
Authorization: Bearer {your_jwt_token}
```

### 2. Test dengan cURL

```bash
curl -X GET "http://localhost:5234/api/DropOut/1%2FPMA%2FDO%2FIII%2F2026/generate-pdf-sk" \
  -H "Authorization: Bearer {your_jwt_token}" \
  --output SK_DO.pdf
```

### 3. Verifikasi

- Pastikan service report di `http://10.5.0.94/api/Report/GetReport` sudah running
- Pastikan data Drop Out dengan ID tersebut ada di database
- Pastikan user memiliki permission `drop_out.export`
- Cek apakah PDF berhasil di-generate dan bisa dibuka

## Customization

### Mengubah Parameter Report

Jika service report membutuhkan parameter berbeda, edit bagian `requestBody`:

```csharp
var requestBody = new
{
    reportName = "Report_SK_Drop_Out_2",
    parameters = new
    {
        // Sesuaikan dengan kebutuhan service report
        droId = id,
        customParam1 = "value1",
        customParam2 = "value2"
    }
};
```

### Mengubah Nama File Output

Edit bagian return:

```csharp
var fileName = $"SK_DO_{id.Replace("/", "-")}_{DateTime.Now:yyyyMMdd}.pdf";
```

### Timeout Configuration

Jika perlu set timeout untuk HTTP request:

```csharp
var client = _httpClientFactory.CreateClient();
client.Timeout = TimeSpan.FromSeconds(60); // 60 detik timeout
```

## Best Practices

1. **Logging** - Tambahkan logging untuk debugging:
   ```csharp
   _logger.LogInformation($"Generating PDF for Drop Out ID: {id}");
   _logger.LogError($"Failed to generate PDF: {ex.Message}");
   ```

2. **Caching** - Jika PDF tidak berubah, pertimbangkan caching
3. **Async/Await** - Sudah menggunakan async untuk performance
4. **Error Handling** - Sudah ada try-catch untuk handle exception
5. **Security** - Sudah ada permission check `drop_out.export`

## Troubleshooting

### PDF tidak ter-generate

1. Cek apakah service report running:
   ```bash
   curl http://10.5.0.94/api/Report/GetReport
   ```

2. Cek log error di console backend
3. Cek apakah data report ada di database
4. Cek format request body sesuai dengan yang diharapkan service report

### Timeout Error

Increase timeout di HttpClient:
```csharp
client.Timeout = TimeSpan.FromMinutes(5);
```

### Permission Denied

Pastikan user memiliki permission `drop_out.export` di database.

## Related Endpoints

- `GET /api/DropOut/{id}/report-sk` - Get data report SK (JSON)
- `GET /api/DropOut/{id}/report-sk-sub` - Get data subreport SK (JSON)
- `GET /api/DropOut/download-sk-file/{droId}` - Download SK file yang sudah diupload
- `GET /api/DropOut/template-sk?type=rpt` - Download template report

## Notes

- Endpoint ini berbeda dengan `download-sk-file` yang download file SK yang sudah diupload
- Endpoint ini generate PDF baru dari service report eksternal
- PDF yang di-generate tidak disimpan di server, langsung di-stream ke client
- Jika ingin menyimpan PDF, tambahkan logic untuk save ke folder `wwwroot/uploads/dropout/sk/`
