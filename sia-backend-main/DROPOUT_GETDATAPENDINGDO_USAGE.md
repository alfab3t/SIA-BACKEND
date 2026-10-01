# Guide: Cara Pakai SP sia_getDataPendingDO (Updated)

## Overview
SP ini sudah diupdate dengan:
- Default sorting: Draft & Revisi muncul paling atas, diurutkan tanggal terbaru
- Pagination pakai ROW_NUMBER()
- Return kolom `Count` untuk total records

## Parameter SP

```sql
@username VARCHAR(50),      -- Username yang login
@keyword VARCHAR(MAX),      -- Keyword pencarian
@sort_by VARCHAR(100),      -- Custom sorting (kosong = default)
@kon_id VARCHAR(50),        -- Filter konsentrasi
@role_id VARCHAR(50),       -- Role ID
@display_name VARCHAR(100), -- Display name
@status VARCHAR(MAX),       -- Filter status (comma-separated)
@Page INT,                  -- Nomor halaman (NULL = tanpa pagination)
@PageSize INT               -- Jumlah data per halaman
```

## Contoh Penggunaan di C# Repository

### 1. Tanpa Pagination (Get All)

```csharp
public async Task<List<DropOutListResponse>> GetAllPendingDropOut(string username, string keyword = "")
{
    var parameters = new[]
    {
        new SqlParameter("@username", username),
        new SqlParameter("@keyword", keyword ?? ""),
        new SqlParameter("@sort_by", ""),  // Empty = default sorting
        new SqlParameter("@kon_id", ""),
        new SqlParameter("@role_id", ""),
        new SqlParameter("@display_name", ""),
        new SqlParameter("@status", ""),
        new SqlParameter("@Page", DBNull.Value),
        new SqlParameter("@PageSize", DBNull.Value)
    };

    var result = await _context.Set<DropOutListResponse>()
        .FromSqlRaw("EXEC sia_getDataPendingDO @username, @keyword, @sort_by, @kon_id, @role_id, @display_name, @status, @Page, @PageSize", parameters)
        .ToListAsync();

    return result;
}
```

### 2. Dengan Pagination

```csharp
public async Task<PaginatedResponse<DropOutListResponse>> GetPendingDropOutPaginated(
    string username, 
    string keyword = "", 
    string status = "",
    int page = 1, 
    int pageSize = 10)
{
    var parameters = new[]
    {
        new SqlParameter("@username", username),
        new SqlParameter("@keyword", keyword ?? ""),
        new SqlParameter("@sort_by", ""),  // Default sorting
        new SqlParameter("@kon_id", ""),
        new SqlParameter("@role_id", ""),
        new SqlParameter("@display_name", ""),
        new SqlParameter("@status", status ?? ""),
        new SqlParameter("@Page", page),
        new SqlParameter("@PageSize", pageSize)
    };

    var result = await _context.Set<DropOutListWithCountResponse>()
        .FromSqlRaw("EXEC sia_getDataPendingDO @username, @keyword, @sort_by, @kon_id, @role_id, @display_name, @status, @Page, @PageSize", parameters)
        .ToListAsync();

    if (!result.Any())
    {
        return new PaginatedResponse<DropOutListResponse>
        {
            Data = new List<DropOutListResponse>(),
            TotalRecords = 0,
            Page = page,
            PageSize = pageSize
        };
    }

    var totalRecords = result.First().Count; // Ambil dari kolom Count

    return new PaginatedResponse<DropOutListResponse>
    {
        Data = result.Select(x => new DropOutListResponse
        {
            DroId = x.dro_id,
            MhsId = x.mhs_id,
            MhsNama = x.mhs_nama,
            KonNama = x.kon_nama,
            DroCreatedDate = x.dro_created_date,
            DroCreatedBy = x.dro_created_by,
            SrtNo = x.srt_no,
            DroStatus = x.dro_status
        }).ToList(),
        TotalRecords = totalRecords,
        Page = page,
        PageSize = pageSize
    };
}
```

### 3. Dengan Filter Status Multiple

```csharp
public async Task<PaginatedResponse<DropOutListResponse>> GetPendingDropOutByStatus(
    string username, 
    List<string> statusList,
    int page = 1, 
    int pageSize = 10)
{
    // Join status dengan koma
    var statusFilter = string.Join(",", statusList);

    var parameters = new[]
    {
        new SqlParameter("@username", username),
        new SqlParameter("@keyword", ""),
        new SqlParameter("@sort_by", ""),
        new SqlParameter("@kon_id", ""),
        new SqlParameter("@role_id", ""),
        new SqlParameter("@display_name", ""),
        new SqlParameter("@status", statusFilter),  // "Draft,Revisi"
        new SqlParameter("@Page", page),
        new SqlParameter("@PageSize", pageSize)
    };

    // ... sama seperti contoh #2
}
```

## DTO Response

### DropOutListWithCountResponse (untuk pagination)
```csharp
public class DropOutListWithCountResponse
{
    public int rownum { get; set; }
    public string dro_id { get; set; }
    public string mhs_id { get; set; }
    public string mhs_nama { get; set; }
    public string kon_nama { get; set; }
    public string dro_created_date { get; set; }
    public string dro_created_by { get; set; }
    public string srt_no { get; set; }
    public string dro_status { get; set; }
    public int Count { get; set; }  // Total records
}
```

## Default Sorting Behavior

Kalau `@sort_by` kosong atau NULL, SP akan pakai sorting ini:

```sql
CASE WHEN dro_status IN ('Draft', 'Revisi') THEN 0 ELSE 1 END, 
dro_created_date DESC
```

Artinya:
1. Status "Draft" dan "Revisi" muncul paling atas (priority 0)
2. Status lainnya di bawah (priority 1)
3. Dalam setiap grup, diurutkan tanggal terbaru duluan

## Custom Sorting

Kalau mau override default sorting, isi parameter `@sort_by`:

```csharp
// Sort by nama mahasiswa A-Z
@sort_by = "mhs_nama ASC"

// Sort by status, lalu tanggal
@sort_by = "dro_status ASC, dro_created_date DESC"

// Sort by tanggal terlama
@sort_by = "dro_created_date ASC"
```

## Filter Status Multiple

Format: comma-separated string

```csharp
// Filter Draft dan Revisi saja
@status = "Draft,Revisi"

// Filter yang perlu approval
@status = "Belum Disetujui Wadir 1,Belum Disetujui Direktur"

// Semua status
@status = ""
```

## Response Structure

Setiap row akan punya:
- `rownum`: Nomor urut row (untuk pagination)
- `Count`: Total records (sebelum pagination) - sama di semua row
- Data dropout lainnya

Contoh response:
```json
[
  {
    "rownum": 1,
    "dro_id": "DO001",
    "mhs_nama": "12345 - John Doe",
    "kon_nama": "TI (Teknik Informatika)",
    "dro_created_date": "25 Mar 2026",
    "dro_status": "Draft",
    "Count": 25  // Total 25 records
  },
  {
    "rownum": 2,
    "dro_id": "DO002",
    "mhs_nama": "12346 - Jane Smith",
    "kon_nama": "SI (Sistem Informasi)",
    "dro_created_date": "24 Mar 2026",
    "dro_status": "Revisi",
    "Count": 25  // Total 25 records
  }
  // ... 8 more records (total 10 per page)
]
```

## Testing

Jalankan script test: `SQL_TEST_GETDATAPENDINGDO_NEW.sql`
