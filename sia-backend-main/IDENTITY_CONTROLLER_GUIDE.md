# Employee Identity Controller Guide

## Overview
Controller sederhana untuk mendapatkan identitas karyawan dan memisahkan role antara Wadir1 dan Finance meskipun mereka memiliki role yang sama di sistem.

## Stored Procedure
Menggunakan stored procedure `all_getIdentityByUser` yang mengembalikan:
- `kry_username`: Username karyawan
- `jab_main_id`: ID posisi utama
- `str_main_id`: ID struktur/departemen  
- `rol_id`: ID role
- `kry_id`: ID karyawan

## Endpoint

### Get Identity
**GET** `/api/employeeidentity/get-identity/{username}`

Mendapatkan identitas lengkap dan informasi role berdasarkan username.

**Parameter:**
- `username` (path parameter): Username karyawan yang ingin dicek

**Contoh Request:**
```
GET /api/employeeidentity/get-identity/john.doe
```

**Response:**
```json
{
  "identity": {
    "kryUsername": "john.doe",
    "jabMainId": "WADIR1",
    "strMainId": "ACADEMIC_DEPT",
    "rolId": "ROLE_001",
    "kryId": "EMP_001"
  },
  "roles": {
    "isWadir": true,
    "isFinance": false,
    "roleType": "wadir"
  }
}
```

## Konfigurasi

### Position IDs
Update ID posisi di `EmployeeIdentityService.cs` sesuai dengan nilai database Anda:

```csharp
// Posisi Wadir
var wadirPositionIds = new[] { "WADIR1", "WADIR2", "WADIR3" };

// Posisi Finance  
var financePositionIds = new[] { "FINANCE", "KEUANGAN", "FIN" };
var financeStructureIds = new[] { "FINANCE_DEPT", "KEUANGAN_DEPT" };
```

## Penggunaan di Controller Lain

Anda dapat inject `IEmployeeIdentityService` ke controller lain untuk mengecek role:

```csharp
public class SomeController : ControllerBase
{
    private readonly IEmployeeIdentityService _employeeIdentityService;

    public SomeController(IEmployeeIdentityService employeeIdentityService)
    {
        _employeeIdentityService = employeeIdentityService;
    }

    [HttpPost("wadir-only-action")]
    public async Task<IActionResult> WadirOnlyAction(string username)
    {
        var isWadir = await _employeeIdentityService.IsWadirAsync(username);
        
        if (!isWadir)
            return Forbid("Hanya Wadir yang dapat melakukan aksi ini");
            
        // Logic khusus Wadir di sini
        return Ok();
    }
}
```

## Contoh Penggunaan

### Curl Command:
```bash
curl -X 'GET' \
  'http://localhost:5234/api/EmployeeIdentity/get-identity/john.doe' \
  -H 'accept: */*'
```

### Response Success:
```json
{
  "identity": {
    "kryUsername": "john.doe",
    "jabMainId": "WADIR1",
    "strMainId": "ACADEMIC_DEPT",
    "rolId": "ROLE_001",
    "kryId": "EMP_001"
  },
  "roles": {
    "isWadir": true,
    "isFinance": false,
    "roleType": "wadir"
  }
}
```

### Response Error:
```json
{
  "message": "Employee identity not found"
}
```

## Catatan
- Endpoint ini tidak memerlukan authentication
- Username diberikan sebagai path parameter
- Sistem akan otomatis mengecek apakah user adalah Wadir atau Finance berdasarkan konfigurasi position IDs